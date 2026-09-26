// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ToilFailConditions_FailOnBurningImmobile
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_ToilFailConditions_FailOnBurningImmobile
{
  private static MethodInfo TargetMethod()
  {
    return AccessTools.InnerTypes(typeof (ToilFailConditions)).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t =>
    {
      Type type;
      if (!t.IsGenericTypeDefinition)
        type = t;
      else
        type = t.MakeGenericType(typeof (Toil));
      return (IEnumerable<MethodInfo>) AccessToolsExtensions.GetDeclaredMethods(type);
    })).First<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name.Contains("<FailOnBurningImmobile>")));
  }

  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    MethodBase original)
  {
    int num = GenCollection.FirstIndexOf<LocalVariableInfo>((IEnumerable<LocalVariableInfo>) original.GetMethodBody().LocalVariables, (Func<LocalVariableInfo, bool>) (l => l.LocalType == typeof (LocalTargetInfo)));
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).Set(OpCodes.Call, (object) (Patch_ToilFailConditions_FailOnBurningImmobile.\u003C\u003EO.\u003C0\u003E__ThingMapOrTargetMapOrPawnMap ?? (Patch_ToilFailConditions_FailOnBurningImmobile.\u003C\u003EO.\u003C0\u003E__ThingMapOrTargetMapOrPawnMap = new Func<Pawn, LocalTargetInfo, Map>(Patch_ToilFailConditions_FailOnBurningImmobile.ThingMapOrTargetMapOrPawnMap))).Method).Insert(new CodeInstruction[1]
    {
      CodeInstruction.LoadLocal(num, false)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }

  private static Map ThingMapOrTargetMapOrPawnMap(Pawn pawn, LocalTargetInfo target)
  {
    Map map = ((LocalTargetInfo) ref target).Thing?.MapHeld ?? TargetMapUtility.get_TargetMapOrPawnMap(pawn);
    return GenGrid.InBounds(((LocalTargetInfo) ref target).Cell, map) ? map : map.BaseMap();
  }
}
