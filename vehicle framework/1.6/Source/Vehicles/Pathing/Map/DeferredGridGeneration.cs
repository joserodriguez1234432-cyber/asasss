// Decompiled with JetBrains decompiler
// Type: Vehicles.DeferredGridGeneration
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class DeferredGridGeneration
{
  private const int DaysUnusedForRemoval = 3;
  private readonly VehiclePathingSystem mapping;
  private readonly DeferredGridGeneration.GridCounter pathGridCounter = new DeferredGridGeneration.GridCounter();

  private bool PassDisabled { get; set; }

  public DeferredGridGeneration(VehiclePathingSystem mapping) => this.mapping = mapping;

  private bool GridGenIsValid() => !this.mapping.map.Disposed;

  internal void GenerateAllPathGrids()
  {
    bool threadAvailable = this.mapping.ThreadAvailable;
    AsyncLongOperationAction longOperation = threadAvailable ? AsyncPool<AsyncLongOperationAction>.Get() : (AsyncLongOperationAction) null;
    bool flag = false;
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      if (threadAvailable)
        flag |= this.TryAddPathGridRequest(vehicleDef, longOperation);
      else
        this.GeneratePathGridFor(vehicleDef);
    }
    if (!threadAvailable)
      return;
    if (flag)
      this.FinalizeAndSendLongOp(longOperation);
    else
      longOperation.ReturnToPool();
  }

  internal void GenerateAllRegionGrids()
  {
    bool threadAvailable = this.mapping.ThreadAvailable;
    AsyncLongOperationAction longOperation = threadAvailable ? AsyncPool<AsyncLongOperationAction>.Get() : (AsyncLongOperationAction) null;
    bool flag = false;
    foreach (VehicleDef allOwner in this.mapping.GridOwners.AllOwners)
    {
      if (threadAvailable)
        flag |= this.TryAddRegionGridRequest(allOwner, longOperation);
      else
        this.GenerateRegionGridFor(allOwner);
    }
    if (!threadAvailable)
      return;
    if (flag)
      this.FinalizeAndSendLongOp(longOperation);
    else
      longOperation.ReturnToPool();
  }

  public void RequestGridsFor(VehicleDef vehicleDef, DeferredGridGeneration.Urgency urgency)
  {
    if (urgency == DeferredGridGeneration.Urgency.None)
      return;
    if (this.mapping.ThreadAvailable && urgency == DeferredGridGeneration.Urgency.Deferred)
    {
      AsyncLongOperationAction longOperation = AsyncPool<AsyncLongOperationAction>.Get();
      if (this.TryAddPathGridRequest(vehicleDef, longOperation) | this.TryAddRegionGridRequest(vehicleDef, longOperation))
        this.FinalizeAndSendLongOp(longOperation);
      else
        longOperation.ReturnToPool();
    }
    else
    {
      this.GeneratePathGridFor(vehicleDef);
      this.GenerateRegionGridFor(vehicleDef);
    }
  }

  private void FinalizeAndSendLongOp(AsyncLongOperationAction longOperation)
  {
    if (!longOperation.IsValid)
    {
      Trace.Fail("Trying to send long op to thread but it's already invalid.");
      longOperation.ReturnToPool();
    }
    else
    {
      longOperation.OnValidate += new Func<bool>(this.GridGenIsValid);
      this.mapping.dedicatedThread.Enqueue((AsyncAction) longOperation);
    }
  }

  internal void DoPass()
  {
    for (int index = 0; index < 3; ++index)
      this.DoIncrementalPass();
  }

  internal void DoPassExpectClear() => this.DoPass();

  internal void DoIncrementalPass()
  {
    if (this.PassDisabled)
      return;
    foreach (Pawn allPawn in this.mapping.map.mapPawns.AllPawns)
    {
      if (allPawn is VehiclePawn vehiclePawn)
        this.pathGridCounter.SetUsed(vehiclePawn.VehicleDef);
    }
    foreach (Pawn pawn in Find.World.worldPawns.AllPawnsAlive)
    {
      if (pawn is VehiclePawn vehiclePawn && FactionUtility.IsPlayerSafe(((Thing) vehiclePawn).Faction))
        this.pathGridCounter.SetUsed(vehiclePawn.VehicleDef);
    }
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      if (!this.pathGridCounter.IsUsed(vehicleDef))
      {
        this.pathGridCounter.IncrementUnused(vehicleDef);
        if (this.pathGridCounter.ShouldRemoveGrid(vehicleDef))
          this.ReleasePathGrid(vehicleDef);
      }
    }
    foreach (VehicleDef allOwner in this.mapping.GridOwners.AllOwners)
    {
      if (!this.mapping[allOwner].VehiclePathGrid.Enabled && !this.mapping.GridOwners.TryForfeitOwnership(allOwner))
        this.ReleaseRegionGrid(allOwner);
    }
    this.pathGridCounter.OnPassComplete();
  }

  private bool TryAddPathGridRequest(VehicleDef vehicleDef, AsyncLongOperationAction longOperation)
  {
    if (this.mapping[vehicleDef].VehiclePathGrid.Enabled)
      return false;
    longOperation.OnInvoke += (Action) (() => this.GeneratePathGridFor(vehicleDef));
    return true;
  }

  private bool TryAddRegionGridRequest(
    VehicleDef vehicleDef,
    AsyncLongOperationAction longOperation)
  {
    if (!this.mapping[vehicleDef].Suspended)
      return false;
    longOperation.OnInvoke += (Action) (() => this.GenerateRegionGridFor(vehicleDef));
    return true;
  }

  private void GeneratePathGridFor(VehicleDef vehicleDef)
  {
    VehiclePathingSystem.VehiclePathData vehiclePathData = this.mapping[vehicleDef];
    if (vehiclePathData.VehiclePathGrid.Enabled)
      return;
    vehiclePathData.VehiclePathGrid.RecalculateAllPerceivedPathCosts();
  }

  private void GenerateRegionGridFor(VehicleDef vehicleDef)
  {
    VehicleDef owner = this.mapping.GridOwners.GetOwner(vehicleDef);
    if (!this.mapping[vehicleDef].Suspended)
      return;
    VehiclePathingSystem.VehiclePathData vehiclePathData = this.mapping[owner];
    vehiclePathData.VehicleRegionAndRoomUpdater.Init();
    vehiclePathData.VehicleRegionAndRoomUpdater.RebuildAllVehicleRegions();
  }

  private void ReleasePathGrid(VehicleDef ownerDef)
  {
    VehiclePathingSystem.VehiclePathData vehiclePathData = this.mapping[ownerDef];
    if (!vehiclePathData.VehiclePathGrid.Enabled)
      return;
    vehiclePathData.VehiclePathGrid.Release();
  }

  private void ReleaseRegionGrid(VehicleDef ownerDef)
  {
    VehiclePathingSystem.VehiclePathData vehiclePathData = this.mapping[ownerDef];
    if (vehiclePathData.Suspended)
      return;
    vehiclePathData.VehicleRegionAndRoomUpdater.Release();
  }

  public static DeferredGridGeneration.Urgency UrgencyFor([NotNull] Map map)
  {
    return map.generationTick == GenTicks.TicksGame || map.ParentFaction != Faction.OfPlayer ? DeferredGridGeneration.Urgency.Urgent : DeferredGridGeneration.Urgency.Deferred;
  }

  public static DeferredGridGeneration.Urgency UrgencyFor(VehiclePawn vehicle)
  {
    if (((Thing) vehicle).Faction == null)
      return DeferredGridGeneration.Urgency.None;
    return !((Thing) vehicle).Faction.IsPlayer ? DeferredGridGeneration.Urgency.Urgent : DeferredGridGeneration.Urgency.Deferred;
  }

  public static DeferredGridGeneration.Urgency UrgencyFor(Map map, VehiclePawn vehicle)
  {
    DeferredGridGeneration.Urgency a = DeferredGridGeneration.UrgencyFor(map);
    if (a == DeferredGridGeneration.Urgency.Urgent)
      return a;
    DeferredGridGeneration.Urgency b = DeferredGridGeneration.UrgencyFor(vehicle);
    return b == DeferredGridGeneration.Urgency.Urgent ? b : Ext_Enum.Max<DeferredGridGeneration.Urgency>(a, b);
  }

  [DebugAction("Vehicle Framework", "Force Remove Unused Regions", false, false, false, false, false, 0, false)]
  private static void DoPassOnAllMaps()
  {
    foreach (Map map in Find.Maps)
      map.GetCachedMapComponent<VehiclePathingSystem>().deferredGridGeneration.DoPass();
  }

  private class GridCounter
  {
    private readonly Dictionary<VehicleDef, int> countdownToRemoval = new Dictionary<VehicleDef, int>();
    private readonly HashSet<VehicleDef> activelyUsed = new HashSet<VehicleDef>();

    public int Count => this.activelyUsed.Count;

    public bool IsUsed(VehicleDef vehicleDef) => this.activelyUsed.Contains(vehicleDef);

    public void SetUsed(VehicleDef vehicleDef)
    {
      this.countdownToRemoval.Remove(vehicleDef);
      this.activelyUsed.Add(vehicleDef);
    }

    public void IncrementUnused(VehicleDef vehicleDef)
    {
      this.countdownToRemoval.TryAdd(vehicleDef, 0);
      this.countdownToRemoval[vehicleDef]++;
    }

    public bool ShouldRemoveGrid(VehicleDef vehicleDef) => this.countdownToRemoval[vehicleDef] >= 3;

    public void OnPassComplete() => this.activelyUsed.Clear();
  }

  public readonly struct PassDisabler : IDisposable
  {
    private readonly DeferredGridGeneration gridGeneration;

    public PassDisabler(DeferredGridGeneration gridGeneration)
    {
      this.gridGeneration = gridGeneration;
      this.gridGeneration.PassDisabled = true;
    }

    void IDisposable.Dispose() => this.gridGeneration.PassDisabled = false;
  }

  public enum Urgency
  {
    None,
    Deferred,
    Urgent,
  }
}
