// Decompiled with JetBrains decompiler
// Type: Vehicles.StatCache
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class StatCache
{
  private readonly VehiclePawn vehicle;
  private readonly float[] cachedValues;
  private readonly bool[] dirty;

  public StatCache(VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    int defCount = DefDatabase<VehicleStatDef>.DefCount;
    this.cachedValues = new float[defCount];
    this.dirty = new bool[defCount].Populate<bool>(true);
  }

  public bool IsDirty(VehicleStatDef statDef) => this.dirty[statDef.DefIndex];

  public float this[VehicleStatDef statDef]
  {
    get
    {
      if (this.dirty[statDef.DefIndex])
        this.RecacheFor(statDef);
      return this.cachedValues[statDef.DefIndex];
    }
  }

  public void MarkDirty(VehicleStatDef statDef) => this.dirty[statDef.DefIndex] = true;

  public void Reset()
  {
    for (int index = 0; index < this.dirty.Length; ++index)
      this.dirty[index] = true;
  }

  private void RecacheFor(VehicleStatDef statDef)
  {
    this.cachedValues[statDef.DefIndex] = statDef.Worker.GetValue(this.vehicle);
    this.dirty[statDef.DefIndex] = false;
  }

  [PublicAPI]
  public class EventLister
  {
    public VehicleStatDef statDef;
    public List<VehicleEventDef> eventDefs;
  }
}
