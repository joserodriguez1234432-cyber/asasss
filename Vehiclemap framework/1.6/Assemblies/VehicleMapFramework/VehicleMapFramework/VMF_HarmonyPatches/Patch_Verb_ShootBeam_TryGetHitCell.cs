// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_ShootBeam_TryGetHitCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Verb_ShootBeam), "TryGetHitCell")]
[PatchLevel(Level.Safe)]
public static class Patch_Verb_ShootBeam_TryGetHitCell
{
  public static bool Prefix(
    IntVec3 source,
    IntVec3 targetCell,
    out IntVec3 hitCell,
    Thing ___caster,
    VerbProperties ___verbProps,
    out bool __result)
  {
    IntVec3 intVec3 = GenSight.LastPointOnLineOfSight(source, targetCell, (Func<IntVec3, bool>) (c => c.CanBeSeenOverOnVehicle(___caster.BaseMap())), true);
    if (___verbProps.beamCantHitWithinMinRange && (double) IntVec3Utility.DistanceTo(intVec3, source) < (double) ___verbProps.minRange)
    {
      hitCell = new IntVec3();
      __result = false;
      return false;
    }
    hitCell = ((IntVec3) ref intVec3).IsValid ? intVec3 : targetCell;
    __result = ((IntVec3) ref intVec3).IsValid;
    return false;
  }
}
