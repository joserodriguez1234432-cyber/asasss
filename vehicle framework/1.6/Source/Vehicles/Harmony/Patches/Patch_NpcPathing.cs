// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_NpcPathing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_NpcPathing : IPatchCategory
{
  private static readonly System.Type JobDriverGotoDisplayClassType = ((IEnumerable<System.Type>) typeof (JobDriver_Goto).GetNestedTypes(AccessTools.all)).FirstOrDefault<System.Type>((Func<System.Type, bool>) (type => type.Name == "<>c__DisplayClass1_0"));

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
  }

  private static void GotoToilsFirstExit(JobDriver_Goto __instance)
  {
    Patch_NpcPathing.TryExitMapForVehicle(__instance, false, true);
  }

  private static void GotoToilsSecondExit(JobDriver_Goto __instance)
  {
    Patch_NpcPathing.TryExitMapForVehicle(__instance, true, true);
  }

  private static void TryExitMapForVehicle(JobDriver_Goto __instance, bool onEdge, bool onExitCell)
  {
    JobDriver_Goto jobDriverGoto = Traverse.Create((object) __instance).Field("<>4__this").GetValue<JobDriver_Goto>();
    VehiclePawn vehicle = ((JobDriver) jobDriverGoto).pawn as VehiclePawn;
    if (vehicle == null || !((JobDriver) jobDriverGoto).job.exitMapOnArrival || !((Thing) vehicle).Spawned)
      return;
    CellRect cellRect1 = CellRect.WholeMap(((Thing) vehicle).Map);
    Rot4 closestEdge = ((CellRect) ref cellRect1).GetClosestEdge(((Thing) vehicle).Position);
    CellRect cellRect2 = vehicle.PawnOccupiedCells(((Thing) vehicle).Position, closestEdge);
    if (!((CellRect) ref cellRect2).Corners.Any<IntVec3>((Func<IntVec3, bool>) (cell =>
    {
      if (onEdge && GenGrid.OnEdge(cell, ((Thing) vehicle).Map))
        return true;
      return onExitCell && ((Thing) vehicle).Map.exitMapGrid.IsExitCell(cell);
    })))
      return;
    PathingHelper.ExitMapForVehicle(vehicle, ((JobDriver) jobDriverGoto).job);
  }
}
