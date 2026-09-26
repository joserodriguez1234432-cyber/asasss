// Decompiled with JetBrains decompiler
// Type: Vehicles.DropZone
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class DropZone : IExposable
{
  public IntVec3 from;
  public IntVec3 to;
  private int count;
  public List<IntVec3> dropPoints;

  public DropZone(IntVec3 from, IntVec3 to, int count)
  {
    this.from = from;
    this.to = to;
    this.count = count;
    this.RecalculateDropPoints();
  }

  private void RecalculateDropPoints()
  {
    this.dropPoints = new List<IntVec3>(this.count);
    for (int index = 0; index < this.count; ++index)
    {
      float num = (float) (index + 1) / (float) (this.count + 2);
      Vector2 vector2 = Vector2.Lerp(((IntVec3) ref this.from).ToVector2(), ((IntVec3) ref this.to).ToVector2(), num);
      this.dropPoints.Add(new IntVec3(Mathf.RoundToInt(vector2.x), 0, Mathf.RoundToInt(vector2.y)));
    }
  }

  void IExposable.ExposeData()
  {
    Scribe_Values.Look<IntVec3>(ref this.from, "from", new IntVec3(), false);
    Scribe_Values.Look<IntVec3>(ref this.to, "from", new IntVec3(), false);
    Scribe_Values.Look<int>(ref this.count, "count", 0, false);
    if (Scribe.mode != 2)
      return;
    this.RecalculateDropPoints();
  }
}
