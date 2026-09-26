// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.RenderData
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using UnityEngine;

#nullable disable
namespace SmashTools.Rendering;

[UsedImplicitly]
public readonly struct RenderData(
  Rect rect,
  Texture mainTex,
  Material material,
  MaterialPropertyBlock propertyBlock) : IComparable<RenderData>
{
  public readonly Rect rect = rect;
  internal readonly Texture mainTex = mainTex;
  internal readonly Material material = material;
  internal readonly MaterialPropertyBlock propertyBlock = propertyBlock;
  internal readonly float layer = 0.0f;
  internal readonly float angle = 0.0f;

  public RenderData(
    Rect rect,
    Texture mainTex,
    Material material,
    MaterialPropertyBlock propertyBlock,
    float layer,
    float angle)
    : this(rect, mainTex, material, propertyBlock)
  {
    this.layer = layer;
    this.angle = angle;
  }

  public static RenderData Invalid
  {
    get
    {
      return new RenderData(Rect.zero, (Texture) null, (Material) null, (MaterialPropertyBlock) null, -1f, 0.0f);
    }
  }

  int IComparable<RenderData>.CompareTo(RenderData other) => this.layer.CompareTo(other.layer);
}
