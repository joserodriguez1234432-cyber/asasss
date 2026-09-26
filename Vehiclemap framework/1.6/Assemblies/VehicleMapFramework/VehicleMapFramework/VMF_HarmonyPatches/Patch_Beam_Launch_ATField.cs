// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Beam_Launch_ATField
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_UFHeavyIndustries")]
[HarmonyPatch]
public static class Patch_Beam_Launch_ATField
{
  [HarmonyPatch(typeof (Beam), "Launch")]
  [PatchLevel(Level.Safe)]
  public static bool Prefix(Beam __instance, Thing launcher, LocalTargetInfo usedTarget)
  {
    if (launcher == null || !launcher.Spawned)
      return true;
    ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(((Thing) __instance).Map);
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      VehiclePawnWithMap vehiclePawnWithMap = readOnlySpan[index];
      using (new VirtualTeleporter(launcher, vehiclePawnWithMap.VehicleMap))
      {
        if (!Patch_Beam_Launch_ATField.PatchPrefix(__instance, launcher, usedTarget))
          return false;
      }
    }
    return true;
  }

  [HarmonyReversePatch]
  [HarmonyPatch]
  [PatchLevel(Level.Mandatory)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool PatchPrefix(Beam __instance, Thing launcher, LocalTargetInfo usedTarget)
  {
    return false;
  }
}
