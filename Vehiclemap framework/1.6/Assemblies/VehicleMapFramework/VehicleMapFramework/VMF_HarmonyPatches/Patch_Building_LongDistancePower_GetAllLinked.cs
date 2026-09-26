// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_LongDistancePower_GetAllLinked
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PowerPoles")]
[HarmonyPatch]
public static class Patch_Building_LongDistancePower_GetAllLinked
{
  [PatchLevel(Level.Safe)]
  public static IEnumerable<Building> Postfix(IEnumerable<Building> values, Building __instance)
  {
    foreach (Building building in values)
    {
      if (((Thing) __instance).Map == ((Thing) building).Map)
        yield return building;
    }
  }

  [HarmonyReversePatch]
  [PatchLevel(Level.Mandatory)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static IEnumerable<Building> GetAllLinked(Building instance, bool sanitize)
  {
    throw new NotImplementedException();
  }
}
