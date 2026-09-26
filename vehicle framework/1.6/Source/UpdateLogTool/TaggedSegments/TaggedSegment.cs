// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.TaggedSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

#nullable disable
namespace UpdateLogTool;

public abstract class TaggedSegment
{
  public abstract (string open, string close) Tags { get; }

  public virtual int HeightOccupied(UpdateLog log, string fullText) => 0;

  public abstract void SegmentAction(Listing_Rich lister, string innerText);
}
