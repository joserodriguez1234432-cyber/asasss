// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenThing_TrueCenter
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenThing), "TrueCenter")]
public static class Patch_GenThing_TrueCenter
{
  private static bool skipFlag;

  [HarmonyBefore(new string[] {"SmashPhil.VehicleFramework"})]
  [HarmonyPatch(new Type[] {typeof (Thing)})]
  [PatchLevel(Level.Mandatory)]
  public static bool Prefix(Thing t, ref Vector3 __result)
  {
    if (t.TryGetDrawPos(ref __result))
      return false;
    Patch_GenThing_TrueCenter.skipFlag = true;
    return true;
  }

  [HarmonyPatch(new Type[] {typeof (Thing)})]
  [PatchLevel(Level.Mandatory)]
  public static void Finalizer() => Patch_GenThing_TrueCenter.skipFlag = false;

  [HarmonyPatch(new Type[] {typeof (IntVec3), typeof (Rot4), typeof (IntVec2), typeof (float)})]
  [PatchLevel(Level.Safe)]
  public static void Postfix(ref Vector3 __result)
  {
    if (Patch_GenThing_TrueCenter.skipFlag)
      return;
    VehiclePawnWithMap focusedVehicle = Command_FocusVehicleMap.FocusedVehicle;
    if (focusedVehicle == null || VehicleSectionLayerManager.CacheMode || VehiclePawnWithMapCache.CacheMode)
      return;
    __result = Vector3Utility.WithY(__result.ToBaseMapCoord(focusedVehicle), __result.y);
  }
}
