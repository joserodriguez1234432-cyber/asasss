// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.TitleSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class TitleSegment : TaggedSegment
{
  public override (string, string) Tags => ("<title>", "</title>");

  public override int HeightOccupied(UpdateLog log, string fullText)
  {
    Text.Font = (GameFont) 1;
    return (int) Text.CalcHeight(fullText, 700f) + 5;
  }

  public override void SegmentAction(Listing_Rich lister, string innerText)
  {
    GameFont font = Text.Font;
    Text.Font = (GameFont) 1;
    innerText = $"<b>{innerText}</b>";
    Rect rect = lister.Label(innerText, -1f, new TipSignal?());
    Widgets.DrawLineHorizontal(0.0f, ((Rect) ref rect).y + Text.CalcHeight(innerText, 9999f), ((Rect) ref rect).width);
    Text.Font = font;
  }
}
