// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_BulkCarrier
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools.Patching;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_BulkCarrier : ConditionalVehiclePatch
{
  public override string PackageId => "Vanya.Tools.BulkCarrier";

  public override PatchSequence PatchAt => PatchSequence.Async;

  public override void PatchAll(ModMetaData mod)
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(AccessTools.TypeByName("BulkCarrier.BulkCarrier"), "Capacity_Prefix", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Compatibility_BulkCarrier), "NoBulkCapacityForVehicles", (System.Type[]) null));
  }

  private static void NoBulkCapacityForVehicles(ref bool __result, Pawn p)
  {
    if (!(p is VehiclePawn))
      return;
    __result = true;
  }
}
