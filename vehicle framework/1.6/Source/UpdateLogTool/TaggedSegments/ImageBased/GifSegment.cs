// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.GifSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class GifSegment : AnimatedSegment
{
  private static readonly IntVec2 defaultGifSize = new IntVec2(10, 10);

  public override (string, string) Tags => ("<gif", "</gif>");

  protected override int DefaultFramesPerSecond => 30;

  protected override IEnumerable<(string name, Type type)> Attributes
  {
    get
    {
      foreach ((string, Type) attribute in base.Attributes)
        yield return attribute;
      yield return ("FPS", typeof (int));
      yield return ("SIZE", typeof (IntVec2));
    }
  }

  protected override void RenderImage(Listing_Rich lister, string innerText, Texture2D texture)
  {
    Lookup lookup = this.ContainerAttributes(innerText);
    int width = lookup.Get<int>("WIDTH", Mathf.RoundToInt(700f));
    int height = this.HeightOccupied(lister.CurrentLog, innerText);
    IntVec2 size = lookup.Get<IntVec2>("SIZE", GifSegment.defaultGifSize);
    int fps = lookup.Get<int>("FPS", 30);
    lister.DrawGif(texture, (float) width, (float) height, size, fps);
  }
}
