// Decompiled with JetBrains decompiler
// Type: Vehicles.ReservationBase
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public abstract class ReservationBase : IExposable
{
  protected VehiclePawn vehicle;
  protected JobDef jobDef;
  protected LocalTargetInfo targetA;
  protected int maxClaimants;
  private int uniqueId = -1;

  public ReservationBase()
  {
  }

  public ReservationBase(VehiclePawn vehicle, Job job, int maxClaimants)
  {
    this.vehicle = vehicle;
    this.jobDef = job.def;
    this.targetA = job.targetA;
    this.maxClaimants = maxClaimants;
    this.uniqueId = VehicleIdManager.Instance.GetNextReservationId();
  }

  public abstract bool RemoveNow { get; }

  public abstract int TotalClaimants { get; }

  public JobDef JobDef => this.jobDef;

  public VehiclePawn Vehicle => this.vehicle;

  public LocalTargetInfo TargetA => this.targetA;

  public abstract void ReleaseReservationBy(Pawn pawn);

  public abstract void VerifyAndValidateClaimants();

  public abstract void ReleaseAllReservations();

  public override string ToString() => $"{this.GetType()} : {((Entity) this.vehicle).LabelShort}";

  public virtual void ExposeData()
  {
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
    Scribe_TargetInfo.Look(ref this.targetA, "targetA");
    Scribe_Defs.Look<JobDef>(ref this.jobDef, "jobDef");
    Scribe_Values.Look<int>(ref this.maxClaimants, "maxClaimants", 0, false);
  }
}
