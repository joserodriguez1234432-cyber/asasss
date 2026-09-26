// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompPowerPlantWind_RecalculateBlockages
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CompPowerPlantWind), "RecalculateBlockages")]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompPowerPlantWind_RecalculateBlockages
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_0));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[4]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (CompPowerPlantWind), "parent", false),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing),
      PatchHelper.get_CallInstruction((Patch_CompPowerPlantWind_RecalculateBlockages.\u003C\u003EO.\u003C0\u003E__Restrict ?? (Patch_CompPowerPlantWind_RecalculateBlockages.\u003C\u003EO.\u003C0\u003E__Restrict = new Func<IEnumerable<IntVec3>, Map, IEnumerable<IntVec3>>(Patch_CompPowerPlantWind_RecalculateBlockages.Restrict))).Method)
    }));
    return (IEnumerable<CodeInstruction>) list.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseRotationSpawned), (MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing));
  }

  private static IEnumerable<IntVec3> Restrict(IEnumerable<IntVec3> enumerable, Map map)
  {
    return enumerable.Where<IntVec3>((Func<IntVec3, bool>) (c => GenGrid.InBounds(c, map)));
  }
}
