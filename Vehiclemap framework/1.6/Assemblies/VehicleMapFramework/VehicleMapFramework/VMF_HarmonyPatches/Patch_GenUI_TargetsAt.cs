// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenUI_TargetsAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenUI), "TargetsAt")]
[PatchLevel(Level.Safe)]
public static class Patch_GenUI_TargetsAt
{
  public static bool Prefix(
    Vector3 clickPos,
    TargetingParameters clickParams,
    bool thingsOnly,
    ITargetingSource source,
    ref IEnumerable<LocalTargetInfo> __result)
  {
    VehiclePawnWithMap vehicle;
    bool convToVehicleMap;
    if (!(convToVehicleMap = Find.CurrentMap.IsVehicleMapOf(out vehicle)))
      clickPos.TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None);
    if (vehicle == null)
      return true;
    __result = GenUIOnVehicle.TargetsAt(clickPos, clickParams, thingsOnly, source, vehicle, convToVehicleMap);
    return false;
  }
}
