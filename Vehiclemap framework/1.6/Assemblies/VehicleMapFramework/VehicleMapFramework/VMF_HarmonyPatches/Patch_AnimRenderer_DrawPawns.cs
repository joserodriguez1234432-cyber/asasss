// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimRenderer_DrawPawns
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
public static class Patch_AnimRenderer_DrawPawns
{
  [PatchLevel(Level.Mandatory)]
  [HarmonyPatch]
  [HarmonyReversePatch]
  private static Vector3 GetWorldPositionOriginal(ref object instance, Vector3 vector)
  {
    throw new NotImplementedException();
  }

  public static Vector3 GetWorldPositionOffset(ref object instance, Vector3 vector)
  {
    Vector3 positionOriginal = Patch_AnimRenderer_DrawPawns.GetWorldPositionOriginal(ref instance, vector);
    VehiclePawnWithMap vehicle;
    return ModCompat.MeleeAnimation.AnimRenderer_Map.Invoke(instance).IsNonFocusedVehicleMapOf(out vehicle) && ModCompat.MeleeAnimation.AnimRenderer_cellData.Invoke(ModCompat.MeleeAnimation.AnimRenderer_Def.Invoke(instance)).Count > 0 ? Vector3Utility.WithY(positionOriginal.ToBaseMapCoord(vehicle), positionOriginal.y) : positionOriginal;
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(ModCompat.MeleeAnimation.m_GetWorldPosition, ModCompat.MeleeAnimation.m_GetWorldPositionOffset);
  }
}
