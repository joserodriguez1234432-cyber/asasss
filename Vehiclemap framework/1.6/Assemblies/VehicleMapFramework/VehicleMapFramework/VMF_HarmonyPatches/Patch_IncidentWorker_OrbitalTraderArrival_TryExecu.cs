// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (IncidentWorker_OrbitalTraderArrival), "TryExecuteWorker")]
[PatchLevel(Level.Sensitive)]
public static class Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker
{
  private static readonly List<Building> buildings = new List<Building>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (CodeInstructionExtensions.LoadsField(instruction, AccessTools.Field(typeof (ListerBuildings), "allBuildingsColonist"), false))
      {
        yield return CodeInstruction.LoadArgument(1, false);
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.\u003C\u003EO.\u003C0\u003E__AddBuildings ?? (Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.\u003C\u003EO.\u003C0\u003E__AddBuildings = new Func<List<Building>, IncidentParms, List<Building>>(Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.AddBuildings))).Method);
      }
    }
  }

  private static List<Building> AddBuildings(List<Building> list, IncidentParms parms)
  {
    List<VehiclePawnWithMap> vehiclePawnWithMapList = VehiclePawnWithMapCache.AllVehiclesOn((Map) parms.target);
    if (GenList.NullOrEmpty<VehiclePawnWithMap>((IList<VehiclePawnWithMap>) vehiclePawnWithMapList))
      return list;
    Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.buildings.Clear();
    for (int index = 0; index < list.Count; ++index)
      Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.buildings.Add(list[index]);
    foreach (VehiclePawnWithMap vehiclePawnWithMap in vehiclePawnWithMapList)
    {
      List<Building> buildingsColonist = vehiclePawnWithMap.VehicleMap.listerBuildings.allBuildingsColonist;
      for (int index = 0; index < buildingsColonist.Count; ++index)
        Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.buildings.Add(buildingsColonist[index]);
    }
    return Patch_IncidentWorker_OrbitalTraderArrival_TryExecuteWorker.buildings;
  }
}
