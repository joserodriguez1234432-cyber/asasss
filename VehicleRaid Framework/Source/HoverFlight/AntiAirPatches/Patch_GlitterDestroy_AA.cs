using HarmonyLib;
using Verse;
using System.Reflection;
using System;
using RimWorld;

namespace VehicleRaid.HoverFlight.AntiAirPatches
{
    [HarmonyPatch]
    public static class Patch_GlitterDestroy_AA
    {
        public static bool Prepare()
        {
            return ModsConfig.IsActive("Solaris.GlitterDestroy");
        }

        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("GD3.ProjectileAntiAir");
            if (type == null) return null;
            return AccessTools.Method(type, "Impact", new Type[] { typeof(Thing), typeof(bool) });
        }

        public static void Prefix(Projectile __instance, Thing hitThing)
        {
            if (hitThing != null && hitThing is HoverVehicleProjectile dummy && dummy.vehicle != null && dummy.vehicle.Spawned)
            {
                int damageAmount = __instance.DamageAmount;
                DamageDef damageDef = __instance.def.projectile.damageDef;
                
                DamageInfo dinfo = new DamageInfo(
                    damageDef, 
                    damageAmount, 
                    __instance.ArmorPenetration, 
                    -1f, 
                    __instance.Launcher, 
                    null, 
                    __instance.equipmentDef
                );
                
                dummy.vehicle.TakeDamage(dinfo);
                
            }
        }
    }
}