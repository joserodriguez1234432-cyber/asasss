// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AttackTargetFinder_CanSee
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AttackTargetFinder), "CanSee")]
[PatchLevel(Level.Safe)]
public static class Patch_AttackTargetFinder_CanSee
{
  public static bool Prefix(
    Thing seer,
    Thing target,
    Func<IntVec3, bool> validator,
    ref bool __result)
  {
    if (seer.Map == target.Map || seer.BaseMap() != target.BaseMap())
      return true;
    __result = seer.CanSee(target, validator);
    return false;
  }
}
