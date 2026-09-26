// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Util_DesignatePlants
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_CutPlantsBeforeBuilding")]
[HarmonyPatch]
public static class Patch_Util_DesignatePlants
{
  public static void Prefix(ref Map map)
  {
    if (Command_FocusVehicleMap.FocusedVehicle == null)
      return;
    map = Command_FocusVehicleMap.FocusedVehicle.CurrentLevel;
  }
}
