using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework.VehicleMapFramework
{
    public enum GravshipCombatSector
    {
        None,
        Front,
        Port,       // Left flank
        Starboard   // Right flank
    }

    /// <summary>
    /// Combat-specific utilities for gravship hover vehicles.
    /// Handles sector-based combat evaluation using RimWorld's ShotReport formula,
    /// dynamic orbit radius calculation, and orbit direction selection (port vs starboard vs front).
    /// Prevents exposing the rear thrusters, guarantees standoff distance so targets never end up
    /// underneath the gravship, and dynamically adapts when turrets on any side are destroyed.
    /// </summary>
    public static class VRF_GravshipCombatUtility
    {
        // ── Orbit tuning constants ──────────────────────────────────────────────
        private const float OrbitRadiusMin = 10f;
        private const float OrbitRadiusMax = 45f;
        private const float OrbitRadiusFallback = 16f;

        // How often (in game ticks) the turret list cache refreshes
        private const int TurretCacheLifetimeTicks = 45; // ~0.75 seconds

        private static readonly Dictionary<int, TurretSectorCache> s_turretCaches =
            new Dictionary<int, TurretSectorCache>();

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates all surviving and operational turrets on the gravship at the ship's intended
        /// combat orbit distance, weighing them by their actual hit probability from ShotReport and expected damage.
        /// Determines the best combat sector to present to the enemy (Front, Port, or Starboard).
        /// Rear is explicitly excluded to protect thrusters.
        /// </summary>
        public static GravshipCombatSector GetBestCombatSector(
            VehiclePawn vehicle,
            LocalTargetInfo enemyTarget,
            out float bestSectorScore)
        {
            bestSectorScore = 0f;
            if (vehicle == null || !enemyTarget.IsValid)
                return GravshipCombatSector.Front;

            TurretSectorCache cache = GetOrBuildCache(vehicle);
            if (cache == null || cache.turrets.Count == 0)
                return GravshipCombatSector.Front;

            float frontScore = 0f;
            float portScore = 0f;
            float starboardScore = 0f;

            int frontActiveTurrets = 0;
            int portActiveTurrets = 0;
            int starboardActiveTurrets = 0;

            // Evaluate firepower at the gravship's intended combat orbit radius.
            // This prevents transient positions (e.g. approaching from afar or brief overshoots)
            // from zeroing out turrets and causing false fallbacks.
            float evalDist = GetGravshipOrbitRadius(vehicle);

            for (int i = 0; i < cache.turrets.Count; i++)
            {
                InteriorTurretInfo t = cache.turrets[i];
                if (!IsTurretOperational(t.building))
                    continue;

                Verb verb = t.building.AttackVerb;
                if (verb == null || verb.verbProps == null)
                    continue;

                // Calculate hit probability using ShotReport mechanics at combat orbit distance
                float hitChance = CalculateHitChance(t.building, verb, enemyTarget, evalDist);
                if (hitChance <= 0.001f)
                    continue;

                int burst = Mathf.Max(1, verb.verbProps.burstShotCount);
                float damage = 15f;
                if (verb.verbProps.defaultProjectile?.projectile != null)
                {
                    damage = Mathf.Max(1f, verb.verbProps.defaultProjectile.projectile.GetDamageAmount((Thing)null));
                }

                // Expected combat output: hitChance * burst * damage
                float turretPower = hitChance * burst * damage;

                if (t.canFront)
                {
                    frontScore += turretPower;
                    frontActiveTurrets++;
                }
                if (t.canPort)
                {
                    portScore += turretPower;
                    portActiveTurrets++;
                }
                if (t.canStarboard)
                {
                    starboardScore += turretPower;
                    starboardActiveTurrets++;
                }
            }

            // Calculate current relative bearing of the enemy to break close ties
            CompVehicleHover hover = vehicle.GetComp<CompVehicleHover>();
            float shipHeading = hover != null ? hover.currentFlyAngle : vehicle.Angle;

            Vector3 enemyDraw = enemyTarget.CenterVector3;
            Vector3 shipDraw = vehicle.DrawPos;
            float dx = enemyDraw.x - shipDraw.x;
            float dz = enemyDraw.z - shipDraw.z;
            float angleToEnemy = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;

            // Relative angle from front: 0 = front, +90 = starboard (right), -90 = port (left)
            float relBearing = Mathf.DeltaAngle(shipHeading, angleToEnemy);

            float frontTurnNeeded = Mathf.Abs(Mathf.DeltaAngle(0f, relBearing));
            float starboardTurnNeeded = Mathf.Abs(Mathf.DeltaAngle(90f, relBearing));
            float portTurnNeeded = Mathf.Abs(Mathf.DeltaAngle(-90f, relBearing));

            // Select the sector based primarily on available firepower.
            // If firepower difference is within 15%, use the flank requiring less rotation.
            GravshipCombatSector bestSector = GravshipCombatSector.Front;
            float maxScore = frontScore;
            bestSectorScore = frontScore;

            if (starboardScore > maxScore)
            {
                maxScore = starboardScore;
                bestSector = GravshipCombatSector.Starboard;
                bestSectorScore = starboardScore;
            }
            if (portScore > maxScore)
            {
                maxScore = portScore;
                bestSector = GravshipCombatSector.Port;
                bestSectorScore = portScore;
            }

            // Tie-breaker if multiple operational sectors have competitive firepower (within 15%)
            if (maxScore > 0f)
            {
                float tieThreshold = maxScore * 0.85f;
                float bestTurn = float.MaxValue;
                GravshipCombatSector tieWinner = bestSector;

                if (frontScore >= tieThreshold && frontTurnNeeded < bestTurn)
                {
                    bestTurn = frontTurnNeeded;
                    tieWinner = GravshipCombatSector.Front;
                }
                if (starboardScore >= tieThreshold && starboardTurnNeeded < bestTurn)
                {
                    bestTurn = starboardTurnNeeded;
                    tieWinner = GravshipCombatSector.Starboard;
                }
                if (portScore >= tieThreshold && portTurnNeeded < bestTurn)
                {
                    bestTurn = portTurnNeeded;
                    tieWinner = GravshipCombatSector.Port;
                }

                bestSector = tieWinner;
                bestSectorScore = (bestSector == GravshipCombatSector.Front) ? frontScore :
                                  (bestSector == GravshipCombatSector.Starboard) ? starboardScore : portScore;
            }
            else
            {
                // Fallback if all scores are 0 (e.g. out of ammo or heavily damaged):
                // Pick whichever sector has any surviving turret requiring least turn
                if (starboardActiveTurrets > 0 && starboardTurnNeeded <= portTurnNeeded && starboardTurnNeeded <= frontTurnNeeded)
                    return GravshipCombatSector.Starboard;
                if (portActiveTurrets > 0 && portTurnNeeded <= starboardTurnNeeded && portTurnNeeded <= frontTurnNeeded)
                    return GravshipCombatSector.Port;
                if (starboardActiveTurrets > 0)
                    return GravshipCombatSector.Starboard;
                if (portActiveTurrets > 0)
                    return GravshipCombatSector.Port;
                if (frontActiveTurrets > 0)
                    return GravshipCombatSector.Front;

                return GravshipCombatSector.Front;
            }

            return bestSector;
        }

        /// <summary>
        /// Calculates the ideal orbit radius around the enemy, factoring in the
        /// weapon ranges and ShotReport accuracy curve of operational turrets.
        /// </summary>
        public static float GetGravshipOrbitRadius(VehiclePawn vehicle)
        {
            TurretSectorCache cache = GetOrBuildCache(vehicle);
            if (cache == null || cache.turrets.Count == 0)
                return OrbitRadiusFallback;

            float totalIdealRange = 0f;
            int count = 0;

            for (int i = 0; i < cache.turrets.Count; i++)
            {
                InteriorTurretInfo t = cache.turrets[i];
                if (!IsTurretOperational(t.building)) continue;

                Verb verb = t.building.AttackVerb;
                if (verb == null || verb.verbProps == null) continue;

                float maxR = verb.EffectiveRange;
                float minR = verb.verbProps.minRange;
                if (maxR <= 0f) continue;

                // Optimal engagement distance: around 60-70% of max range, well clear of minRange
                float ideal = Mathf.Clamp(maxR * 0.65f, minR + 4f, maxR - 2f);
                totalIdealRange += ideal;
                count++;
            }

            if (count == 0) return OrbitRadiusFallback;

            float avg = totalIdealRange / count;
            return Mathf.Clamp(avg, OrbitRadiusMin, OrbitRadiusMax);
        }

        /// <summary>
        /// Advances the orbit angle smoothly along the perimeter around the enemy.
        /// Uses vehicle speed and orbit circumference to ensure a smooth, small angular step
        /// that never cuts chords across the center.
        /// - Starboard faces enemy during clockwise orbit (+1).
        /// - Port faces enemy during counter-clockwise orbit (-1).
        /// - Front maintains an orbit ring without charging through the target.
        /// </summary>
        public static float AdvanceOrbitAngle(
            float currentAngle,
            GravshipCombatSector sector,
            float speedScale,
            float orbitRadius,
            int tickInterval)
        {
            float safeRadius = Mathf.Max(5f, orbitRadius);
            float circumference = 2f * Mathf.PI * safeRadius;

            // Distance covered in tickInterval
            float distanceCovered = Mathf.Max(0.5f, speedScale * (tickInterval / 60f));
            float degStep = (distanceCovered / circumference) * 360f;

            // Clamp angular step between 12 and 26 degrees to prevent chords cutting into the center
            degStep = Mathf.Clamp(degStep, 12f, 26f);

            // Orbit Direction:
            // Starboard (Right) faces inward when orbiting clockwise (+degStep).
            // Port (Left) faces inward when orbiting counter-clockwise (-degStep).
            // Front uses clockwise orbit by default while strafing.
            float dir = (sector == GravshipCombatSector.Port) ? -1f : 1f;

            float newAngle = (currentAngle + dir * degStep + 360f) % 360f;
            return newAngle;
        }

        /// <summary>
        /// Calculates the hit chance of a turret against a target using Verse.ShotReport logic.
        /// Uses Verse.ShotReport.HitFactorFromShooter and VerbProperties.GetHitChanceFactor.
        /// </summary>
        public static float CalculateHitChance(
            Building_TurretGun turret,
            Verb verb,
            LocalTargetInfo target,
            float distance)
        {
            if (turret == null || verb == null || verb.verbProps == null)
                return 0f;

            if (distance < verb.verbProps.minRange || distance > verb.EffectiveRange)
                return 0f;

            // Mathematical estimation using the exact ShotReport algorithm:
            // 1. Shooter accuracy factor (ShootingAccuracyTurret ^ distance)
            float factorFromShooter = !verb.verbProps.canGoWild
                ? 1f
                : ShotReport.HitFactorFromShooter(turret, distance);

            // 2. Weapon equipment accuracy factor
            float factorFromEquipment = verb.verbProps.GetHitChanceFactor(
                (Thing)verb.EquipmentSource, distance);

            float hitChance = factorFromShooter * factorFromEquipment;

            // 3. Target size factor (from ShotReport)
            if (target.HasThing && target.Thing != null)
            {
                float targetSize = 1f;
                if (target.Thing is Pawn p)
                {
                    targetSize = p.BodySize;
                }
                else if (target.Thing.def != null)
                {
                    targetSize = target.Thing.def.fillPercent *
                                 target.Thing.def.size.x *
                                 target.Thing.def.size.z * 2.5f;
                }
                hitChance *= Mathf.Clamp(targetSize, 0.5f, 2f);
            }

            // 4. Weather factor
            Map map = turret.Map ?? (target.HasThing ? target.Thing.Map : null);
            if (map?.weatherManager != null)
            {
                hitChance *= map.weatherManager.CurWeatherAccuracyMultiplier;
            }

            return Mathf.Clamp01(hitChance);
        }

        /// <summary>
        /// Checks if a turret is fully operational: spawned, alive, powered, manned (if applicable),
        /// not broken down, and with ammo/fuel remaining.
        /// </summary>
        public static bool IsTurretOperational(Building_TurretGun turret)
        {
            if (turret == null || turret.Destroyed || !turret.Spawned || turret.HitPoints <= 0)
                return false;

            // Breakdown check
            var breakdown = turret.TryGetComp<RimWorld.CompBreakdownable>();
            if (breakdown != null && breakdown.BrokenDown)
                return false;

            // Power check
            var power = turret.TryGetComp<RimWorld.CompPowerTrader>();
            if (power != null && !power.PowerOn)
                return false;

            // Mannable check
            var mannable = turret.TryGetComp<RimWorld.CompMannable>();
            if (mannable != null && !mannable.MannedNow)
                return false;

            // Refuelable / Ammo check
            var refuelable = turret.TryGetComp<RimWorld.CompRefuelable>();
            if (refuelable != null && !refuelable.HasFuel)
                return false;

            return true;
        }

        public static void ClearCache(int vehicleId)
        {
            s_turretCaches.Remove(vehicleId);
        }

        // ── Internal Cache Helpers ──────────────────────────────────────────────

        private static TurretSectorCache GetOrBuildCache(VehiclePawn vehicle)
        {
            if (vehicle == null) return null;

            int id = vehicle.thingIDNumber;
            int now = Find.TickManager.TicksGame;

            if (s_turretCaches.TryGetValue(id, out TurretSectorCache existing) &&
                now - existing.builtAtTick < TurretCacheLifetimeTicks)
            {
                return existing;
            }

            TurretSectorCache fresh = BuildCache(vehicle);
            if (fresh != null)
                s_turretCaches[id] = fresh;
            else
                s_turretCaches.Remove(id);

            return fresh;
        }

        private static TurretSectorCache BuildCache(VehiclePawn vehicle)
        {
            var gravship = vehicle as global::VehicleMapFramework.VehiclePawnWithMap;
            if (gravship?.VehicleMap?.listerThings == null)
                return null;

            Map interiorMap = gravship.VehicleMap;
            Vector2 mapCenter = new Vector2(
                interiorMap.Size.x * 0.5f,
                interiorMap.Size.z * 0.5f);

            var things = interiorMap.listerThings.ThingsInGroup(
                ThingRequestGroup.BuildingArtificial);

            var list = new List<InteriorTurretInfo>();

            for (int i = 0; i < things.Count; i++)
            {
                if (!(things[i] is Building_TurretGun turretBuilding)) continue;
                if (turretBuilding.Destroyed) continue;

                Verb attackVerb = turretBuilding.AttackVerb;
                if (attackVerb?.verbProps == null) continue;

                // Relative offset from gravship interior center
                // In RimWorld coords: +Z = North (Forward/Nose), +X = East (Right/Starboard)
                // -X = West (Left/Port), -Z = South (Rear/Thrusters)
                float localDx = turretBuilding.Position.x + 0.5f - mapCenter.x;
                float localDz = turretBuilding.Position.z + 0.5f - mapCenter.y;

                // Determine sectors this turret can cover based on placement and ship hull geometry
                bool canPort = false;
                bool canStarboard = false;
                bool canFront = false;

                // Left flank turret: localDx < -1.0
                if (localDx < -1.0f)
                {
                    canPort = true;
                    if (localDz > 1.5f) canFront = true; // Forward-left turret
                }
                // Right flank turret: localDx > 1.0
                else if (localDx > 1.0f)
                {
                    canStarboard = true;
                    if (localDz > 1.5f) canFront = true; // Forward-right turret
                }
                // Centerline turret
                else
                {
                    if (localDz > 0f)
                    {
                        canFront = true;
                        canPort = true;
                        canStarboard = true;
                    }
                    else
                    {
                        // Mid-rear centerline: broadsides only, NOT direct rear (protect thrusters)
                        canPort = true;
                        canStarboard = true;
                    }
                }

                list.Add(new InteriorTurretInfo
                {
                    building = turretBuilding,
                    localOffset = new Vector2(localDx, localDz),
                    canPort = canPort,
                    canStarboard = canStarboard,
                    canFront = canFront
                });
            }

            return new TurretSectorCache
            {
                builtAtTick = Find.TickManager.TicksGame,
                turrets = list
            };
        }

        // ── Data structures ─────────────────────────────────────────────────────

        private class InteriorTurretInfo
        {
            public Building_TurretGun building;
            public Vector2 localOffset;
            public bool canPort;
            public bool canStarboard;
            public bool canFront;
        }

        private class TurretSectorCache
        {
            public int builtAtTick;
            public List<InteriorTurretInfo> turrets;
        }
    }
}
