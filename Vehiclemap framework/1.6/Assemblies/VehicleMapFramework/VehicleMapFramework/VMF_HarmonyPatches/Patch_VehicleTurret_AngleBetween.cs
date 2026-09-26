// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleTurret_AngleBetween
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleTurret), "AngleBetween")]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleTurret_AngleBetween
{
  public static void Prefix(VehicleTurret __instance, ref Vector3 position)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) __instance.vehicle).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    ref Vector3 local = ref position;
    Vector3 vector3_1 = position;
    Vector3 turretLocation = __instance.TurretLocation;
    Rot8 fullRotation = vehicle.FullRotation;
    double asAngle = (double) ((Rot8) ref fullRotation).AsAngle;
    Vector3 vector3_2 = Ext_Math.RotatePoint(vector3_1, turretLocation, (float) asAngle);
    local = vector3_2;
  }
}
