// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.ImageSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class ImageSegment : ContainerSegment
{
  public override (string, string) Tags => ("<img", "</img>");

  public override void SegmentAction(Listing_Rich lister, string innerText)
  {
    string innerText1 = this.GetInnerText(innerText);
    UpdateLog currentLog = lister.CurrentLog;
    if ((object) currentLog == null)
      return;
    Texture2D texture;
    if (currentLog.cachedTextures.TryGetValue(innerText1, out texture))
    {
      this.RenderImage(lister, innerText, texture);
    }
    else
    {
      WebTexture webTexture;
      if (currentLog.cachedDownloadedTextures.TryGetValue(innerText1, out webTexture))
      {
        if (webTexture.Status == DownloadStatus.Success)
          this.RenderImage(lister, innerText, webTexture.texture);
        else
          this.RenderLoadingPlaceholder(lister, innerText, WebTexture.StringStatus(innerText1, webTexture.Status));
      }
      else
        Log.ErrorOnce($"Failed to retrieve cached texture for {innerText1}.", innerText1.GetHashCode());
    }
  }

  protected virtual void RenderImage(Listing_Rich lister, string innerText, Texture2D texture)
  {
    int width = this.ContainerAttributes(innerText).Get<int>("WIDTH", Mathf.RoundToInt(700f));
    int height = this.HeightOccupied(lister.CurrentLog, innerText);
    lister.DrawTexture(texture, (float) width, (float) height);
  }

  protected virtual void RenderLoadingPlaceholder(
    Listing_Rich lister,
    string innerText,
    string loadingMessage)
  {
    int width = this.ContainerAttributes(innerText).Get<int>("WIDTH", Mathf.RoundToInt(700f));
    int height = this.HeightOccupied(lister.CurrentLog, innerText);
    lister.DrawLoadingPlaceholder((float) width, (float) height, loadingMessage);
  }
}
