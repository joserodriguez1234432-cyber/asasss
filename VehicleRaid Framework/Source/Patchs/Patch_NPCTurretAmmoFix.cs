using HarmonyLib;
using Vehicles;
using Verse;
using RimWorld;
using System.Linq;

namespace VehicleRaidFramework
{

    [HarmonyPatch(typeof(VehiclePawn), "SpawnSetup")]
    public static class Patch_VehiclePawn_PostSpawnSetup_MaxBurstFireMode
    {
        static void Postfix(VehiclePawn __instance, bool respawningAfterLoad)
        {
            if (__instance.Faction == null || __instance.Faction.IsPlayer) return;

            var turretComp = __instance.CompVehicleTurrets;
            if (turretComp?.Turrets == null) return;

            foreach (var turret in turretComp.Turrets)
            {
                if (turret.def?.fireModes == null || turret.def.fireModes.Count <= 1) continue;

                int bestIndex = 0;
                int bestShots = -1;
                for (int i = 0; i < turret.def.fireModes.Count; i++)
                {
                    int shots = turret.def.fireModes[i].shotsPerBurst.TrueMax;
                    if (shots > bestShots)
                    {
                        bestShots = shots;
                        bestIndex = i;
                    }
                }

                turret.currentFireMode = bestIndex;
            }
        }
    }

    [HarmonyPatch(typeof(CompVehicleTurrets), "FillMagazineCapacity")]
    public static class Patch_FillMagazineCapacity_SetAmmoType
    {
        static void Postfix(CompVehicleTurrets __instance)
        {
            if (__instance.Turrets == null) return;

            foreach (var turret in __instance.Turrets)
            {
                if (turret.def?.ammunition == null) continue;
                if (turret.loadedAmmo != null) continue;

                ThingDef firstAmmo = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => turret.def.ammunition.Allows(d));
                if (firstAmmo != null)
                {
                    turret.loadedAmmo = firstAmmo;
                    turret.savedAmmoType = firstAmmo;
                }
            }
        }
    }

    [HarmonyPatch(typeof(VehicleTurret), "Tick")]
    public static class Patch_VehicleTurret_Tick_AmmoFix
    {
        public static void Prefix(VehicleTurret __instance)
        {
            if (__instance.vehicle == null || __instance.vehicle.Faction == null || __instance.vehicle.Faction.IsPlayer) return;

            if (__instance.def?.ammunition != null && __instance.shellCount > 0 && __instance.loadedAmmo == null)
            {
                ThingDef firstAmmo = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => __instance.def.ammunition.Allows(d));
                if (firstAmmo != null)
                {
                    __instance.loadedAmmo = firstAmmo;
                    __instance.savedAmmoType = firstAmmo;
                }
            }
        }
    }

    [HarmonyPatch(typeof(VehicleTurret), "ScanForTarget")]
    public static class Patch_VehicleTurret_ScanForTarget_AmmoFix
    {
        public static void Prefix(VehicleTurret __instance)
        {
            if (__instance.vehicle == null || __instance.vehicle.Faction == null || __instance.vehicle.Faction.IsPlayer) return;

            if (__instance.def?.ammunition != null && __instance.shellCount > 0 && __instance.loadedAmmo == null)
            {
                ThingDef firstAmmo = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => __instance.def.ammunition.Allows(d));
                if (firstAmmo != null)
                {
                    __instance.loadedAmmo = firstAmmo;
                    __instance.savedAmmoType = firstAmmo;
                }
            }
        }
    }
}
