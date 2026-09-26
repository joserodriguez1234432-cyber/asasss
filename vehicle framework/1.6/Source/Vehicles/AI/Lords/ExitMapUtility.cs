// Decompiled with JetBrains decompiler
// Type: Vehicles.ExitMapUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public static class ExitMapUtility
{
  public static bool IsFollowingAnyone(Pawn pawn)
  {
    return ((LocalTargetInfo) ref pawn.mindState.duty.focus).HasThing;
  }

  public static void SetFollower(Pawn pawn, Pawn follower)
  {
    pawn.mindState.duty.focus = LocalTargetInfo.op_Implicit((Thing) follower);
    pawn.mindState.duty.radius = 10f;
  }

  public static void CheckArrived(
    Lord lord,
    List<Pawn> pawnsToCheck,
    IntVec3 meetingPoint,
    string memo,
    Predicate<Pawn> shouldCheckIfArrived,
    Predicate<Pawn> extraValidator = null)
  {
    VehiclePawn leadVehicle = ((LordJob_FormAndSendVehicles) lord.LordJob).LeadVehicle;
    foreach (Pawn pawn in pawnsToCheck)
    {
      if (shouldCheckIfArrived(pawn))
      {
        if (!((Thing) pawn).Spawned)
          return;
        if (pawn == leadVehicle)
        {
          IntVec3 position = ((Thing) leadVehicle).Position;
          if (!((IntVec3) ref position).InHorDistOf(meetingPoint, (float) ((BuildableDef) leadVehicle.VehicleDef).Size.z) || !leadVehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(meetingPoint), (PathEndMode) 3, (Danger) 3, (TraverseMode) 0))
            return;
        }
        else
        {
          Pawn_MindState mindState = pawn.mindState;
          Thing thing;
          if (mindState == null)
          {
            thing = (Thing) null;
          }
          else
          {
            PawnDuty duty = mindState.duty;
            thing = duty != null ? ((LocalTargetInfo) ref duty.focus).Thing : (Thing) null;
          }
          if (thing is VehiclePawn vehiclePawn)
          {
            IntVec3 position = ((Thing) pawn).Position;
            if (!((IntVec3) ref position).InHorDistOf(((Thing) vehiclePawn).Position, 15f))
              return;
          }
        }
        if (extraValidator != null && !extraValidator(pawn))
          return;
      }
    }
    lord.ReceiveMemo(memo);
  }
}
