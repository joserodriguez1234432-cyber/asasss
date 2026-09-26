// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PawnFlyer_SpawnSetup
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (PawnFlyer), "SpawnSetup")]
[PatchLevel(Level.Safe)]
public static class Patch_PawnFlyer_SpawnSetup
{
  public static void Prefix(
    Map map,
    Vector3 ___startVec,
    IntVec3 ___destCell,
    ref float ___flightDistance)
  {
    ___flightDistance = IntVec3Utility.DistanceTo(___destCell.ToBaseMapCoord(map), IntVec3Utility.ToIntVec3(___startVec));
  }
}
