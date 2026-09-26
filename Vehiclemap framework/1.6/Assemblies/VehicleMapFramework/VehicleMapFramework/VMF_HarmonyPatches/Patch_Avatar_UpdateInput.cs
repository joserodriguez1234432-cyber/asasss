// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Avatar_UpdateInput
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PerspectiveShift")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Avatar_UpdateInput
{
  public static void Postfix(ref Vector3 ___moveInput, Pawn ___pawn)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) ___pawn).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    ___moveInput = Vector3Utility.RotatedBy(___moveInput, -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
  }
}
