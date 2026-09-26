// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleHandlerReservation
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehicleHandlerReservation : Reservation<VehicleRoleHandler>
{
  private Dictionary<Pawn, VehicleRoleHandler> claimants;
  private Dictionary<VehicleRoleHandler, int> handlerClaimants;
  private List<VehicleRoleHandler> pawnClaimants = new List<VehicleRoleHandler>();
  private List<int> claimantCounts = new List<int>();
  private static readonly List<Pawn> removeActors = new List<Pawn>();

  public VehicleHandlerReservation()
  {
  }

  public VehicleHandlerReservation(VehiclePawn vehicle, Job job, int maxClaimants)
    : base(vehicle, job, maxClaimants)
  {
    this.claimants = new Dictionary<Pawn, VehicleRoleHandler>();
    this.handlerClaimants = new Dictionary<VehicleRoleHandler, int>();
  }

  public override int TotalClaimants => this.claimants.Count;

  public override bool RemoveNow => !this.claimants.Any<KeyValuePair<Pawn, VehicleRoleHandler>>();

  public int ClaimantsOnHandler(VehicleRoleHandler handler)
  {
    return this.claimants.Where<KeyValuePair<Pawn, VehicleRoleHandler>>((Func<KeyValuePair<Pawn, VehicleRoleHandler>, bool>) (c => c.Value == handler)).Count<KeyValuePair<Pawn, VehicleRoleHandler>>();
  }

  public VehicleRoleHandler ReservedHandler(Pawn pawn)
  {
    return GenCollection.TryGetValue<Pawn, VehicleRoleHandler>((IReadOnlyDictionary<Pawn, VehicleRoleHandler>) this.claimants, pawn, (VehicleRoleHandler) null);
  }

  public override bool AddClaimant(Pawn pawn, VehicleRoleHandler target)
  {
    if (this.claimants.ContainsKey(pawn))
    {
      Log.Error($"Attempting to reserve Vehicle with {((Entity) pawn).LabelShort}. Handler {target} is already reserved.");
      return false;
    }
    this.claimants[pawn] = target;
    if (this.handlerClaimants.ContainsKey(target))
      this.handlerClaimants[target]++;
    else
      this.handlerClaimants[target] = 1;
    return true;
  }

  public override bool CanReserve(
    Pawn pawn,
    VehicleRoleHandler target,
    StringBuilder stringBuilder = null)
  {
    int num = GenCollection.TryGetValue<VehicleRoleHandler, int>((IReadOnlyDictionary<VehicleRoleHandler, int>) this.handlerClaimants, target, 0);
    if (((ThingOwner) target.thingOwner).Count + num >= target.role.Slots)
    {
      stringBuilder?.AppendLine($"Roles not available.  Existing={((ThingOwner) target.thingOwner).Count} Claimants={string.Join<Pawn>(",", this.claimants.Where<KeyValuePair<Pawn, VehicleRoleHandler>>((Func<KeyValuePair<Pawn, VehicleRoleHandler>, bool>) (kvp => kvp.Value == target)).Select<KeyValuePair<Pawn, VehicleRoleHandler>, Pawn>((Func<KeyValuePair<Pawn, VehicleRoleHandler>, Pawn>) (kvp => kvp.Key)))} Allowed: {target.role.Slots}");
      return false;
    }
    if (pawn == null)
    {
      stringBuilder?.AppendLine("Null Claimant");
      return true;
    }
    bool flag = !this.claimants.ContainsKey(pawn);
    stringBuilder?.AppendLine($"{pawn} is new claimant? {flag}");
    return flag;
  }

  public override bool ReservedBy(Pawn pawn, VehicleRoleHandler target)
  {
    VehicleRoleHandler vehicleRoleHandler;
    return this.claimants.TryGetValue(pawn, out vehicleRoleHandler) && vehicleRoleHandler == target;
  }

  public override void ReleaseAllReservations()
  {
    foreach (Pawn key in this.claimants.Keys)
    {
      if (key?.jobs != null)
      {
        key.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
        key.ClearMind_NewTemp(false, false, true, false);
      }
    }
  }

  public override void ReleaseReservationBy(Pawn pawn)
  {
    if (!this.claimants.ContainsKey(pawn))
      return;
    if (--this.handlerClaimants[this.claimants[pawn]] <= 0)
      this.handlerClaimants.Remove(this.claimants[pawn]);
    this.claimants.Remove(pawn);
  }

  public override void VerifyAndValidateClaimants()
  {
    VehicleHandlerReservation.removeActors.Clear();
    foreach (Pawn key in this.claimants.Keys)
    {
      Job job = key.CurJob;
      if (key?.jobs != null && key.CurJob?.def != this.jobDef)
      {
        JobQueue jobQueue = key.jobs.jobQueue;
        job = jobQueue != null ? ((IEnumerable<QueuedJob>) jobQueue).FirstOrDefault<QueuedJob>((Func<QueuedJob, bool>) (j => j.job.def == this.jobDef))?.job : (Job) null;
      }
      if (((Thing) key).Spawned && !key.InMentalState && !key.Downed && !key.Dead && job?.def == this.jobDef)
      {
        LocalTargetInfo? targetA1 = job?.targetA;
        LocalTargetInfo targetA2 = this.targetA;
        if ((targetA1.HasValue ? (LocalTargetInfo.op_Inequality(targetA1.GetValueOrDefault(), targetA2) ? 1 : 0) : 1) == 0)
          continue;
      }
      if (--this.handlerClaimants[this.claimants[key]] <= 0)
        this.handlerClaimants.Remove(this.claimants[key]);
      VehicleHandlerReservation.removeActors.Add(key);
    }
    foreach (Pawn removeActor in VehicleHandlerReservation.removeActors)
      this.claimants.Remove(removeActor);
    VehicleHandlerReservation.removeActors.Clear();
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<Pawn, VehicleRoleHandler>(ref this.claimants, "claimants", (LookMode) 3, (LookMode) 3);
    Scribe_Collections.Look<VehicleRoleHandler, int>(ref this.handlerClaimants, "handlerClaimants", (LookMode) 3, (LookMode) 1, ref this.pawnClaimants, ref this.claimantCounts, true, false, false);
  }
}
