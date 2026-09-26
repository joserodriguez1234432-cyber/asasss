// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.IBlitTarget
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace Vehicles.Rendering;

public interface IBlitTarget
{
  (int width, int height) TextureSize(in BlitRequest request);

  IEnumerable<SmashTools.Rendering.RenderData> GetRenderData(Rect rect, BlitRequest request);
}
