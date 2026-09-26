// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Comp_Shield_CurShieldPosition
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TabulaRasa")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Comp_Shield_CurShieldPosition
{
  public static void Postfix(ThingWithComps ___parent, ref Vector3 __result)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) ___parent).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    __result = __result.ToBaseMapCoord(vehicle);
  }
}
