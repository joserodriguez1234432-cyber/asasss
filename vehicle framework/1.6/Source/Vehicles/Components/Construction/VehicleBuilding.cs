// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleBuilding
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class VehicleBuilding : Building
{
  public VehiclePawn vehicle;

  public VehicleDef VehicleDef
  {
    get => !(((Thing) this).def is VehicleBuildDef def) ? (VehicleDef) null : def.thingToSpawn;
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    if (this.vehicle != null)
    {
      ((Thing) this.vehicle).DrawNowAt(drawLoc, flip);
      this.vehicle.CompVehicleTurrets?.PostDraw();
    }
    else
    {
      Log.ErrorOnce($"VehicleReference for building {((Entity) this).LabelShort} is null.", ((object) this).GetHashCode());
      ((ThingWithComps) this).DrawAt(drawLoc, flip);
    }
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (this.vehicle == null && this.VehicleDef != null)
      this.vehicle = VehicleSpawner.GenerateVehicle(this.VehicleDef, ((Thing) this).Faction);
    this.vehicle?.CompVehicleTurrets?.RevalidateTurrets();
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
  }
}
