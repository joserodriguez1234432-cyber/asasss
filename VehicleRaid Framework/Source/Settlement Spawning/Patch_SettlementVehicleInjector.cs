using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(GenStep_Settlement), "ScatterAt")]
    public static class Patch_SettlementVehicleInjector
    {
        [HarmonyPostfix]
        public static void Postfix(IntVec3 c, Map map, GenStepParams parms)
        {
            if (!MapGenerator.TryGetVar<CellRect>("SettlementRect", out var settlementRect))
                return;

            Faction faction = map.ParentFaction;
            if (faction == null) return;

            if (!VRF_SettlementSpawnHelper.TryMarkInjected(map)) return;

            var factionConfig = VRF_Mod.Settings?.GetFactionConfig(faction.def.defName);
            if (factionConfig == null) return;

            var eligible = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>();
            foreach (var e in factionConfig.settlementVehicleEntries.Where(e => e.enabled))
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(e.vehicleKindDefName);
                if (kind == null || !(kind.race is VehicleDef vd)) continue;
                eligible.Add((kind, e, vd));
            }
            if (eligible.Count == 0) return;

            var toSpawn = Patch_CaravanAmbushVehicleInjector.BuildSpawnList(
                factionConfig, eligible, factionConfig.settlementBudget);
            if (toSpawn.Count == 0) return;

            CellRect capturedRect    = settlementRect;
            Faction  capturedFaction = faction;
            var      capturedSpawn   = toSpawn;

            LongEventHandler.ExecuteWhenFinished(() =>
                VRF_SettlementSpawnHelper.SpawnVehiclesDeferred(
                    map, capturedRect, capturedFaction, capturedSpawn));
        }
    }
}
