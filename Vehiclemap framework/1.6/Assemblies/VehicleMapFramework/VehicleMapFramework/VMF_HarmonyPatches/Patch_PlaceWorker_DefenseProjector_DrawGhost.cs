// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PlaceWorker_DefenseProjector_DrawGhost
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_EccentricTech_DefenseGrid")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_PlaceWorker_DefenseProjector_DrawGhost
{
  public static void Prefix(ref IntVec3 center, Thing thing)
  {
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnNonFocusedVehicleMapOf(out vehicle) && (vehicle = Command_FocusVehicleMap.FocusedVehicle) == null)
      return;
    center = center.ToBaseMapCoord(vehicle);
  }
}
