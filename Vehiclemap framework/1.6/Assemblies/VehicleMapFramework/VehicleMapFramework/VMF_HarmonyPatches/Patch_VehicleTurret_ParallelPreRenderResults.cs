// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleTurret_ParallelPreRenderResults
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using SmashTools.Rendering;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleTurret), "ParallelPreRenderResults")]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleTurret_ParallelPreRenderResults
{
  public static void Prefix(
    VehicleTurret __instance,
    ref TransformData transformData,
    ref float rotation,
    ref float parentRotation)
  {
    if (!(__instance.vehicle is VehiclePawnWithMap) || !Find.CurrentMap.IsVehicleMapOf(out VehiclePawnWithMap _) || !Rot4.op_Equality(Rot8.op_Implicit(transformData.orientation), Rot4.West))
      return;
    float num = transformData.rotation * 2f;
    rotation -= num;
    parentRotation -= num;
  }
}
