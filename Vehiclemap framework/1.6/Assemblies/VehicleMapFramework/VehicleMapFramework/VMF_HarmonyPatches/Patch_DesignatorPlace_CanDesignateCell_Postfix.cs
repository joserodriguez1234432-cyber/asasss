// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_DesignatorPlace_CanDesignateCell_Postfix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_RealFogOfWar")]
[HarmonyPatch]
public static class Patch_DesignatorPlace_CanDesignateCell_Postfix
{
  public static bool Prefix() => Command_FocusVehicleMap.FocusedVehicle == null;
}
