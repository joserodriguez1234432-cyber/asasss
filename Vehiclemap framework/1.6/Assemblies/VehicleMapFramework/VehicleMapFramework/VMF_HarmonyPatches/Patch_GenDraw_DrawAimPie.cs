// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenDraw_DrawAimPie
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenDraw), "DrawAimPie")]
public static class Patch_GenDraw_DrawAimPie
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(Thing shooter, ref LocalTargetInfo target)
  {
    Map map;
    if (((LocalTargetInfo) ref target).HasThing || !shooter.TryGetTargetMap(out map))
      return;
    target = LocalTargetInfo.op_Implicit(((LocalTargetInfo) ref target).Cell.ToBaseMapCoord(map));
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap), (MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMap));
  }
}
