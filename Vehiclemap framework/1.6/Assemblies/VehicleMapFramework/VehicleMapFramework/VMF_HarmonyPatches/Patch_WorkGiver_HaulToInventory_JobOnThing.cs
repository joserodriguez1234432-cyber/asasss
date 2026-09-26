// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WorkGiver_HaulToInventory_JobOnThing
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PickUpAndHaul")]
[HarmonyPatch]
public static class Patch_WorkGiver_HaulToInventory_JobOnThing
{
  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);
    MethodInfo methodInfo1 = AccessTools.Method("PickUpAndHaul.WorkGiver_HaulToInventory:HaulToHopperJob", (Type[]) null, (Type[]) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo1)
    });
    codeMatcher.Insert(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_TargetMapOrMap)
    });
    MethodInfo methodInfo2 = AccessTools.Method("PickUpAndHaul.WorkGiver_HaulToInventory:CapacityAt", (Type[]) null, (Type[]) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo2)
    });
    codeMatcher.Insert(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_TargetMapOrMap)
    });
    MethodInfo methodInfo3 = AccessTools.PropertyGetter("PickUpAndHaul.WorkGiver_HaulToInventory+StoreTarget:Position");
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo3)
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.InsertAfter(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_TargetMapOrPawnMap),
      PatchHelper.get_CallInstruction((Patch_WorkGiver_HaulToInventory_JobOnThing.\u003C\u003EO.\u003C0\u003E__ToBaseMapCoord ?? (Patch_WorkGiver_HaulToInventory_JobOnThing.\u003C\u003EO.\u003C0\u003E__ToBaseMapCoord = new Func<IntVec3, Map, IntVec3>(VehicleMapUtility.ToBaseMapCoord))).Method)
    });
    int num = 0;
    return Transpilers.Manipulator((IEnumerable<CodeInstruction>) codeMatcher.Instructions(), (Func<CodeInstruction, bool>) (c => CodeInstructionExtensions.Calls(c, MethodInfoCache.CachedMethodInfo.g_Thing_Position)), (Action<CodeInstruction>) (c =>
    {
      ++num;
      if (num > 2)
        return;
      c.opcode = OpCodes.Call;
      c.operand = (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap;
    }));
  }

  [PatchLevel(Level.Safe)]
  public static void Prefix(Pawn pawn, Thing thing, ref Map __state)
  {
    Map mapHeld1 = thing.MapHeld;
    Map mapHeld2 = ((Thing) pawn).MapHeld;
    if (mapHeld1 == null || mapHeld1 == mapHeld2)
      return;
    __state = mapHeld2;
    ((Thing) pawn).VirtualMapTransfer(mapHeld1);
  }

  [PatchLevel(Level.Safe)]
  public static void Finalizer(Pawn pawn, Job __result, Map __state)
  {
    if (__state != null)
      ((Thing) pawn).VirtualMapTransfer(__state);
    Map map;
    if (__result == null || !((Thing) pawn).TryGetTargetMap(out map) || !(((Def) __result.def)?.defName == "HaulToInventory") || !((LocalTargetInfo) ref __result.targetB).IsValid)
      return;
    __result.globalTarget = ((LocalTargetInfo) ref __result.targetB).ToGlobalTargetInfo(map);
  }
}
