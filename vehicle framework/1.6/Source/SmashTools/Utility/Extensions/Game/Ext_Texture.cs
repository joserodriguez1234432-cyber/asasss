// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Texture
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class Ext_Texture
{
  private static RenderTexture previous;

  public static bool TryReplaceInContentFinder<T>(string itemPath, T item) where T : Object
  {
    List<ModContentPack> modsListForReading = LoadedModManager.RunningModsListForReading;
    for (int index = modsListForReading.Count - 1; index >= 0; --index)
    {
      ModContentPack modContentPack = modsListForReading[index];
      T obj = modContentPack.GetContentHolder<T>().Get(itemPath);
      if (Object.op_Inequality((Object) obj, (Object) null) && Object.op_Inequality((Object) obj, (Object) item))
      {
        Object.Destroy((Object) obj);
        Ext_Texture.InjectIntoContentFinder<T>(modContentPack, itemPath, item);
        return true;
      }
    }
    return false;
  }

  private static RenderTexture ConvertToRenderTex(Texture2D source)
  {
    RenderTexture temporary = RenderTexture.GetTemporary(((Texture) source).width, ((Texture) source).height, 0, (RenderTextureFormat) 7, (RenderTextureReadWrite) 1);
    Graphics.Blit((Texture) source, temporary);
    return temporary;
  }

  private static void ReleaseMemory(RenderTexture renderTex)
  {
    RenderTexture.active = Ext_Texture.previous;
    RenderTexture.ReleaseTemporary(renderTex);
  }

  private static void CompressAndApply(Texture2D source, Texture2D newTexture)
  {
    if (Prefs.TextureCompression && ((Texture) newTexture).width % 4 == 0)
    {
      int num = ((Texture) newTexture).height % 4;
    }
    ((Texture) newTexture).filterMode = ((Texture) source).filterMode;
    ((Texture) newTexture).anisoLevel = ((Texture) source).anisoLevel;
    newTexture.Apply(true, true);
  }

  private static void InjectIntoContentFinder<T>(
    ModContentPack modContentPack,
    string itemPath,
    T item)
    where T : class
  {
    modContentPack.GetContentHolder<T>().contentList[itemPath] = item;
  }

  public static Texture2D CreateReadableTexture(Texture2D source, TextureWrapMode? wrapMode = null)
  {
    RenderTexture renderTex = Ext_Texture.ConvertToRenderTex(source);
    Ext_Texture.previous = RenderTexture.active;
    RenderTexture.active = renderTex;
    Texture2D texture2D = new Texture2D(((Texture) source).width, ((Texture) source).height, (TextureFormat) 4, ((Texture) source).mipmapCount > 1);
    ((Object) texture2D).name = ((Object) source).name;
    ((Texture) texture2D).wrapMode = wrapMode ?? ((Texture) source).wrapMode;
    Texture2D readableTexture = texture2D;
    readableTexture.ReadPixels(new Rect(0.0f, 0.0f, (float) ((Texture) renderTex).width, (float) ((Texture) renderTex).height), 0, 0);
    readableTexture.Apply();
    Ext_Texture.ReleaseMemory(renderTex);
    return readableTexture;
  }

  public static Texture2D WrapTexture(Texture2D source, TextureWrapMode wrapMode)
  {
    if (((Texture) source).isReadable)
    {
      ((Texture) source).wrapMode = wrapMode;
      return source;
    }
    RenderTexture renderTex = Ext_Texture.ConvertToRenderTex(source);
    Texture2D readableTexture = Ext_Texture.CreateReadableTexture(source, new TextureWrapMode?(wrapMode));
    readableTexture.ReadPixels(new Rect(0.0f, 0.0f, (float) ((Texture) renderTex).width, (float) ((Texture) renderTex).height), 0, 0);
    Ext_Texture.CompressAndApply(source, readableTexture);
    Ext_Texture.ReleaseMemory(renderTex);
    return readableTexture;
  }

  public static Texture2D Rotate(this Texture2D source, float angle)
  {
    if (!Mathf.Approximately(angle, 90f) && !Mathf.Approximately(angle, 180f) && !Mathf.Approximately(angle, 270f))
    {
      Log.Error($"Unable to rotate {((Object) source).name} by angle=\"{angle}\". Angle must equal 90, 180, or 270.");
      return source;
    }
    if (((Texture) source).width != ((Texture) source).height)
      Log.Warning($"Rotating patterns with non-square dimensions may result in inaccurate conversions. Tex=\"{((Object) source).name}\" ({((Texture) source).width},{((Texture) source).height})");
    Texture2D originTexture = ((Texture) source).isReadable ? source : Ext_Texture.CreateReadableTexture(source);
    Texture2D texture2D = new Texture2D(((Texture) source).width, ((Texture) source).height, (TextureFormat) 4, ((Texture) source).mipmapCount > 1);
    ((Object) texture2D).name = ((Object) source).name;
    Texture2D newTexture = texture2D;
    Color32[] color32Array = Ext_Texture.RotateSquare(originTexture.GetPixels32(), angle * ((float) Math.PI / 180f), originTexture);
    Object.Destroy((Object) originTexture);
    newTexture.SetPixels32(color32Array);
    Ext_Texture.CompressAndApply(source, newTexture);
    return newTexture;
  }

  private static Color32[] RotateSquare(Color32[] arr, float phi, Texture2D originTexture)
  {
    float num1 = Mathf.Sin(phi);
    float num2 = Mathf.Cos(phi);
    Color32[] pixels32 = originTexture.GetPixels32();
    int width = ((Texture) originTexture).width;
    int height = ((Texture) originTexture).height;
    int num3 = width / 2;
    int num4 = height / 2;
    for (int index1 = 0; index1 < height; ++index1)
    {
      for (int index2 = 0; index2 < width; ++index2)
      {
        pixels32[index1 * width + index2] = new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0);
        int num5 = (int) ((double) num2 * (double) (index2 - num3) + (double) num1 * (double) (index1 - num4) + (double) num3);
        int num6 = (int) (-(double) num1 * (double) (index2 - num3) + (double) num2 * (double) (index1 - num4) + (double) num4);
        if (num5 > -1 && num5 < width && num6 > -1 && num6 < height)
          pixels32[index1 * width + index2] = arr[num6 * width + num5];
      }
    }
    return pixels32;
  }
}
