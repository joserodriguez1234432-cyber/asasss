// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.RenderTextureDrawer
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Rendering;

[StaticConstructorOnStartup]
public static class RenderTextureDrawer
{
  private static readonly List<RenderData> RenderDatas = new List<RenderData>();
  private static RenderTexture renderTexture;

  public static bool InUse => Object.op_Implicit((Object) RenderTextureDrawer.renderTexture);

  public static void Add(RenderData renderData) => RenderTextureDrawer.RenderDatas.Add(renderData);

  public static void Open(RenderTexture renderTexture)
  {
    RenderTextureDrawer.renderTexture = renderTexture;
    RenderTextureDrawer.RenderDatas.Clear();
  }

  public static void Close()
  {
    RenderTextureDrawer.RenderDatas.Clear();
    RenderTextureDrawer.renderTexture = (RenderTexture) null;
  }

  public static void Draw(Rect rect, float scale = 1f, bool center = false)
  {
    if (!Object.op_Implicit((Object) RenderTextureDrawer.renderTexture) || !RenderTextureDrawer.renderTexture.IsCreated())
    {
      Trace.Fail("Trying to blit with null render texture.");
    }
    else
    {
      RenderTextureDrawer.RenderDatas.Sort();
      RenderTexture.active = RenderTextureDrawer.renderTexture;
      try
      {
        GL.PushMatrix();
        GL.Viewport(new Rect(0.0f, 0.0f, (float) ((Texture) RenderTextureDrawer.renderTexture).width, (float) ((Texture) RenderTextureDrawer.renderTexture).height));
        GL.LoadPixelMatrix(0.0f, (float) ((Texture) RenderTextureDrawer.renderTexture).width, (float) ((Texture) RenderTextureDrawer.renderTexture).height, 0.0f);
        GL.Clear(true, true, Color.clear);
        foreach (RenderData renderData in RenderTextureDrawer.RenderDatas)
          DrawRenderData(rect, in renderData, scale, center);
      }
      finally
      {
        GL.PopMatrix();
        GL.Flush();
        RenderTextureDrawer.RenderDatas.Clear();
        RenderTexture.active = (RenderTexture) null;
      }
    }

    static void DrawRenderData(Rect rect, in RenderData renderData, float scale, bool center)
    {
      if (Object.op_Implicit((Object) renderData.material) && !renderData.material.SetPass(0))
        return;
      GL.PushMatrix();
      GL.LoadIdentity();
      try
      {
        Rect input;
        if (!center)
        {
          input = renderData.rect;
        }
        else
        {
          Rect rect1 = renderData.rect;
          ((Rect) ref rect1).center = ((Rect) ref rect).center;
          input = rect1;
        }
        Rect rect2 = NormalizeRect(input, rect);
        Vector3 vector3 = Vector2.op_Implicit(Vector2.op_Multiply(((Rect) ref rect2).size, scale));
        Quaternion quaternion = Quaternion.Euler(0.0f, 0.0f, renderData.angle);
        GL.MultMatrix(Matrix4x4.op_Multiply(Matrix4x4.TRS(Vector2.op_Implicit(((Rect) ref rect2).center), quaternion, vector3), Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0.0f))));
        Graphics.DrawTexture(new Rect(0.0f, 0.0f, 1f, 1f), renderData.mainTex, renderData.material);
      }
      finally
      {
        GL.PopMatrix();
      }
    }

    static Rect NormalizeRect(Rect input, Rect rect)
    {
      float num1 = (float) ((Texture) RenderTextureDrawer.renderTexture).width / ((Rect) ref rect).width;
      float num2 = (float) ((Texture) RenderTextureDrawer.renderTexture).height / ((Rect) ref rect).height;
      return new Rect(((Rect) ref input).x * num1, ((Rect) ref input).y * num2, ((Rect) ref input).width * num1, ((Rect) ref input).height * num2);
    }
  }
}
