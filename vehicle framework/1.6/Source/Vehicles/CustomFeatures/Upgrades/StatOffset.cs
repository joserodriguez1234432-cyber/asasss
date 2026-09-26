// Decompiled with JetBrains decompiler
// Type: Vehicles.StatOffset
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class StatOffset
{
  public readonly VehiclePawn vehicle;
  public readonly StatDef statDef;
  public readonly VehicleStatDef vehicleStatDef;
  public readonly StatUpgradeCategoryDef upgradeCategoryDef;
  private float offset;
  private List<(string key, float value)> overrideValues;

  public StatOffset(VehiclePawn vehicle, StatUpgradeCategoryDef upgradeCategoryDef)
  {
    this.vehicle = vehicle;
    this.upgradeCategoryDef = upgradeCategoryDef;
  }

  public StatOffset(VehiclePawn vehicle, VehicleStatDef vehicleStatDef)
  {
    this.vehicle = vehicle;
    this.vehicleStatDef = vehicleStatDef;
  }

  public StatOffset(VehiclePawn vehicle, StatDef statDef)
  {
    this.vehicle = vehicle;
    this.statDef = statDef;
  }

  public float Offset
  {
    get => this.offset;
    set
    {
      if (Mathf.Approximately(this.offset, value))
        return;
      this.offset = value;
      this.vehicle.statHandler.MarkAllDirty();
    }
  }

  public bool TryGetOverride(out float value)
  {
    value = float.NaN;
    if (GenList.NullOrEmpty<(string, float)>((IList<(string, float)>) this.overrideValues))
      return false;
    value = this.overrideValues.LastOrDefault<(string, float)>().value;
    return true;
  }

  public void RemoveOverride(string key)
  {
    GenCollection.RemoveWhere<(string, float)>((IList<(string, float)>) this.overrideValues, (Func<(string, float), bool>) (tuple => tuple.key == key));
  }

  public void AddOverride(string key, float value)
  {
    if (this.overrideValues == null)
      this.overrideValues = new List<(string, float)>();
    this.overrideValues.Add((key, value));
  }
}
