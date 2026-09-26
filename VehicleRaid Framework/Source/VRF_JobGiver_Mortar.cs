using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    public class VRF_JobGiver_Mortar : ThinkNode_JobGiver
    {
        private const int ManeuverCandidateCount = 8;
        private const float ManeuverDistance = 2.5f;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn is VehiclePawn vehicle) || !vehicle.Spawned || vehicle.Map == null) return null;

            var leaderManager = vehicle.Map.GetComponent<VRF_LeaderManager>();
            if (leaderManager == null || !leaderManager.IsMortar(vehicle)) return null;

            var turretComp = vehicle.CompVehicleTurrets;
            bool needsDeploy = turretComp != null && turretComp.CanDeploy;
            bool isDeployed = turretComp != null && turretComp.Deployed;
            bool isDeploying = turretComp != null && turretComp.Deploying;

            if (isDeploying)
            {
                return null;
            }

            if (vehicle.CurJobDef == JobDefOf.Goto && vehicle.pather.Moving)
            {
                return null;
            }

            Thing enemy = FindBestEnemyForMortar(vehicle);

            if (isDeployed)
            {
                if (enemy == null)
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 250, true);
                }

                vehicle.mindState.enemyTarget = enemy;
                bool anyTurretCanFire = false;
                bool anyTurretBlocked = false;
                CheckTurretAngles(turretComp, enemy, out anyTurretCanFire, out anyTurretBlocked);

                if (anyTurretCanFire)
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 120, true);
                }

                if (anyTurretBlocked)
                {
                    IntVec3 maneuverCell = FindManeuverCell(vehicle, enemy);
                    if (maneuverCell.IsValid)
                    {
                        return StartDeployJob(turretComp, vehicle);
                    }
                }

                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 120, true);
            }

            if (enemy != null)
            {
                vehicle.mindState.enemyTarget = enemy;
                bool anyTurretCanFire = false;
                bool anyTurretBlocked = false;
                CheckTurretAngles(turretComp, enemy, out anyTurretCanFire, out anyTurretBlocked);

                if (anyTurretBlocked && !anyTurretCanFire)
                {
                    if (!CrewManager.CanMove(vehicle))
                    {
                        if (needsDeploy)
                            return StartDeployJob(turretComp, vehicle);
                        return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);
                    }

                    IntVec3 maneuverCell = FindManeuverCell(vehicle, enemy);
                    if (maneuverCell.IsValid)
                    {
                        Job maneuverJob = JobMaker.MakeJob(JobDefOf.Goto, maneuverCell);
                        maneuverJob.expiryInterval = 600;
                        return maneuverJob;
                    }
                }

                if (anyTurretCanFire || !anyTurretBlocked)
                {
                    if (needsDeploy)
                    {
                        bool isMoving = vehicle.vehiclePather != null && vehicle.vehiclePather.Moving;
                        if (!isMoving)
                        {
                            return StartDeployJob(turretComp, vehicle);
                        }
                    }
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 120, true);
                }
            }

            if (!CrewManager.CanMove(vehicle))
            {
                if (needsDeploy)
                    return StartDeployJob(turretComp, vehicle);
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);
            }

            IntVec3 chillSpot = IntVec3.Invalid;
            if (vehicle.mindState.duty != null && vehicle.mindState.duty.focus.IsValid)
            {
                chillSpot = vehicle.mindState.duty.focus.Cell;
            }

            if (!chillSpot.IsValid || !chillSpot.InBounds(vehicle.Map))
            {
                chillSpot = vehicle.Position;
                CellFinder.TryFindRandomCellNear(vehicle.Position, vehicle.Map, 15,
                    c => c.InBounds(vehicle.Map) && c.Standable(vehicle.Map) && !c.Fogged(vehicle.Map),
                    out chillSpot);

                if (vehicle.mindState.duty != null)
                {
                    vehicle.mindState.duty.focus = new LocalTargetInfo(chillSpot);
                }
            }

            if (vehicle.Position.DistanceToSquared(chillSpot) > 25)
            {
                if (vehicle.CanReachVehicle(new LocalTargetInfo(chillSpot), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                {
                    return JobMaker.MakeJob(JobDefOf.Goto, chillSpot);
                }
            }

            if (needsDeploy)
            {
                return StartDeployJob(turretComp, vehicle);
            }

            return JobMaker.MakeJob(JobDefOf.Wait_Combat, 60, true);
        }

        private Job StartDeployJob(CompVehicleTurrets turretComp, VehiclePawn vehicle)
        {
            var deployJobDef = DefDatabase<JobDef>.GetNamedSilentFail("DeployVehicle");
            if (deployJobDef == null)
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 120, true);

            var deployJob = JobMaker.MakeJob(deployJobDef, vehicle);
            var fieldInfo = turretComp.GetType().GetField("deployTicks",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
                fieldInfo.SetValue(turretComp, turretComp.DeployTicks);
            return deployJob;
        }

        private void CheckTurretAngles(CompVehicleTurrets turretComp, Thing enemy, out bool canFire, out bool blocked)
        {
            canFire = false;
            blocked = false;
            if (turretComp == null || turretComp.Turrets == null) return;

            foreach (var turret in turretComp.Turrets)
            {
                if (!turret.InRange(new LocalTargetInfo(enemy))) continue;

                if (turret.AngleBetween(enemy.DrawPos))
                    canFire = true;
                else
                    blocked = true;
            }
        }

        private static bool WouldFaceEnemy(
            CompVehicleTurrets turretComp, VehiclePawn vehicle,
            IntVec3 dest, IntVec3 approachFrom, Thing enemy)
        {
            IntVec3 step = dest - approachFrom;  
            Rot4 simRot;
            float simAngle;
            if (step.x > 0)       { simRot = Rot4.East;  simAngle = step.z > 0 ? -45f : (step.z < 0 ? 45f : 0f); }
            else if (step.x < 0)  { simRot = Rot4.West;  simAngle = step.z < 0 ? -45f : (step.z > 0 ? 45f : 0f); }
            else if (step.z > 0)  { simRot = Rot4.North; simAngle = 0f; }
            else if (step.z < 0)  { simRot = Rot4.South; simAngle = 0f; }
            else return false; 

            float baseAngle = simRot.AsAngle + simAngle;

            foreach (var turret in turretComp.Turrets)
            {
                if (turret.angleRestricted == Vector2.zero) return true; 

                float lo = ((turret.angleRestricted.x + baseAngle) % 360f + 360f) % 360f;
                float hi = ((turret.angleRestricted.y + baseAngle) % 360f + 360f) % 360f;
                float span = hi - lo < 0f ? hi - lo + 360f : hi - lo;

                float toEnemy = Vector3Utility.AngleFlat(enemy.DrawPos - dest.ToVector3Shifted());
                float delta = toEnemy - lo;
                if (delta < 0f) delta += 360f;
                if (delta < span) return true;
            }
            return false;
        }

        private IntVec3 FindManeuverCell(VehiclePawn vehicle, Thing enemy)
        {
            Map map = vehicle.Map;
            var turretComp = vehicle.CompVehicleTurrets;

            IntVec3[] steps = new IntVec3[]
            {
                new IntVec3( 0,  0,  1), 
                new IntVec3( 1,  0,  1), 
                new IntVec3( 1,  0,  0),
                new IntVec3( 1,  0, -1),
                new IntVec3( 0,  0, -1), 
                new IntVec3(-1,  0, -1), 
                new IntVec3(-1,  0,  0),  
                new IntVec3(-1,  0,  1), 
            };

            var candidates = new List<(IntVec3 cell, IntVec3 approachFrom, int priority)>();

            for (int radius = 1; radius <= 3; radius++)
            {
                foreach (IntVec3 step in steps)
                {
                    IntVec3 dest = vehicle.Position + step * radius;
                    if (!dest.InBounds(map) || !dest.Standable(map) || dest == vehicle.Position) continue;

                    bool occupied = false;
                    foreach (Thing t in dest.GetThingList(map))
                    {
                        if (t is VehiclePawn other && other != vehicle) { occupied = true; break; }
                    }
                    if (occupied) continue;

                    IntVec3 approachFrom = dest - step;

                    bool faceOk = turretComp != null && WouldFaceEnemy(turretComp, vehicle, dest, approachFrom, enemy);
                    int priority = (faceOk ? 0 : 1) * 10 + radius;
                    candidates.Add((dest, approachFrom, priority));
                }
            }

            candidates.Sort((a, b) => a.priority.CompareTo(b.priority));

            foreach (var (dest, _, _) in candidates)
            {
                if (vehicle.CanReachVehicle(new LocalTargetInfo(dest), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                    return dest;
            }

            return IntVec3.Invalid;
        }

        private Thing FindBestEnemyForMortar(VehiclePawn vehicle)
        {
            var hostileTargets = vehicle.Map.attackTargetsCache.TargetsHostileToFaction(vehicle.Faction);
            if (hostileTargets == null) return null;

            float maxRange = vehicle.CompVehicleTurrets?.MaxRange ?? 100f;
            float maxRangeSq = maxRange * maxRange;

            Thing bestTarget = null;
            float bestDistSq = float.MaxValue;

            foreach (var target in hostileTargets)
            {
                Thing t = target.Thing;
                if (t == null || t.Destroyed || !t.Spawned) continue;
                if (t.Map != vehicle.Map) continue;
                if (t.Position.Fogged(vehicle.Map)) continue;

                if (t is Pawn p && (p.Dead || p.Downed)) continue;

                float distSq = t.Position.DistanceToSquared(vehicle.Position);

                if (distSq < bestDistSq && distSq <= maxRangeSq)
                {
                    bestDistSq = distSq;
                    bestTarget = t;
                }
            }

            return bestTarget;
        }
    }
}

