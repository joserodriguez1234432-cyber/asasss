// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.TextureDrawer
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Rendering;

public static class TextureDrawer
{
  private static readonly List<RenderData> RenderDatas = new List<RenderData>();

  public static bool InUse { get; private set; }

  public static void Add(RenderData renderData) => TextureDrawer.RenderDatas.Add(renderData);

  public static void Open()
  {
    TextureDrawer.InUse = true;
    TextureDrawer.RenderDatas.Clear();
  }

  public static void Close()
  {
    TextureDrawer.RenderDatas.Clear();
    TextureDrawer.InUse = false;
  }

  public static void Draw(Rect rect, float scale = 1f, bool forceCentering = false)
  {
    GUI.BeginClip(rect);
    try
    {
      TextureDrawer.RenderDatas.Sort();
      foreach (RenderData renderData in TextureDrawer.RenderDatas)
      {
        Rect rect1 = renderData.rect;
        Vector2 vector2_1 = Vector2.op_Subtraction(((Rect) ref rect1).position, ((Rect) ref rect).position);
        Rect rect2;
        ref Rect local = ref rect2;
        Vector2 vector2_2 = vector2_1;
        rect1 = renderData.rect;
        Vector2 size = ((Rect) ref rect1).size;
        // ISSUE: explicit constructor call
        ((Rect) ref local).\u002Ector(vector2_2, size);
        if (forceCentering)
          ((Rect) ref rect2).center = ((Rect) ref rect).center;
        if (!Mathf.Approximately(scale, 1f))
        {
          rect1 = renderData.rect;
          Vector2 vector2_3 = Vector2.op_Multiply(((Rect) ref rect1).size, scale - 1f);
          rect2 = (double) scale > 1.0 ? GenUI.ExpandedBy(rect2, vector2_3.x, vector2_3.y) : GenUI.ContractedBy(rect2, vector2_3.x, vector2_3.y);
        }
        Rect rect3 = rect2;
        Texture mainTex = renderData.mainTex;
        Material material = renderData.material;
        double angle = (double) renderData.angle;
        rect1 = new Rect();
        Rect texCoords = rect1;
        UIElements.DrawTextureWithMaterialOnGUI(rect3, mainTex, material, (float) angle, texCoords);
      }
    }
    finally
    {
      GUI.EndClip();
    }
  }
}
