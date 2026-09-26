using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;
using Vehicles;

namespace VehicleRaidFramework
{
    public static class VRF_OutpostVehicleCounter
    {
        public static int CountExpectedVehicles(Faction faction)
        {
            if (faction == null) return 0;

            var factionConfig = VRF_Mod.Settings?.GetFactionConfig(faction.def.defName);
            if (factionConfig == null) return 0;

            var eligible = new List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry)>();
            foreach (var e in factionConfig.vehicleEntries.Where(e => e.enabled))
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(e.vehicleKindDefName);
                if (kind == null || !(kind.race is VehicleDef)) continue;
                eligible.Add((kind, e));
            }
            if (eligible.Count == 0) return 0;

            float budget = 600f;
            int count = 0;
            bool firstPass = true;
            int safetyLimit = 15;

            while (safetyLimit > 0)
            {
                var affordable = eligible.Where(e =>
                {
                    float cost = e.entry.combatPowerOverride > 0f ? e.entry.combatPowerOverride : e.kind.combatPower;
                    return firstPass || cost <= budget;
                }).ToList();

                if (affordable.Count == 0) break;

                float minCost = affordable.Min(e =>
                    e.entry.combatPowerOverride > 0f ? e.entry.combatPowerOverride : e.kind.combatPower);

                budget -= minCost;
                count++;
                firstPass = false;
                safetyLimit--;

                if (budget <= 0f) break;
            }

            return count;
        }
    }

    [HarmonyPatch(typeof(SitePartWorker_Outpost), "GetPostProcessedThreatLabel")]
    public static class Patch_OutpostThreatLabel
    {
        [HarmonyPostfix]
        public static void Postfix(ref string __result, Site site, SitePart sitePart)
        {
            if (__result == null) return;
            if (site?.Faction == null || site.Faction.IsPlayer) return;

            int vehicleCount = VRF_OutpostVehicleCounter.CountExpectedVehicles(site.Faction);
            if (vehicleCount <= 0) return;

            string vehicleLabel = vehicleCount == 1
                ? "VRF_Vehicle".Translate()
                : "VRF_Vehicles".Translate();

            __result = __result + ", " + "KnownSiteThreatEnemyCountAppend"
                .Translate((NamedArgument)vehicleCount, (NamedArgument)vehicleLabel);
        }
    }

    [HarmonyPatch(typeof(SitePartWorker_Outpost), "Notify_GeneratedByQuestGen")]
    public static class Patch_OutpostQuestGenDescription
    {
        [HarmonyPostfix]
        public static void Postfix(
            SitePart part,
            Slate slate,
            List<Rule> outExtraDescriptionRules,
            Dictionary<string, string> outExtraDescriptionConstants)
        {
            if (part?.site?.Faction == null || part.site.Faction.IsPlayer) return;

            int vehicleCount = VRF_OutpostVehicleCounter.CountExpectedVehicles(part.site.Faction);
            if (vehicleCount <= 0) return;

            string vehicleLabel = vehicleCount == 1
                ? "VRF_Vehicle".Translate()
                : "VRF_Vehicles".Translate();

            string vehicleAppend = " " + "and".Translate() + " " + vehicleCount + " " + vehicleLabel;

            Rule_String enemiesLabelRule = outExtraDescriptionRules
                .OfType<Rule_String>()
                .FirstOrDefault(r => r.keyword == "enemiesLabel");

            if (enemiesLabelRule != null)
            {
                string current = enemiesLabelRule.Generate();
                outExtraDescriptionRules.Remove(enemiesLabelRule);
                outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", current + vehicleAppend));
            }
            else
            {
                outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", (vehicleCount + " " + vehicleLabel)));
            }
        }
    }
}
