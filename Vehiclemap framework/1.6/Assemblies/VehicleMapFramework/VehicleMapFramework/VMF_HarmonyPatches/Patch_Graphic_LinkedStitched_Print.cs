// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_LinkedStitched_Print
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_NightmareCore")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Graphic_LinkedStitched_Print
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_RotationForPrint).ToList<CodeInstruction>();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    list.Insert(list.FindLastIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, AccessTools.Method(typeof (Vector3), "op_Addition", (Type[]) null, (Type[]) null)))), PatchHelper.get_CallInstruction((Patch_Graphic_LinkedStitched_Print.\u003C\u003EO.\u003C0\u003E__RotateVector ?? (Patch_Graphic_LinkedStitched_Print.\u003C\u003EO.\u003C0\u003E__RotateVector = new Func<Vector3, Vector3>(Patch_Graphic_LinkedStitched_Print.RotateVector))).Method));
    return (IEnumerable<CodeInstruction>) list;
  }

  private static Vector3 RotateVector(Vector3 vector)
  {
    return Vector3Utility.RotatedBy(vector, VehicleSectionLayerManager.RotForPrintCounter);
  }
}
