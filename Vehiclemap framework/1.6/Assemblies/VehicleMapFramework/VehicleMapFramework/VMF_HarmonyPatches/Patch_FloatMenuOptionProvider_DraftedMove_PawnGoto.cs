// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuOptionProvider_DraftedMove_PawnGotoAction
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (FloatMenuOptionProvider_DraftedMove), "PawnGotoAction")]
[PatchLevel(Level.Safe)]
public static class Patch_FloatMenuOptionProvider_DraftedMove_PawnGotoAction
{
  public static bool Prefix(IntVec3 clickCell, Pawn pawn, IntVec3 gotoLoc)
  {
    Map map;
    if (!((Thing) pawn).TryGetTargetMap(out map) || ((Thing) pawn).Map == map)
      return true;
    TargetInfo exitSpot;
    TargetInfo enterSpot;
    List<TraverseSpots> spotsQueue;
    if (pawn.CanReach(LocalTargetInfo.op_Implicit(gotoLoc), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, map, out exitSpot, out enterSpot, out spotsQueue))
      Patch_FloatMenuOptionProvider_DraftedMove_PawnGotoAction.PawnGotoAction(clickCell, pawn, map, exitSpot, enterSpot, spotsQueue, LocalTargetInfo.op_Implicit(gotoLoc));
    return false;
  }

  public static void PawnGotoAction(
    IntVec3 clickCell,
    Pawn pawn,
    Map map,
    TargetInfo exitSpot,
    TargetInfo enterSpot,
    List<TraverseSpots> spotsQueue,
    LocalTargetInfo dest)
  {
    Map map1 = map.BaseMap();
    bool flag;
    if (((Thing) pawn).Map == map && IntVec3.op_Equality(((Thing) pawn).Position, ((LocalTargetInfo) ref dest).Cell))
    {
      flag = true;
      if (pawn.CurJobDef == VMF_DefOf.VMF_GotoAcrossMaps)
        pawn.jobs.EndCurrentJob((JobCondition) 2, true, true);
    }
    else if (pawn.CurJobDef == VMF_DefOf.VMF_GotoAcrossMaps && ((Thing) pawn).Map == map && LocalTargetInfo.op_Equality(pawn.CurJob.targetA, dest))
    {
      flag = true;
    }
    else
    {
      Job jobAcrossMaps = JobMaker.MakeJob(VMF_DefOf.VMF_GotoAcrossMaps, dest).SetSpotsToJobAcrossMaps(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
      if (!map.IsVehicleMapOf(out VehiclePawnWithMap _) && map.exitMapGrid.IsExitCell(clickCell))
        jobAcrossMaps.exitMapOnArrival = !pawn.IsColonyMech;
      else if (!map1.IsPlayerHome && !map1.exitMapGrid.MapUsesExitGrid && ((Thing) pawn).Map == map1)
      {
        CellRect cellRect = CellRect.WholeMap(map1);
        if (((CellRect) ref cellRect).IsOnEdge(clickCell, 3) && ((WorldObject) map1.Parent).GetComponent<FormCaravanComp>() != null && MessagesRepeatAvoider.MessageShowAllowed("MessagePlayerTriedToLeaveMapViaExitGrid-" + map1.uniqueID.ToString(), 60f))
          Messages.Message(TaggedString.op_Implicit(((WorldObject) map1.Parent).GetComponent<FormCaravanComp>().CanFormOrReformCaravanNow ? Translator.Translate("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform") : Translator.Translate("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform")), LookTargets.op_Implicit((WorldObject) map1.Parent), MessageTypeDefOf.RejectInput, false);
      }
      flag = pawn.jobs.TryTakeOrderedJob(jobAcrossMaps, new JobTag?((JobTag) 0), false);
    }
    if (!flag)
      return;
    FleckMaker.Static(((LocalTargetInfo) ref dest).Cell, map, FleckDefOf.FeedbackGoto, 1f);
  }
}
