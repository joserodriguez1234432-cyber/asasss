// Decompiled with JetBrains decompiler
// Type: Vehicles.World.FlakWorker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class FlakWorker : AntiAircraftWorker
{
  protected readonly List<Vector3> spawnLocations = new List<Vector3>();
  protected List<Thing> turretsCachedInMap = new List<Thing>();
  protected List<int> flakTurrets = new List<int>();
  protected int turretFiring;

  public FlakWorker(AirDefense airDefense, AntiAircraftDef def)
    : base(airDefense, def)
  {
    this.flakTurrets = new List<int>();
    for (int index = 0; index < airDefense.defenseBuildings; ++index)
      this.flakTurrets.Add(this.CooldownRange);
    if (this.turretsCachedInMap != null)
      return;
    this.turretsCachedInMap = new List<Thing>();
  }

  public virtual int CooldownRange
  {
    get
    {
      return Mathf.CeilToInt(Rand.Range((float) this.airDefense.antiAircraftDef.ticksBetweenShots * 0.75f, (float) this.airDefense.antiAircraftDef.ticksBetweenShots * 1.25f));
    }
  }

  public override bool ShouldDrawSearchLight
  {
    get
    {
      if (!(this.airDefense.parent is MapParent parent))
        return false;
      return parent.Map == null || !GenList.NullOrEmpty<Thing>((IList<Thing>) this.turretsCachedInMap);
    }
  }

  public virtual List<Vector3> SpawnLocations
  {
    get
    {
      if (GenList.NullOrEmpty<Vector3>((IList<Vector3>) this.spawnLocations))
        this.spawnLocations.AddRange(Building_Artillery.RandomWorldPosition(this.airDefense.parent.Tile, this.airDefense.defenseBuildings));
      return this.spawnLocations;
    }
  }

  public override void Tick()
  {
    if (this.CurrentTarget == null)
      return;
    if (this.airDefense.parent is MapParent parent && parent.HasMap)
    {
      if (!this.CurrentTarget.Vehicle.CompVehicleLauncher.inFlight || (double) Ext_Math.SphericalDistance(this.airDefense.parent.DrawPos, ((WorldObject) this.CurrentTarget).DrawPos) > (double) this.airDefense.MaxDistance)
      {
        foreach (Building_Artillery turretsCachedIn in this.turretsCachedInMap)
          turretsCachedIn.NotifyTargetOutOfRange(GlobalTargetInfo.op_Implicit((WorldObject) this.CurrentTarget));
      }
      else
      {
        foreach (Building_Artillery turretsCachedIn in this.turretsCachedInMap)
          turretsCachedIn.NotifyTargetInRange(GlobalTargetInfo.op_Implicit((WorldObject) this.airDefense.activeTargets.FirstOrDefault<AerialVehicleInFlight>()));
      }
    }
    else
    {
      for (int index = 0; index < this.flakTurrets.Count; ++index)
      {
        this.flakTurrets[index]--;
        if (this.flakTurrets[index] <= 0)
        {
          this.flakTurrets[index] = this.CooldownRange;
          this.Launch();
        }
      }
    }
  }

  public override void TickRare()
  {
    if (!(this.airDefense.parent is MapParent parent))
      return;
    int num = parent.HasMap ? 1 : 0;
  }

  public override void Launch()
  {
    AntiAircraft instance = (AntiAircraft) Activator.CreateInstance(this.def.worldObjectClass);
    instance.def = (WorldObjectDef) this.def;
    instance.ID = Find.UniqueIDsManager.GetNextWorldObjectID();
    instance.creationGameTicks = Find.TickManager.TicksGame;
    instance.Tile = this.airDefense.parent.Tile;
    instance.Initialize(this.airDefense.parent, this.CurrentTarget, this.SpawnLocations[this.turretFiring]);
    instance.PostMake();
    Find.WorldObjects.Add((WorldObject) instance);
    ++this.turretFiring;
    if (this.turretFiring < this.flakTurrets.Count)
      return;
    this.turretFiring = 0;
  }
}
