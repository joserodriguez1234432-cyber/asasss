// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_Jump_DrawHighlight
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Verb_Jump), "DrawHighlight")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Verb_Jump_DrawHighlight
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    instructions = (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_TargetMapOrThingMap);
    MethodInfo m_CenterVector3 = AccessTools.PropertyGetter(typeof (LocalTargetInfo), "CenterVector3");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_CenterVector3Offset = (Patch_Verb_Jump_DrawHighlight.\u003C\u003EO.\u003C0\u003E__CenterVector3Offset ?? (Patch_Verb_Jump_DrawHighlight.\u003C\u003EO.\u003C0\u003E__CenterVector3Offset = new \u003C\u003EF\u007B00000001\u007D<LocalTargetInfo, Verb, Vector3>(Patch_Verb_Jump_DrawHighlight.CenterVector3Offset))).Method;
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, m_CenterVector3))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return PatchHelper.get_CallInstruction(m_CenterVector3Offset);
      }
      else
        yield return instruction;
    }
  }

  public static Vector3 CenterVector3Offset(ref LocalTargetInfo target, Verb verb)
  {
    Thing caster = verb.caster;
    Thing thing = ((LocalTargetInfo) ref target).Thing;
    if (thing != null)
    {
      if (thing.Spawned)
        return thing.DrawPos;
      if (thing.SpawnedOrAnyParentSpawned)
      {
        Map map;
        if (!caster.TryGetTargetMap(out map))
        {
          IntVec3 positionHeld = thing.PositionHeld;
          return ((IntVec3) ref positionHeld).ToVector3Shifted();
        }
        IntVec3 positionHeld1 = thing.PositionHeld;
        return ((IntVec3) ref positionHeld1).ToVector3Shifted().ToBaseMapCoord(map);
      }
      Map map1;
      if (!caster.TryGetTargetMap(out map1))
      {
        IntVec3 position = thing.Position;
        return ((IntVec3) ref position).ToVector3Shifted();
      }
      IntVec3 position1 = thing.Position;
      return ((IntVec3) ref position1).ToVector3Shifted().ToBaseMapCoord(map1);
    }
    IntVec3 cell = ((LocalTargetInfo) ref target).Cell;
    if (!((IntVec3) ref cell).IsValid)
      return new Vector3();
    Map map2;
    return !caster.TryGetTargetMap(out map2) ? ((IntVec3) ref cell).ToVector3Shifted() : ((IntVec3) ref cell).ToVector3Shifted().ToBaseMapCoord(map2);
  }
}
