// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamManipulatorUtility_FillTransferQueue
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_BeamManipulatorUtility_FillTransferQueue
{
  private static bool working;

  public static void Postfix(
    object op,
    int desiredCount,
    object destinationQueue,
    HashSet<Thing> excludedThings,
    HashSet<IntVec3> excludedDestinations,
    HashSet<IntVec3> candidateSeenCellsScratch,
    List<IntVec3> candidateCellsScratch)
  {
    if (Patch_BeamManipulatorUtility_FillTransferQueue.working)
      return;
    Thing thing = ModCompat.ManipulatorBeamEmitter.OperatorThing(op);
    if (thing == null || !thing.Spawned)
      return;
    Map map = thing.Map;
    Patch_BeamManipulatorUtility_FillTransferQueue.working = true;
    try
    {
      foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
      {
        using (new VirtualTeleporter(thing, mapAndVehicleMap))
          ModCompat.ManipulatorBeamEmitter.FillTransferQueue.Invoke((object) null, VehicleMapFramework.Params<(object, int, object, object, object, object, object)>.Get((op, desiredCount, destinationQueue, (object) excludedThings, (object) excludedDestinations, (object) candidateSeenCellsScratch, (object) candidateCellsScratch)));
      }
    }
    finally
    {
      Patch_BeamManipulatorUtility_FillTransferQueue.working = false;
    }
  }
}
