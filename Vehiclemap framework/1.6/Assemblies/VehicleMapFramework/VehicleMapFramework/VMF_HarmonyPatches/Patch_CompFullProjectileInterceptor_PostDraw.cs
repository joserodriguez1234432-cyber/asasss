// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompFullProjectileInterceptor_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_CeleTechArsenal")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompFullProjectileInterceptor_PostDraw
{
  private static readonly HashSet<IAttackTarget> tmpSet = new HashSet<IAttackTarget>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(AccessTools.Field(typeof (Map), "attackTargetsCache"), false)
    }).RemoveInstruction().Set(OpCodes.Call, (object) (Patch_CompFullProjectileInterceptor_PostDraw.\u003C\u003EO.\u003C0\u003E__TargetsHostileToColonyCrossMap ?? (Patch_CompFullProjectileInterceptor_PostDraw.\u003C\u003EO.\u003C0\u003E__TargetsHostileToColonyCrossMap = new Func<Map, HashSet<IAttackTarget>>(Patch_CompFullProjectileInterceptor_PostDraw.TargetsHostileToColonyCrossMap))).Method).InstructionEnumeration();
  }

  private static HashSet<IAttackTarget> TargetsHostileToColonyCrossMap(Map map)
  {
    Patch_CompFullProjectileInterceptor_PostDraw.tmpSet.Clear();
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(true))
      GenCollection.AddRange<IAttackTarget>(Patch_CompFullProjectileInterceptor_PostDraw.tmpSet, mapAndVehicleMap.attackTargetsCache.TargetsHostileToColony);
    return Patch_CompFullProjectileInterceptor_PostDraw.tmpSet;
  }
}
