// Decompiled with JetBrains decompiler
// Type: Vehicles.MoteThrownExpand
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class MoteThrownExpand : MoteThrown
{
  public float growthRate;

  protected virtual void Tick()
  {
    ((Mote) this).Tick();
    ((Mote) this).linearScale = Vector3.op_Addition(((Mote) this).linearScale, new Vector3(this.growthRate, 0.0f, this.growthRate));
  }
}
