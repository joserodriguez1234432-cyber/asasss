using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.DetermineNextJob))]
    public static class Patch_NPCVehicleRepair
    {
        private const float EnemyDetectionRadius = 40f;
        private const float TriggerThreshold = 0.70f;

        private static JobDef cachedRepairJobDef;
        private static JobDef cachedBoardJobDef;
        private static DutyDef cachedAssaultDuty;
        private static DutyDef cachedAssaultTransportDuty;

        private static JobDef RepairJobDef => cachedRepairJobDef ?? (cachedRepairJobDef = DefDatabase<JobDef>.GetNamedSilentFail("VRF_RepairVehicle"));
        private static JobDef BoardJobDef => cachedBoardJobDef ?? (cachedBoardJobDef = DefDatabase<JobDef>.GetNamedSilentFail("Board"));
        private static DutyDef AssaultDuty => cachedAssaultDuty ?? (cachedAssaultDuty = VRF_DutyDefOf.VRF_InfantryAssault ?? DefDatabase<DutyDef>.GetNamedSilentFail("VRF_InfantryAssault"));
        private static DutyDef AssaultTransportDuty => cachedAssaultTransportDuty ?? (cachedAssaultTransportDuty = VRF_DutyDefOf.VRF_InfantryAssault_Transport ?? DefDatabase<DutyDef>.GetNamedSilentFail("VRF_InfantryAssault_Transport"));

        [HarmonyPriority(Priority.Low)]
        public static void Postfix(Pawn_JobTracker __instance, ref ThinkResult __result)
        {
            Pawn pawn = __instance.pawn;

            if (pawn == null || pawn.Dead || pawn.Downed) return;
            if (pawn.Faction == null || pawn.Faction.IsPlayer) return;
            if (pawn is VehiclePawn) return;
            if (pawn.Map == null) return;
            if (pawn.ParentHolder is VehicleRoleHandler) return;

            Lord lord = pawn.GetLord();
            if (!(lord?.LordJob is LordJob_VehicleRaid)) return;

            if (__result.Job == null) return;
            JobDef curJobDef = __result.Job.def;
            if (curJobDef == RepairJobDef || curJobDef == BoardJobDef || curJobDef.defName == "VRF_RepairVehicle" || curJobDef.defName == "Board") return;

            DutyDef duty = pawn.mindState?.duty?.def;
            if (duty == null) return;
            bool isAssaultDuty = duty == AssaultDuty || duty == AssaultTransportDuty ||
                                 duty.defName == "VRF_InfantryAssault" || duty.defName == "VRF_InfantryAssault_Transport";
            if (!isAssaultDuty) return;

            if (VRF_TransportUtil.HasEnemy(pawn, EnemyDetectionRadius)) return;

            VehiclePawn target = FindRepairTarget(pawn, lord);
            if (target == null) return;

            IntVec3 standCell = FindStandCell(pawn, target);
            if (!standCell.IsValid) return;

            JobDef repairJobDef = RepairJobDef;
            if (repairJobDef == null) return;

            Job repairJob = JobMaker.MakeJob(repairJobDef, target, standCell);

            __result = new ThinkResult(repairJob, __result.SourceNode, __result.Tag, false);
        }

        private static VehiclePawn FindRepairTarget(Pawn pawn, Lord lord)
        {
            VehiclePawn best = null;
            float bestDistSq = float.MaxValue;

            List<Pawn> lordPawns = lord.ownedPawns;
            for (int i = 0; i < lordPawns.Count; i++)
            {
                if (!(lordPawns[i] is VehiclePawn v)) continue;
                if (v.Dead || !v.Spawned || v.Map != pawn.Map) continue;
                if (v.Faction != pawn.Faction) continue;

                float distSq = v.Position.DistanceToSquared(pawn.Position);
                if (distSq >= bestDistSq) continue;

                if (v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne == true) continue;
                if (v.vehiclePather != null && v.vehiclePather.Moving) continue;
                if (!VehicleNeedsRepair(v)) continue;
                if (!pawn.CanReach(v, PathEndMode.Touch, Danger.Deadly)) continue;

                bestDistSq = distSq;
                best = v;
            }

            return best;
        }

        private static bool VehicleNeedsRepair(VehiclePawn vehicle)
        {
            var components = vehicle.statHandler?.components;
            if (components == null) return false;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].HealthPercent < TriggerThreshold)
                    return true;
            }
            return false;
        }

        private static IntVec3 FindStandCell(Pawn pawn, VehiclePawn vehicle)
        {
            Map map = vehicle.Map;
            CellRect occupied = vehicle.OccupiedRect();

            foreach (IntVec3 cell in occupied.AdjacentCells)
            {
                if (!cell.InBounds(map)) continue;
                if (!cell.Standable(map)) continue;
                if (occupied.Contains(cell)) continue;
                if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly)) continue;
                return cell;
            }

            return IntVec3.Invalid;
        }
    }
}
