using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Vehicles;
using SmashTools;

namespace VehicleRaidFramework
{
    public class JobGiver_VehicleTransportMove : ThinkNode_JobGiver
    {
        private const float DropOffRange       = 32f;
        private const float TooCloseRange      = 10f;
        private const float WaitRange          = 14f;
        private const int   DisembarkWaitTicks = 600;

        private static readonly Dictionary<int, int> lastDisembarkBeganTick = new Dictionary<int, int>();
        private static int disembarkTimerCleanupTick;

        private static void PruneDisembarkTimers()
        {
            int now = Find.TickManager.TicksGame;
            if (now < disembarkTimerCleanupTick) return;
            disembarkTimerCleanupTick = now + 30000;

            List<int> stale = null;
            foreach (KeyValuePair<int, int> entry in lastDisembarkBeganTick)
            {
                if (now - entry.Value > 30000)
                {
                    if (stale == null) stale = new List<int>();
                    stale.Add(entry.Key);
                }
            }
            if (stale != null)
            {
                foreach (int key in stale) lastDisembarkBeganTick.Remove(key);
            }
        }

        private const int BoardingWaitTimeoutTicks = 360; // 6 seconds (60 ticks/sec)
        private const int BoardingCooldownTicks    = 600; // 10 seconds ignore boarding after timeout

        private static readonly Dictionary<int, int> boardingWaitStartTick   = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> boardingCooldownUntilTick = new Dictionary<int, int>();

        private static bool ShouldWaitToBoard(VehiclePawn vehicle, bool hasPawnsTryingToBoard)
        {
            int now = Find.TickManager.TicksGame;
            int vid = vehicle.thingIDNumber;

            if (boardingCooldownUntilTick.TryGetValue(vid, out int cooldownUntil) && now < cooldownUntil)
            {
                boardingWaitStartTick.Remove(vid);
                return false; // Timed out recently: do not wait, start moving!
            }

            if (!hasPawnsTryingToBoard)
            {
                boardingWaitStartTick.Remove(vid);
                return false;
            }

            if (!boardingWaitStartTick.TryGetValue(vid, out int startTick))
            {
                boardingWaitStartTick[vid] = now;
                return true; // Start waiting
            }

            if (now - startTick >= BoardingWaitTimeoutTicks)
            {
                // 6 seconds reached with no one boarding! Give up waiting and move
                boardingWaitStartTick.Remove(vid);
                boardingCooldownUntilTick[vid] = now + BoardingCooldownTicks;
                return false;
            }

            return true; // Still within 6 seconds window
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

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn is VehiclePawn vehicle)) return null;
            if (!vehicle.Spawned || vehicle.Map == null) return null;

            PruneDisembarkTimers();

            if (vehicle.VehicleDef.type == VehicleType.Sea)
            {
                return JobGiver_SeaVehicleMove.TryGiveSeaVehicleJob(vehicle, FindNearestVisibleEnemy(vehicle));
            }

            if (VRF_TransportUtil.IsVehicleImmobilized(vehicle))
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 2000, true);

            if (CrewManager.IsAnyPawnBoarding(vehicle))
            {
                if (ShouldWaitToBoard(vehicle, true))
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 60, true);
            }

            bool hasPassengers = false;
            foreach (Pawn p in vehicle.AllPawnsAboard)
            {
                if (p.Dead || p.Downed) continue;
                VehicleRoleHandler h = null;
                foreach (VehicleRoleHandler hh in vehicle.handlers)
                {
                    if (hh.thingOwner.Contains(p)) { h = hh; break; }
                }
                if (h?.role == null) continue;
                if ((h.role.HandlingTypes & HandlingType.Movement) == 0 &&
                    (h.role.HandlingTypes & HandlingType.Turret) == 0)
                {
                    hasPassengers = true;
                    break;
                }
            }

            Thing enemy = FindNearestVisibleEnemy(vehicle);

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

            float dist = vehicle.Position.DistanceTo(enemy.Position);

            if (dist < TooCloseRange)
            {
                IntVec3 retreat = FindRetreatCell(vehicle, enemy);
                if (retreat.IsValid)
                {
                    Job j = JobMaker.MakeJob(JobDefOf.Goto, retreat);
                    j.expiryInterval = 1200;
                    j.checkOverrideOnExpire = true;
                    return j;
                }
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 300, true);
            }

            if (dist <= WaitRange)
            {
                if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 600, true);
            }

            bool nearbyMissing = false;
            if (VRF_TransportUtil.HasAvailablePassengerSlots(vehicle))
            {
                foreach (Pawn p in vehicle.Map.mapPawns.AllPawnsSpawned)
                {
                    if (p is VehiclePawn || p.Faction != vehicle.Faction || p.Dead || p.Downed || !p.Spawned) continue;
                    if (p.ParentHolder is VehicleRoleHandler) continue;
                    if (p.GetLord() == vehicle.GetLord())
                    {
                        float pDist = p.Position.DistanceTo(vehicle.Position);
                        if (pDist <= 30f)
                            nearbyMissing = true;
                    }
                }
            }

            if (!hasPassengers || nearbyMissing)
            {
                bool pawnsApproaching = false;
                foreach (Pawn p in vehicle.Map.mapPawns.AllPawnsSpawned)
                {
                    if (p is VehiclePawn || p.Faction != vehicle.Faction || p.Dead || p.Downed || !p.Spawned) continue;
                    if (p.ParentHolder is VehicleRoleHandler) continue;

                    if (p.CurJob != null && p.CurJob.def == VRF_AIDutyDefs.Board && p.CurJob.targetA.Thing == vehicle)
                    {
                        pawnsApproaching = true;
                        continue;
                    }

                    if (p.GetLord() == vehicle.GetLord())
                    {
                        float pDist = p.Position.DistanceTo(vehicle.Position);
                        if (pDist < 20f &&
                            !VRF_TransportUtil.HasEnemy(p, VRF_TransportUtil.ImmediateThreatRadius) &&
                            !VRF_TransportUtil.HasEnemy(vehicle, VRF_TransportUtil.GetVehicleCombatRadius(vehicle)) &&
                            VRF_TransportUtil.HasAvailablePassengerSlots(vehicle))
                        {
                            if (p.CurJob == null || p.CurJob.def.defName != "Board")
                            {
                                VehicleRoleHandler handler = VRF_TransportUtil.GetPassengerHandler(vehicle, p);
                                if (handler != null && p.CanReach(vehicle, PathEndMode.Touch, Danger.Deadly))
                                {
                                    JobDef boardJobDef = VRF_AIDutyDefs.Board;
                                    if (boardJobDef != null)
                                    {
                                        vehicle.GiveLoadJob(p, handler);
                                        Job boardJob = JobMaker.MakeJob(boardJobDef, vehicle);
                                        boardJob.expiryInterval = 3000;
                                        boardJob.locomotionUrgency = LocomotionUrgency.Sprint;
                                        p.jobs.StartJob(boardJob, JobCondition.InterruptForced, null, false, true);
                                    }
                                }
                            }
                            pawnsApproaching = true;
                        }
                    }
                }

                if (pawnsApproaching && ShouldWaitToBoard(vehicle, true))
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 60, true);

                Pawn squadPawn = null;
                foreach (Pawn p in vehicle.Map.mapPawns.AllPawnsSpawned)
                {
                    if (p == vehicle || p is VehiclePawn) continue;
                    if (p.Faction != vehicle.Faction || p.Dead || p.Downed) continue;
                    if (p.ParentHolder is VehicleRoleHandler) continue;
                    if (p.GetLord() != vehicle.GetLord()) continue;
                    float squadDist = p.Position.DistanceTo(vehicle.Position);
                    if (squadDist > 20f && squadDist <= 45f) { squadPawn = p; break; }
                }

                if (squadPawn != null)
                {
                    Job followJob = JobMaker.MakeJob(JobDefOf.Goto, squadPawn.Position);
                    followJob.expiryInterval = 1200;
                    followJob.checkOverrideOnExpire = true;
                    return followJob;
                }

                if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 800, true);
            }

            IntVec3 dropCell = FindDropOffCell(vehicle, enemy, DropOffRange);
            if (!dropCell.IsValid)
            {
                if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, 500, true);
            }

            if (dist <= DropOffRange + 5f)
            {
                if (!lastDisembarkBeganTick.ContainsKey(vehicle.thingIDNumber))
                    MarkDisembarkBegan(vehicle);

                if (lastDisembarkBeganTick.ContainsKey(vehicle.thingIDNumber) && !CrewManager.IsAnyPawnBoarding(vehicle))
                {
                    bool allSeatsFull = !VRF_TransportUtil.HasAvailablePassengerSlots(vehicle);

                    bool anyPendingPassenger = false;
                    if (!allSeatsFull)
                    {
                        foreach (Pawn p in vehicle.Map.mapPawns.AllPawnsSpawned)
                        {
                            if (p is VehiclePawn) continue;
                            if (p.Faction != vehicle.Faction || p.Dead || p.Downed || !p.Spawned) continue;
                            if (p.ParentHolder is VehicleRoleHandler) continue;
                            if (p.GetLord() != vehicle.GetLord()) continue;
                            if (VRF_TransportUtil.GetPassengerHandler(vehicle, p) != null) { anyPendingPassenger = true; break; }
                        }
                    }

                    if (!anyPendingPassenger)
                        ClearDisembarkTimer(vehicle);
                }

                if (IsDisembarkingOrCoolingDown(vehicle))
                {
                    if (vehicle.CurJobDef == JobDefOf.Wait_Combat) return null;
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, 300, true);
                }

                ClearDisembarkTimer(vehicle);
            }
            else
            {
                ClearDisembarkTimer(vehicle);
            }

            if (vehicle.CurJobDef == JobDefOf.Goto &&
                vehicle.CurJob?.targetA.Cell == dropCell) return null;

            if (vehicle.CurJobDef == JobDefOf.Wait_Combat)
                vehicle.jobs.StopAll();

            Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, dropCell);
            gotoJob.expiryInterval = 2500;
            gotoJob.checkOverrideOnExpire = true;
            return gotoJob;
        }

        public Thing FindNearestVisibleEnemy(VehiclePawn vehicle)
        {
            var targets = vehicle.Map.attackTargetsCache.GetPotentialTargetsFor(vehicle);
            if (targets == null || targets.Count == 0) return null;

            Thing best = null;
            float bestDist = float.MaxValue;
            foreach (var t in targets)
            {
                if (t.ThreatDisabled(vehicle)) continue;
                if (!AttackTargetFinder.IsAutoTargetable(t)) continue;
                Thing thing = t.Thing;
                if (thing == null || thing.Destroyed) continue;
                if (thing.Map == null || thing.Map.fogGrid.IsFogged(thing.Position)) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;
                float d = thing.Position.DistanceToSquared(vehicle.Position);
                if (d < bestDist) { bestDist = d; best = thing; }
            }
            return best;
        }

        private IntVec3 FindDropOffCell(VehiclePawn vehicle, Thing enemy, float targetDist)
        {
            Map map = vehicle.Map;
            IntVec3 bestCell = IntVec3.Invalid;
            float bestScore = float.MaxValue;

            for (int i = 0; i < 30; i++)
            {
                IntVec3 cell = enemy.Position + GenRadial.RadialPattern[Rand.Range(
                    GenRadial.NumCellsInRadius(targetDist - 4f),
                    GenRadial.NumCellsInRadius(targetDist + 4f))];

                if (!cell.InBounds(map) || !cell.Standable(map)) continue;
                if (!vehicle.DrivableRectOnCell(cell, Ext_Vehicles.DestinationHitboxReq.AnyRotation)) continue;

                float score = cell.DistanceToSquared(vehicle.Position);
                if (score < bestScore && vehicle.CanReachVehicle(new LocalTargetInfo(cell), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                {
                    bestScore = score;
                    bestCell = cell;
                }
            }

            return bestCell;
        }

        private IntVec3 FindRetreatCell(VehiclePawn vehicle, Thing enemy)
        {
            Map map = vehicle.Map;
            Vector3 awayDir = (vehicle.Position.ToVector3Shifted() - enemy.Position.ToVector3Shifted()).normalized;

            for (int dist = 8; dist <= 20; dist += 4)
            {
                IntVec3 candidate = (vehicle.Position.ToVector3Shifted() + awayDir * dist).ToIntVec3();
                if (!candidate.InBounds(map)) continue;
                if (!candidate.Standable(map)) continue;
                if (!vehicle.DrivableRectOnCell(candidate, Ext_Vehicles.DestinationHitboxReq.AnyRotation)) continue;
                if (vehicle.CanReachVehicle(new LocalTargetInfo(candidate), PathEndMode.OnCell, Danger.Deadly, TraverseMode.NoPassClosedDoors))
                    return candidate;
            }
            return IntVec3.Invalid;
        }
    }
}
