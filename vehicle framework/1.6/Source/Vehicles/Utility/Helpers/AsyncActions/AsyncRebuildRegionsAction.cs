// Decompiled with JetBrains decompiler
// Type: Vehicles.AsyncRebuildRegionsAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;

#nullable disable
namespace Vehicles;

public class AsyncRebuildRegionsAction : AsyncAction
{
  private VehiclePathingSystem.VehiclePathData pathData;

  public void Set(VehiclePathingSystem.VehiclePathData pathData) => this.pathData = pathData;

  public override void Invoke()
  {
    this.pathData.VehicleRegionAndRoomUpdater.TryRebuildVehicleRegions();
  }

  public override void ReturnToPool() => AsyncPool<AsyncRebuildRegionsAction>.Return(this);
}
