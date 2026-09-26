// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_TurretGunNonSnap_TryFindNewTarget2
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_GiantImperialTurret")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Building_TurretGunNonSnap_TryFindNewTarget2
{
  public static void Postfix(
    Building_TurretGun __instance,
    ref float ___curAngle,
    LocalTargetInfo ___currentTargetInt,
    LocalTargetInfo __result)
  {
    VehiclePawnWithMap vehicle;
    if (((LocalTargetInfo) ref ___currentTargetInt).IsValid || !((LocalTargetInfo) ref __result).IsValid || !((Thing) __instance).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    ___curAngle = Ext_Math.RotateAngle(___curAngle, VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
  }
}
