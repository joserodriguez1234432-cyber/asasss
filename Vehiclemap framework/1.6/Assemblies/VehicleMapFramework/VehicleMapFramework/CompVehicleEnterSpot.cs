// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompVehicleEnterSpot
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public abstract class CompVehicleEnterSpot : ThingComp
{
  public CompProperties_VehicleEnterSpot Props => (CompProperties_VehicleEnterSpot) this.props;

  protected abstract bool Available { get; }

  public abstract bool ShouldOffsetOnEdge { get; }

  protected abstract TargetInfo AccessSpot { get; }

  public TargetInfo AvailableAccessSpot => !this.Available ? TargetInfo.Invalid : this.AccessSpot;

  public abstract float MovePerTick(Pawn pawn);

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      if (((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
        vehicle.EnterComps.Add(this);
      CrossMapReachabilityCache.ClearCacheFor(((Thing) this.parent).Map);
    }));
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    base.PostDeSpawn(map, mode);
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
      vehicle.EnterComps.Remove(this);
    CrossMapReachabilityCache.ClearCacheFor(map);
  }

  public enum Kind
  {
    RampOnly,
    GroundAccessOnly,
    DirectAccessOnly,
    All,
  }
}
