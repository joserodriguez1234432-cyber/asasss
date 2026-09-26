// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleTurret_TurretRotationTargeted
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleTurret_TurretRotationTargeted
{
  public static void Prefix(ref float value, VehicleTurret __instance)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) __instance.vehicle).IsOnNonFocusedVehicleMapOf(out vehicle) || !__instance.TargetLocked && TurretTargeter.Turret != __instance)
      return;
    value = Ext_Math.RotateAngle(value, -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
  }
}
