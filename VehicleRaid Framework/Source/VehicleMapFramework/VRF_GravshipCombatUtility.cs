using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework.VehicleMapFramework
{
    /// <summary>
    /// Combat-specific utilities for gravship hover vehicles.
    /// Handles orbit radius calculation based on interior turret ranges, and
    /// the best-angle selection that steers the gravship toward positions where
    /// the most interior turrets can fire at the current enemy.
    /// </summary>
    public static class VRF_GravshipCombatUtility
    {
        // ── Orbit tuning constants ──────────────────────────────────────────────
        /// <summary>
        /// Fraction of max turret range used as orbit radius.
        /// 0.85 keeps the gravship well inside firing range.
        /// </summary>
        private const float OrbitRadiusFraction = 0.85f;

        /// <summary>Minimum orbit radius regardless of turret range.</summary>
        private const float OrbitRadiusMin = 8f;

        /// <summary>
        /// Maximum orbit radius.  Large maps are ~250 cells but we never want
        /// the ship circling so far that the orbit looks silly.
        /// </summary>
        private const float OrbitRadiusMax = 60f;

        /// <summary>Fallback radius when no turrets are found.</summary>
        private const float OrbitRadiusFallback = 15f;

        /// <summary>
        /// Number of evenly-spaced candidate angles evaluated each update tick
        /// when choosing the orbit direction.  More candidates = better coverage
        /// detection but slightly more work per tick.
        /// </summary>
        private const int OrbitCandidateCount = 24;

        /// <summary>
        /// How many degrees ahead of the current orbit angle the movement
        /// waypoint is placed, so the vehicle faces its direction of travel.
        /// </summary>
        public const float OrbitWaypointLeadDeg = 45f;

        /// <summary>
        /// Base angular advance per tick at hover-speed 1.0.
        /// Scaled by EffectiveHoverMoveSpeed so faster ships orbit faster.
        /// </summary>
        public const float OrbitDegPerTickBase = 1.2f;

        // ── Cached turret data ──────────────────────────────────────────────────
        /// <summary>
        /// Per-vehicle cache of interior turret data.  Rebuilt whenever the
        /// vehicle id is first seen or the cached tick is stale.
        /// Keyed by vehicle thingIDNumber.
        /// </summary>
        private static readonly Dictionary<int, TurretCache> s_turretCache =
            new Dictionary<int, TurretCache>();

        /// <summary>How many game-ticks a turret cache entry is valid for.</summary>
        private const int TurretCacheLifetimeTicks = 300; // 5 seconds

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the orbit radius the gravship should maintain around its
        /// target.  Computed as <see cref="OrbitRadiusFraction"/> × (max range
        /// of any interior turret), clamped to [<see cref="OrbitRadiusMin"/>,
        /// <see cref="OrbitRadiusMax"/>].
        /// </summary>
        public static float GetGravshipOrbitRadius(VehiclePawn vehicle)
        {
            TurretCache cache = GetOrBuildCache(vehicle);
            if (cache == null || cache.maxRange <= 0f)
                return OrbitRadiusFallback;

            return Mathf.Clamp(cache.maxRange * OrbitRadiusFraction,
                               OrbitRadiusMin, OrbitRadiusMax);
        }

        /// <summary>
        /// Advances the orbit angle and selects the best next orbit angle for
        /// the gravship, favouring positions where more interior turrets have
        /// range and line-of-sight to <paramref name="enemyPos"/>.
        /// </summary>
        /// <param name="vehicle">The gravship vehicle.</param>
        /// <param name="enemyPos">Enemy position on the base (world) map.</param>
        /// <param name="orbitRadius">Radius of the orbit circle.</param>
        /// <param name="currentAngle">Current orbit angle in degrees (0 = north).</param>
        /// <param name="tickInterval">Ticks elapsed since last update (usually 60).</param>
        /// <returns>
        /// The new orbit angle in degrees.  The caller should then compute the
        /// lead waypoint as <c>newAngle + <see cref="OrbitWaypointLeadDeg"/></c>.
        /// </returns>
        public static float GetBestOrbitAngle(
            VehiclePawn vehicle,
            Vector2     enemyPos,
            float       orbitRadius,
            float       currentAngle,
            float       speedScale,
            int         tickInterval)
        {
            // ── 1. Advance the base angle at the natural orbit speed ──────────
            float degPerInterval = OrbitDegPerTickBase * tickInterval * speedScale;
            float advancedAngle  = (currentAngle + degPerInterval) % 360f;

            TurretCache cache = GetOrBuildCache(vehicle);

            // No turret data → just advance normally
            if (cache == null || cache.turrets.Count == 0)
                return advancedAngle;

            // ── 2. Evaluate N candidate angles around the orbit circle ────────
            // We scan a window of ±180° from the advanced angle so the ship
            // can only steer toward nearby positions (avoids teleporting).
            float bestScore   = -1f;
            float bestAngle   = advancedAngle;
            float stepDeg     = 360f / OrbitCandidateCount;

            for (int i = 0; i < OrbitCandidateCount; i++)
            {
                float candidateAngle = (advancedAngle + i * stepDeg) % 360f;
                float score = ScoreCandidateAngle(cache, enemyPos, orbitRadius, candidateAngle);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAngle = candidateAngle;
                }
            }

            // ── 3. Blend best candidate with natural advance ──────────────────
            // We don't jump directly to bestAngle; instead we nudge the advanced
            // angle toward it.  The blend weight controls aggressiveness.
            // A value of 0.25 means 25% pull toward the best spot per update,
            // resulting in a smooth arc rather than sudden direction changes.
            const float BlendWeight = 0.25f;
            float delta  = Mathf.DeltaAngle(advancedAngle, bestAngle);
            float result = (advancedAngle + delta * BlendWeight + 360f) % 360f;

            return result;
        }

        // ── Internals ───────────────────────────────────────────────────────────

        /// <summary>
        /// Score a candidate orbit angle by counting how many interior turrets
        /// would be able to fire at <paramref name="enemyPos"/> if the gravship
        /// were positioned at that angle on the orbit circle.
        /// </summary>
        private static float ScoreCandidateAngle(
            TurretCache cache,
            Vector2     enemyPos,
            float       orbitRadius,
            float       candidateAngle)
        {
            // Gravship center if it were at this orbit angle
            float rad = candidateAngle * Mathf.Deg2Rad;
            Vector2 candidateShipPos = enemyPos + new Vector2(
                Mathf.Sin(rad),
                Mathf.Cos(rad)) * orbitRadius;

            float score = 0f;

            for (int i = 0; i < cache.turrets.Count; i++)
            {
                InteriorTurretInfo t = cache.turrets[i];

                // World position of this turret if the ship were at candidateShipPos.
                // The local offset is already stored rotated to North; we rotate it
                // by the orbit angle so it follows the ship's heading.
                // (For NPC gravships the heading approximates the orbit tangent;
                //  using the orbit angle as proxy is accurate enough for scoring.)
                Vector2 turretWorldPos = candidateShipPos + t.localOffset;

                float distToEnemy = Vector2.Distance(turretWorldPos, enemyPos);

                // Full score for a turret squarely in range
                if (distToEnemy >= t.minRange && distToEnemy <= t.maxRange)
                {
                    score += 1f;
                }
                // Partial credit for a turret nearly in range (within 20%)
                else if (distToEnemy < t.maxRange * 1.2f && distToEnemy > t.minRange * 0.8f)
                {
                    score += 0.3f;
                }
            }

            return score;
        }

        /// <summary>
        /// Returns the cached turret data for <paramref name="vehicle"/>,
        /// rebuilding it if missing or stale.
        /// </summary>
        private static TurretCache GetOrBuildCache(VehiclePawn vehicle)
        {
            if (vehicle == null) return null;

            int id  = vehicle.thingIDNumber;
            int now = Find.TickManager.TicksGame;

            if (s_turretCache.TryGetValue(id, out TurretCache existing) &&
                now - existing.builtAtTick < TurretCacheLifetimeTicks)
            {
                return existing;
            }

            TurretCache fresh = BuildCache(vehicle);
            if (fresh != null)
                s_turretCache[id] = fresh;
            else
                s_turretCache.Remove(id);

            return fresh;
        }

        private static TurretCache BuildCache(VehiclePawn vehicle)
        {
            // ── Try interior map first (VMF gravship) ─────────────────────────
            var gravship = vehicle as global::VehicleMapFramework.VehiclePawnWithMap;
            if (gravship != null)
            {
                Map vehicleMap = gravship.VehicleMap;
                if (vehicleMap != null && vehicleMap.listerThings != null)
                    return BuildCacheFromInteriorMap(vehicle, vehicleMap);
            }

            // ── Fallback: conventional CompVehicleTurrets ─────────────────────
            var turretsComp = vehicle.CompVehicleTurrets;
            if (turretsComp?.turrets != null && turretsComp.turrets.Count > 0)
                return BuildCacheFromVehicleTurrets(vehicle, turretsComp);

            return null;
        }

        /// <summary>
        /// Builds turret cache from standard RimWorld <see cref="Building_TurretGun"/>
        /// buildings inside the gravship's interior map.
        /// </summary>
        private static TurretCache BuildCacheFromInteriorMap(VehiclePawn vehicle, Map vehicleMap)
        {
            var turrets = new List<InteriorTurretInfo>();
            float maxRange = 0f;

            // Interior map centre in local cell coords
            Vector2 mapCenter = new Vector2(
                vehicleMap.Size.x * 0.5f,
                vehicleMap.Size.z * 0.5f);

            var things = vehicleMap.listerThings.ThingsInGroup(
                ThingRequestGroup.BuildingArtificial);

            for (int i = 0; i < things.Count; i++)
            {
                if (!(things[i] is Building_TurretGun turretBuilding)) continue;
                if (turretBuilding.Destroyed) continue;

                Verb attackVerb = turretBuilding.AttackVerb;
                if (attackVerb?.verbProps == null) continue;

                float range    = attackVerb.EffectiveRange;
                float minRange = attackVerb.verbProps.minRange;

                if (range <= 0f) continue;

                // Local offset from the map centre (in cells).
                // This represents where this turret sits relative to the ship centre.
                Vector2 localOffset = new Vector2(
                    turretBuilding.Position.x + 0.5f - mapCenter.x,
                    turretBuilding.Position.z + 0.5f - mapCenter.y);

                turrets.Add(new InteriorTurretInfo
                {
                    localOffset = localOffset,
                    maxRange    = range,
                    minRange    = minRange
                });

                if (range > maxRange) maxRange = range;
            }

            if (turrets.Count == 0) return null;

            return new TurretCache
            {
                turrets    = turrets,
                maxRange   = maxRange,
                builtAtTick = Find.TickManager.TicksGame
            };
        }

        /// <summary>
        /// Fallback: builds turret cache from <see cref="CompVehicleTurrets"/>
        /// for gravships that have conventional vehicle turrets instead of
        /// interior map buildings.
        /// </summary>
        private static TurretCache BuildCacheFromVehicleTurrets(
            VehiclePawn vehicle, CompVehicleTurrets turretsComp)
        {
            var turrets = new List<InteriorTurretInfo>();
            float maxRange = 0f;

            foreach (VehicleTurret turret in turretsComp.turrets)
            {
                if (turret == null) continue;
                float range    = turret.MaxRange;
                float minRange = turret.MinRange;
                if (range <= 0f) continue;

                // No known offset for conventional turrets; treat as centred.
                turrets.Add(new InteriorTurretInfo
                {
                    localOffset = Vector2.zero,
                    maxRange    = range,
                    minRange    = minRange
                });

                if (range > maxRange) maxRange = range;
            }

            if (turrets.Count == 0) return null;

            return new TurretCache
            {
                turrets    = turrets,
                maxRange   = maxRange,
                builtAtTick = Find.TickManager.TicksGame
            };
        }

        // ── Data types ──────────────────────────────────────────────────────────

        private class TurretCache
        {
            public List<InteriorTurretInfo> turrets;
            public float maxRange;
            public int   builtAtTick;
        }

        private struct InteriorTurretInfo
        {
            /// <summary>
            /// Offset from the gravship centre in world-space cells (north-up).
            /// </summary>
            public Vector2 localOffset;
            public float   maxRange;
            public float   minRange;
        }
    }
}
