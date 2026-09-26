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
    [HarmonyPatch(typeof(GenStep_Outpost), "Generate")]
    public static class Patch_MissionSiteVehicleInjector
    {
        [HarmonyPostfix]
        public static void Postfix(Map map, GenStepParams parms)
        {
            Faction faction = map.ParentFaction;
            if (faction == null || faction == Faction.OfPlayer) return;

            var factionConfig = VRF_Mod.Settings?.GetFactionConfig(faction.def.defName);
            if (factionConfig == null) return;

            List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
            if (usedRects == null || usedRects.Count == 0) return;
            CellRect outpostRect = usedRects[usedRects.Count - 1];

            var eligible = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)>();
            foreach (var e in factionConfig.outpostVehicleEntries.Where(e => e.enabled))
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(e.vehicleKindDefName);
                if (kind == null || !(kind.race is VehicleDef vd)) continue;
                eligible.Add((kind, e, vd));
            }
            if (eligible.Count == 0) return;

            var toSpawn = Patch_CaravanAmbushVehicleInjector.BuildSpawnList(
                factionConfig, eligible, factionConfig.outpostBudget);
            if (toSpawn.Count == 0) return;

            CellRect capturedRect    = outpostRect;
            Faction  capturedFaction = faction;
            var      capturedSpawn   = toSpawn;

            LongEventHandler.ExecuteWhenFinished(() =>
                VRF_SettlementSpawnHelper.SpawnVehiclesDeferred(
                    map, capturedRect, capturedFaction, capturedSpawn));
        }
    }
}
