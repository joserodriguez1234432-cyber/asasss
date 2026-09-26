// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Skyfaller_Tick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyAfter(new string[] {"Neronix17.TabulaRasa.RimWorld"})]
[HarmonyPatchCategory("VMF_Patches_TabulaRasa")]
[HarmonyPatch(typeof (Skyfaller), "Tick")]
[PatchLevel(Level.Safe)]
public static class Patch_Skyfaller_Tick
{
  public static List<Func<Skyfaller, bool>> Prefixes { get; } = new List<Func<Skyfaller, bool>>(1)
  {
    new Func<Skyfaller, bool>(Patch_Patch_Skyfaller_Tick_Prefix.PrefixPatch)
  };

  public static bool Prefix(Skyfaller __instance)
  {
    foreach (Map mapAndVehicleMap in ((Thing) __instance).Map.BaseMapAndVehicleMaps(false))
    {
      TargetMapUtility.set_TargetMap((Thing) __instance, mapAndVehicleMap);
      try
      {
        for (int index = 0; index < Patch_Skyfaller_Tick.Prefixes.Count; ++index)
        {
          if (!Patch_Skyfaller_Tick.Prefixes[index](__instance))
            return false;
        }
      }
      finally
      {
        ((Thing) __instance).RemoveTargetInfo();
      }
    }
    return true;
  }
}
