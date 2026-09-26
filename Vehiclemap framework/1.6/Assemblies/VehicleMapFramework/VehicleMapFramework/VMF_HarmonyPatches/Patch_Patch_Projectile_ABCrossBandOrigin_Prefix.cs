// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_Projectile_ABCrossBandOrigin_Prefix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AsAboveSoBelow")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Patch_Projectile_ABCrossBandOrigin_Prefix
{
  public static bool Prefix(Thing launcher, LocalTargetInfo usedTarget)
  {
    return Patch_Patch_ShotReport_ABCrossBandDistance_Prefix.Prefix(launcher, usedTarget);
  }
}
