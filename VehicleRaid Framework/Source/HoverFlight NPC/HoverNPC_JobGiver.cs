using UnityEngine;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Vehicles;
using VehicleRaid;
using VehicleRaidFramework.VehicleMapFramework;

namespace VehicleRaidFramework
{
    public class JobGiver_HoverNPCAssault : ThinkNode_JobGiver
    {
        private const int TicksBetweenTargetUpdate = 60;
        private const int MaxPositionSearchCells = 300;

        // ── Gravship orbit state ────────────────────────────────────────────────────
        private const float GravshipOrbitApproachTol = 3.5f; // enter-orbit dead-band (cells)

        // Minimum enemy displacement (cells) before switching from orbit to pursuit translation
        private const float EnemyMovedThreshold = 2.0f;

        // Tracks the last known enemy position per gravship (vehicleId → position)
        private static readonly Dictionary<int, Vector2> s_lastEnemyPos =
            new Dictionary<int, Vector2>();

        private static readonly System.Reflection.FieldInfo s_isFacingTargetField =
            typeof(CompVehicleHover).GetField("isFacingTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        private static readonly System.Reflection.FieldInfo s_facingTargetField =
            typeof(CompVehicleHover).GetField("facingTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        private static void ApplyFacingState(CompVehicleHover hoverComp, Thing enemy, GravshipCombatSector sector)
        {
            if (hoverComp == null) return;

            // Only lock the ship's nose directly onto the enemy if the chosen attack sector is Front
            if (sector == GravshipCombatSector.Front && enemy != null)
            {
                hoverComp.facingTargetNPC = enemy;
                hoverComp.isFacingTargetNPC = true;
                try
                {
                    s_isFacingTargetField?.SetValue(hoverComp, true);
                    s_facingTargetField?.SetValue(hoverComp, new LocalTargetInfo(enemy));
                }
                catch { }
            }
            else
            {
                // For Port (Left) or Starboard (Right), do NOT lock nose onto target.
                // The hover flight tangent naturally points the chosen broadside towards the enemy.
                hoverComp.isFacingTargetNPC = false;
                hoverComp.facingTargetNPC = null;
                try
                {
                    s_isFacingTargetField?.SetValue(hoverComp, false);
                    s_facingTargetField?.SetValue(hoverComp, LocalTargetInfo.Invalid);
                }
                catch { }
            }
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn is VehiclePawn vehicle)) return null;
            if (!vehicle.Spawned || vehicle.Map == null) return null;

            CompVehicleHover hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || hoverComp.State != HoverState.Hovering) return null;

            if (vehicle.mindState?.duty?.def?.defName != "VRF_VehicleSearchAndDestroy") return null;

            // ── Gravship dynamic sector orbit behaviour ─────────────────────────────
            if (CrewManager.IsGravshipVehicle(vehicle))
            {
                if (vehicle.IsHashIntervalTick(TicksBetweenTargetUpdate))
                {
                    Thing enemy = FindBestEnemy(vehicle, hoverComp, 0f, float.MaxValue);

                    if (enemy != null)
                    {
                        // Always maintain weapons locked on enemy
                        hoverComp.attackTarget = enemy;
                        hoverComp.isAttackingTarget = true;

                        Vector2 hoverPos  = hoverComp.realPos;
                        Vector2 enemyPos2 = new Vector2(enemy.DrawPos.x, enemy.DrawPos.z);
                        Vector2 toHover   = hoverPos - enemyPos2;
                        float   dist      = toHover.magnitude;

                        // 1. Evaluate best combat sector using ShotReport accuracy & surviving turrets
                        GravshipCombatSector bestSector = VRF_GravshipCombatUtility
                            .GetBestCombatSector(vehicle, enemy, out float sectorScore);

                        // 2. Dynamic orbit radius based on optimal ShotReport range of surviving turrets
                        float orbitRadius = VRF_GravshipCombatUtility
                            .GetGravshipOrbitRadius(vehicle);

                        // 3. Set facing state:
                        // Front = locks nose onto target; Port/Starboard = broadside aligns with tangent
                        ApplyFacingState(hoverComp, enemy, bestSector);

                        float safeMinDist = orbitRadius * 0.82f;

                        // ── Standoff safety: Prevent the enemy from EVER ending up underneath the gravship ──
                        if (dist < safeMinDist)
                        {
                            // Ship has closed in too close (or enemy walked toward it); push radially outward
                            Vector2 outwardDir;
                            if (dist > 0.1f)
                            {
                                outwardDir = toHover.normalized;
                            }
                            else
                            {
                                float headingRad = hoverComp.currentFlyAngle * Mathf.Deg2Rad;
                                outwardDir = new Vector2(Mathf.Sin(headingRad), Mathf.Cos(headingRad));
                            }

                            Vector2 pushTarget = enemyPos2 + outwardDir * (orbitRadius + 2f);
                            pushTarget = ClampToMap(pushTarget, vehicle.Map);
                            hoverComp.SetTarget(new Vector3(pushTarget.x, 0f, pushTarget.y));
                        }
                        else if (dist > orbitRadius + GravshipOrbitApproachTol)
                        {
                            // ── Phase 1: Tangential approach to orbit perimeter ──────────────────────────
                            float entryAngle = Mathf.Atan2(toHover.x, toHover.y) * Mathf.Rad2Deg;
                            float rad = entryAngle * Mathf.Deg2Rad;
                            Vector2 entryPoint = enemyPos2 + new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * orbitRadius;
                            entryPoint = ClampToMap(entryPoint, vehicle.Map);
                            hoverComp.SetTarget(new Vector3(entryPoint.x, 0f, entryPoint.y));
                        }
                        else
                        {
                            // ── Phase 2: Dynamic Sector Orbit / Target-Tracking Translation ──────────────
                            int vehicleId = vehicle.thingIDNumber;
                            float currentAngle = Mathf.Atan2(toHover.x, toHover.y) * Mathf.Rad2Deg;
                            float speedScale = hoverComp.EffectiveHoverMoveSpeed;

                            bool enemyMoved = false;
                            if (s_lastEnemyPos.TryGetValue(vehicleId, out Vector2 prevEnemyPos))
                            {
                                float enemyDelta = (enemyPos2 - prevEnemyPos).magnitude;
                                enemyMoved = enemyDelta > EnemyMovedThreshold;
                            }

                            // Always update the stored enemy position
                            s_lastEnemyPos[vehicleId] = enemyPos2;

                            Vector2 waypoint;
                            if (enemyMoved)
                            {
                                // Enemy moved: translate the gravship by the same delta the enemy moved,
                                // preserving its relative bearing and distance (no additional orbit spin).
                                Vector2 enemyDelta = enemyPos2 - prevEnemyPos;
                                Vector2 translatedPos = hoverPos + enemyDelta;
                                translatedPos = ClampToMap(translatedPos, vehicle.Map);
                                waypoint = translatedPos;
                            }
                            else
                            {
                                // Enemy stationary: advance orbit angle normally
                                float nextAngle = VRF_GravshipCombatUtility.AdvanceOrbitAngle(
                                    currentAngle,
                                    bestSector,
                                    speedScale,
                                    orbitRadius,
                                    TicksBetweenTargetUpdate);

                                float nextRad = nextAngle * Mathf.Deg2Rad;
                                waypoint = enemyPos2 + new Vector2(Mathf.Sin(nextRad), Mathf.Cos(nextRad)) * orbitRadius;
                                waypoint = ClampToMap(waypoint, vehicle.Map);
                            }

                            hoverComp.SetTarget(new Vector3(waypoint.x, 0f, waypoint.y));
                        }
                    }
                    else
                    {
                        // No enemy – clear attack and hover in place
                        hoverComp.isAttackingTarget = false;
                        hoverComp.attackTarget      = LocalTargetInfo.Invalid;
                        ApplyFacingState(hoverComp, null, GravshipCombatSector.None);
                        s_lastEnemyPos.Remove(vehicle.thingIDNumber);
                    }
                }
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, TicksBetweenTargetUpdate, true);
            }
            // ── End gravship orbit behaviour ────────────────────────────────────────

            if (vehicle.GetLord()?.CurLordToil is LordToil_VehicleHoldPosition)
            {
                if (vehicle.IsHashIntervalTick(TicksBetweenTargetUpdate))
                {
                    hoverComp.isAttackingTarget = false;
                    hoverComp.attackTarget      = LocalTargetInfo.Invalid;
                    hoverComp.isFacingTargetNPC = false;
                    hoverComp.facingTargetNPC   = null;

                    IntVec3 focusCell = vehicle.mindState.duty?.focus.Cell ?? IntVec3.Invalid;
                    if (focusCell.IsValid)
                        hoverComp.SetTarget(focusCell.ToVector3Shifted());
                }
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, TicksBetweenTargetUpdate, true);
            }

            if (vehicle.IsHashIntervalTick(TicksBetweenTargetUpdate))
            {
                float maxRange = vehicle.CompVehicleTurrets?.MaxRange ?? 60f;
                float minRange = GetEffectiveMinRange(vehicle);
                float idealRange = Mathf.Clamp(maxRange * 0.5f, minRange + 2f, maxRange - 2f);
                Vector2 hoverPos = hoverComp.realPos;

                if (HasEnemyTooClose(vehicle, hoverComp, minRange))
                {
                    Vector3 evadePos = FindEvadePosition(vehicle, hoverComp, minRange, maxRange);
                    if (evadePos != Vector3.zero)
                        hoverComp.SetTarget(evadePos);
                }
                else
                {
                    Thing enemy = FindBestEnemy(vehicle, hoverComp, minRange, maxRange);
                    if (enemy != null)
                    {
                        float dist = Vector2.Distance(hoverPos, new Vector2(enemy.DrawPos.x, enemy.DrawPos.z));
                        IntVec3 hoverCell = new IntVec3(Mathf.RoundToInt(hoverPos.x - 0.5f), 0, Mathf.RoundToInt(hoverPos.y - 0.5f));
                        bool hasLOS = GenSight.LineOfSight(hoverCell, enemy.Position, vehicle.Map, true);

                        RoofDef enemyRoof = vehicle.Map.roofGrid.RoofAt(enemy.Position);
                        bool enemyUnderBlockingRoof = enemyRoof != null && HoverRoofUtil.IsBlockingRoof(enemyRoof);

                        if (enemyUnderBlockingRoof)
                        {
                            hoverComp.isAttackingTarget = false;
                            hoverComp.attackTarget = LocalTargetInfo.Invalid;
                            hoverComp.isFacingTargetNPC = false;
                            hoverComp.facingTargetNPC = null;
                        }
                        else
                        {
                            bool needsReposition = !hasLOS || dist < minRange || dist > maxRange;

                            if (hoverComp.FlightType == FlightType.Airplane &&
                                vehicle.CompVehicleTurrets?.turrets?.Count > 0)
                            {
                                bool readyToFire = HoverNPC_AirplaneCombatPlanner.IsReadyToFire(vehicle, hoverComp, enemy);
                                if (!readyToFire || needsReposition)
                                {
                                    Vector3 engagePoint = HoverNPC_AirplaneCombatPlanner.CalcEngagementPoint(vehicle, hoverComp, enemy);
                                    if (engagePoint != Vector3.zero)
                                        hoverComp.SetTarget(engagePoint);
                                }
                            }
                            else
                            {
                                if (needsReposition)
                                {
                                    Vector3 bestPos = FindBestFirePosition(vehicle, hoverComp, enemy, idealRange, maxRange, minRange);
                                    if (bestPos != Vector3.zero)
                                        hoverComp.SetTarget(bestPos);
                                }
                            }

                            if (!hoverComp.isAttackingTarget || hoverComp.attackTarget.Thing != enemy)
                            {
                                hoverComp.attackTarget = enemy;
                                hoverComp.isAttackingTarget = true;
                                hoverComp.facingTargetNPC = enemy;
                                hoverComp.isFacingTargetNPC = true;
                            }
                        }
                    }
                    else
                    {
                        hoverComp.isAttackingTarget = false;
                        hoverComp.attackTarget = LocalTargetInfo.Invalid;
                        hoverComp.isFacingTargetNPC = false;
                        hoverComp.facingTargetNPC = null;
                    }
                }
            }

            return JobMaker.MakeJob(JobDefOf.Wait_Combat, TicksBetweenTargetUpdate, true);
        }

        private bool HasEnemyTooClose(VehiclePawn vehicle, CompVehicleHover hoverComp, float minRange)
        {
            if (minRange <= 0f) return false;

            var hostileTargets = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (hostileTargets == null || hostileTargets.Count == 0) return false;

            Vector2 hoverPos = hoverComp.realPos;
            float threshold = minRange + 2f;
            float thresholdSq = threshold * threshold;

            foreach (var target in hostileTargets)
            {
                Thing t = target.Thing;
                if (t == null || t.Destroyed || t.Map == null) continue;
                if (t is Pawn p && (p.Dead || p.Downed)) continue;

                Vector2 enemyPos = new Vector2(t.DrawPos.x, t.DrawPos.z);
                if (Vector2.SqrMagnitude(hoverPos - enemyPos) < thresholdSq)
                    return true;
            }

            return false;
        }

        private Vector3 FindEvadePosition(VehiclePawn vehicle, CompVehicleHover hoverComp, float minRange, float maxRange)
        {
            Vector2 hoverPos = hoverComp.realPos;
            Vector2 awayDir = Vector2.zero;

            var hostileTargets = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (hostileTargets != null)
            {
                foreach (var target in hostileTargets)
                {
                    Thing t = target.Thing;
                    if (t == null || t.Destroyed || t.Map == null) continue;
                    if (t is Pawn p && (p.Dead || p.Downed)) continue;

                    Vector2 enemyPos = new Vector2(t.DrawPos.x, t.DrawPos.z);
                    Vector2 diff = hoverPos - enemyPos;
                    float distSq = diff.sqrMagnitude;
                    if (distSq > 0.01f && distSq < (minRange + 5f) * (minRange + 5f))
                    {
                        awayDir += diff.normalized / Mathf.Max(Mathf.Sqrt(distSq), 0.1f);
                    }
                }
            }

            if (awayDir == Vector2.zero)
                awayDir = new Vector2(Mathf.Sin(hoverComp.currentFlyAngle * Mathf.Deg2Rad), Mathf.Cos(hoverComp.currentFlyAngle * Mathf.Deg2Rad));

            awayDir.Normalize();
            float evadeDist = minRange + 5f;
            Vector2 targetPos2 = hoverPos + awayDir * evadeDist;

            targetPos2 = ClampToMap(targetPos2, vehicle.Map);

            return new Vector3(targetPos2.x, 0f, targetPos2.y);
        }

        private Vector3 FindBestFirePosition(VehiclePawn vehicle, CompVehicleHover hoverComp, Thing enemy,
            float idealRange, float maxRange, float minRange)
        {
            Vector2 hoverPos = hoverComp.realPos;
            Vector2 enemyPos = new Vector2(enemy.DrawPos.x, enemy.DrawPos.z);

            Vector2 dirToVehicle = (hoverPos - enemyPos).normalized;
            if (dirToVehicle == Vector2.zero)
                dirToVehicle = Vector2.up;

            float currentAngle = Mathf.Atan2(dirToVehicle.x, dirToVehicle.y) * Mathf.Rad2Deg;

            Vector3 bestPos = Vector3.zero;
            float bestScore = float.MinValue;

            for (int i = 0; i < 8; i++)
            {
                float testAngle = (currentAngle + (i * 45f) - 90f) * Mathf.Deg2Rad;
                Vector2 candidatePos = enemyPos + new Vector2(Mathf.Sin(testAngle), Mathf.Cos(testAngle)) * idealRange;

                candidatePos = ClampToMap(candidatePos, vehicle.Map);

                IntVec3 cell = new IntVec3(Mathf.RoundToInt(candidatePos.x - 0.5f), 0, Mathf.RoundToInt(candidatePos.y - 0.5f));

                if (!cell.InBounds(vehicle.Map)) continue;

                RoofDef roof = vehicle.Map.roofGrid.RoofAt(cell);
                if (roof != null && HoverRoofUtil.IsBlockingRoof(roof)) continue;

                if (!GenSight.LineOfSight(cell, enemy.Position, vehicle.Map, true)) continue;

                float distToTarget = Vector2.Distance(candidatePos, enemyPos);
                if (distToTarget < minRange || distToTarget > maxRange) continue;

                float moveDist = Vector2.Distance(hoverPos, candidatePos);
                float score = -moveDist;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = new Vector3(candidatePos.x, 0f, candidatePos.y);
                }
            }

            return bestPos;
        }

        private Thing FindBestEnemy(VehiclePawn vehicle, CompVehicleHover hoverComp, float minRange, float maxRange)
        {
            var hostileTargets = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (hostileTargets == null || hostileTargets.Count == 0) return null;

            Vector2 hoverPos = hoverComp.realPos;

            Thing bestInRange = null;
            float bestInRangeDistSq = float.MaxValue;
            Thing bestOutOfRange = null;
            float bestOutOfRangeDistSq = float.MaxValue;

            foreach (var target in hostileTargets)
            {
                Thing t = target.Thing;
                if (t == null || t.Destroyed || t.Map == null) continue;
                if (t is Pawn p && (p.Dead || p.Downed)) continue;

                // Skip unmanned vehicles — no crew means no threat and no valid target
                if (t is VehiclePawn targetVehicle && !HasLiveCrew(targetVehicle)) continue;

                // Hover vehicle without integrated weapons never targets colony animals
                // (e.g. gravship: its turrets live in the interior map, not built into the vehicle)
                if (!HasIntegratedWeapons(vehicle) && IsColonyAnimal(t)) continue;

                if (t.Map.fogGrid.IsFogged(t.Position)) continue;

                RoofDef roof = t.Map.roofGrid.RoofAt(t.Position);
                if (roof != null && HoverRoofUtil.IsBlockingRoof(roof)) continue;

                float dist = Vector2.Distance(hoverPos, new Vector2(t.DrawPos.x, t.DrawPos.z));

                if (dist >= minRange && dist <= maxRange)
                {
                    float distSq = dist * dist;
                    if (distSq < bestInRangeDistSq)
                    {
                        bestInRangeDistSq = distSq;
                        bestInRange = t;
                    }
                }
                else if (dist > minRange)
                {
                    float distSq = dist * dist;
                    if (distSq < bestOutOfRangeDistSq)
                    {
                        bestOutOfRangeDistSq = distSq;
                        bestOutOfRange = t;
                    }
                }
            }

            return bestInRange ?? bestOutOfRange;
        }

        /// <summary>
        /// True if the vehicle has integrated weapons (Vehicle Framework turrets built into the
        /// vehicle itself). Turrets inside a vehicle interior map (e.g. gravship) do NOT count:
        /// they are separate buildings, not integrated weapons.
        /// </summary>
        private static bool HasIntegratedWeapons(VehiclePawn vehicle)
        {
            var turretComp = vehicle.CompVehicleTurrets;
            return turretComp != null && turretComp.Turrets != null && turretComp.Turrets.Count > 0;
        }

        /// <summary>
        /// True if the thing is an animal that belongs to the player colony (including
        /// trained/owned colony animals).
        /// </summary>
        private static bool IsColonyAnimal(Thing t)
        {
            return t is Pawn p && p.Spawned && p.RaceProps.Animal
                && p.Faction == Faction.OfPlayer;
        }

        /// <summary>
        /// Returns true if the vehicle has at least one conscious, living crew member aboard
        /// (either in a role handler or in the interior map for gravships).
        /// Vehicles with no crew are ignored as targets — they pose no active threat.
        /// </summary>
        private static bool HasLiveCrew(VehiclePawn targetVehicle)
        {
            if (targetVehicle == null) return false;

            // Check pawns in role handlers (driver seat, gunner seats, etc.)
            if (targetVehicle.handlers != null)
            {
                foreach (VehicleRoleHandler handler in targetVehicle.handlers)
                {
                    if (handler?.thingOwner == null) continue;
                    foreach (Pawn pawn in handler.thingOwner)
                    {
                        if (pawn != null && !pawn.Dead && !pawn.Downed)
                            return true;
                    }
                }
            }

            // For gravships (VehiclePawnWithMap): also check the interior map crew
            if (targetVehicle is global::VehicleMapFramework.VehiclePawnWithMap gravship)
            {
                Map interiorMap = gravship.VehicleMap;
                if (interiorMap != null)
                {
                    foreach (Pawn pawn in interiorMap.mapPawns.AllPawnsSpawned)
                    {
                        if (pawn == null || pawn is VehiclePawn) continue;
                        if (pawn.Faction == targetVehicle.Faction && !pawn.Dead && !pawn.Downed)
                            return true;
                    }
                }
            }

            // Also consider cargo pawns (passengers count as crew for threat purposes)
            if (targetVehicle.inventory?.innerContainer != null)
            {
                foreach (Thing thing in targetVehicle.inventory.innerContainer)
                {
                    if (thing is Pawn passenger && !passenger.Dead && !passenger.Downed)
                        return true;
                }
            }

            return false;
        }

        private float GetEffectiveMinRange(VehiclePawn vehicle)
        {
            var turretComp = vehicle.CompVehicleTurrets;
            if (turretComp == null || turretComp.turrets == null) return 0f;

            float maxMinRange = 0f;
            foreach (var turret in turretComp.turrets)
            {
                if (turret == null) continue;
                if (turret.MinRange > maxMinRange)
                    maxMinRange = turret.MinRange;
            }
            return maxMinRange;
        }

        private static Vector2 ClampToMap(Vector2 pos, Map map)
        {
            const float margin = 4f;
            pos.x = Mathf.Clamp(pos.x, margin, map.Size.x - margin);
            pos.y = Mathf.Clamp(pos.y, margin, map.Size.z - margin);
            return pos;
        }
    }

    public class JobGiver_HoverNPCExit : ThinkNode_JobGiver
    {
        private const int TicksBetweenUpdate = 120;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn is VehiclePawn vehicle)) return null;
            if (!vehicle.Spawned || vehicle.Map == null) return null;

            CompVehicleHover hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || hoverComp.State != HoverState.Hovering) return null;

            string dutyName = vehicle.mindState?.duty?.def?.defName;
            if (dutyName != "VRF_VehicleExitMap" && dutyName != "ExitMapBest") return null;

            if (vehicle.IsHashIntervalTick(TicksBetweenUpdate))
            {
                hoverComp.TryExitMapForNPC();
            }

            return JobMaker.MakeJob(JobDefOf.Wait_Combat, TicksBetweenUpdate, true);
        }
    }
}
