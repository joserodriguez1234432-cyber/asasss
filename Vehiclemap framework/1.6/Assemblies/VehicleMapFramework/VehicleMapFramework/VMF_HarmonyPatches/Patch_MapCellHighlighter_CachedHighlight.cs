// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapCellHighlighter_CachedHighlight
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AllowTool")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapCellHighlighter_CachedHighlight
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessToolsExtensions.Constructor(AccessTools.TypeByName("AllowTool.MapCellHighlighter+CachedHighlight"), new Type[2]
    {
      typeof (Vector3),
      typeof (Material)
    }, false);
  }

  public static void Prefix(ref Vector3 drawPosition)
  {
    VehiclePawnWithMap vehicle;
    if (!Find.CurrentMap.IsVehicleMapOf(out vehicle) && (vehicle = Command_FocusVehicleMap.FocusedVehicle) == null)
      return;
    drawPosition = Vector3Utility.WithY(drawPosition.ToBaseMapCoord(vehicle), drawPosition.y);
  }
}
