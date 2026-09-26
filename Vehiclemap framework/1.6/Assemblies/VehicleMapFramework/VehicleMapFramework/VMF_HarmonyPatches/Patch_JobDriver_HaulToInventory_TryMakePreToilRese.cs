// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_HaulToInventory_TryMakePreToilReservations
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PickUpAndHaul")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriver_HaulToInventory_TryMakePreToilReservations
{
  public static bool Prefix(Job ___job, Pawn ___pawn, ref bool __result)
  {
    if (Ext_IList.NotNullAndAny<LocalTargetInfo>(___job.targetQueueA, (Predicate<LocalTargetInfo>) null))
      return true;
    ReservationUtility.ReserveAsManyAsPossible(___pawn, ___job.targetQueueA, ___job, 1, -1, (ReservationLayerDef) null);
    ReservationUtility.ReserveAsManyAsPossible(___pawn, ___job.targetQueueB, ___job, 1, -1, (ReservationLayerDef) null);
    __result = ReservationUtility.Reserve(___pawn, ___job.targetB, ___job, 1, -1, (ReservationLayerDef) null, true, false);
    return false;
  }
}
