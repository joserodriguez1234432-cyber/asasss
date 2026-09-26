// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ToilFailConditions_FailOnForbidden_Delegate
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

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_ToilFailConditions_FailOnForbidden_Delegate
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (ToilFailConditions), (Func<Type, MethodInfo>) (t =>
    {
      Type type;
      if (!t.IsGenericTypeDefinition)
        type = t;
      else
        type = t.MakeGenericType(typeof (Toil));
      return GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(type), (Predicate<MethodInfo>) (m => m.Name.Contains("<FailOnForbidden>")));
    }));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_IsForbidden)
    }).InsertAndAdvance(new CodeInstruction[1]
    {
      CodeInstruction.LoadLocal(2, false)
    }).SetOperandAndAdvance((object) MethodInfoCache.CachedMethodInfo.m_CrossMapIsForbidden1).InstructionEnumeration();
  }
}
