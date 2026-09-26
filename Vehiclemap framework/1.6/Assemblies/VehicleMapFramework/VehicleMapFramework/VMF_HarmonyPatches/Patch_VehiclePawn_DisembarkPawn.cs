// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehiclePawn_DisembarkPawn
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Linq;
using Vehicles;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyAfter(new string[] {"VRF.VehicleRaidFramework"})]
[HarmonyPatch(typeof (VehiclePawn), "DisembarkPawn")]
[PatchLevel(Level.Safe)]
public static class Patch_VehiclePawn_DisembarkPawn
{
  public static bool Prefix(Pawn pawn, VehiclePawn __instance)
  {
    if (!(GenCollection.FirstOrDefault<VehicleRoleHandler>(__instance.handlers, (Predicate<VehicleRoleHandler>) (h => ((ThingOwner) h.thingOwner).Contains((Thing) pawn)))?.role is VehicleRoleBuildable role) || !(__instance is VehiclePawnWithMap vehiclePawnWithMap))
      return true;
    ThingWithComps parent = role.upgradeComp.parent;
    Map map = ((Thing) parent).Map ?? vehiclePawnWithMap.VehicleMap;
    __instance.RemovePawn(pawn);
    if (!((Thing) pawn).Spawned)
    {
      CellRect cellRect1 = GenAdj.OccupiedRect((Thing) parent);
      CellRect cellRect2 = ((CellRect) ref cellRect1).ExpandedBy(1);
      IntVec3 intVec3_1 = ((Thing) parent).Position;
      IntVec3 intVec3_2;
      if (GenCollection.TryRandomElement<IntVec3>(((CellRect) ref cellRect2).EdgeCells.Where<IntVec3>((Func<IntVec3, bool>) (c => GenGrid.InBounds(c, map) && GenGrid.Standable(c, map) && !Ext_IList.NotNullAndAny<Thing>(GridsUtility.GetThingList(c, map), (Predicate<Thing>) (t => t is Pawn)))), ref intVec3_2))
        intVec3_1 = intVec3_2;
      GenSpawn.Spawn((Thing) pawn, intVec3_1, map, (WipeMode) 0);
      if (!GenGrid.Standable(intVec3_1, map))
        pawn.pather.TryRecoverFromUnwalkablePosition(false);
      Lord lord = LordUtility.GetLord((Pawn) __instance);
      if (lord != null)
      {
        LordUtility.GetLord(pawn)?.Notify_PawnLost(pawn, (PawnLostCondition) 9, new DamageInfo?());
        lord.AddPawn(pawn);
      }
    }
    __instance.EventRegistry[VehicleEventDefOf.PawnExited].ExecuteEvents();
    return false;
  }
}
