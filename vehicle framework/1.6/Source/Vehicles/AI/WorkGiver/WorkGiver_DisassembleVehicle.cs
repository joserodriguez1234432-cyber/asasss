// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_DisassembleVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class WorkGiver_DisassembleVehicle : WorkGiver_RemoveBuilding
{
  protected virtual JobDef RemoveBuildingJob => JobDefOf_Vehicles.DisassembleVehicle;

  protected virtual DesignationDef Designation => DesignationDefOf.Deconstruct;

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    return JobMaker.MakeJob(base.RemoveBuildingJob, LocalTargetInfo.op_Implicit(t));
  }

  public virtual bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
  {
    return thing is VehiclePawn vehiclePawn && vehiclePawn.DeconstructibleBy(((Thing) pawn).Faction) && ReservationUtility.CanReserve(pawn, LocalTargetInfo.op_Implicit((Thing) vehiclePawn), 1, -1, (ReservationLayerDef) null, false) && ((Thing) pawn).Map.designationManager.DesignationOn((Thing) vehiclePawn, base.Designation) != null;
  }
}
