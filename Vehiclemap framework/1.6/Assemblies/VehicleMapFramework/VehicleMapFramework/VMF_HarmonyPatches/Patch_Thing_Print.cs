// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Thing_Print
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
public static class Patch_Thing_Print
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) GenTypes.AllSubclasses(typeof (Thing)).Append<Type>(typeof (Thing)).Where<Type>((Func<Type, bool>) (t => t != typeof (MinifiedThing))).Select<Type, MethodInfo>((Func<Type, MethodInfo>) (t => AccessTools.DeclaredMethod(t, "Print", (Type[]) null, (Type[]) null))).Where<MethodInfo>((Func<MethodInfo, bool>) (m => (object) m != null && PatchHelper.ReadMethodBodyWrapper((MethodBase) m).Any<KeyValuePair<OpCode, object>>((Func<KeyValuePair<OpCode, object>, bool>) (i => OpCodes.Ldc_R4.Equals(i.Key) && 0.0f.Equals(i.Value)))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsConstant(0.0)
    }).Repeat((Action<CodeMatcher>) (matcher => matcher.InsertAndAdvance(new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(0, false)
    }).SetInstruction(new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PrintExtraRotation))), (Action<string>) null);
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
