using HarmonyLib;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using VehicleRaid;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(VehicleRaidUtility), nameof(VehicleRaidUtility.SpawnArmoredDivision))]
    public static class HoverNPC_SpawnArmoredDivision_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(System.Collections.Generic.List<Pawn> __result)
        {
            if (__result == null || __result.Count == 0) return;

            foreach (Pawn pawn in __result)
            {
                if (!(pawn is VehiclePawn vehicle)) continue;
                if (vehicle.Faction == null || vehicle.Faction.IsPlayer) continue;
                if (!vehicle.Spawned || vehicle.Map == null) continue;

                // Gravships activate hover flight but never register as hover transports
                if (CrewManager.IsGravshipVehicle(vehicle))
                {
                    CompVehicleHover gravHover = vehicle.GetComp<CompVehicleHover>();
                    if (gravHover != null && gravHover.State == HoverState.Grounded)
                        gravHover.ActivateHoverNPC();
                    continue;
                }

                CompVehicleHover hoverComp = vehicle.GetComp<CompVehicleHover>();
                if (hoverComp == null) continue;
                if (hoverComp.State != HoverState.Grounded) continue;

                hoverComp.ActivateHoverNPC();

                if (VRF_TransportUtil.IsTransportVehicle(vehicle) && vehicle.AllPawnsAboard.Count > 1)
                {
                    var manager = HoverNPC_TransportManager.GetFor(vehicle.Map);
                    manager?.RegisterVehicle(vehicle);
                }
                else if (!VRF_TransportUtil.IsTransportVehicle(vehicle) && vehicle.AllPawnsAboard.Count > 1)
                {
                    var manager = HoverNPC_TransportManager.GetFor(vehicle.Map);
                    manager?.RegisterArmedVehicle(vehicle);
                }
            }
        }
    }

    [HarmonyPatch(typeof(AirdropSkyfaller), "SpawnThings")]
    public static class Patch_AirdropSkyfaller_AssaultDuty
    {
        [HarmonyPostfix]
        public static void Postfix(AirdropSkyfaller __instance)
        {
            if (__instance?.Map == null) return;

            Map map = __instance.Map;

            foreach (IntVec3 cell in GenAdj.OccupiedRect((Thing)__instance).ExpandedBy(3).ClipInsideMap(map))
            {
                if (!cell.InBounds(map)) continue;
                foreach (Thing thing in cell.GetThingList(map))
                {
                    if (!(thing is Pawn pawn)) continue;
                    if (pawn is VehiclePawn) continue;
                    if (pawn.Dead || pawn.Downed || !pawn.Spawned) continue;
                    if (pawn.Faction == null || pawn.Faction.IsPlayer) continue;

                    Lord lord = pawn.GetLord();
                    if (lord == null) continue;
                    if (!(lord.LordJob is LordJob_VehicleRaid)) continue;

                    DutyDef assaultDuty = VRF_DutyDefOf.VRF_InfantryAssault
                        ?? DefDatabase<DutyDef>.GetNamed("VRF_InfantryAssault", false);
                    if (assaultDuty == null) continue;

                    if (pawn.mindState?.duty?.def == assaultDuty) continue;

                    pawn.mindState.duty = new PawnDuty(assaultDuty);
                    pawn.jobs?.StopAll();
                }
            }
        }
    }
}
