// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Dialog_SplitCaravan_TrySplitCaravan
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Dialog_SplitCaravan), "TrySplitCaravan")]
[PatchLevel(Level.Safe)]
public static class Patch_Dialog_SplitCaravan_TrySplitCaravan
{
  public static void Prefix(Caravan ___caravan, List<TransferableOneWay> ___transferables)
  {
    for (int index = ___transferables.Count - 1; index >= 0; --index)
    {
      TransferableOneWay transferable = ___transferables[index];
      if (((Transferable) transferable).CountToTransfer > 0)
      {
        int countToTransfer = ((Transferable) transferable).CountToTransfer;
        foreach (Thing thing1 in transferable.things)
        {
          if (thing1.IsOnVehicleMapOf(out VehiclePawnWithMap _))
          {
            int num = Math.Min(countToTransfer, thing1.stackCount);
            countToTransfer -= num;
            Thing thing2 = thing1.SplitOff(num);
            ___caravan.AddPawnOrItem(thing2, false);
            ___transferables.RemoveAt(index);
          }
        }
      }
    }
  }
}
