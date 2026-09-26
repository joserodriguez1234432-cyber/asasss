// Decompiled with JetBrains decompiler
// Type: Vehicles.CompProjectileExitMap
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public class CompProjectileExitMap : ThingComp
{
  public AntiAircraftDef airDefenseDef;
  public AerialVehicleInFlight target;
  public Vector3 spawnPos;

  public CompProjectileExitMap(ThingWithComps parent) => this.parent = parent;

  public void LeaveMap()
  {
    AntiAircraft instance = (AntiAircraft) Activator.CreateInstance(this.airDefenseDef.worldObjectClass);
    instance.def = (WorldObjectDef) this.airDefenseDef;
    instance.ID = Find.UniqueIDsManager.GetNextWorldObjectID();
    instance.creationGameTicks = Find.TickManager.TicksGame;
    instance.Tile = ((Thing) this.parent).Map.Tile;
    instance.Initialize((WorldObject) ((Thing) this.parent).Map.Parent, this.target, this.spawnPos);
    instance.PostMake();
    Find.WorldObjects.Add((WorldObject) instance);
  }
}
