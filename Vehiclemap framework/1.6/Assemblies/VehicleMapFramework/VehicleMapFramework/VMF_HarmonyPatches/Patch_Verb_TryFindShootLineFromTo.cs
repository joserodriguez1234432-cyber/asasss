// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_TryFindShootLineFromTo
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Verb), "TryFindShootLineFromTo")]
[PatchLevel(Level.Safe)]
public static class Patch_Verb_TryFindShootLineFromTo
{
  private static bool Prepare() => !ModCompat.CombatExtended;

  public static bool Prefix(
    Verb __instance,
    IntVec3 root,
    LocalTargetInfo targ,
    ref ShootLine resultingLine,
    bool ignoreRange,
    ref bool __result)
  {
    if (!VerbOnVehicleUtility.ShouldConsiderCrossMap(__instance.caster, root, targ))
      return true;
    __result = __instance.TryFindShootLineFromToOnVehicle(root, targ, out resultingLine, ignoreRange);
    return false;
  }
}
