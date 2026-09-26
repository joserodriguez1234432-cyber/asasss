// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ActionController_CheckCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_ActionController_CheckCell
{
  public static bool Prefix(ref IntVec3 cell, Map map, ref bool __result)
  {
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
    {
      cell = cell.ToVehicleMapCoord(vehicle);
      if (!GenGrid.InBounds(cell, map))
      {
        __result = true;
        return false;
      }
    }
    return true;
  }
}
