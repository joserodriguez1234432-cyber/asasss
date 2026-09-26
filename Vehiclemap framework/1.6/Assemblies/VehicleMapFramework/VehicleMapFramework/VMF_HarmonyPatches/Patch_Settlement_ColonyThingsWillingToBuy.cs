// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Settlement_ColonyThingsWillingToBuy
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Settlement), "ColonyThingsWillingToBuy")]
[PatchLevel(Level.Safe)]
public static class Patch_Settlement_ColonyThingsWillingToBuy
{
  public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, Pawn playerNegotiator)
  {
    return Patch_Caravan_ColonyThingsWillingToBuy.Postfix(values, playerNegotiator);
  }
}
