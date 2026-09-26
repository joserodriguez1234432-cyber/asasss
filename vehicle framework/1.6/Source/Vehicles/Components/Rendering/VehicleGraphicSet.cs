// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleGraphicSet
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleGraphicSet
{
  public VehiclePawn vehicle;
  public Graphic rottingGraphic;
  public Graphic dessicatedGraphic;
  public Graphic packGraphic;
  private List<Material> cachedMatsBodyBase = new List<Material>();
  private int cachedMatsBodyBaseHash = -1;

  public VehicleGraphicSet(VehiclePawn vehicle) => this.vehicle = vehicle;

  public bool AllResolved => this.vehicle.VehicleGraphic != null;

  public List<Material> MatsBodyBaseAt(Rot8 facing)
  {
    if (facing.IsHorizontal && (double) this.vehicle.Angle != (double) this.vehicle.CachedAngle)
    {
      this.cachedMatsBodyBase.Clear();
      this.cachedMatsBodyBaseHash = -1;
      this.vehicle.CachedAngle = this.vehicle.Angle;
    }
    int asInt = facing.AsInt;
    if (asInt != this.cachedMatsBodyBaseHash)
    {
      this.cachedMatsBodyBase.Clear();
      this.cachedMatsBodyBaseHash = asInt;
      this.cachedMatsBodyBase.Add(this.vehicle.VehicleGraphic.MatAtFull(facing));
    }
    return this.cachedMatsBodyBase;
  }

  public void ClearCache() => this.cachedMatsBodyBaseHash = -1;

  public void ResolveAllGraphics() => this.ClearCache();

  public void SetAllGraphicsDirty()
  {
    if (!this.AllResolved)
      return;
    this.ResolveAllGraphics();
  }
}
