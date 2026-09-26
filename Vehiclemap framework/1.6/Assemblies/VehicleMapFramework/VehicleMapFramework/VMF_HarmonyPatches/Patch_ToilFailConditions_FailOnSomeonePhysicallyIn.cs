// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ToilFailConditions_FailOnSomeonePhysicallyInteracting
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
public static class Patch_ToilFailConditions_FailOnSomeonePhysicallyInteracting
{
  private static MethodInfo TargetMethod()
  {
    return AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (ToilFailConditions), (Func<Type, MethodInfo>) (t =>
    {
      Type type;
      if (!t.IsGenericTypeDefinition)
        type = t;
      else
        type = t.MakeGenericType(typeof (Toil));
      return GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(type), (Predicate<MethodInfo>) (m => m.Name.Contains("<FailOnSomeonePhysicallyInteracting>")));
    }));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> codes = instructions.ToList<CodeInstruction>();
    return codes.Select<CodeInstruction, CodeInstruction>((Func<CodeInstruction, int, CodeInstruction>) ((c, i) =>
    {
      if (c.opcode != OpCodes.Callvirt || !CodeInstructionExtensions.OperandIs(c, (MemberInfo) MethodInfoCache.CachedMethodInfo.g_Thing_Map))
        return c;
      codes[i - 1].opcode = OpCodes.Ldloc_1;
      c.operand = (object) MethodInfoCache.CachedMethodInfo.g_Thing_MapHeld;
      return c;
    }));
  }
}
