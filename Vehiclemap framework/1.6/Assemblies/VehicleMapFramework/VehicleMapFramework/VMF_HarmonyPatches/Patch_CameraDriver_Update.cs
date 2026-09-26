// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CameraDriver_Update
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CameraDriver), "Update")]
[PatchLevel(Level.Sensitive)]
public static class Patch_CameraDriver_Update
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    LocalBuilder localBuilder;
    Label label1;
    LocalBuilder vehicle;
    Label label2;
    return new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsConstant(-2.0)
    }).DeclareLocal(typeof (VehiclePawnWithMap), ref vehicle).DeclareLocal(typeof (bool), ref localBuilder).CreateLabel(ref label1).Insert(new CodeInstruction[7]
    {
      CodeInstruction.LoadField(typeof (VehicleMapFramework.VehicleMapFramework), "settings", false),
      CodeInstruction.LoadField(typeof (VehicleMapSettings), "drawPlanet", false),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.g_Find_CurrentMap),
      new CodeInstruction(OpCodes.Ldloca_S, (object) vehicle),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsVehicleMapOf),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder)
    }).Repeat((Action<CodeMatcher>) (c => c.CreateLabel(ref label2).InsertAndAdvance(new CodeInstruction[4]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label2),
      new CodeInstruction(OpCodes.Pop, (object) null),
      new CodeInstruction(OpCodes.Ldc_R4, (object) 200f)
    }).Advance(1)), (Action<string>) null).InstructionEnumeration();
  }
}
