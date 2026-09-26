// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_LordAi
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_LordAi : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GatheringsUtility), "ShouldGuestKeepAttendingGathering", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(AccessTools.Method(typeof (Patch_LordAi), "VehiclesDontParty", (System.Type[]) null, (System.Type[]) null), 800, (string[]) null, (string[]) null, new bool?()));
  }

  public static bool VehiclesDontParty(Pawn p, ref bool __result)
  {
    if (!(p is VehiclePawn))
      return true;
    __result = false;
    return false;
  }
}
