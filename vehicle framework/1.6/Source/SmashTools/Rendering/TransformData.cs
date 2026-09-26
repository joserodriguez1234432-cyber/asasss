// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.TransformData
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Rendering;

[PublicAPI]
public readonly record struct TransformData
{
  public readonly Vector3 position;
  public readonly Rot8 orientation;
  public readonly float rotation;

  public TransformData()
  {
    this.position = Vector3.zero;
    this.orientation = Rot8.Invalid;
    this.rotation = 0.0f;
  }

  public TransformData(Vector3 position)
  {
    this.orientation = new Rot8();
    this.rotation = 0.0f;
    this.position = position;
  }

  public TransformData(Vector3 position, Rot8 orientation)
    : this(position)
  {
    this.position = position;
    this.orientation = orientation;
  }

  public TransformData(Vector3 position, Rot8 orientation, float rotation)
    : this(position, orientation)
  {
    this.rotation = rotation;
  }

  public TransformData Add(Vector3 position, float rotation)
  {
    return new TransformData(Vector3.op_Addition(this.position, position), this.orientation, this.rotation + rotation);
  }

  public static TransformData For(Thing thing, Rot8? rot = null, float? extraRotation = null)
  {
    return new TransformData(thing.DrawPos, rot ?? (Rot8) thing.Rotation, extraRotation.GetValueOrDefault());
  }

  public static TransformData operator +(TransformData lhs, TransformData rhs)
  {
    return new TransformData(Vector3.op_Addition(lhs.position, rhs.position), lhs.orientation, lhs.rotation + rhs.rotation);
  }

  [CompilerGenerated]
  public override int GetHashCode()
  {
    return (EqualityComparer<Vector3>.Default.GetHashCode(this.position) * -1521134295 + EqualityComparer<Rot8>.Default.GetHashCode(this.orientation)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.rotation);
  }

  [CompilerGenerated]
  public bool Equals(TransformData other)
  {
    return EqualityComparer<Vector3>.Default.Equals(this.position, other.position) && EqualityComparer<Rot8>.Default.Equals(this.orientation, other.orientation) && EqualityComparer<float>.Default.Equals(this.rotation, other.rotation);
  }
}
