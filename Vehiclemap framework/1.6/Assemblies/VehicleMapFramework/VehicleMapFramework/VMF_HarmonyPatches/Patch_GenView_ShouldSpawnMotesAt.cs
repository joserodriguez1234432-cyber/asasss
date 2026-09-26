// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenView_ShouldSpawnMotesAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenView), "ShouldSpawnMotesAt")]
[PatchLevel(Level.Safe)]
public static class Patch_GenView_ShouldSpawnMotesAt
{
  [HarmonyPatch(new Type[] {typeof (IntVec3), typeof (Map), typeof (bool)})]
  public static void Postfix(IntVec3 loc, Map map, ref bool __result)
  {
    __result = __result || VehicleMapUtility.get_IsVehicleMap(map) && VehicleMapUtility.get_BaseMapOrCaravan(map) == VehicleMapUtility.get_BaseMapOrCaravan(Find.CurrentMap) && GenGrid.InBounds(loc, map);
  }
}
