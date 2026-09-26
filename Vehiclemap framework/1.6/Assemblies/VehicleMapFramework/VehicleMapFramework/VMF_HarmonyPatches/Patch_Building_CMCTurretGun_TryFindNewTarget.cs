// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_CMCTurretGun_TryFindNewTarget
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
public static class Patch_Building_CMCTurretGun_TryFindNewTarget
{
  private static readonly List<IAttackTarget> tmpList = new List<IAttackTarget>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(AccessTools.Field(typeof (Map), "attackTargetsCache"), false)
    }).RemoveInstruction().MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (AttackTargetsCache), "GetPotentialTargetsFor", (Type[]) null, (Type[]) null))
    }).Set(OpCodes.Call, (object) (Patch_Building_CMCTurretGun_TryFindNewTarget.\u003C\u003EO.\u003C0\u003E__GetPotentialTargetsForCrossMap ?? (Patch_Building_CMCTurretGun_TryFindNewTarget.\u003C\u003EO.\u003C0\u003E__GetPotentialTargetsForCrossMap = new Func<Map, IAttackTargetSearcher, List<IAttackTarget>>(Patch_Building_CMCTurretGun_TryFindNewTarget.GetPotentialTargetsForCrossMap))).Method).InstructionEnumeration();
  }

  private static List<IAttackTarget> GetPotentialTargetsForCrossMap(
    Map map,
    IAttackTargetSearcher attackTargetSearcher)
  {
    Patch_Building_CMCTurretGun_TryFindNewTarget.tmpList.Clear();
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(true))
      Patch_Building_CMCTurretGun_TryFindNewTarget.tmpList.AddRange((IEnumerable<IAttackTarget>) mapAndVehicleMap.attackTargetsCache.GetPotentialTargetsFor(attackTargetSearcher));
    return Patch_Building_CMCTurretGun_TryFindNewTarget.tmpList;
  }
}
