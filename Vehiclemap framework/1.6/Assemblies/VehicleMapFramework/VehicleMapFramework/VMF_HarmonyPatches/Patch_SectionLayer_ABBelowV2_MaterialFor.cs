// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SectionLayer_ABBelowV2_MaterialFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AsAboveSoBelow")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_SectionLayer_ABBelowV2_MaterialFor
{
  public static void Postfix(Map map, ref Material __result)
  {
    if (!VehicleMapUtility.get_IsVehicleMap(map))
      return;
    __result = SectionLayer_TerrainOnVehicle.GetMaterialWithZ(__result);
  }
}
