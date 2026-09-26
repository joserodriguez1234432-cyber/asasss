// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ResourceCounter_UpdateResourceCounts
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ResourceCounter), "UpdateResourceCounts")]
[PatchLevel(Level.Safe)]
public static class Patch_ResourceCounter_UpdateResourceCounts
{
  public static void Postfix(Map ___map, Dictionary<ThingDef, int> ___countedAmounts)
  {
    foreach (VehiclePawnWithMap vehiclePawnWithMap in VehiclePawnWithMapCache.AllVehiclesOn(___map))
    {
      foreach (SlotGroup slotGroup in vehiclePawnWithMap.VehicleMap.haulDestinationManager.AllGroupsListForReading)
      {
        foreach (Thing heldThing in slotGroup.HeldThings)
        {
          Thing innerIfMinified = MinifyUtility.GetInnerIfMinified(heldThing);
          if (innerIfMinified.def.CountAsResource && !RottableUtility.IsNotFresh(innerIfMinified))
          {
            ThingDef def = innerIfMinified.def;
            ___countedAmounts[def] += innerIfMinified.stackCount;
          }
        }
      }
    }
  }
}
