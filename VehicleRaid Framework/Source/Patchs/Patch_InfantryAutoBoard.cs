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
    public static class Patch_InfantryAutoBoard
    {
        private const float NearEnemyRadius = VRF_TransportUtil.CombatNearRadius;

        private static JobDef cachedBoardJobDef;
        private static JobDef cachedMountJobDef;
        private static DutyDef cachedTransportDutyDef;

        private static JobDef BoardJobDef => cachedBoardJobDef ?? (cachedBoardJobDef = DefDatabase<JobDef>.GetNamedSilentFail("Board"));
        private static JobDef MountJobDef => cachedMountJobDef ?? (cachedMountJobDef = DefDatabase<JobDef>.GetNamedSilentFail("Mount"));
        private static DutyDef TransportDutyDef => cachedTransportDutyDef ?? (cachedTransportDutyDef = VRF_DutyDefOf.VRF_InfantryAssault_Transport ?? DefDatabase<DutyDef>.GetNamedSilentFail("VRF_InfantryAssault_Transport"));

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

            DutyDef duty = pawn.mindState?.duty?.def;
            if (duty == null) return;
            if (duty != TransportDutyDef && (TransportDutyDef == null || duty.defName != "VRF_InfantryAssault_Transport")) return;

            if (__result.Job == null) return;
            JobDef curJobDef = __result.Job.def;
            if (curJobDef == BoardJobDef || curJobDef == MountJobDef || curJobDef.defName == "Board" || curJobDef.defName == "Mount") return;

            // Fast reboard cooldown check before expensive distance/threat scans
            if (VRF_TransportUtil.IsOnReboardCooldown(pawn)) return;
            if (VRF_TransportUtil.HasEnemy(pawn, NearEnemyRadius)) return;

            VehiclePawn vehicle = FindTransportVehicle(pawn, lord);
            if (vehicle == null) return;
            if (VRF_TransportUtil.HasEnemy(vehicle, VRF_TransportUtil.GetVehicleCombatRadius(vehicle))) return;

            VehicleRoleHandler handler = VRF_TransportUtil.GetPassengerHandler(vehicle, pawn);
            if (handler == null) return;
            if (!pawn.CanReach(vehicle, PathEndMode.Touch, Danger.Deadly)) return;

            JobDef boardJobDef = BoardJobDef;
            if (boardJobDef == null) return;

            pawn.jobs?.jobQueue?.EnqueueFirst(__result.Job);

            vehicle.GiveLoadJob(pawn, handler);
            Job boardJob = JobMaker.MakeJob(boardJobDef, vehicle);
            boardJob.expiryInterval = 3000;
            boardJob.locomotionUrgency = vehicle.Position.DistanceToSquared(pawn.Position) > 100
                ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog;

            __result = new ThinkResult(boardJob, __result.SourceNode, __result.Tag, false);
        }

        private static VehiclePawn FindTransportVehicle(Pawn pawn, Lord lord)
        {
            VehiclePawn best = null;
            float bestDistSq = float.MaxValue;
            float maxDistSq = VRF_TransportUtil.BoardSearchRadius * VRF_TransportUtil.BoardSearchRadius;

            List<Pawn> lordPawns = lord.ownedPawns;
            for (int i = 0; i < lordPawns.Count; i++)
            {
                if (!(lordPawns[i] is VehiclePawn v)) continue;
                if (v.Dead || !v.Spawned || v.Map != pawn.Map) continue;
                if (v.Faction != pawn.Faction) continue;

                float distSq = v.Position.DistanceToSquared(pawn.Position);
                if (distSq >= bestDistSq || distSq > maxDistSq) continue;

                if (VRF_TransportUtil.IsVehicleImmobilized(v)) continue;
                if (v.GetComp<VehicleRaid.CompVehicleHover>()?.IsAirborne == true) continue;
                if (!VRF_TransportUtil.IsTransportVehicle(v) && !VRF_TransportUtil.IsArmedTransportVehicle(v)) continue;
                if (!VRF_TransportUtil.HasAvailablePassengerSlots(v)) continue;

                bestDistSq = distSq;
                best = v;
            }
            return best;
        }
    }
}
