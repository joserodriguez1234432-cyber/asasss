using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework
{
    [HarmonyPatch(typeof(MapParent), nameof(MapParent.PostMapGenerate))]
    public static class Patch_CustomSettlementInjector
    {
        [HarmonyPostfix]
        public static void Postfix(MapParent __instance)
        {
            Map map = __instance.Map;
            if (map == null || map.Disposed) return;

            Faction faction = map.ParentFaction;
            if (faction == null || faction == Faction.OfPlayer) return;

            if (!IsSettlementMap(__instance)) return;

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

            CellRect spawnRect = VRF_SettlementSpawnHelper.GetFallbackRect(map, faction);

            Faction capturedFaction = faction;
            var     capturedSpawn   = toSpawn;

            LongEventHandler.ExecuteWhenFinished(() =>
                VRF_SettlementSpawnHelper.SpawnVehiclesDeferred(
                    map, spawnRect, capturedFaction, capturedSpawn));
        }
        private static bool IsSettlementMap(MapParent parent)
        {
            if (parent is Settlement) return true;

            MapGeneratorDef gen = parent.MapGeneratorDef;
            if (gen == null) return false;

            if (gen == MapGeneratorDefOf.Base_Player) return false;
            if (gen == MapGeneratorDefOf.Encounter)   return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(MapParent), nameof(MapParent.Notify_MyMapRemoved))]
    public static class Patch_CustomSettlementInjector_Cleanup
    {
        [HarmonyPostfix]
        public static void Postfix(Map map)
        {
            VRF_SettlementSpawnHelper.ClearInjected(map);
        }
    }
}
