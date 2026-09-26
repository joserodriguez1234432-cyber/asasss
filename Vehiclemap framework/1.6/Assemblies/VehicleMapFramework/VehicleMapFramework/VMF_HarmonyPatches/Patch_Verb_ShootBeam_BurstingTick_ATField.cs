// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_ShootBeam_BurstingTick_ATField
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_UFHeavyIndustries")]
[HarmonyPatch]
public static class Patch_Verb_ShootBeam_BurstingTick_ATField
{
  [HarmonyPatch(typeof (Verb_ShootBeam), "BurstingTick")]
  [PatchLevel(Level.Safe)]
  public static bool Prefix(Verb_ShootBeam __instance)
  {
    Thing caster = ((Verb) __instance).Caster;
    if (caster == null || !caster.Spawned)
      return true;
    ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(((Verb) __instance).Caster.Map);
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      VehiclePawnWithMap vehiclePawnWithMap = readOnlySpan[index];
      using (new VirtualTeleporter(((Verb) __instance).Caster, vehiclePawnWithMap.VehicleMap))
      {
        if (!Patch_Verb_ShootBeam_BurstingTick_ATField.PatchPrefix(__instance))
          return false;
      }
    }
    return true;
  }

  [HarmonyReversePatch]
  [HarmonyPatch]
  [PatchLevel(Level.Mandatory)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool PatchPrefix(Verb_ShootBeam __instance) => false;
}
