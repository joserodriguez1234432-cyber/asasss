// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriverGetItemForVehicleBase
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public abstract class JobDriverGetItemForVehicleBase : JobDriverLoadVehicleBase
{
  private static readonly ObjectPool<JobDriverGetItemForVehicleBase.ThingDefSet> SetPool = new ObjectPool<JobDriverGetItemForVehicleBase.ThingDefSet>(5);
  private static readonly ObjectPool<JobDriverGetItemForVehicleBase.ThingDefCountSearch> SearchPool = new ObjectPool<JobDriverGetItemForVehicleBase.ThingDefCountSearch>(5);

  protected abstract string ListerTag { get; }

  protected VehiclePawn Vehicle => this.Carrier as VehiclePawn;

  protected abstract IEnumerable<ThingDefCountClass> ThingsToLoad { get; }

  protected override bool HasDuplicateOpportunity(Thing thing)
  {
    return this.ThingsToLoad.FirstOrDefault<ThingDefCountClass>((Func<ThingDefCountClass, bool>) (thingDefCount => thingDefCount.thingDef == thing.def)) != null;
  }

  protected override bool ShouldFailJob()
  {
    return !this.Map.GetCachedMapComponent<VehicleReservationManager>().VehicleListed(this.Vehicle, this.ListerTag);
  }

  protected override int CountLeftToTransfer()
  {
    return JobDriverGetItemForVehicleBase.CountLeftToPack(this.pawn, this.job.def, this.GetMatchingThing(this.ToHaul));
  }

  protected override Thing FindThingToHaul()
  {
    return JobDriverGetItemForVehicleBase.FindThingToPack(this.Vehicle, this.pawn, this.ThingsToLoad);
  }

  protected override bool IsUsableCarrier(Pawn carrier, bool allowColonists = true)
  {
    return !ThingUtility.DestroyedOrNull((Thing) carrier) && ((Thing) carrier).Spawned && ((Thing) carrier).Faction == ((Thing) this.pawn).Faction && !FireUtility.IsBurning((Thing) carrier) && carrier == this.Vehicle;
  }

  public static int CountLeftToPack(Pawn pawn, JobDef jobDef, ThingDefCountClass thingDefCount)
  {
    if (thingDefCount.count <= 0 || thingDefCount.thingDef == null)
      return 0;
    JobDriverGetItemForVehicleBase.ThingDefCountSearch jobSearcher;
    using (JobDriverGetItemForVehicleBase.SearchPool.GetTemporary(out jobSearcher))
    {
      jobSearcher.Init(jobDef, thingDefCount);
      int num1 = JobDriverLoadVehicleBase.Search.CountAlreadyBeingPacked(pawn, (ISharedJobSearch) jobSearcher);
      int num2 = 0;
      foreach (Thing thing in JobDriverLoadVehicleBase.UnpackedCaravanItems.Invoke(pawn.inventory))
        num2 += thing.def == thingDefCount.thingDef ? thing.stackCount : 0;
      return Mathf.Clamp(thingDefCount.count - num1 - num2, 0, int.MaxValue);
    }
  }

  public static Thing FindThingToPack(
    VehiclePawn vehicle,
    Pawn pawn,
    [CanBeNull] IEnumerable<ThingDefCountClass> thingDefCounts)
  {
    return JobDriverGetItemForVehicleBase.FindThingToPack(vehicle, pawn, pawn.CurJobDef, thingDefCounts);
  }

  public static Thing FindThingToPack(
    VehiclePawn vehicle,
    Pawn pawn,
    JobDef jobDef,
    [CanBeNull] IEnumerable<ThingDefCountClass> thingDefCounts)
  {
    if (thingDefCounts == null)
      return (Thing) null;
    JobDriverGetItemForVehicleBase.ThingDefSet thingDefSet;
    using (JobDriverGetItemForVehicleBase.SetPool.GetTemporary(out thingDefSet))
    {
      foreach (ThingDefCountClass thingDefCount in thingDefCounts)
      {
        if (JobDriverGetItemForVehicleBase.CountLeftToPack(pawn, jobDef, thingDefCount) > 0)
          thingDefSet.Add(thingDefCount.thingDef);
      }
      return thingDefSet.Count == 0 ? (Thing) null : JobDriverLoadVehicleBase.Search.FindNearestThing(pawn, new Predicate<Thing>(thingDefSet.IsValid));
    }
  }

  private ThingDefCountClass GetMatchingThing(Thing thing)
  {
    foreach (ThingDefCountClass matchingThing in this.ThingsToLoad)
    {
      if (matchingThing.thingDef == thing.def)
        return matchingThing;
    }
    return (ThingDefCountClass) null;
  }

  private sealed class ThingDefSet : IPoolable
  {
    private readonly HashSet<ThingDef> neededThingDefs = new HashSet<ThingDef>();

    public int Count => this.neededThingDefs.Count;

    bool IPoolable.InPool { get; set; }

    public void Add(ThingDef thingDef) => this.neededThingDefs.Add(thingDef);

    public bool IsValid(Thing thing) => this.neededThingDefs.Contains(thing.def);

    void IPoolable.Reset() => this.neededThingDefs.Clear();
  }

  protected sealed class ThingDefCountSearch : ISharedJobSearch, IPoolable
  {
    private JobDef jobDef;
    private ThingDefCountClass thingDefCount;

    bool IPoolable.InPool { get; set; }

    ThingDef ISharedJobSearch.ThingDef => this.thingDefCount.thingDef;

    public void Init(JobDef jobDef, ThingDefCountClass thingDefCount)
    {
      this.jobDef = jobDef;
      this.thingDefCount = thingDefCount;
    }

    bool ISharedJobSearch.IsMatchingThing(Thing thing) => thing.def == this.thingDefCount.thingDef;

    bool ISharedJobSearch.ShouldConsiderPawn(Pawn otherPawn) => otherPawn.CurJobDef == this.jobDef;

    void IPoolable.Reset()
    {
      this.jobDef = (JobDef) null;
      this.thingDefCount = (ThingDefCountClass) null;
    }
  }
}
