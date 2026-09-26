// Decompiled with JetBrains decompiler
// Type: Vehicles.ComponentRequirement
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class ComponentRequirement
{
  private string key;
  private float healthPercent;
  private ComparisonType comparison = ComparisonType.GreaterThan;

  public VehicleComponent Component { get; private set; }

  public string Label => this.Component.props.label;

  public bool MeetsRequirements { get; private set; } = true;

  public void RecacheComponent(VehiclePawn vehicle)
  {
    this.Component = vehicle.statHandler.GetComponent(this.key);
  }

  public void RegisterEvents(VehiclePawn vehicle)
  {
    vehicle.RemoveEvent<VehicleEventDef>(VehicleEventDefOf.HealthChanged, new Action(this.OnHealthChanged));
    vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.HealthChanged, new Action(this.OnHealthChanged));
  }

  private void OnHealthChanged()
  {
    this.MeetsRequirements = this.Component != null && this.comparison.Compare(this.Component.HealthPercent, this.healthPercent);
  }

  public static ComponentRequirement CopyFrom(ComponentRequirement reference)
  {
    return new ComponentRequirement()
    {
      key = reference.key,
      healthPercent = reference.healthPercent,
      comparison = reference.comparison
    };
  }
}
