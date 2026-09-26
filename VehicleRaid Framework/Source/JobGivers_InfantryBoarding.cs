using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;
using SmashTools;

namespace VehicleRaidFramework
{
    public static class VRF_TransportUtil
    {
        public const float CombatNearRadius = 22f;
        public const float BoardSearchRadius = 999f;
        public const float ImmediateThreatRadius = 10f;
        public const float BoardWorthyDistance = 22f;
        public const float OpportunisticBoardDistance = 20f;
        public const float OpportunisticEnemyMin = 15f;
        public const int ReboardCooldownTicks = 300;

        public static Dictionary<int, int> LastDisembarkTick = new Dictionary<int, int>();

        public static bool IsOnReboardCooldown(Pawn pawn)
        {
            if (LastDisembarkTick.TryGetValue(pawn.thingIDNumber, out int tick))
                return Find.TickManager.TicksGame - tick < ReboardCooldownTicks;
            return false;
        }

        public static bool IsVehicleImmobilized(VehiclePawn v)
        {
            if (v == null || v.Dead || !v.Spawned) return true;
            if (IsSiegeDropVehicle(v)) return false;
            if (!v.CanMove) return true;
            if (!CrewManager.HasFunctionalEngine(v)) return true;
            if (v.CompFueledTravel != null && v.CompFueledTravel.EmptyTank) return true;
            if (v.GetStatValue(VehicleStatDefOf.MoveSpeed) <= 0.05f) return true;
            return false;
        }

        public static bool HasEnemy(Pawn pawn, float radius)
        {
            if (pawn.Map == null) return false;
            float rSq = radius * radius;
            foreach (IAttackTarget t in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn))
            {
                if (t.ThreatDisabled(pawn)) continue;
                Thing thing = t.Thing;
                if (thing == null || thing.Destroyed) continue;
                if (thing.Map == null || thing.Map.fogGrid.IsFogged(thing.Position)) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;
                if (thing.Position.DistanceToSquared(pawn.Position) <= rSq) return true;
            }
            return false;
        }

        public static bool IsTransportVehicle(VehiclePawn vehicle)
        {
            var turretComp = vehicle.CompVehicleTurrets;
            return turretComp == null || turretComp.Turrets == null || turretComp.Turrets.Count == 0;
        }

        public static bool HasPassengerOnlySlots(VehiclePawn vehicle)
        {
            if (vehicle.handlers == null) return false;
            foreach (var h in vehicle.handlers)
            {
                if (h?.role == null) continue;
                if ((h.role.HandlingTypes & HandlingType.Movement) == 0 &&
                    (h.role.HandlingTypes & HandlingType.Turret) == 0 &&
                    h.role.Slots > 0)
                    return true;
            }
            return false;
        }

        public static bool IsArmedTransportVehicle(VehiclePawn vehicle)
        {
            var turretComp = vehicle.CompVehicleTurrets;
            if (turretComp == null || turretComp.Turrets == null || turretComp.Turrets.Count == 0)
                return false;
            if (vehicle.VehicleDef.type != VehicleType.Land)
                return false;
            return HasPassengerOnlySlots(vehicle);
        }

        public static float GetVehicleCombatRadius(VehiclePawn vehicle)
        {
            if (IsArmedTransportVehicle(vehicle))
                return vehicle.CompVehicleTurrets?.MaxRange ?? CombatNearRadius;
            if (IsTransportVehicle(vehicle))
                return 35f;
            return CombatNearRadius;
        }

        public static bool IsMortarVehicle(VehiclePawn vehicle)
        {
            var leaderManager = vehicle.Map?.GetComponent<VRF_LeaderManager>();
            return leaderManager != null && leaderManager.IsMortar(vehicle);
        }

        public static bool IsSiegeDropVehicle(VehiclePawn vehicle)
        {
            if (vehicle == null) return false;
            VehicleDef vDef = vehicle.VehicleDef;
            if (vDef == null || vDef.type != VehicleType.Air) return false;
            return VRF_AerialVehicleClassifier.IsSiegePod(vDef);
        }

        public static bool HasPassengerSlots(VehiclePawn vehicle)
        {
            if (vehicle.handlers == null) return false;
            foreach (var h in vehicle.handlers)
            {
                if (h != null && h.role != null && h.AreSlotsAvailable)
                    return true;
            }
            return false;
        }

        public static bool HasAvailablePassengerSlots(VehiclePawn vehicle)
        {
            if (vehicle.handlers == null) return false;
            foreach (var h in vehicle.handlers)
            {
                if (h?.role == null) continue;
                if ((h.role.HandlingTypes & HandlingType.Movement) == 0 &&
                    (h.role.HandlingTypes & HandlingType.Turret) == 0 &&
                    h.AreSlotsAvailable)
                    return true;
            }
            return false;
        }

        public static VehicleRoleHandler GetPassengerHandler(VehiclePawn vehicle, Pawn pawn)
        {
            if (vehicle.handlers == null) return null;
            if (VRF_TransportUtil.IsArmedTransportVehicle(vehicle))
            {
                foreach (var h in vehicle.handlers)
                {
                    if (h?.role == null) continue;
                    if ((h.role.HandlingTypes & HandlingType.Movement) != 0) continue;
                    if ((h.role.HandlingTypes & HandlingType.Turret) != 0) continue;
                    if (h.AreSlotsAvailable) return h;
                }
                return null;
            }
            if (!CrewManager.HasOperationalDriver(vehicle))
            {
                var move = vehicle.GetNextAvailableHandler(pawn, HandlingType.Movement);
                if (move != null && move.AreSlotsAvailable) return move;
            }
            var any = vehicle.GetNextAvailableHandler(pawn, HandlingType.None);
            if (any != null && any.AreSlotsAvailable) return any;
            
            foreach (var h in vehicle.handlers)
            {
                if (h != null && h.role != null && h.AreSlotsAvailable)
                    return h;
            }
            return null;
        }

        public static VehicleRoleHandler GetBestAvailableHandler(VehiclePawn vehicle, Pawn pawn)
        {
            var move = vehicle.GetNextAvailableHandler(pawn, HandlingType.Movement);
            if (move != null && move.AreSlotsAvailable) return move;

            var turret = vehicle.GetNextAvailableHandler(pawn, HandlingType.Turret);
            if (turret != null && turret.AreSlotsAvailable) return turret;

            var any = vehicle.GetNextAvailableHandler(pawn, HandlingType.None);
            if (any != null && any.AreSlotsAvailable) return any;

            return null;
        }

        public static bool TryBoardVehicle(Pawn pawn, VehiclePawn vehicle, VehicleRoleHandler handler)
        {
            JobDef boardJobDef = DefDatabase<JobDef>.GetNamed("Board", false);
            if (boardJobDef == null || handler == null) return false;
            if (!pawn.CanReach(vehicle, PathEndMode.Touch, Danger.Deadly)) return false;

            vehicle.GiveLoadJob(pawn, handler);
            Job job = JobMaker.MakeJob(boardJobDef, vehicle);
            job.expiryInterval = 3000;
            job.locomotionUrgency = pawn.Position.DistanceToSquared(vehicle.Position) > 100f
                ? LocomotionUrgency.Sprint
                : LocomotionUrgency.Jog;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, false, true);
            return true;
        }
    }

    public class JobGiver_InfantryBoardNearbyVehicle : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Map == null || pawn.Downed || pawn.Dead) return null;
            if (pawn.CurJob != null && pawn.CurJob.def.defName == "Board") return null;
            if (pawn.mindState?.wantsToTradeWithColony == true) return null;

            if (VRF_TransportUtil.HasEnemy(pawn, VRF_TransportUtil.CombatNearRadius)) return null;

            var vehicles = pawn.Map.mapPawns.AllPawnsSpawned
                .OfType<VehiclePawn>()
                .Where(v =>
                    v.Faction == pawn.Faction &&
                    !VRF_TransportUtil.IsVehicleImmobilized(v) &&
                    !VRF_TransportUtil.IsSiegeDropVehicle(v) &&
                    v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne != true &&
                    v.GetLord()?.LordJob is LordJob_VehicleRaid &&
                    v.handlers.Any(h => h.AreSlotsAvailable) &&
                    !VRF_TransportUtil.IsTransportVehicle(v) &&
                    v.VehicleDef.type != VehicleType.Sea)
                .OrderBy(v => v.Position.DistanceToSquared(pawn.Position));

            foreach (var vehicle in vehicles)
            {
                if (pawn.Position.DistanceTo(vehicle.Position) > VRF_TransportUtil.BoardSearchRadius) continue;

                VehicleRoleHandler handler = VRF_TransportUtil.GetBestAvailableHandler(vehicle, pawn);
                if (handler == null) continue;

                bool onlyCombatSlotsFull = !vehicle.handlers.Any(h =>
                    h?.role != null &&
                    (h.role.HandlingTypes & (HandlingType.Movement | HandlingType.Turret)) != 0 &&
                    h.AreSlotsAvailable);
                if (onlyCombatSlotsFull && (handler.role.HandlingTypes & (HandlingType.Movement | HandlingType.Turret)) == 0)
                    continue;

                if (!pawn.CanReach(vehicle, PathEndMode.Touch, Danger.Deadly)) continue;

                JobDef boardJobDef = DefDatabase<JobDef>.GetNamed("Board", false);
                if (boardJobDef == null) continue;

                vehicle.GiveLoadJob(pawn, handler);
                Job job = JobMaker.MakeJob(boardJobDef, vehicle);
                job.expiryInterval = 3000;
                job.locomotionUrgency = pawn.Position.DistanceToSquared(vehicle.Position) > 100f
                    ? LocomotionUrgency.Sprint
                    : LocomotionUrgency.Jog;
                return job;
            }

            return null;
        }
    }

    public class JobGiver_FollowVehicle : ThinkNode_JobGiver
    {
        private float followRadius = 100f;
        private float minDistance = 10f;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Map == null || pawn.Downed || pawn.Dead) return null;

            VehiclePawn closest = null;
            float closestDist = float.MaxValue;

            foreach (Pawn p in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (p is VehiclePawn v && v.Faction == pawn.Faction && !v.Dead && v.Spawned && CrewManager.HasOperationalDriver(v))
                {
                    if (v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne == true) continue;
                    if (v.GetLord()?.LordJob is LordJob_VehicleRaid == false) continue;
                    if (VRF_TransportUtil.IsTransportVehicle(v)) continue;
                    if (v.VehicleDef.type == VehicleType.Sea) continue;

                    float dist = v.Position.DistanceToSquared(pawn.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = v;
                    }
                }
            }

            if (closest == null) return null;
            if (closestDist > followRadius * followRadius) return null;
            if (closestDist < minDistance * minDistance) return null;

            if (!pawn.CanReach(closest, PathEndMode.Touch, Danger.Deadly)) return null;

            Job job = JobMaker.MakeJob(JobDefOf.Goto, closest.Position);
            job.expiryInterval = 500;
            job.checkOverrideOnExpire = true;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            return job;
        }
    }

    public class JobGiver_BoardTransportVehicleForExit : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Map == null || pawn.Downed || pawn.Dead) return null;
            if (pawn.CurJob != null && pawn.CurJob.def.defName == "Board") return null;

            VehiclePawn best = null;
            float bestDist = float.MaxValue;

            foreach (Pawn p in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!(p is VehiclePawn v)) continue;
                if (v.Faction != pawn.Faction) continue;
                if (VRF_TransportUtil.IsVehicleImmobilized(v)) continue;
                if (v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne == true) continue;
                if (!VRF_TransportUtil.IsTransportVehicle(v)) continue;
                if (v.VehicleDef.type == VehicleType.Sea) continue;
                if (!v.handlers.Any(h => h.AreSlotsAvailable)) continue;

                Lord vLord = v.GetLord();
                if (!(vLord?.LordJob is LordJob_VehicleRaid)) continue;

                float dist = v.Position.DistanceToSquared(pawn.Position);
                if (dist < bestDist && dist <= VRF_TransportUtil.BoardSearchRadius * VRF_TransportUtil.BoardSearchRadius)
                {
                    bestDist = dist;
                    best = v;
                }
            }

            if (best == null) return null;

            VehicleRoleHandler handler = VRF_TransportUtil.GetBestAvailableHandler(best, pawn);
            if (handler == null) return null;
            if (!pawn.CanReach(best, PathEndMode.Touch, Danger.Deadly)) return null;

            JobDef boardJobDef = DefDatabase<JobDef>.GetNamed("Board", false);
            if (boardJobDef == null) return null;

            best.GiveLoadJob(pawn, handler);
            Job job = JobMaker.MakeJob(boardJobDef, best);
            job.expiryInterval = 3000;
            job.locomotionUrgency = bestDist > 100f ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog;
            return job;
        }
    }

    public class JobGiver_BoardTransportVehicle : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Map == null || pawn.Downed || pawn.Dead) return null;
            if (pawn.mindState?.wantsToTradeWithColony == true) return null;
            if (pawn.CurJob != null && pawn.CurJob.def.defName == "Board") return pawn.CurJob;
            if (VRF_TransportUtil.IsOnReboardCooldown(pawn)) return null;

            if (VRF_TransportUtil.HasEnemy(pawn, VRF_TransportUtil.ImmediateThreatRadius)) return null;

            VehiclePawn best = FindBestTransportVehicle(pawn);
            if (best == null) return null;
            if (VRF_TransportUtil.HasEnemy(best, VRF_TransportUtil.GetVehicleCombatRadius(best))) return null;

            float distToVehicle = pawn.Position.DistanceTo(best.Position);
            float distToEnemy = FindNearestEnemyDistance(pawn);

            bool shouldBoard = false;

            if (distToEnemy < 0f)
                shouldBoard = true;
            else if (distToEnemy >= VRF_TransportUtil.BoardWorthyDistance)
                shouldBoard = true;
            else if (distToVehicle <= VRF_TransportUtil.OpportunisticBoardDistance &&
                     distToEnemy >= VRF_TransportUtil.OpportunisticEnemyMin)
                shouldBoard = true;

            if (!shouldBoard) return null;

            VehicleRoleHandler handler = VRF_TransportUtil.GetPassengerHandler(best, pawn);
            if (handler == null) return null;
            if (!pawn.CanReach(best, PathEndMode.Touch, Danger.Deadly)) return null;

            JobDef boardJobDef = DefDatabase<JobDef>.GetNamed("Board", false);
            if (boardJobDef == null) return null;

            best.GiveLoadJob(pawn, handler);
            Job job = JobMaker.MakeJob(boardJobDef, best);
            job.expiryInterval = 3000;
            job.locomotionUrgency = distToVehicle > 10f
                ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog;
            return job;
        }

        private static VehiclePawn FindBestTransportVehicle(Pawn pawn)
        {
            VehiclePawn best = null;
            float bestDist = float.MaxValue;
            foreach (Pawn p in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!(p is VehiclePawn v)) continue;
                if (v.Faction != pawn.Faction) continue;
                if (VRF_TransportUtil.IsVehicleImmobilized(v)) continue;
                if (v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne == true) continue;
                if (!VRF_TransportUtil.IsTransportVehicle(v) && !VRF_TransportUtil.IsArmedTransportVehicle(v)) continue;
                if (v.VehicleDef.type == VehicleType.Sea) continue;
                if (!VRF_TransportUtil.HasPassengerOnlySlots(v)) continue;
                Lord vLord = v.GetLord();
                if (!(vLord?.LordJob is LordJob_VehicleRaid)) continue;
                float dist = v.Position.DistanceToSquared(pawn.Position);
                if (dist < bestDist && dist <= VRF_TransportUtil.BoardSearchRadius * VRF_TransportUtil.BoardSearchRadius)
                {
                    bestDist = dist;
                    best = v;
                }
            }
            return best;
        }

        private static float FindNearestEnemyDistance(Pawn pawn)
        {
            float bestDistSq = float.MaxValue;
            bool found = false;
            foreach (IAttackTarget t in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn))
            {
                if (t.ThreatDisabled(pawn)) continue;
                Thing thing = t.Thing;
                if (thing == null || thing.Destroyed) continue;
                if (thing.Map == null || thing.Map.fogGrid.IsFogged(thing.Position)) continue;
                if (thing is Pawn p && (p.Dead || p.Downed)) continue;
                float dSq = thing.Position.DistanceToSquared(pawn.Position);
                if (dSq < bestDistSq) { bestDistSq = dSq; found = true; }
            }
            return found ? UnityEngine.Mathf.Sqrt(bestDistSq) : -1f;
        }
    }

    public class JobGiver_DisembarkForCombat : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Map == null || pawn.Downed || pawn.Dead) return null;

            if (!(pawn.ParentHolder is VehicleRoleHandler handler)) return null;
            VehiclePawn vehicle = handler.vehicle;
            if (vehicle == null || !VRF_TransportUtil.IsTransportVehicle(vehicle)) return null;

            float radius = VRF_TransportUtil.GetVehicleCombatRadius(vehicle);
            bool enemyNear = VRF_TransportUtil.HasEnemy(vehicle, radius);
            
            if (vehicle.VehicleDef.type == VehicleType.Sea)
            {
                if (!JobGiver_SeaVehicleMove.lastDisembarkBeganTick.ContainsKey(vehicle.thingIDNumber))
                {
                    if (!enemyNear) return null;
                }
            }
            else
            {
                if (!enemyNear) return null;
            }

            IntVec3 exitCell = FindDisembarkCell(vehicle, pawn);
            if (!exitCell.IsValid) return null;

            VRF_TransportUtil.LastDisembarkTick[pawn.thingIDNumber] = Find.TickManager.TicksGame;
            vehicle.DisembarkPawn(pawn);

            Job job = JobMaker.MakeJob(JobDefOf.Goto, exitCell);
            job.expiryInterval = 300;
            job.checkOverrideOnExpire = true;
            return job;
        }

        private IntVec3 FindDisembarkCell(VehiclePawn vehicle, Pawn pawn)
        {
            Map map = vehicle.Map;
            CellRect rect = vehicle.OccupiedRect();

            foreach (IntVec3 cell in rect.ExpandedBy(3).Cells)
            {
                if (!cell.InBounds(map)) continue;
                if (!cell.Standable(map)) continue;
                if (rect.Contains(cell)) continue;
                if (!GenSight.LineOfSight(vehicle.Position, cell, map)) continue;
                return cell;
            }
            return IntVec3.Invalid;
        }
    }

    public class JobGiver_FollowTransportVehicle : ThinkNode_JobGiver
    {
        private float followRadius = 100f;
        private float minDistance = 8f;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Map == null || pawn.Downed || pawn.Dead) return null;
            if (pawn.ParentHolder is VehicleRoleHandler) return null;

            if (VRF_TransportUtil.HasEnemy(pawn, VRF_TransportUtil.CombatNearRadius)) return null;

            VehiclePawn closest = null;
            float closestDist = float.MaxValue;

            foreach (Pawn p in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!(p is VehiclePawn v)) continue;
                if (v.Faction != pawn.Faction || v.Dead || !v.Spawned) continue;
                if (!CrewManager.HasOperationalDriver(v)) continue;
                if (v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne == true) continue;
                if (v.GetLord()?.LordJob is LordJob_VehicleRaid == false) continue;
                if (!VRF_TransportUtil.IsTransportVehicle(v)) continue;
                if (v.VehicleDef.type == VehicleType.Sea) continue;

                float dist = v.Position.DistanceToSquared(pawn.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = v;
                }
            }

            if (closest == null) return null;
            if (closestDist > followRadius * followRadius) return null;
            if (closestDist < minDistance * minDistance) return null;
            if (!pawn.CanReach(closest, PathEndMode.Touch, Danger.Deadly)) return null;

            Job job = JobMaker.MakeJob(JobDefOf.Goto, closest.Position);
            job.expiryInterval = 500;
            job.checkOverrideOnExpire = true;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            return job;
        }
    }
}

