// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TurretTop_TurretTopTick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (TurretTop), "TurretTopTick")]
[PatchLevel(Level.Sensitive)]
public static class Patch_TurretTop_TurretTopTick
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return CodeInstruction.LoadField(typeof (TurretTop), "parentTurret", false);
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_TargetCellOnBaseMap);
      }
      else
        yield return instruction;
    }
  }
}
