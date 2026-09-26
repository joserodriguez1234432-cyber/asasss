// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AntiAircraftWorker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using System.Linq;

#nullable disable
namespace Vehicles.World;

public abstract class AntiAircraftWorker
{
  protected AntiAircraftDef def;
  protected AirDefense airDefense;

  public AntiAircraftWorker(AirDefense airDefense, AntiAircraftDef def)
  {
    this.airDefense = airDefense;
    this.def = def;
  }

  public virtual AerialVehicleInFlight CurrentTarget
  {
    get => this.airDefense.activeTargets.FirstOrDefault<AerialVehicleInFlight>();
  }

  public virtual bool ShouldDrawSearchLight => true;

  public abstract void Launch();

  public virtual void Tick()
  {
  }

  public virtual void TickRare()
  {
  }

  public virtual void TickLong()
  {
  }

  public virtual bool CanUseAirDefense(WorldObject worldObject)
  {
    return worldObject is MapParent mapParent && ((WorldObject) mapParent).Faction != null && ((WorldObject) mapParent).Faction.def.techLevel >= 4;
  }
}
