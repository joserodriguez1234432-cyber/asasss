// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LordJob_Joinable_Gathering_VoluntaryJoinPriorityFor
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
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_LordJob_Joinable_Gathering_VoluntaryJoinPriorityFor
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) GenTypes.AllSubclassesNonAbstract(typeof (LordJob_Joinable_Gathering)).Select<Type, MethodInfo>((Func<Type, MethodInfo>) (t => AccessTools.DeclaredMethod(t, "VoluntaryJoinPriorityFor", (Type[]) null, (Type[]) null))).Where<MethodInfo>((Func<MethodInfo, bool>) (m => (object) m != null && PatchHelper.ReadMethodBodyWrapper((MethodBase) m).Any<KeyValuePair<OpCode, object>>((Func<KeyValuePair<OpCode, object>, bool>) (i => MethodInfoCache.CachedMethodInfo.m_IsForbidden.Equals(i.Value)))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, MethodInfoCache.CachedMethodInfo.m_IsForbidden))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return new CodeInstruction(OpCodes.Callvirt, (object) AccessTools.PropertyGetter(typeof (LordJob), "Map"));
        yield return new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_CrossMapIsForbidden2);
      }
      else
        yield return instruction;
    }
  }
}
