// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Projectile_Liquid_DoImpact
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Projectile_Liquid), "DoImpact")]
[PatchLevel(Level.Safe)]
public static class Patch_Projectile_Liquid_DoImpact
{
  public static bool Prefix(
    Projectile_Liquid __instance,
    Thing hitThing,
    IntVec3 cell,
    ThingDef ___targetCoverDef)
  {
    VehiclePawnWithMap vehicle;
    if (!cell.TryGetVehicleMap(((Thing) __instance).Map, out vehicle))
      return true;
    IntVec3 vehicleMapCoord = cell.ToVehicleMapCoord(vehicle);
    if (((Thing) __instance).def.projectile.filth != null && ((IntRange) ref ((Thing) __instance).def.projectile.filthCount).TrueMax > 0 && !GridsUtility.Filled(vehicleMapCoord, vehicle.VehicleMap))
      FilthMaker.TryMakeFilth(vehicleMapCoord, vehicle.VehicleMap, ((Thing) __instance).def.projectile.filth, ((IntRange) ref ((Thing) __instance).def.projectile.filthCount).RandomInRange, (FilthSourceFlags) 0, true);
    List<Thing> thingList = GridsUtility.GetThingList(vehicleMapCoord, vehicle.VehicleMap);
    for (int index = 0; index < thingList.Count; ++index)
    {
      Thing thing = thingList[index];
      if (!(thing is Mote) && !(thing is Filth) && thing != hitThing)
      {
        Find.BattleLog.Add((LogEntry) new BattleLogEntry_RangedImpact(((Projectile) __instance).Launcher, thing, thing, ((Projectile) __instance).EquipmentDef, ((Thing) __instance).def, ___targetCoverDef));
        DamageInfo damageInfo;
        // ISSUE: explicit constructor call
        ((DamageInfo) ref damageInfo).\u002Ector(((Thing) __instance).def.projectile.damageDef, (float) ((Thing) __instance).def.projectile.GetDamageAmount((Thing) null, (StringBuilder) null), ((Thing) __instance).def.projectile.GetArmorPenetration((Thing) null, (StringBuilder) null), -1f, ((Projectile) __instance).Launcher, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Thing) null, true, true, (QualityCategory) 2, true, false);
        thing.TakeDamage(damageInfo);
      }
    }
    return false;
  }
}
