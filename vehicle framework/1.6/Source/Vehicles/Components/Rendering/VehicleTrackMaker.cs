// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTrackMaker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleTrackMaker
{
  private readonly VehiclePawn vehicle;
  private Vector3 lastTrackPlacePos;

  public VehicleTrackMaker(VehiclePawn vehicle) => this.vehicle = vehicle;

  public void ProcessPostTickVisuals(int ticksPassed)
  {
    if (this.vehicle.VehicleDef.properties.track == null || GridsUtility.GetTerrain(((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map) == null)
      return;
    this.vehicle.VehicleDef.properties.track.TryPlaceTrack(this.vehicle, ref this.lastTrackPlacePos);
  }
}
