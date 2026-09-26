// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AnimalPenUtility), "GetPenAnimalShouldBeTakenTo")]
[PatchLevel(Level.Sensitive)]
public static class Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    FieldInfo f_allPenMarkers = AccessTools.Field(typeof (ListerBuildings), "allBuildingsAnimalPenMarkers");
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldfld && CodeInstructionExtensions.OperandIs(c, (MemberInfo) f_allPenMarkers))) + 1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[2]
    {
      CodeInstruction.LoadLocal(1, false),
      PatchHelper.get_CallInstruction((Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo.\u003C\u003EO.\u003C0\u003E__AddPenMarkers ?? (Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo.\u003C\u003EO.\u003C0\u003E__AddPenMarkers = new Func<HashSet<Building>, Map, HashSet<Building>>(Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo.AddPenMarkers))).Method)
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) list.MethodReplacer(AccessTools.Method(typeof (AnimalPenUtility), "CheckUseAndReach", (Type[]) null, (Type[]) null), (Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo.\u003C\u003EO.\u003C1\u003E__CheckUseAndReach ?? (Patch_AnimalPenUtility_GetPenAnimalShouldBeTakenTo.\u003C\u003EO.\u003C1\u003E__CheckUseAndReach = new \u003C\u003EF\u007B00049000\u007D<Pawn, CompAnimalPenMarker, bool, Pawn, bool, bool, bool, bool>(AnimalPenUtilityOnVehicle.CheckUseAndReach))).Method);
  }

  private static HashSet<Building> AddPenMarkers(HashSet<Building> hashSet, Map map)
  {
    HashSet<Building> buildingSet = new HashSet<Building>((IEnumerable<Building>) hashSet);
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
      GenCollection.AddRange<Building>(buildingSet, mapAndVehicleMap.listerBuildings.allBuildingsAnimalPenMarkers);
    return buildingSet;
  }
}
