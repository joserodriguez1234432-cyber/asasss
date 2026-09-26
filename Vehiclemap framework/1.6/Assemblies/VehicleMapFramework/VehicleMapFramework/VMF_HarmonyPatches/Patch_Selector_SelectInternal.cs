// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Selector_SelectInternal
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Selector), "SelectInternal")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Selector_SelectInternal
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldloc_3), (object) null, (string) null)
    }).InsertAndAdvance(new CodeInstruction[1]
    {
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    }).InsertAfterAndAdvance(new CodeInstruction[1]
    {
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap)
    }).InsertAndAdvance(new CodeInstruction[1]
    {
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    }).InsertAfterAndAdvance(new CodeInstruction[1]
    {
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMapOrCaravan_Map)
    });
    LocalBuilder localBuilder;
    Label label;
    return codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.IsStloc(codeMatcher.Instructions().Select<CodeInstruction, object>((Func<CodeInstruction, object>) (c => c.operand)).OfType<LocalBuilder>().First<LocalBuilder>((Func<LocalBuilder, bool>) (l => l.LocalType == typeof (IntVec3))))
    }).DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder).CreateLabel(ref label).Insert(new CodeInstruction[6]
    {
      CodeInstruction.LoadLocal(3, false),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoordCell)
    }).InstructionEnumeration();
  }
}
