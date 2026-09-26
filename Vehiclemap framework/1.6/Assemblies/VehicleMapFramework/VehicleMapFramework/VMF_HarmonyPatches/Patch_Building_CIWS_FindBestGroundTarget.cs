// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_CIWS_FindBestGroundTarget
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
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_IRBM")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Building_CIWS_FindBestGroundTarget
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).End().MatchEndBackwards(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Ret), (object) null, (string) null)
    }).Insert(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadLocal(0, false),
      PatchHelper.get_CallInstruction((Patch_Building_CIWS_FindBestGroundTarget.\u003C\u003EO.\u003C0\u003E__PostfixWithMaxRange ?? (Patch_Building_CIWS_FindBestGroundTarget.\u003C\u003EO.\u003C0\u003E__PostfixWithMaxRange = new Func<Thing, IAttackTargetSearcher, float, Thing>(Patch_Building_CIWS_FindBestGroundTarget.PostfixWithMaxRange))).Method)
    }).InstructionEnumeration();
  }

  private static Thing PostfixWithMaxRange(
    Thing thing,
    IAttackTargetSearcher instance,
    float groundMaxRange)
  {
    foreach (Map mapAndVehicleMap in instance.Thing.Map.BaseMapAndVehicleMaps(false))
    {
      List<IAttackTarget> potentialTargetsFor = mapAndVehicleMap.attackTargetsCache.GetPotentialTargetsFor(instance);
      float num1 = thing != null ? Vector3.Distance(instance.Thing.DrawPos, thing.DrawPos) : float.MaxValue;
      for (int index = 0; index < potentialTargetsFor.Count; ++index)
      {
        Thing thing1 = potentialTargetsFor[index].Thing;
        if (!thing1.Destroyed && thing1.Spawned && GenHostility.HostileTo(thing1, Faction.OfPlayer) && (!(thing1 is Pawn pawn) || !pawn.Downed && !pawn.Dead))
        {
          float num2 = Vector3.Distance(instance.Thing.DrawPos, thing1.DrawPos);
          if ((double) num2 <= (double) groundMaxRange && instance.Thing.CanSee(thing1) && (double) num2 < (double) num1)
          {
            num1 = num2;
            thing = thing1;
          }
        }
      }
    }
    return thing;
  }
}
