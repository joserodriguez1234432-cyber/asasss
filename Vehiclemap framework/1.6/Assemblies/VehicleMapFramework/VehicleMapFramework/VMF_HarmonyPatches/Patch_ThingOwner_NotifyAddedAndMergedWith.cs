// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ThingOwner_NotifyAddedAndMergedWith
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ThingOwner), "NotifyAddedAndMergedWith")]
[PatchLevel(Level.Safe)]
public static class Patch_ThingOwner_NotifyAddedAndMergedWith
{
  public static void Postfix(Thing item, IThingHolder ___owner, int mergedCount)
  {
    if (!(___owner is Pawn_InventoryTracker inventoryTracker) || !(inventoryTracker.pawn is VehiclePawnWithMap pawn))
      return;
    foreach (Thing thing in pawn.VehicleMap.listerBuildings.allBuildingsColonist.Where<Building>((Func<Building, bool>) (b => ThingCompUtility.HasComp<CompBuildableContainer>((Thing) b))))
      ThingCompUtility.TryGetComp<CompBuildableContainer>(thing).Notify_ThingAddedAndMergedWith(item, mergedCount);
  }
}
