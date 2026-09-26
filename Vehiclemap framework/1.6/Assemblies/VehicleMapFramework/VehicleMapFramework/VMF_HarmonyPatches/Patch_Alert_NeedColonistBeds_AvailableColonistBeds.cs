// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Alert_NeedColonistBeds_AvailableColonistBeds
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Alert_NeedColonistBeds), "AvailableColonistBeds")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Alert_NeedColonistBeds_AvailableColonistBeds
{
  private static readonly List<Building> buildings = new List<Building>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Thing))
    {
      yield return instruction;
      if (CodeInstructionExtensions.LoadsField(instruction, AccessTools.Field(typeof (ListerBuildings), "allBuildingsColonist"), false))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_Alert_NeedColonistBeds_AvailableColonistBeds.\u003C\u003EO.\u003C0\u003E__AddBuildings ?? (Patch_Alert_NeedColonistBeds_AvailableColonistBeds.\u003C\u003EO.\u003C0\u003E__AddBuildings = new Func<List<Building>, Map, List<Building>>(Patch_Alert_NeedColonistBeds_AvailableColonistBeds.AddBuildings))).Method);
      }
    }
  }

  private static List<Building> AddBuildings(List<Building> list, Map map)
  {
    List<VehiclePawnWithMap> source = VehiclePawnWithMapCache.AllVehiclesOn(map);
    if (GenList.NullOrEmpty<VehiclePawnWithMap>((IList<VehiclePawnWithMap>) source))
      return list;
    Patch_Alert_NeedColonistBeds_AvailableColonistBeds.buildings.Clear();
    Patch_Alert_NeedColonistBeds_AvailableColonistBeds.buildings.AddRange((IEnumerable<Building>) list);
    Patch_Alert_NeedColonistBeds_AvailableColonistBeds.buildings.AddRange(source.SelectMany<VehiclePawnWithMap, Building>((Func<VehiclePawnWithMap, IEnumerable<Building>>) (v => (IEnumerable<Building>) v.VehicleMap.listerBuildings.allBuildingsColonist)));
    return Patch_Alert_NeedColonistBeds_AvailableColonistBeds.buildings;
  }
}
