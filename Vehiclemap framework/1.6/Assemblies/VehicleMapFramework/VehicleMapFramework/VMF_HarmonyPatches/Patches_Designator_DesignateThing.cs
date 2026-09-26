// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_Designator_DesignateThing
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

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patches_Designator_DesignateThing
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return ((IEnumerable<MethodBase>) GenTypes.AllSubclasses(typeof (Designator)).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t => (IEnumerable<MethodInfo>) AccessToolsExtensions.GetDeclaredMethods(t))).Where<MethodInfo>((Func<MethodInfo, bool>) (m =>
    {
      string name = m.Name;
      return name == "DesignateThing" || name == "CanDesignateThing";
    }))).WhereCallsMethod((MethodBase) MethodInfoCache.CachedMethodInfo.g_Designator_Map);
  }

  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, MethodInfoCache.CachedMethodInfo.g_Designator_Map))
      {
        Label label = generator.DefineLabel();
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return CodeInstruction.LoadArgument(1, false);
        yield return new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_MapHeld);
        yield return new CodeInstruction(OpCodes.Dup, (object) null);
        yield return new CodeInstruction(OpCodes.Brtrue_S, (object) label);
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return CodeInstruction.LoadArgument(0, false);
        yield return new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.g_Designator_Map);
        yield return CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Nop, (object) null), new Label[1]
        {
          label
        });
      }
      else
        yield return instruction;
    }
  }
}
