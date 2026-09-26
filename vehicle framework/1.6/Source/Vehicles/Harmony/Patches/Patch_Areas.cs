// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Areas
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools;
using SmashTools.Patching;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_Areas : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (AreaManager), "AddStartingAreas", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Areas), "AddVehicleAreas", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Map), "FinalizeInit", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Areas), "BackfillVehicleAreas", (System.Type[]) null));
  }

  private static void AddVehicleAreas(AreaManager __instance)
  {
    __instance.map.EnsureAreaInitialized<Area_Road>();
    __instance.map.EnsureAreaInitialized<Area_RoadAvoidal>();
  }

  private static void BackfillVehicleAreas(Map __instance)
  {
    __instance.EnsureAreaInitialized<Area_Road>();
    __instance.EnsureAreaInitialized<Area_RoadAvoidal>();
  }
}
