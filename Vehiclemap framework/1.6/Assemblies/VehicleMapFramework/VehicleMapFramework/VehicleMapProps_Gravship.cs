// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapProps_Gravship
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapProps_Gravship : VehicleMapProps_Unique, IExposable
{
  public string defName;

  public void ExposeData()
  {
    Scribe_Defs.Look<VehicleDef>(ref this.baseDef, "baseDef");
    Scribe_Values.Look<Vector3>(ref this.offset, "offset", new Vector3(), false);
    Scribe_Values.Look<IntVec2>(ref this.size, "size", new IntVec2(), false);
    Scribe_Collections.Look<IntVec2>(ref this.outOfBoundsCells, "outOfBoundsCells", (LookMode) 0, Array.Empty<object>());
    if (Scribe.mode != 2 || this.baseDef == null)
      return;
    Scribe_Values.Look<string>(ref this.defName, "defName", (string) null, false);
  }
}
