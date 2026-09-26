// Decompiled with JetBrains decompiler
// Type: Vehicles.AsyncConnectionAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using Verse;

#nullable disable
namespace Vehicles;

public class AsyncConnectionAction : AsyncAction
{
  private VehicleRegionConnector connector;
  private VehicleRegion region;

  public override bool IsValid
  {
    get
    {
      VehicleRegion region = this.region;
      if (region != null && !region.InPool && region.valid)
      {
        Map map = region.Map;
        if (map != null)
          return !map.Disposed;
      }
      return false;
    }
  }

  public void Set(VehicleRegionConnector connector, VehicleRegion region)
  {
    this.connector = connector;
    this.region = region;
  }

  public override void Invoke() => this.connector.RecalculateWeights(this.region);

  public override void ReturnToPool()
  {
    this.connector = (VehicleRegionConnector) null;
    this.region = (VehicleRegion) null;
    AsyncPool<AsyncConnectionAction>.Return(this);
  }
}
