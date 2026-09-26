// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuOptionProvider_Entity_GetOptionFor
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

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_FloatMenuOptionProvider_Entity_GetOptionFor
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return AccessTools.FindIncludingInnerTypes<MethodBase>(typeof (FloatMenuOptionProvider_CaptureEntity), new Func<Type, MethodBase>(GetOptionsFor_MoveNext));
    yield return AccessTools.FindIncludingInnerTypes<MethodBase>(typeof (FloatMenuOptionProvider_TransferEntity), new Func<Type, MethodBase>(GetOptionsFor_MoveNext));

    static MethodBase GetOptionsFor_MoveNext(Type t)
    {
      return t.Name.Contains("<GetOptionsFor>") ? (MethodBase) AccessTools.Method(t, "MoveNext", (Type[]) null, (Type[]) null) : (MethodBase) null;
    }
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    MethodInfo methodInfo = AccessTools.Method(typeof (ListerBuildings), "AllBuildingsColonistOfClass", (Type[]) null, (Type[]) null).MakeGenericMethod(typeof (Building_HoldingPlatform));
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo)
    }).Advance(1);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.Insert(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction((Patch_FloatMenuOptionProvider_Entity_GetOptionFor.\u003C\u003EO.\u003C1\u003E__AddHoldingPlatforms ?? (Patch_FloatMenuOptionProvider_Entity_GetOptionFor.\u003C\u003EO.\u003C1\u003E__AddHoldingPlatforms = new Func<IEnumerable<Building_HoldingPlatform>, IEnumerable<Building_HoldingPlatform>>(Patch_FloatMenuOptionProvider_Entity_GetOptionFor.AddHoldingPlatforms))).Method)
    });
    codeMatcher.MatchStartBackwards(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    });
    codeMatcher.Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing);
    MethodInfo method1 = GenClosest.ClosestThing_Global_Reachable.Method;
    MethodInfo method2 = GenClosestCrossMap.ClosestThing_Global_Reachable.Method;
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(method1)
    });
    codeMatcher.Operand = (object) method2;
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }

  private static IEnumerable<Building_HoldingPlatform> AddHoldingPlatforms(
    IEnumerable<Building_HoldingPlatform> enumerable)
  {
    return enumerable.Concat<Building_HoldingPlatform>(VehiclePawnWithMapCache.AllVehiclesOn(Find.CurrentMap).SelectMany<VehiclePawnWithMap, Building_HoldingPlatform>((Func<VehiclePawnWithMap, IEnumerable<Building_HoldingPlatform>>) (v => v.VehicleMap.listerBuildings.AllBuildingsColonistOfClass<Building_HoldingPlatform>())));
  }
}
