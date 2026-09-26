using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(VehiclePathFollower), nameof(VehiclePathFollower.PatherTick))]
    public static class Patch_VehicleNPCPathUpdate
    {
        private static readonly List<CellRect> tmpAllyRects = new List<CellRect>();
        private static readonly List<KeyValuePair<IntVec3, int>> tmpAllyDestinations = new List<KeyValuePair<IntVec3, int>>();
        private static readonly List<KeyValuePair<IntVec3, float>> tmpCandidates = new List<KeyValuePair<IntVec3, float>>();

        [HarmonyPostfix]
        public static void Postfix(VehiclePathFollower __instance, VehiclePawn ___vehicle)
        {
            if (___vehicle == null || !___vehicle.Spawned || ___vehicle.Map == null) return;
            if (___vehicle.Faction == null || ___vehicle.Faction.IsPlayer) return;
            if (!__instance.Moving) return;
            if (___vehicle.CurJobDef != JobDefOf.Goto) return;
            if (___vehicle.VehicleDef.type == VehicleType.Sea) return;
            if (!(___vehicle.GetLord()?.LordJob is LordJob_VehicleRaid)) return;
            if (__instance.RequestStatus == VehiclePathFollower.PathRequestStatus.Calculating) return;

            if (!___vehicle.IsHashIntervalTick(60)) return;

            Thing enemy = FindNearestEnemy(___vehicle);
            if (enemy == null || enemy.Destroyed || !enemy.Spawned) return;

            LocalTargetInfo currentDest = __instance.Destination;
            if (!currentDest.IsValid) return;

            float maxRange = ___vehicle.CompVehicleTurrets?.MaxRange ?? 60f;
            float minRange = ___vehicle.CompVehicleTurrets?.MinRange ?? 0f;

            bool isMortar = false;
            var compTurrets = ___vehicle.CompVehicleTurrets;
            if (compTurrets != null && compTurrets.Turrets != null)
            {
                var turrets = compTurrets.Turrets;
                for (int i = 0; i < turrets.Count; i++)
                {
                    if (turrets[i].ProjectileDef?.projectile?.flyOverhead == true)
                    {
                        isMortar = true;
                        break;
                    }
                }
            }

            bool isTransporting = false;
            if (___vehicle.handlers != null)
            {
                for (int i = 0; i < ___vehicle.handlers.Count; i++)
                {
                    var h = ___vehicle.handlers[i];
                    if (h?.role == null) continue;
                    if ((h.role.HandlingTypes & HandlingType.Movement) != 0 || (h.role.HandlingTypes & HandlingType.Turret) != 0) continue;
                    for (int j = 0; j < h.thingOwner.Count; j++)
                    {
                        if (h.thingOwner[j] is Pawn p && !p.Dead && !p.Downed)
                        {
                            isTransporting = true;
                            break;
                        }
                    }
                    if (isTransporting) break;
                }
            }

            if (isTransporting)
            {
                maxRange = 36f;
                minRange = 10f;
            }

            float currentDistSq = ___vehicle.Position.DistanceToSquared(enemy.Position);
            float minRangeSq = minRange * minRange;
            float maxRangeSq = maxRange * maxRange;

            bool currentPosValid = currentDistSq >= minRangeSq && currentDistSq <= maxRangeSq
                                   && (isMortar || GenSight.LineOfSight(___vehicle.Position, enemy.Position, ___vehicle.Map));

            if (currentPosValid)
            {
                ___vehicle.jobs.EndCurrentJob(JobCondition.Succeeded);
                return;
            }

            float destToEnemySq = currentDest.Cell.DistanceToSquared(enemy.Position);
            bool destStillValid = destToEnemySq >= minRangeSq && destToEnemySq <= maxRangeSq
                                  && (isMortar || GenSight.LineOfSight(currentDest.Cell, enemy.Position, ___vehicle.Map));

            if (destStillValid) return;

            float currentDist = Mathf.Sqrt(currentDistSq);
            if (currentDist > maxRange + 15f && currentDist > 80f)
            {
                return;
            }

            float idealRange = Mathf.Clamp(maxRange * 0.7f, minRange + 3f, maxRange - 2f);
            if (isTransporting) idealRange = 32f;

            IntVec3 newDest = FindPositionAtIdealRange(___vehicle, enemy, idealRange, maxRange, minRange, isMortar);
            if (!newDest.IsValid) return;
            if (currentDest.Cell.DistanceToSquared(newDest) < 225 && !currentDest.HasThing) return;

            Job newGotoJob = JobMaker.MakeJob(JobDefOf.Goto, newDest);
            newGotoJob.expiryInterval = 2000;
            newGotoJob.checkOverrideOnExpire = true;
            ___vehicle.jobs.StartJob(newGotoJob, JobCondition.InterruptForced);
        }

        private static Thing FindNearestEnemy(VehiclePawn vehicle)
        {
            var targets = vehicle.Map.attackTargetsCache.GetPotentialTargetsFor(vehicle);
            if (targets == null || targets.Count == 0) return null;
            Thing best = null;
            float bestDistSq = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t.ThreatDisabled(vehicle)) continue;
                if (!AttackTargetFinder.IsAutoTargetable(t)) continue;
                Thing thing = t.Thing;
                if (thing == null || thing.Destroyed || thing.Map == null) continue;
                if (thing.Map.fogGrid.IsFogged(thing.Position)) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;
                float d = thing.Position.DistanceToSquared(vehicle.Position);
                if (d < bestDistSq) { bestDistSq = d; best = thing; }
            }
            return best;
        }

        private static IntVec3 FindPositionAtIdealRange(VehiclePawn vehicle, Thing target, float idealRange, float maxRange, float minRange, bool isMortar)
        {
            Map map = vehicle.Map;
            
            float searchRadius = Mathf.Min(maxRange, 80f);
            float searchMin = Mathf.Min(minRange, searchRadius - 5f);
            if (searchMin < 0) searchMin = 0;
            float searchIdeal = Mathf.Clamp(idealRange, searchMin + 3f, searchRadius - 2f);

            tmpAllyRects.Clear();
            tmpAllyDestinations.Clear();

            Lord lord = vehicle.GetLord();
            if (lord != null)
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    Pawn p = lord.ownedPawns[i];
                    if (p is VehiclePawn v && v != vehicle && v.Faction == vehicle.Faction && v.Spawned && v.Map == map)
                    {
                        int vSize = Mathf.Max(v.def.size.x, v.def.size.z);
                        tmpAllyRects.Add(v.OccupiedRect().ExpandedBy(3));

                        if (v.CurJob != null && v.CurJob.def == JobDefOf.Goto && v.CurJob.targetA.IsValid)
                        {
                            tmpAllyDestinations.Add(new KeyValuePair<IntVec3, int>(v.CurJob.targetA.Cell, vSize));
                        }
                    }
                }
            }
            else
            {
                var factionPawns = map.mapPawns.SpawnedPawnsInFaction(vehicle.Faction);
                for (int i = 0; i < factionPawns.Count; i++)
                {
                    Pawn p = factionPawns[i];
                    if (p is VehiclePawn v && v != vehicle)
                    {
                        int vSize = Mathf.Max(v.def.size.x, v.def.size.z);
                        tmpAllyRects.Add(v.OccupiedRect().ExpandedBy(3));

                        if (v.CurJob != null && v.CurJob.def == JobDefOf.Goto && v.CurJob.targetA.IsValid)
                        {
                            tmpAllyDestinations.Add(new KeyValuePair<IntVec3, int>(v.CurJob.targetA.Cell, vSize));
                        }
                    }
                }
            }

            tmpCandidates.Clear();
            int validCellsFound = 0;
            float skipRate = (searchRadius > 40f) ? 0.75f : 0.0f; 

            float searchMinSq = searchMin * searchMin;
            float searchRadiusSq = searchRadius * searchRadius;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(target.Position, searchRadius, true))
            {
                if (!cell.InBounds(map)) continue;

                float distToTargetSq = cell.DistanceToSquared(target.Position);
                if (distToTargetSq < searchMinSq || distToTargetSq > searchRadiusSq) continue;

                if (skipRate > 0f && Rand.Value < skipRate) continue;

                if (!cell.Standable(map)) continue;

                bool insideAlly = false;
                for (int i = 0; i < tmpAllyRects.Count; i++)
                {
                    if (tmpAllyRects[i].Contains(cell)) { insideAlly = true; break; }
                }
                if (insideAlly) continue;

                bool destinationTaken = false;
                for (int i = 0; i < tmpAllyDestinations.Count; i++)
                {
                    float standoffDistance = tmpAllyDestinations[i].Value + 2f;
                    if (cell.DistanceToSquared(tmpAllyDestinations[i].Key) < (standoffDistance * standoffDistance))
                    {
                        destinationTaken = true;
                        break;
                    }
                }
                if (destinationTaken) continue;

                // Perform line of sight check ONLY after all positional/geometry filters pass
                if (!isMortar && !GenSight.LineOfSight(cell, target.Position, map)) continue;

                float distToTarget = Mathf.Sqrt(distToTargetSq);
                float rangeScore = Mathf.Abs(distToTarget - searchIdeal) * 2f;
                float travelScore = cell.DistanceTo(vehicle.Position) * 0.3f;
                float score = rangeScore + travelScore + Rand.Range(0f, 8f);

                tmpCandidates.Add(new KeyValuePair<IntVec3, float>(cell, score));
                validCellsFound++;

                if (validCellsFound >= 35) break;
            }

            tmpCandidates.Sort((a, b) => a.Value.CompareTo(b.Value));

            int pathChecks = 0;
            for (int i = 0; i < tmpCandidates.Count; i++)
            {
                if (pathChecks++ >= 5) break;
                IntVec3 candCell = tmpCandidates[i].Key;
                if (vehicle.CanReachVehicle(new LocalTargetInfo(candCell), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                {
                    return candCell;
                }
            }

            return IntVec3.Invalid;
        }
    }

    public static class Patch_VVE_MovementController_NPC
    {
        public static System.Type controllerType;
        public static FieldInfo curMovementModeField;
        public static FieldInfo currentSpeedField;
        public static PropertyInfo vehicleProperty;

        public static void TryApply(Harmony harmony)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    controllerType = asm.GetType("VanillaVehiclesExpanded.CompVehicleMovementController");
                    if (controllerType != null) break;
                }
                catch { }
            }

            if (controllerType == null) return;

            curMovementModeField = AccessTools.Field(controllerType, "curMovementMode");
            currentSpeedField    = AccessTools.Field(controllerType, "currentSpeed");
            vehicleProperty      = AccessTools.Property(controllerType, "Vehicle");

            MethodInfo target = AccessTools.Method(controllerType, "StartMove");
            if (target == null) return;

            harmony.Patch(target,
                postfix: new HarmonyMethod(typeof(Patch_VVE_MovementController_NPC), nameof(Postfix)));
        }

        public static void Postfix(object __instance)
        {
            if (curMovementModeField == null || currentSpeedField == null || vehicleProperty == null) return;

            var vehicle = vehicleProperty.GetValue(__instance) as VehiclePawn;
            if (vehicle == null) return;
            if (vehicle.Faction == null || vehicle.Faction.IsPlayer) return;

            Lord lord = vehicle.GetLord();
            if (!(lord?.LordJob is LordJob_VehicleRaid)) return;

            curMovementModeField.SetValue(__instance, System.Enum.ToObject(curMovementModeField.FieldType, 1));
            currentSpeedField.SetValue(__instance, 0f);
        }
    }
}
