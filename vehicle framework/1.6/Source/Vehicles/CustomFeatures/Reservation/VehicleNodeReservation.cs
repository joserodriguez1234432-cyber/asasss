// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleNodeReservation
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class VehicleNodeReservation : Reservation<ThingDefCountClass>
{
  private readonly ObjectPool<ThingDefCountList> countListPool = new ObjectPool<ThingDefCountList>(10);
  private Dictionary<Pawn, ThingDefCountClass> claimants;

  public VehicleNodeReservation()
  {
  }

  public VehicleNodeReservation(VehiclePawn vehicle, Job job, int maxClaimants)
    : base(vehicle, job, maxClaimants)
  {
    this.claimants = new Dictionary<Pawn, ThingDefCountClass>();
  }

  public override int TotalClaimants => this.claimants.Count;

  public override bool RemoveNow => !this.claimants.Any<KeyValuePair<Pawn, ThingDefCountClass>>();

  private bool AnyMissingIngredients
  {
    get
    {
      foreach (ThingDefCountClass thingDefCountClass1 in this.vehicle.CompUpgradeTree.NodeUnlocking.MaterialsRequired(this.Vehicle))
      {
        int num = 0;
        foreach (ThingDefCountClass thingDefCountClass2 in this.claimants.Values)
        {
          if (thingDefCountClass2.thingDef == thingDefCountClass1.thingDef)
            num += thingDefCountClass2.count;
        }
        if (num < thingDefCountClass1.count)
          return true;
      }
      return false;
    }
  }

  public override bool AddClaimant(Pawn pawn, ThingDefCountClass target)
  {
    if (this.claimants.TryAdd(pawn, target))
      return true;
    Trace.Fail($"Attempting to reserve Vehicle with {((Entity) pawn).LabelShort}. Target {target} is already reserved.");
    return false;
  }

  public override bool CanReserve(
    Pawn pawn,
    ThingDefCountClass target,
    StringBuilder stringBuilder = null)
  {
    return !this.claimants.ContainsKey(pawn) && this.claimants.Count < this.maxClaimants && this.vehicle.CompUpgradeTree.Upgrading && this.AnyMissingIngredients;
  }

  public override bool ReservedBy(Pawn pawn, ThingDefCountClass target)
  {
    ThingDefCountClass thingDefCountClass;
    return this.claimants.TryGetValue(pawn, out thingDefCountClass) && thingDefCountClass == target;
  }

  public override void ReleaseAllReservations()
  {
    foreach (Pawn key in this.claimants.Keys)
    {
      key.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
      key.ClearMind_NewTemp(false, false, true, false);
    }
  }

  public override void ReleaseReservationBy(Pawn pawn) => this.claimants.Remove(pawn);

  public override void VerifyAndValidateClaimants()
  {
    foreach (Pawn key in this.claimants.Keys.ToList<Pawn>())
    {
      if (key.CurJob.def != this.jobDef || key.Drafted || this.vehicle.Drafted)
        this.claimants.Remove(key);
    }
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<Pawn, ThingDefCountClass>(ref this.claimants, "claimants", (LookMode) 3, (LookMode) 3);
  }
}
