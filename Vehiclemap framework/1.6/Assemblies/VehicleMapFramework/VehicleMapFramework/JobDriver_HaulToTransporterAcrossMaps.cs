// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriver_HaulToTransporterAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

[Obsolete]
public class JobDriver_HaulToTransporterAcrossMaps : JobDriver_HaulToContainer
{
  public int initialCount;

  public CompTransporter Transporter
  {
    get
    {
      Thing container = this.Container;
      return container == null ? (CompTransporter) null : ThingCompUtility.TryGetComp<CompTransporter>(container);
    }
  }

  public virtual void ExposeData()
  {
    ((JobDriver) this).ExposeData();
    Scribe_Values.Look<int>(ref this.initialCount, "initialCount", 0, false);
  }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    ReservationUtility.ReserveAsManyAsPossible(((JobDriver) this).pawn, ((JobDriver) this).job.GetTargetQueue((TargetIndex) 1), ((JobDriver) this).job, 1, -1, (ReservationLayerDef) null);
    ReservationUtility.ReserveAsManyAsPossible(((JobDriver) this).pawn, ((JobDriver) this).job.GetTargetQueue((TargetIndex) 2), ((JobDriver) this).job, 1, -1, (ReservationLayerDef) null);
    return true;
  }

  public virtual void Notify_Starting()
  {
    ((JobDriver) this).Notify_Starting();
    ThingCount thingToLoad;
    if (((LocalTargetInfo) ref ((JobDriver) this).job.targetA).IsValid)
    {
      // ISSUE: explicit constructor call
      ((ThingCount) ref thingToLoad).\u002Ector(((LocalTargetInfo) ref ((JobDriver) this).job.targetA).Thing, ((LocalTargetInfo) ref ((JobDriver) this).job.targetA).Thing.stackCount, false);
    }
    else
    {
      CompTransporter comp = ThingCompUtility.TryGetComp<CompTransporter>(this.Container);
      bool gatherFromBaseMap = !(comp is CompBuildableContainer buildableContainer) || buildableContainer.GatherFromBaseMap;
      thingToLoad = LoadTransportersJobOnVehicleUtility.FindThingToLoad(((JobDriver) this).pawn, comp, gatherFromBaseMap);
    }
    if (((JobDriver) this).job.playerForced && ((JobDriver) this).pawn.carryTracker.CarriedThing != null && ((JobDriver) this).pawn.carryTracker.CarriedThing != ((ThingCount) ref thingToLoad).Thing)
    {
      Thing thing;
      ((JobDriver) this).pawn.carryTracker.TryDropCarriedThing(((Thing) ((JobDriver) this).pawn).Position, (ThingPlaceMode) 1, ref thing, (Action<Thing, int>) null);
    }
    ((JobDriver) this).job.targetA = LocalTargetInfo.op_Implicit(((ThingCount) ref thingToLoad).Thing);
    ((JobDriver) this).job.count = ((ThingCount) ref thingToLoad).Count;
    this.initialCount = ((ThingCount) ref thingToLoad).Count;
    ((JobDriver) this).pawn.Reserve(((ThingCount) ref thingToLoad).Thing.MapHeld, LocalTargetInfo.op_Implicit(((ThingCount) ref thingToLoad).Thing), ((JobDriver) this).job);
  }
}
