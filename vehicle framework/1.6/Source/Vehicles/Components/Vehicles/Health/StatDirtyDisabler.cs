// Decompiled with JetBrains decompiler
// Type: Vehicles.StatDirtyDisabler
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;

#nullable disable
namespace Vehicles;

public readonly struct StatDirtyDisabler : IDisposable
{
  private readonly bool prevValue;
  private readonly VehicleStatHandler statHandler;

  public StatDirtyDisabler(VehiclePawn vehicle)
  {
    this.statHandler = vehicle.statHandler;
    this.prevValue = this.statHandler.CanDirty;
    this.statHandler.CanDirty = false;
  }

  public void Dispose() => this.statHandler.CanDirty = this.prevValue;
}
