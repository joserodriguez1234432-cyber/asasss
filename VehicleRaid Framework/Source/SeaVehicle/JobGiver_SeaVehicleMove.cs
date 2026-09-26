using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    public static class JobGiver_SeaVehicleMove
    {
        private static readonly Dictionary<int, IntVec3> cachedShoreCells = new Dictionary<int, IntVec3>();
        private static readonly Dictionary<int, int> shoreCellTick = new Dictionary<int, int>();
        private static readonly HashSet<IntVec3> unreachableCells = new HashSet<IntVec3>();

        public static readonly Dictionary<int, int> lastDisembarkBeganTick = new Dictionary<int, int>();
        private const int DisembarkWaitTicks = 600;

        public static Job TryGiveSeaVehicleJob(VehiclePawn vehicle, Thing enemy)
        {
            if (enemy == null)
            {
                if (vehicle.CurJobDef == JobDefOf.Goto)
                {
                    vehicle.jobs.StopAll();
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 600, true);
                }
                if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 1000, true);
            }

            IntVec3 shoreCell = FindClosestShoreCell(vehicle, enemy);
            if (!shoreCell.IsValid)
            {
                if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);
            }

            float distFromShoreToEnemy = shoreCell.DistanceTo(enemy.Position);
            bool hasTurretRange = false;

            if (vehicle.CompVehicleTurrets != null && vehicle.CompVehicleTurrets.Turrets.Count > 0)
            {
                float maxRange = vehicle.CompVehicleTurrets.MaxRange;
                if (distFromShoreToEnemy <= maxRange)
                {
                    hasTurretRange = true;
                }
            }

            float distVehicleToShore = vehicle.Position.DistanceTo(shoreCell);

            if (hasTurretRange)
            {
                if (distVehicleToShore < 5f)
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);
                }

                if (vehicle.CurJobDef == JobDefOf.Goto && vehicle.CurJob.targetA.Cell == shoreCell) return null;

                if (vehicle.CurJobDef == JobDefOf.Wait_Combat)
                    vehicle.jobs.StopAll();

                if (!vehicle.CanReachVehicle(new LocalTargetInfo(shoreCell), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                {
                    unreachableCells.Add(shoreCell);
                    cachedShoreCells.Remove(vehicle.thingIDNumber);
                    shoreCellTick.Remove(vehicle.thingIDNumber);
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 120, true);
                }

                Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, shoreCell);
                gotoJob.expiryInterval = 2500;
                gotoJob.checkOverrideOnExpire = true;
                return gotoJob;
            }
            else
            {
                if (distVehicleToShore < 6f)
                {
                    if (!lastDisembarkBeganTick.ContainsKey(vehicle.thingIDNumber))
                        MarkDisembarkBegan(vehicle);

                    if (lastDisembarkBeganTick.ContainsKey(vehicle.thingIDNumber) && !CrewManager.IsAnyPawnBoarding(vehicle))
                    {
                        bool allSeatsFull = !VRF_TransportUtil.HasAvailablePassengerSlots(vehicle);
                        bool anyPendingPassenger = false;
                        if (!allSeatsFull)
                        {
                            var allPawns = vehicle.Map.mapPawns.AllPawnsSpawned;
                            for (int i = 0; i < allPawns.Count; i++)
                            {
                                Pawn p = allPawns[i];
                                if (!(p is VehiclePawn) &&
                                    p.Faction == vehicle.Faction &&
                                    !p.Dead && !p.Downed && p.Spawned &&
                                    !(p.ParentHolder is VehicleRoleHandler) &&
                                    p.GetLord() == vehicle.GetLord() &&
                                    VRF_TransportUtil.GetPassengerHandler(vehicle, p) != null)
                                {
                                    anyPendingPassenger = true;
                                    break;
                                }
                            }
                        }

                        if (!anyPendingPassenger)
                        {
                            ClearDisembarkTimer(vehicle);
                        }
                    }

                    if (IsDisembarkingOrCoolingDown(vehicle))
                    {
                        if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                        return JobMaker.MakeJob(JobDefOf.Wait_Combat, 300, true);
                    }

                    ClearDisembarkTimer(vehicle);
                    if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);
                }
                else
                {
                    ClearDisembarkTimer(vehicle);

                    if (vehicle.CurJobDef == JobDefOf.Goto && vehicle.CurJob.targetA.Cell == shoreCell) return null;

                    if (vehicle.CurJobDef == JobDefOf.Wait_Combat)
                        vehicle.jobs.StopAll();

                    if (!vehicle.CanReachVehicle(new LocalTargetInfo(shoreCell), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                    {
                        unreachableCells.Add(shoreCell);
                        cachedShoreCells.Remove(vehicle.thingIDNumber);
                        shoreCellTick.Remove(vehicle.thingIDNumber);
                        return JobMaker.MakeJob(JobDefOf.Wait_Combat, 120, true);
                    }

                    Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, shoreCell);
                    gotoJob.expiryInterval = 2500;
                    gotoJob.checkOverrideOnExpire = true;
                    return gotoJob;
                }
            }
        }

        private static IntVec3 FindClosestShoreCell(VehiclePawn vehicle, Thing enemy)
        {
            if (shoreCellTick.TryGetValue(vehicle.thingIDNumber, out int tick) && Find.TickManager.TicksGame - tick < 1500)
            {
                if (cachedShoreCells.TryGetValue(vehicle.thingIDNumber, out IntVec3 cached))
                {
                    if (cached.DistanceToSquared(enemy.Position) < vehicle.Position.DistanceToSquared(enemy.Position) + 100)
                        return cached;
                }
            }

            Map map = vehicle.Map;
            IntVec3 bestCell = vehicle.Position;
            float bestDist = bestCell.DistanceToSquared(enemy.Position);

            Queue<IntVec3> queue = new Queue<IntVec3>();
            HashSet<IntVec3> visited = new HashSet<IntVec3>();

            queue.Enqueue(vehicle.Position);
            visited.Add(vehicle.Position);

            int cellsChecked = 0;
            while (queue.Count > 0)
            {
                IntVec3 curr = queue.Dequeue();
                cellsChecked++;

                float dist = curr.DistanceToSquared(enemy.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestCell = curr;
                }

                if (dist <= 16f) break; 
                if (cellsChecked > 1200) break;

                for (int i = 0; i < 4; i++)
                {
                    IntVec3 next = curr + GenAdj.CardinalDirections[i];
                    if (next.InBounds(map) && visited.Add(next) && !unreachableCells.Contains(next) && SeaVehicleSpawnUtility.IsWater(next, map))
                    {
                        if (CanVehicleFit(next, vehicle, map))
                        {
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            cachedShoreCells[vehicle.thingIDNumber] = bestCell;
            shoreCellTick[vehicle.thingIDNumber] = Find.TickManager.TicksGame;

            return bestCell;
        }

        private static bool CanVehicleFit(IntVec3 cell, VehiclePawn vehicle, Map map)
        {
            CellRect rect = CellRect.CenteredOn(cell, 1, 1);
            foreach (IntVec3 c in rect)
            {
                if (!c.InBounds(map) || !SeaVehicleSpawnUtility.IsWater(c, map)) return false;
            }
            return true;
        }

        private static void MarkDisembarkBegan(VehiclePawn vehicle)
        {
            lastDisembarkBeganTick[vehicle.thingIDNumber] = Find.TickManager.TicksGame;
        }

        private static bool IsDisembarkingOrCoolingDown(VehiclePawn vehicle)
        {
            if (!lastDisembarkBeganTick.TryGetValue(vehicle.thingIDNumber, out int tick)) return false;
            return Find.TickManager.TicksGame - tick < DisembarkWaitTicks;
        }

        private static void ClearDisembarkTimer(VehiclePawn vehicle)
        {
            lastDisembarkBeganTick.Remove(vehicle.thingIDNumber);
        }
    }
}
