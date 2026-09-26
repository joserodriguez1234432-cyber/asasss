// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_X2_JobGiver_Work_TryIssueJobPackage
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

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MiscRobots")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_X2_JobGiver_Work_TryIssueJobPackage
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator,
    MethodBase original)
  {
    LocalBuilder scanner;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Isinst && c.operand.Equals((object) typeof (WorkGiver_Scanner))), (string) null)
    }).DeclareLocal(typeof (WorkGiver_Scanner), ref scanner).InsertAfterAndAdvance(new CodeInstruction[2]
    {
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Stloc_S, (object) scanner)
    }).MatchEndForward(new CodeMatch[2]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (ListerThings), "ThingsMatching", (Type[]) null, (Type[]) null)),
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) c.operand).LocalType == typeof (IEnumerable<Thing>)), (string) null)
    }).Repeat((Action<CodeMatcher>) (matcher => matcher.InsertAndAdvance(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Ldloc_S, (object) scanner),
      PatchHelper.get_CallInstruction((Patch_X2_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C0\u003E__AddSearchSet ?? (Patch_X2_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C0\u003E__AddSearchSet = new Func<List<Thing>, Pawn, WorkGiver_Scanner, IEnumerable<Thing>>(Patch_JobGiver_Work_TryIssueJobPackage.AddSearchSet))).Method)
    }).Advance(1)), (Action<string>) null);
    MethodInfo method1 = GenClosest.ClosestThing_Global.Method;
    MethodInfo method2 = GenClosestCrossMap.ClosestThing_Global.Method;
    MethodInfo method3 = GenClosest.ClosestThing_Global_Reachable.Method;
    MethodInfo method4 = GenClosestCrossMap.ClosestThing_Global_Reachable.Method;
    MethodInfo methodInfo1 = AccessTools.Method(typeof (WorkGiver_Scanner), "PotentialWorkThingsGlobal", (Type[]) null, (Type[]) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method5 = (Patch_X2_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C5\u003E__PotentialWorkThingsGlobalAll ?? (Patch_X2_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C5\u003E__PotentialWorkThingsGlobalAll = new Func<WorkGiver_Scanner, Pawn, IEnumerable<Thing>>(Patch_JobGiver_Work_TryIssueJobPackage.PotentialWorkThingsGlobalAll))).Method;
    MethodInfo methodInfo2 = AccessTools.Method(typeof (WorkGiver_Scanner), "JobOnThing", (Type[]) null, (Type[]) null);
    MethodInfo method6 = Patch_JobGiver_Work_TryIssueJobPackage.JobOnThingMap.Method;
    return (IEnumerable<CodeInstruction>) codeMatcher.InstructionEnumeration().MethodReplacer(((MethodBase) method1, (MethodBase) method2), ((MethodBase) method3, (MethodBase) method4), ((MethodBase) methodInfo1, (MethodBase) method5), ((MethodBase) methodInfo2, (MethodBase) method6));
  }
}
