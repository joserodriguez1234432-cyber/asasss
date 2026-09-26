// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuOptionProvider_OrderVehicle_PawnGotoAction
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (FloatMenuOptionProvider_OrderVehicle), "PawnGotoAction")]
[PatchLevel(Level.Safe)]
public static class Patch_FloatMenuOptionProvider_OrderVehicle_PawnGotoAction
{
  public static bool Prefix(IntVec3 clickCell, VehiclePawn vehicle, IntVec3 gotoLoc, ref Rot8 rot)
  {
    Map map;
    if (((Thing) vehicle).TryGetTargetMap(out map))
    {
      VehiclePawnWithMap vehicle1;
      if (map.IsVehicleMapOf(out vehicle1) && ((Rot8) ref rot).IsValid)
      {
        ref Rot8 local = ref rot;
        int asIntClockwise1 = ((Rot8) ref rot).AsIntClockwise;
        Rot8 fullRotation = vehicle1.FullRotation;
        int asIntClockwise2 = ((Rot8) ref fullRotation).AsIntClockwise;
        Rot8 rot8 = new Rot8(Rot8.FromIntClockwise(GenMath.PositiveMod(asIntClockwise1 - asIntClockwise2, 8)));
        local = rot8;
      }
      if (((Thing) vehicle).Map != map)
      {
        TargetInfo exitSpot;
        TargetInfo enterSpot;
        if (vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(gotoLoc), (PathEndMode) 1, (Danger) 3, (TraverseMode) 0, map, out exitSpot, out enterSpot))
        {
          Patch_FloatMenuOptionProvider_OrderVehicle_PawnGotoAction.PawnGotoAction(clickCell, vehicle, map, gotoLoc, rot, exitSpot, enterSpot);
          ((Thing) vehicle).RemoveTargetInfo();
        }
        return false;
      }
    }
    return true;
  }

  public static void PawnGotoAction(
    IntVec3 clickCell,
    VehiclePawn vehicle,
    Map map,
    IntVec3 gotoLoc,
    Rot8 rot,
    TargetInfo exitSpot,
    TargetInfo enterSpot)
  {
    bool flag1;
    if (((Thing) vehicle).Map == map && IntVec3.op_Equality(((Thing) vehicle).Position, gotoLoc))
    {
      flag1 = true;
      vehicle.FullRotation = rot;
      if (((Pawn) vehicle).CurJobDef == VMF_DefOf.VMF_GotoAcrossMaps)
        ((Pawn) vehicle).jobs.EndCurrentJob((JobCondition) 2, true, true);
    }
    else if (((Pawn) vehicle).CurJobDef == VMF_DefOf.VMF_GotoAcrossMaps && ((Pawn) vehicle).jobs?.curDriver is JobDriverAcrossMaps curDriver && curDriver.DestMap == map && IntVec3.op_Equality(((LocalTargetInfo) ref ((Pawn) vehicle).CurJob.targetA).Cell, gotoLoc))
    {
      flag1 = true;
    }
    else
    {
      Job job = JobMaker.MakeJob(VMF_DefOf.VMF_GotoAcrossMaps, LocalTargetInfo.op_Implicit(gotoLoc));
      job.SetSpotsToJobAcrossMaps((Pawn) vehicle, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot));
      job.globalTarget = new GlobalTargetInfo(gotoLoc, map, false);
      Map baseMap = map.BaseMap();
      int num1 = map == baseMap ? 1 : 0;
      int num2;
      if (num1 != 0)
      {
        CellRect cellRect = CellRect.WholeMap(baseMap);
        num2 = ((CellRect) ref cellRect).IsOnEdge(clickCell, 3) ? 1 : 0;
      }
      else
        num2 = 0;
      bool flag2 = num2 != 0;
      bool flag3 = (num1 != 0 && baseMap.exitMapGrid.IsExitCell(clickCell)) | (num1 != 0 && Ext_IEnumerable.NotNullAndAny<IntVec3>(vehicle.InhabitedCellsProjected(clickCell, rot, 0), (Predicate<IntVec3>) (cell => GenGrid.InBounds(cell, baseMap) && baseMap.exitMapGrid.IsExitCell(cell))));
      job.exitMapOnArrival = flag3;
      if (((flag3 || baseMap.IsPlayerHome ? 0 : (!baseMap.exitMapGrid.MapUsesExitGrid ? 1 : 0)) & (flag2 ? 1 : 0)) != 0)
      {
        FormCaravanComp component = ((WorldObject) baseMap.Parent).GetComponent<FormCaravanComp>();
        if (component != null && MessagesRepeatAvoider.MessageShowAllowed($"MessagePlayerTriedToLeaveMapViaExitGrid-{baseMap.uniqueID}", 60f))
          Messages.Message(TaggedString.op_Implicit(component.CanFormOrReformCaravanNow ? Translator.Translate("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform") : Translator.Translate("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform")), LookTargets.op_Implicit((WorldObject) baseMap.Parent), MessageTypeDefOf.RejectInput, false);
      }
      Pawn_JobTracker jobs = ((Pawn) vehicle).jobs;
      flag1 = jobs != null && jobs.TryTakeOrderedJob(job, new JobTag?((JobTag) 0), false);
      if (flag1)
        vehicle.vehiclePather.SetEndRotation(rot);
    }
    if (!flag1)
      return;
    FleckMaker.Static(gotoLoc, map, FleckDefOf.FeedbackGoto, 1f);
  }
}
