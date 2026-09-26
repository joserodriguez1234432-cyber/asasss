// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.IParallelRenderer
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using Verse;

#nullable disable
namespace SmashTools.Rendering;

public interface IParallelRenderer
{
  bool IsDirty { get; set; }

  void DynamicDrawPhaseAt(DrawPhase phase, in TransformData transformData, bool forceDraw = false);
}
