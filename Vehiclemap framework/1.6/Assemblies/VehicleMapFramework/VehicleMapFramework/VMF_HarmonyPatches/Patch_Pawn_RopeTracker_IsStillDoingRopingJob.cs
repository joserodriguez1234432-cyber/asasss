// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_RopeTracker_IsStillDoingRopingJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn_RopeTracker), "IsStillDoingRopingJob")]
[PatchLevel(Level.Safe)]
public static class Patch_Pawn_RopeTracker_IsStillDoingRopingJob
{
  public static void Postfix(Pawn roper, ref bool __result)
  {
    __result = __result || roper.jobs.curDriver is JobDriver_GotoDestMap curDriver && curDriver.nextJob.GetCachedDriver(roper) is JobDriver_RopeToDestination;
  }
}
