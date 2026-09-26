// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.RenderTextureUtil
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;

#nullable disable
namespace SmashTools.Rendering;

public static class RenderTextureUtil
{
  public static RenderTexture CreateRenderTexture(int width, int height)
  {
    RenderTexture renderTexture1 = width > 0 && height > 0 ? new RenderTexture(width, height, 0, ((RenderTextureFormat) 11).OrNextSupportedFormat()) : throw new ArgumentException("RenderTexture size must have dimensions greater than 0.");
    ((Texture) renderTexture1).filterMode = (FilterMode) 0;
    ((Texture) renderTexture1).wrapMode = (TextureWrapMode) 1;
    RenderTexture renderTexture2 = renderTexture1;
    renderTexture2.Create();
    return renderTexture2;
  }

  public static RenderTextureFormat OrNextSupportedFormat(
    this RenderTextureFormat renderTextureFormat)
  {
    if (SystemInfo.SupportsRenderTextureFormat(renderTextureFormat))
      return renderTextureFormat;
    RenderTextureFormat renderTextureFormat1 = renderTextureFormat;
    RenderTextureFormat renderTextureFormat2;
    int num;
    switch (renderTextureFormat1 - 12)
    {
      case 0:
        if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat) 11))
        {
          renderTextureFormat2 = (RenderTextureFormat) 11;
          goto label_16;
        }
        goto case 1;
      case 1:
label_15:
        renderTextureFormat2 = renderTextureFormat;
        goto label_16;
      case 2:
      case 3:
label_11:
        if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat) 12))
        {
          renderTextureFormat2 = (RenderTextureFormat) 12;
          goto label_16;
        }
        goto case 0;
      case 4:
        if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat) 25))
        {
          renderTextureFormat2 = (RenderTextureFormat) 25;
          goto label_16;
        }
        num = 1;
        break;
      default:
        if (renderTextureFormat1 == 25)
        {
          num = 2;
          break;
        }
        goto case 1;
    }
    if (!SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat) 0))
    {
      switch (num)
      {
        case 1:
          goto label_11;
        case 2:
          goto label_15;
      }
    }
    renderTextureFormat2 = (RenderTextureFormat) 0;
label_16:
    return renderTextureFormat2;
  }
}
