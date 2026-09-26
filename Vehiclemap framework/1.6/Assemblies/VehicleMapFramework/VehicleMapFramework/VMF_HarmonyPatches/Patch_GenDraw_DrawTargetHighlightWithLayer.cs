// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenDraw_DrawTargetHighlightWithLayer
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenDraw), "DrawTargetHighlightWithLayer")]
public static class Patch_GenDraw_DrawTargetHighlightWithLayer
{
  [PatchLevel(Level.Sensitive)]
  [HarmonyPatch(new Type[] {typeof (IntVec3), typeof (AltitudeLayer), typeof (Material)})]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    list.Insert(list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_0)), new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord1));
    return (IEnumerable<CodeInstruction>) list;
  }
}
