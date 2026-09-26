// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.FontSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class FontSegment : TaggedSegment
{
  public override (string, string) Tags => ("<font>", "</font>");

  public override int HeightOccupied(UpdateLog log, string fullText)
  {
    Text.Font = (GameFont) Enum.Parse(typeof (GameFont), fullText, true);
    return 0;
  }

  public override void SegmentAction(Listing_Rich lister, string innerText)
  {
    Text.Font = (GameFont) Enum.Parse(typeof (GameFont), innerText, true);
  }
}
