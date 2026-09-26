// Decompiled with JetBrains decompiler
// Type: Vehicles.FleckOneShot
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class FleckOneShot
{
  public FleckDef def;
  public FloatRange angle = new FloatRange(0.0f, 360f);
  public Vector3 originOffset = Vector3.zero;
  public Range<Vector3> originOffsetRange;
  public int emitAtTick = -1;
  public bool lockFleckX = true;
  public bool lockFleckZ = true;
  public FloatRange? airTime;
  public FloatRange? speed;
  public FloatRange? rotationRate;
  public FloatRange? size;
}
