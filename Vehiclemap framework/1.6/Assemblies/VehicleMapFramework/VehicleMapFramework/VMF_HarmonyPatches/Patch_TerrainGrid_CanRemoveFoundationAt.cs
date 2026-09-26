// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TerrainGrid_CanRemoveFoundationAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Odyssey")]
[HarmonyPatch(typeof (TerrainGrid), "CanRemoveFoundationAt")]
[PatchLevel(Level.Safe)]
public static class Patch_TerrainGrid_CanRemoveFoundationAt
{
  public static void Postfix(ref bool __result, Map ___map)
  {
    VehiclePawnWithMap vehicle;
    __result = ((__result ? 1 : 0) & (!___map.IsVehicleMapOf(out vehicle) ? 1 : (!((Def) ((Thing) vehicle).def).HasModExtension<VehicleMapProps_Gravship>() ? 1 : 0))) != 0;
  }
}
