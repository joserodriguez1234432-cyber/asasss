// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FoodUtility_BestFoodSourceOnMap
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

[HarmonyPatch(typeof (FoodUtility), "BestFoodSourceOnMap")]
[PatchLevel(Level.Sensitive)]
public static class Patch_FoodUtility_BestFoodSourceOnMap
{
  private static readonly List<Thing> searchSet = new List<Thing>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    MethodInfo m_ThingsMatching = AccessTools.Method(typeof (ListerThings), "ThingsMatching", (Type[]) null, (Type[]) null);
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) m_ThingsMatching))) + 1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadLocal(1, false),
      PatchHelper.get_CallInstruction((Patch_FoodUtility_BestFoodSourceOnMap.\u003C\u003EO.\u003C0\u003E__AddSearchSet ?? (Patch_FoodUtility_BestFoodSourceOnMap.\u003C\u003EO.\u003C0\u003E__AddSearchSet = new Func<List<Thing>, Pawn, ThingRequest, List<Thing>>(Patch_FoodUtility_BestFoodSourceOnMap.AddSearchSet))).Method)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }

  private static List<Thing> AddSearchSet(List<Thing> list, Pawn getter, ThingRequest req)
  {
    Patch_FoodUtility_BestFoodSourceOnMap.searchSet.Clear();
    Patch_FoodUtility_BestFoodSourceOnMap.searchSet.AddRange((IEnumerable<Thing>) list);
    foreach (Map mapAndVehicleMap in ((Thing) getter).Map.BaseMapAndVehicleMaps(false))
      Patch_FoodUtility_BestFoodSourceOnMap.searchSet.AddRange((IEnumerable<Thing>) mapAndVehicleMap.listerThings.ThingsMatching(req));
    return Patch_FoodUtility_BestFoodSourceOnMap.searchSet;
  }
}
