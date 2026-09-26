// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamManipulatorUtility_IsStorageDestinationStillValid
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_BeamManipulatorUtility_IsStorageDestinationStillValid
{
  public static void Prefix(ref Map map, Thing thing, IntVec3 destination)
  {
    if (!((IntVec3) ref destination).IsValid)
      return;
    Map targetMap = TargetMapUtility.get_TargetMap(thing);
    if (targetMap == null || !GenGrid.InBounds(destination, targetMap))
      return;
    map = TargetMapUtility.get_TargetMap(thing);
  }
}
