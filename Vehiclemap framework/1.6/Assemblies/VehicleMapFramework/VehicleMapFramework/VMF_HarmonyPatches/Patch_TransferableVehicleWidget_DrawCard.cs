// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TransferableVehicleWidget_DrawCard
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Vehicles.World;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[VFVersionalPatch]
[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (TransferableVehicleWidget), "DrawCard")]
[PatchLevel(Level.Safe)]
public static class Patch_TransferableVehicleWidget_DrawCard
{
  internal static VehiclePawnWithMap vehicle;

  public static void Prefix(TransferableOneWay transferable)
  {
    if (Event.current.type != 7)
      return;
    Patch_TransferableVehicleWidget_DrawCard.vehicle = ((Transferable) transferable).AnyThing as VehiclePawnWithMap;
  }
}
