// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_ClearReservationsForJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn), "ClearReservationsForJob")]
[PatchLevel(Level.Mandatory)]
public static class Patch_Pawn_ClearReservationsForJob
{
  public static void Prefix(ref Job job, Pawn __instance)
  {
    if (job?.def == null || !(job.GetCachedDriver(__instance) is JobDriver_GotoDestMap cachedDriver))
      return;
    job = cachedDriver.nextJob;
  }
}
