// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_TurretGun_DrawExtraSelectionOverlays
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Building_TurretGun), "DrawExtraSelectionOverlays")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_TurretGun_DrawExtraSelectionOverlays
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_TargetCellOnBaseMap);
      }
      else
        yield return instruction;
    }
  }
}
