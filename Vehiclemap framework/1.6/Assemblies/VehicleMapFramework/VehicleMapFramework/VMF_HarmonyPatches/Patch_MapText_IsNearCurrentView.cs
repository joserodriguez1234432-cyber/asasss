// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapText_IsNearCurrentView
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TextTool")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapText_IsNearCurrentView
{
  public static bool Prefix(
    Thing __instance,
    float ___scale,
    Vector3 ___exactPosition,
    ref bool __result)
  {
    VehiclePawnWithMap vehicle;
    if (!__instance.IsOnNonFocusedVehicleMapOf(out vehicle))
      return true;
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    int num1 = Mathf.CeilToInt(Mathf.Max(30f, ___scale * 16f));
    ref bool local = ref __result;
    CellRect cellRect = ((CellRect) ref currentViewRect).ExpandedBy(num1);
    int num2 = ((CellRect) ref cellRect).Contains(IntVec3Utility.ToIntVec3(___exactPosition.ToBaseMapCoord(vehicle))) ? 1 : 0;
    local = num2 != 0;
    return false;
  }
}
