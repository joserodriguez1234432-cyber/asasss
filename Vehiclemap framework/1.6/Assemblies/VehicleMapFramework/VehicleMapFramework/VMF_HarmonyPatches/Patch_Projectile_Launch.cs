// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Projectile_Launch
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Projectile), "Launch", new Type[] {typeof (Thing), typeof (Vector3), typeof (LocalTargetInfo), typeof (LocalTargetInfo), typeof (ProjectileHitFlags), typeof (bool), typeof (Thing), typeof (ThingDef)})]
[PatchLevel(Level.Cautious)]
public static class Patch_Projectile_Launch
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell)
    }).InsertAndAdvance(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      CodeInstruction.LoadArgument(7, false)
    }).SetInstruction(PatchHelper.get_CallInstruction((Patch_Projectile_Launch.\u003C\u003EO.\u003C0\u003E__TargetCell ?? (Patch_Projectile_Launch.\u003C\u003EO.\u003C0\u003E__TargetCell = new \u003C\u003EF\u007B00000001\u007D<LocalTargetInfo, Thing, Thing, IntVec3>(Patch_Projectile_Launch.TargetCell))).Method)).InstructionEnumeration();
  }

  private static IntVec3 TargetCell(ref LocalTargetInfo targ, Thing launcher, Thing equipment)
  {
    VehiclePawnWithMap vehicle;
    if (launcher.IsOnNonFocusedVehicleMapOf(out vehicle) && !((Thing) vehicle).Spawned)
      return ((LocalTargetInfo) ref targ).Cell;
    Thing thing = launcher;
    CompMannable compMannable;
    if (equipment != null && ThingCompUtility.TryGetComp<CompMannable>(equipment, ref compMannable) && compMannable.ManningPawn == launcher)
      thing = equipment;
    return targ.TargetCellOnBaseMap(thing);
  }
}
