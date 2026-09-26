// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Gravship_DetermineLaunchDirection
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Odyssey")]
[HarmonyPatch(typeof (Gravship), "DetermineLaunchDirection")]
[PatchLevel(Level.Mandatory)]
public static class Patch_Gravship_DetermineLaunchDirection
{
  public static bool Prefix(IntVec3 ___launchDirection, Building ___pilotConsole)
  {
    return IntVec3.op_Inequality(___launchDirection, IntVec3.Zero) || ___pilotConsole != null;
  }
}
