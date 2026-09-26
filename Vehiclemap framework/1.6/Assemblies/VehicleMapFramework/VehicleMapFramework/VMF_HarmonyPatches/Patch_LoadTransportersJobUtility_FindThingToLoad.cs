// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LoadTransportersJobUtility_FindThingToLoad
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (LoadTransportersJobUtility), "FindThingToLoad")]
public static class Patch_LoadTransportersJobUtility_FindThingToLoad
{
  [HarmonyReversePatch]
  [PatchLevel(Level.Mandatory)]
  public static ThingCount FindThingToLoad(Pawn p, CompTransporter transporter)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
      codeMatcher.MatchStartForward(new CodeMatch[1]
      {
        CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
      });
      codeMatcher.Insert(new CodeInstruction[3]
      {
        new CodeInstruction(OpCodes.Pop, (object) null),
        CodeInstruction.LoadArgument(1, false),
        CodeInstruction.LoadField(typeof (ThingComp), "parent", false)
      });
      codeMatcher.MatchStartForward(new CodeMatch[1]
      {
        CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
      });
      codeMatcher.Insert(new CodeInstruction[3]
      {
        new CodeInstruction(OpCodes.Pop, (object) null),
        CodeInstruction.LoadArgument(1, false),
        CodeInstruction.LoadField(typeof (ThingComp), "parent", false)
      });
      MethodInfo method1 = GenClosest.ClosestThingReachable.Method;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      MethodInfo method2 = (Patch_LoadTransportersJobUtility_FindThingToLoad.\u003C\u003EO.\u003C1\u003E__ClosestThingReachableOriginal ?? (Patch_LoadTransportersJobUtility_FindThingToLoad.\u003C\u003EO.\u003C1\u003E__ClosestThingReachableOriginal = new Func<IntVec3, Map, ThingRequest, PathEndMode, TraverseParms, float, Predicate<Thing>, IEnumerable<Thing>, int, int, bool, RegionType, bool, bool, Thing>(Patch_GenClosest_ClosestThingReachable.ClosestThingReachableOriginal))).Method;
      return (IEnumerable<CodeInstruction>) codeMatcher.Instructions().MethodReplacer(method1, method2);
    }
  }

  [PatchLevel(Level.Safe)]
  public static bool Prefix(Pawn p, CompTransporter transporter, ref ThingCount __result)
  {
    if (!(transporter is CompBuildableContainer buildableContainer) || buildableContainer.GatherFromBaseMap)
      return true;
    __result = Patch_LoadTransportersJobUtility_FindThingToLoad.FindThingToLoad(p, transporter);
    return false;
  }
}
