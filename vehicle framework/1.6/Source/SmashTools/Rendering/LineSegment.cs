// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.LineSegment
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace SmashTools.Rendering;

[PublicAPI]
public readonly record struct LineSegment(Vector3 from, Vector3 to, Color color)
{
  public readonly Vector3 from = from;
  public readonly Vector3 to = to;
  public readonly Color color = color;

  public LineSegment(Vector3 from, Vector3 to)
    : this(from, to, Color.white)
  {
  }

  [CompilerGenerated]
  public override int GetHashCode()
  {
    return (EqualityComparer<Vector3>.Default.GetHashCode(this.from) * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(this.to)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.color);
  }

  [CompilerGenerated]
  public bool Equals(LineSegment other)
  {
    return EqualityComparer<Vector3>.Default.Equals(this.from, other.from) && EqualityComparer<Vector3>.Default.Equals(this.to, other.to) && EqualityComparer<Color>.Default.Equals(this.color, other.color);
  }
}
