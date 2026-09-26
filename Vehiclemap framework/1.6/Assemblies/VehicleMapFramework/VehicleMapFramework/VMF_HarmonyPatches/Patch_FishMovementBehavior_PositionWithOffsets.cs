// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FishMovementBehavior_PositionWithOffsets
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Aquariums")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_FishMovementBehavior_PositionWithOffsets
{
  public static void Postfix(object ___aquariumFish, ref Vector3 __result)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) ModCompat.Aquariums.CurrentTank.Invoke(___aquariumFish, Array.Empty<object>())).IsOnVehicleMapOf(out vehicle))
      return;
    __result = __result.ToBaseMapCoord(vehicle);
  }
}
