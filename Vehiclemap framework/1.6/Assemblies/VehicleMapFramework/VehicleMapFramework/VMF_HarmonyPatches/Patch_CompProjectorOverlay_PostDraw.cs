// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompProjectorOverlay_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_EccentricTech_DefenseGrid")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompProjectorOverlay_PostDraw
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    FieldInfo fieldInfo = AccessTools.Field(typeof (Vector3), "y");
    LocalBuilder vehicle;
    Label label;
    return new CodeMatcher(instructions, generator).AddAltitudeFor(out vehicle, 0.109756105f, getInstance: new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (ThingComp), "parent", false)
    }).Advance(1).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.StoresField(fieldInfo)
    }).Repeat((Action<CodeMatcher>) (matcher => matcher.CreateLabel(ref label).InsertAndAdvance(new CodeInstruction[6]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_YOffsetFull),
      new CodeInstruction(OpCodes.Ldc_R4, (object) 0.109756105f),
      new CodeInstruction(OpCodes.Add, (object) null)
    }).Advance(1)), (Action<string>) null).Reset(true).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Quaternion_identity)
    }).Advance(1).AddExtraAngle(vehicle).InstructionEnumeration();
  }
}
