// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BlitRequest_For
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Vehicles;
using Vehicles.Rendering;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[VFVersionalPatch]
[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (BlitRequest), "For", new Type[] {typeof (VehiclePawn)})]
[PatchLevel(Level.Safe)]
public static class Patch_BlitRequest_For
{
  public static void Postfix(VehiclePawn vehicle, ref BlitRequest __result)
  {
    if (!(vehicle is VehiclePawnWithMap vehiclePawnWithMap))
      return;
    __result.blitTargets.Add((IBlitTarget) vehiclePawnWithMap.VehicleMapBlitter);
  }
}
