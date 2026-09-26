// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriverAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public abstract class JobDriverAcrossMaps : JobDriverBodyOffset
{
  private TargetInfo exitSpotA = TargetInfo.Invalid;
  private TargetInfo enterSpotA = TargetInfo.Invalid;
  private TargetInfo exitSpotB = TargetInfo.Invalid;
  private TargetInfo enterSpotB = TargetInfo.Invalid;
  private List<TraverseSpots> spotsQueueA;
  private List<TraverseSpots> spotsQueueB;
  private List<TraverseSpots> consumedSpots = new List<TraverseSpots>();
  private List<TraverseSpotsSaveLoader> spotsQueueA_SaveLoader;
  private List<TraverseSpotsSaveLoader> spotsQueueB_SaveLoader;
  private List<TraverseSpotsSaveLoader> consumedSpots_SaveLoader;
  private Map targetAMap;
  private Map destMap;

  public Map DestMap
  {
    get
    {
      if (this.destMap != null)
        return this.destMap;
      if (!GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) this.spotsQueueB))
      {
        TraverseSpots traverseSpots = this.spotsQueueB.Last<TraverseSpots>();
        if (((TargetInfo) ref traverseSpots.enterSpot).Map != null)
          return ((TargetInfo) ref traverseSpots.enterSpot).Map;
        if (((TargetInfo) ref traverseSpots.exitSpot).Map != null)
          return ((TargetInfo) ref traverseSpots.exitSpot).Map;
      }
      if (!GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) this.spotsQueueA))
      {
        TraverseSpots traverseSpots = this.spotsQueueA.Last<TraverseSpots>();
        if (((TargetInfo) ref traverseSpots.enterSpot).Map != null)
          return ((TargetInfo) ref traverseSpots.enterSpot).Map;
        if (((TargetInfo) ref traverseSpots.exitSpot).Map != null)
          return ((TargetInfo) ref traverseSpots.exitSpot).Map;
      }
      if (((TargetInfo) ref this.enterSpotB).Map != null)
        return ((TargetInfo) ref this.enterSpotB).Map;
      if (((TargetInfo) ref this.exitSpotB).Map != null)
        return JobDriverAcrossMaps.AccessSpotMapOrBaseMap(this.exitSpotB);
      if (((TargetInfo) ref this.enterSpotA).Map != null)
        return ((TargetInfo) ref this.enterSpotA).Map;
      return ((TargetInfo) ref this.exitSpotA).Map == null ? this.Map : JobDriverAcrossMaps.AccessSpotMapOrBaseMap(this.exitSpotA);
    }
  }

  public Map TargetAMap
  {
    get
    {
      if (this.targetAMap != null)
        return this.targetAMap;
      if (!GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) this.spotsQueueA))
      {
        TraverseSpots traverseSpots = this.spotsQueueA.Last<TraverseSpots>();
        if (((TargetInfo) ref traverseSpots.enterSpot).Map != null)
          return ((TargetInfo) ref traverseSpots.enterSpot).Map;
        if (((TargetInfo) ref traverseSpots.exitSpot).Map != null)
          return ((TargetInfo) ref traverseSpots.exitSpot).Map;
      }
      if (((TargetInfo) ref this.enterSpotA).Map != null)
        return ((TargetInfo) ref this.enterSpotA).Map;
      return ((TargetInfo) ref this.exitSpotA).Map == null ? this.Map : JobDriverAcrossMaps.AccessSpotMapOrBaseMap(this.exitSpotA);
    }
  }

  private static Map AccessSpotMapOrBaseMap(TargetInfo target)
  {
    Thing thing = ((TargetInfo) ref target).Thing;
    CompVehicleEnterSpot comp = thing != null ? ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(thing) : (CompVehicleEnterSpot) null;
    if (comp != null)
    {
      TargetInfo availableAccessSpot = comp.AvailableAccessSpot;
      if (((TargetInfo) ref availableAccessSpot).IsValid)
        return ((TargetInfo) ref availableAccessSpot).Map;
    }
    return VehicleMapUtility.get_GroundMap(((TargetInfo) ref target).Map);
  }

  public override Vector3 ForcedBodyOffset => this.drawOffset;

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    JobDriverAcrossMaps driverAcrossMaps = this;
    if (num != 0)
      return false;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated method
    ToilFailConditions.FailOn<JobDriverAcrossMaps>(driverAcrossMaps, new Func<bool>(driverAcrossMaps.\u003CMakeNewToils\u003Eb__19_0));
    return false;
  }

  public void SetSpots(
    TargetInfo? exitSpot1 = null,
    TargetInfo? enterSpot1 = null,
    TargetInfo? exitSpot2 = null,
    TargetInfo? enterSpot2 = null)
  {
    this.consumedSpots.Clear();
    TargetInfo? nullable = exitSpot1;
    this.exitSpotA = nullable ?? TargetInfo.Invalid;
    nullable = enterSpot1;
    this.enterSpotA = nullable ?? TargetInfo.Invalid;
    nullable = exitSpot2;
    this.exitSpotB = nullable ?? TargetInfo.Invalid;
    nullable = enterSpot2;
    this.enterSpotB = nullable ?? TargetInfo.Invalid;
    this.targetAMap = this.TargetAMap;
    this.destMap = this.DestMap;
    TargetInfo exitSpotA = this.exitSpotA;
    if (!((TargetInfo) ref exitSpotA).IsValid || ((TargetInfo) ref exitSpotA).Map != null)
    {
      TargetInfo targetInfo = this.enterSpotA;
      if (!((TargetInfo) ref targetInfo).IsValid || ((TargetInfo) ref targetInfo).Map != null)
      {
        targetInfo = this.exitSpotB;
        if (!((TargetInfo) ref targetInfo).IsValid || ((TargetInfo) ref targetInfo).Map != null)
        {
          targetInfo = this.enterSpotB;
          if (!((TargetInfo) ref targetInfo).IsValid || ((TargetInfo) ref targetInfo).Map != null)
            return;
        }
      }
    }
    VMF_Log.Error("SetSpots with null map.");
  }

  public void SetSpots(List<TraverseSpots> _spotsQueueA = null, List<TraverseSpots> _spotsQueueB = null)
  {
    this.consumedSpots.Clear();
    this.spotsQueueA = _spotsQueueA;
    this.spotsQueueB = _spotsQueueB;
    this.targetAMap = this.TargetAMap;
    this.destMap = this.DestMap;
    if ((this.spotsQueueA == null || !GenCollection.Any<TraverseSpots>(this.spotsQueueA, new Predicate<TraverseSpots>(MapAnyNull))) && (this.spotsQueueB == null || !GenCollection.Any<TraverseSpots>(this.spotsQueueB, new Predicate<TraverseSpots>(MapAnyNull))))
      return;
    VMF_Log.Error("SetSpots with null map.");

    static bool MapAnyNull(TraverseSpots spots)
    {
      TargetInfo exitSpot = spots.exitSpot;
      if (((TargetInfo) ref exitSpot).IsValid && ((TargetInfo) ref exitSpot).Map == null)
        return true;
      TargetInfo enterSpot = spots.enterSpot;
      return ((TargetInfo) ref enterSpot).IsValid && ((TargetInfo) ref enterSpot).Map == null;
    }
  }

  public void ConsumeSpots(TraverseSpots spots)
  {
    if (((TargetInfo) ref spots.exitSpot).Map == null && ((TargetInfo) ref spots.enterSpot).Map == null)
      return;
    this.consumedSpots.Add(spots);
  }

  public bool Consumed(TraverseSpots spots) => this.consumedSpots.Contains(spots);

  protected IEnumerable<Toil> GotoTargetMap(TargetIndex ind)
  {
    TargetIndex targetIndex = ind;
    return targetIndex == 1 ? (GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) this.spotsQueueA) ? ToilsAcrossMaps.GotoTargetMap(this, new TraverseSpots(this.exitSpotA, this.enterSpotA)) : this.spotsQueueA.SelectMany<TraverseSpots, Toil>((Func<TraverseSpots, IEnumerable<Toil>>) (s => ToilsAcrossMaps.GotoTargetMap(this, s)))) : (targetIndex == 2 ? (GenList.NullOrEmpty<TraverseSpots>((IList<TraverseSpots>) this.spotsQueueB) ? ToilsAcrossMaps.GotoTargetMap(this, new TraverseSpots(this.exitSpotB, this.enterSpotB)) : this.spotsQueueB.SelectMany<TraverseSpots, Toil>((Func<TraverseSpots, IEnumerable<Toil>>) (s => ToilsAcrossMaps.GotoTargetMap(this, s)))) : ((Func<IEnumerable<Toil>>) (() =>
    {
      VMF_Log.Error("GotoTargetMap() does not support TargetIndex.C.");
      return (IEnumerable<Toil>) Array.Empty<Toil>();
    }))());
  }

  public virtual void ExposeData()
  {
    Scribe_TargetInfo.Look(ref this.exitSpotA, "exitSpotA");
    Scribe_TargetInfo.Look(ref this.enterSpotA, "enterSpotA");
    Scribe_TargetInfo.Look(ref this.exitSpotB, "exitSpotB");
    Scribe_TargetInfo.Look(ref this.enterSpotB, "enterSpotB");
    Scribe_Values.Look<Vector3>(ref this.drawOffset, "drawOffset", new Vector3(), false);
    Scribe_References.Look<Map>(ref this.targetAMap, "targetAMap", false);
    Scribe_References.Look<Map>(ref this.destMap, "destMap", false);
    int num = Scribe.mode == 1 ? 1 : 0;
    if (num != 0)
    {
      List<TraverseSpots> spotsQueueA = this.spotsQueueA;
      this.spotsQueueA_SaveLoader = spotsQueueA != null ? spotsQueueA.Select<TraverseSpots, TraverseSpotsSaveLoader>((Func<TraverseSpots, TraverseSpotsSaveLoader>) (spots => new TraverseSpotsSaveLoader(spots))).ToList<TraverseSpotsSaveLoader>() : (List<TraverseSpotsSaveLoader>) null;
      List<TraverseSpots> spotsQueueB = this.spotsQueueB;
      this.spotsQueueB_SaveLoader = spotsQueueB != null ? spotsQueueB.Select<TraverseSpots, TraverseSpotsSaveLoader>((Func<TraverseSpots, TraverseSpotsSaveLoader>) (spots => new TraverseSpotsSaveLoader(spots))).ToList<TraverseSpotsSaveLoader>() : (List<TraverseSpotsSaveLoader>) null;
      List<TraverseSpots> consumedSpots = this.consumedSpots;
      this.consumedSpots_SaveLoader = consumedSpots != null ? consumedSpots.Select<TraverseSpots, TraverseSpotsSaveLoader>((Func<TraverseSpots, TraverseSpotsSaveLoader>) (spots => new TraverseSpotsSaveLoader(spots))).ToList<TraverseSpotsSaveLoader>() : (List<TraverseSpotsSaveLoader>) null;
    }
    Scribe_Collections.Look<TraverseSpotsSaveLoader>(ref this.spotsQueueA_SaveLoader, "spotsQueueA", (LookMode) 2, Array.Empty<object>());
    Scribe_Collections.Look<TraverseSpotsSaveLoader>(ref this.spotsQueueB_SaveLoader, "spotsQueueB", (LookMode) 2, Array.Empty<object>());
    Scribe_Collections.Look<TraverseSpotsSaveLoader>(ref this.consumedSpots_SaveLoader, "consumedSpots", (LookMode) 2, Array.Empty<object>());
    bool flag = Scribe.mode == 4;
    if (flag)
    {
      List<TraverseSpotsSaveLoader> queueASaveLoader = this.spotsQueueA_SaveLoader;
      this.spotsQueueA = queueASaveLoader != null ? queueASaveLoader.Select<TraverseSpotsSaveLoader, TraverseSpots>((Func<TraverseSpotsSaveLoader, TraverseSpots>) (loader => loader.spots)).ToList<TraverseSpots>() : (List<TraverseSpots>) null;
      List<TraverseSpotsSaveLoader> queueBSaveLoader = this.spotsQueueB_SaveLoader;
      this.spotsQueueB = queueBSaveLoader != null ? queueBSaveLoader.Select<TraverseSpotsSaveLoader, TraverseSpots>((Func<TraverseSpotsSaveLoader, TraverseSpots>) (loader => loader.spots)).ToList<TraverseSpots>() : (List<TraverseSpots>) null;
      List<TraverseSpotsSaveLoader> consumedSpotsSaveLoader = this.consumedSpots_SaveLoader;
      this.consumedSpots = consumedSpotsSaveLoader != null ? consumedSpotsSaveLoader.Select<TraverseSpotsSaveLoader, TraverseSpots>((Func<TraverseSpotsSaveLoader, TraverseSpots>) (loader => loader.spots)).ToList<TraverseSpots>() : (List<TraverseSpots>) null;
    }
    if ((num | (flag ? 1 : 0)) != 0)
    {
      this.spotsQueueA_SaveLoader = (List<TraverseSpotsSaveLoader>) null;
      this.spotsQueueB_SaveLoader = (List<TraverseSpotsSaveLoader>) null;
      this.consumedSpots_SaveLoader = (List<TraverseSpotsSaveLoader>) null;
    }
    base.ExposeData();
  }
}
