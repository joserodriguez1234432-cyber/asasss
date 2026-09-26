// Decompiled with JetBrains decompiler
// Type: Vehicles.ActivatableThingComp
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class ActivatableThingComp
{
  private readonly VehiclePawn vehicle;
  private ThingComp comp;
  private int owners;
  private System.Type type;

  public ActivatableThingComp(VehiclePawn vehicle) => this.vehicle = vehicle;

  private bool Deactivated => this.owners == 0;

  public System.Type Type => this.type;

  public ThingComp Comp => this.comp;

  public int Owners
  {
    get => this.owners;
    set
    {
      if (this.owners == value)
        return;
      this.owners = Mathf.Clamp(value, 0, int.MaxValue);
      this.RevalidateCompStatus();
    }
  }

  private void RevalidateCompStatus()
  {
    if (this.Deactivated)
    {
      if (!this.vehicle.RemoveComp(this.comp))
        return;
      this.vehicle.deactivatedComps.Add(this.comp);
      this.vehicle.deactivatedCompTypes.Add(this.comp.GetType());
      this.vehicle.activatableComps.Remove(this);
    }
    else
    {
      if (((ThingWithComps) this.vehicle).AllComps.Contains(this.comp))
        return;
      this.vehicle.AddComp(this.comp);
    }
  }

  public void Init(ThingComp comp)
  {
    this.comp = comp;
    this.type = comp.GetType();
  }
}
