using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    public class VRF_JobGiver_DefendBaseAssault : ThinkNode_JobGiver
    {
        private const float MaxReposition = 18f;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn is VehiclePawn vehicle)) return null;
            if (!vehicle.Spawned || vehicle.Map == null) return null;

            var turretComp = vehicle.CompVehicleTurrets;
            if (turretComp == null) return null;

            var leaderManager = vehicle.Map.GetComponent<VRF_LeaderManager>();
            if (leaderManager != null && leaderManager.IsMortar(vehicle)) return null;

            if (!CrewManager.CanMove(vehicle) && !turretComp.CanDeploy)
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);

            Thing enemy = FindEnemyInRange(vehicle, turretComp);
            if (enemy == null)
                return null; 

            vehicle.mindState.enemyTarget = enemy;

            float maxRange = turretComp.MaxRange;
            float minRange = turretComp.MinRange;
            float dist     = vehicle.Position.DistanceTo(enemy.Position);

            if (dist >= minRange && dist <= maxRange
                && GenSight.LineOfSight(vehicle.Position, enemy.Position, vehicle.Map))
            {
                bool canFire = false, blocked = false;
                CheckTurretAngles(turretComp, enemy, out canFire, out blocked);

                if (canFire)
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 180, true);

                if (blocked && CrewManager.CanMove(vehicle))
                {
                    IntVec3 alignCell = FindSmallAlignCell(vehicle, enemy, maxRange, minRange);
                    if (alignCell.IsValid)
                    {
                        Job j = JobMaker.MakeJob(JobDefOf.Goto, alignCell);
                        j.expiryInterval = 600;
                        return j;
                    }
                }

                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 180, true);
            }

            if (!CrewManager.CanMove(vehicle))
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 300, true);

            float idealRange = Mathf.Clamp(maxRange * 0.75f, minRange + 3f, maxRange - 2f);
            IntVec3 dest = FindLimitedRangeCell(vehicle, enemy, idealRange, maxRange, minRange);
            if (dest.IsValid)
            {
                Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, dest);
                gotoJob.expiryInterval = 900;
                gotoJob.checkOverrideOnExpire = true;
                return gotoJob;
            }

            return JobMaker.MakeJob(JobDefOf.Wait_Combat, 180, true);
        }


        private static Thing FindEnemyInRange(VehiclePawn vehicle, CompVehicleTurrets turretComp)
        {
            float maxRange   = turretComp.MaxRange;
            float maxRangeSq = maxRange * maxRange;

            var hostiles = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (hostiles == null) return null;

            Thing best   = null;
            float bestSq = float.MaxValue;

            foreach (var t in hostiles)
            {
                Thing thing = t.Thing;
                if (thing == null || thing.Destroyed || !thing.Spawned) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;
                if (thing.Map != vehicle.Map) continue;
                if (thing.Position.Fogged(vehicle.Map)) continue;

                float sq = thing.Position.DistanceToSquared(vehicle.Position);
                if (sq > maxRangeSq) continue;     
                if (sq < bestSq) { bestSq = sq; best = thing; }
            }

            return best;
        }

        private static void CheckTurretAngles(CompVehicleTurrets comp, Thing target,
            out bool canFire, out bool blocked)
        {
            canFire = false; blocked = false;
            if (comp?.Turrets == null) return;
            foreach (var turret in comp.Turrets)
            {
                if (!turret.InRange(new LocalTargetInfo(target))) continue;
                if (turret.AngleBetween(target.DrawPos)) canFire = true;
                else                                      blocked = true;
            }
        }

        private static IntVec3 FindSmallAlignCell(VehiclePawn vehicle, Thing enemy,
            float maxRange, float minRange)
        {
            Map map = vehicle.Map;
            for (int radius = 2; radius <= (int)MaxReposition; radius += 2)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(vehicle.Position, radius, false))
                {
                    if (!cell.InBounds(map) || !cell.Standable(map)) continue;
                    float d = cell.DistanceTo(enemy.Position);
                    if (d < minRange + 1f || d > maxRange) continue;
                    if (!GenSight.LineOfSight(cell, enemy.Position, map)) continue;
                    if (!vehicle.CanReachVehicle(new LocalTargetInfo(cell),
                            PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                        continue;
                    return cell;
                }
            }
            return IntVec3.Invalid;
        }

        private static IntVec3 FindLimitedRangeCell(VehiclePawn vehicle, Thing enemy,
            float idealRange, float maxRange, float minRange)
        {
            Map map          = vehicle.Map;
            float maxRepoSq  = MaxReposition * MaxReposition;

            IntVec3 best     = IntVec3.Invalid;
            float   bestScore = float.MaxValue;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(enemy.Position, maxRange, true))
            {
                if (!cell.InBounds(map) || !cell.Standable(map)) continue;

                float distToEnemy = cell.DistanceTo(enemy.Position);
                if (distToEnemy < minRange + 1f || distToEnemy > maxRange) continue;
                if (!GenSight.LineOfSight(cell, enemy.Position, map)) continue;

                float repoSq = cell.DistanceToSquared(vehicle.Position);
                if (repoSq > maxRepoSq) continue;

                float score = Mathf.Abs(distToEnemy - idealRange) + Mathf.Sqrt(repoSq) * 0.3f;
                if (score < bestScore)
                {
                    if (!vehicle.CanReachVehicle(new LocalTargetInfo(cell),
                            PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                        continue;
                    bestScore = score;
                    best      = cell;
                }
            }

            return best;
        }
    }
}
