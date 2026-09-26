// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PawnLeaner_LeanOffset
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_PawnLeaner_LeanOffset
{
  public static void Postfix(Pawn ___pawn, ref Vector3 __result)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) ___pawn).IsOnVehicleMapOf(out vehicle))
      return;
    __result = Vector3Utility.RotatedBy(__result, -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
  }
}
