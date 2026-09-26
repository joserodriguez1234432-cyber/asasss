// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleGhostUtility_DrawGhostOverlays
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[VFVersionalPatch]
[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleGhostUtility), "DrawGhostOverlays")]
[PatchLevel(Level.Sensitive)]
public static class Patch_VehicleGhostUtility_DrawGhostOverlays
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_GenThing_TrueCenter2)
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.InsertAfter(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(6, false),
      PatchHelper.get_CallInstruction((Patch_VehicleGhostUtility_DrawGhostOverlays.\u003C\u003EO.\u003C0\u003E__ToTargetMapCoord ?? (Patch_VehicleGhostUtility_DrawGhostOverlays.\u003C\u003EO.\u003C0\u003E__ToTargetMapCoord = new Func<Vector3, Thing, Vector3>(Patch_VehicleGhostUtility_DrawGhostVehicleDef.ToTargetMapCoord))).Method)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
