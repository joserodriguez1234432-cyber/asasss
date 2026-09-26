// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamClaimUtility_ReleaseClaim
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_BeamClaimUtility_ReleaseClaim
{
  public static void Postfix(object transfer)
  {
    if (transfer == null)
      return;
    ModCompat.ManipulatorBeamEmitter.thing.Invoke(transfer).RemoveTargetInfo();
  }
}
