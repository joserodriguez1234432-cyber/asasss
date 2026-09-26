// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTargetReservation
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehicleTargetReservation : Reservation<LocalTargetInfo>
{
  private Dictionary<Pawn, LocalTargetInfo> claimants;
  private List<Pawn> pawnClaimants;
  private List<LocalTargetInfo> pawnTargets;

  public VehicleTargetReservation()
  {
  }

  public VehicleTargetReservation(VehiclePawn vehicle, Job job, int maxClaimants)
    : base(vehicle, job, maxClaimants)
  {
    this.claimants = new Dictionary<Pawn, LocalTargetInfo>();
  }

  public override int TotalClaimants => this.claimants.Count;

  public override bool RemoveNow => !this.claimants.Any<KeyValuePair<Pawn, LocalTargetInfo>>();

  public override bool AddClaimant(Pawn pawn, LocalTargetInfo target)
  {
    if (this.claimants.ContainsKey(pawn))
    {
      Log.Error($"Attempting to reserve Vehicle with {((Entity) pawn).LabelShort}. Target {target} is already reserved.");
      return false;
    }
    this.claimants[pawn] = target;
    return true;
  }

  public override bool CanReserve(Pawn pawn, LocalTargetInfo target, StringBuilder stringBuilder = null)
  {
    return this.claimants.ContainsKey(pawn) ? LocalTargetInfo.op_Equality(this.claimants[pawn], target) : !this.claimants.ContainsValue(target);
  }

  public override bool ReservedBy(Pawn pawn, LocalTargetInfo target)
  {
    LocalTargetInfo localTargetInfo;
    return this.claimants.TryGetValue(pawn, out localTargetInfo) && LocalTargetInfo.op_Equality(localTargetInfo, target);
  }

  public override void ReleaseAllReservations()
  {
    List<Pawn> list = this.claimants.Keys.ToList<Pawn>();
    for (int index = list.Count - 1; index >= 0; --index)
    {
      Pawn pawn = list[index];
      pawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
      pawn.ClearMind_NewTemp(false, false, true, false);
    }
  }

  public override void ReleaseReservationBy(Pawn pawn)
  {
    if (!this.claimants.ContainsKey(pawn))
      return;
    this.claimants.Remove(pawn);
  }

  public override void VerifyAndValidateClaimants()
  {
    foreach (Pawn key in new List<Pawn>((IEnumerable<Pawn>) this.claimants.Keys))
    {
      if (key != null && ((Thing) key).Spawned && !key.Dead && key.CurJob.def == this.jobDef && !LocalTargetInfo.op_Inequality(key.CurJob.targetA, this.targetA))
      {
        LocalTargetInfo claimant = this.claimants[key];
        if (((LocalTargetInfo) ref claimant).IsValid && !key.Drafted && !this.vehicle.Drafted)
          continue;
      }
      this.claimants.Remove(key);
    }
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<Pawn, LocalTargetInfo>(ref this.claimants, "claimants", (LookMode) 3, (LookMode) 5, ref this.pawnClaimants, ref this.pawnTargets, true, false, false);
  }
}
