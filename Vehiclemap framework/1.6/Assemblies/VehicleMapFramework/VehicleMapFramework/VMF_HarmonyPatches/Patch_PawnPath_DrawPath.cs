// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PawnPath_DrawPath
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (PawnPath), "DrawPath")]
[PatchLevel(Level.Sensitive)]
public static class Patch_PawnPath_DrawPath
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    LocalBuilder vehicle;
    Label label;
    return new CodeMatcher(instructions, generator).AddAltitudeFor(out vehicle, getInstance: new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(1, false)
    }).MatchEndForward(new CodeMatch[2]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3Shifted),
      CodeMatch.IsStloc((LocalBuilder) null)
    }).Repeat((Action<CodeMatcher>) (c => c.CreateLabel(ref label).Insert(new CodeInstruction[4]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord2)
    })), (Action<string>) null).InstructionEnumeration();
  }
}
