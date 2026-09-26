// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.AnchorSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class AnchorSegment : TaggedSegment
{
  public override (string, string) Tags => ("<anchor>", "</anchor>");

  public override int HeightOccupied(UpdateLog log, string fullText)
  {
    Text.Anchor = (TextAnchor) Enum.Parse(typeof (TextAnchor), fullText, true);
    return 0;
  }

  public override void SegmentAction(Listing_Rich lister, string innerText)
  {
    Text.Anchor = (TextAnchor) Enum.Parse(typeof (TextAnchor), innerText, true);
  }
}
