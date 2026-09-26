// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CameraJumper_GetWorldTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CameraJumper), "GetWorldTarget")]
[PatchLevel(Level.Safe)]
public static class Patch_CameraJumper_GetWorldTarget
{
  public static void Prefix(ref GlobalTargetInfo target)
  {
    VehiclePawnWithMap vehicle;
    if (!((GlobalTargetInfo) ref target).Thing.IsOnVehicleMapOf(out vehicle))
      return;
    target = GlobalTargetInfo.op_Implicit((Thing) vehicle);
  }
}
