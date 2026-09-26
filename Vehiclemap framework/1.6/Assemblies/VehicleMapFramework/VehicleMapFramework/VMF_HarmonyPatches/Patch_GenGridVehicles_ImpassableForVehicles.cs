// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenGridVehicles_ImpassableForVehicles
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (GenGridVehicles), "ImpassableForVehicles")]
[PatchLevel(Level.Mandatory)]
public static class Patch_GenGridVehicles_ImpassableForVehicles
{
  public static void Postfix(ThingDef thingDef, ref bool __result)
  {
    __result &= !GenTypes.SameOrSubclassOf(thingDef.thingClass, typeof (Building_VehicleRamp));
  }
}
