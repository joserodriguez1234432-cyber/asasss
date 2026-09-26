// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenGrid_InNoZoneEdgeArea
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenGrid), "InNoZoneEdgeArea")]
[PatchLevel(Level.Safe)]
public static class Patch_GenGrid_InNoZoneEdgeArea
{
  public static void Postfix(ref bool __result, Map map)
  {
    __result &= !map.IsVehicleMapOf(out VehiclePawnWithMap _);
  }
}
