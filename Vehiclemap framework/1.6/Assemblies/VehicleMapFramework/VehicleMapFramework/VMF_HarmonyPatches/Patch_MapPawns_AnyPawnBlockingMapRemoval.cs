// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapPawns_AnyPawnBlockingMapRemoval
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapPawns_AnyPawnBlockingMapRemoval
{
  public static void Postfix(ref bool __result, Map ___map)
  {
    if (__result)
      return;
    ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(___map);
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      if (readOnlySpan[index].VehicleMap.mapPawns.AnyPawnBlockingMapRemoval)
      {
        __result = true;
        break;
      }
    }
  }
}
