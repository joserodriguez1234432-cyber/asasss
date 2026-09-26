// Decompiled with JetBrains decompiler
// Type: Vehicles.ColorPicker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class ColorPicker
{
  private static readonly Color Blackist = new Color(0.06f, 0.06f, 0.06f);
  private bool draggingColorPicker;
  private bool draggingHue;
  private readonly Texture2D colorChart = new Texture2D((int) byte.MaxValue, (int) byte.MaxValue);
  private readonly Texture2D hueChart = new Texture2D(1, (int) byte.MaxValue);

  public ColorPicker()
  {
    for (int index = 0; index < (int) byte.MaxValue; ++index)
      this.hueChart.SetPixel(0, index, Color.HSVToRGB(Mathf.InverseLerp(0.0f, (float) byte.MaxValue, (float) index), 1f, 1f));
    this.hueChart.Apply(false);
    for (int index1 = 0; index1 < (int) byte.MaxValue; ++index1)
    {
      for (int index2 = 0; index2 < (int) byte.MaxValue; ++index2)
      {
        Color color1 = Color.Lerp(Color.clear, Color.white, Mathf.InverseLerp(0.0f, (float) byte.MaxValue, (float) index1));
        Color color2 = Color32.op_Implicit(Color32.Lerp(Color32.op_Implicit(Color.black), Color32.op_Implicit(color1), Mathf.InverseLerp(0.0f, (float) byte.MaxValue, (float) index2)));
        this.colorChart.SetPixel(index1, index2, color2);
      }
    }
    this.colorChart.Apply(false);
  }

  public Rect Draw(
    Rect fullRect,
    ref float hue,
    ref float saturation,
    ref float value,
    ColorPicker.SetColor setColor)
  {
    Rect rect1 = GenUI.ContractedBy(fullRect, 10f);
    ((Rect) ref rect1).width = 15f;
    if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect1) && !this.draggingHue)
      this.draggingHue = true;
    if (this.draggingHue && Event.current.isMouse)
    {
      float num = hue;
      hue = Mathf.InverseLerp(((Rect) ref rect1).height, 0.0f, Event.current.mousePosition.y - ((Rect) ref rect1).y);
      if (!Mathf.Approximately(hue, num))
        setColor(hue, saturation, value);
    }
    if (Input.GetMouseButtonUp(0))
      this.draggingHue = false;
    Widgets.DrawBoxSolid(GenUI.ExpandedBy(rect1, 1f), Color.grey);
    Widgets.DrawTexturePart(rect1, new Rect(0.0f, 0.0f, 1f, 1f), this.hueChart);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(0.0f, 0.0f, 16f, 16f);
    ((Rect) ref rect2).center = GenUI.Rounded(new Vector2(((Rect) ref rect1).center.x, ((Rect) ref rect1).height * (1f - hue) + ((Rect) ref rect1).y));
    Rect rect3 = rect2;
    Widgets.DrawTextureRotated(rect3, (Texture) VehicleTex.ColorHue, 0.0f, (Material) null);
    rect1 = GenUI.ContractedBy(fullRect, 10f);
    ((Rect) ref rect1).x = ((Rect) ref rect1).xMax - ((Rect) ref rect1).height;
    ((Rect) ref rect1).width = ((Rect) ref rect1).height;
    if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect1) && !this.draggingColorPicker)
      this.draggingColorPicker = true;
    if (this.draggingColorPicker)
    {
      saturation = Mathf.InverseLerp(0.0f, ((Rect) ref rect1).width, Event.current.mousePosition.x - ((Rect) ref rect1).x);
      value = Mathf.InverseLerp(((Rect) ref rect1).width, 0.0f, Event.current.mousePosition.y - ((Rect) ref rect1).y);
      setColor(hue, saturation, value);
    }
    if (Input.GetMouseButtonUp(0))
      this.draggingColorPicker = false;
    Widgets.DrawBoxSolid(GenUI.ExpandedBy(rect1, 1f), Color.grey);
    Widgets.DrawBoxSolid(rect1, Color.white);
    GUI.color = Color.HSVToRGB(hue, 1f, 1f);
    Widgets.DrawTextureFitted(rect1, (Texture) this.colorChart, 1f, 1f);
    GUI.color = Color.white;
    GUI.BeginClip(rect1);
    ((Rect) ref rect3).center = new Vector2(((Rect) ref rect1).width * saturation, ((Rect) ref rect1).width * (1f - value));
    if ((double) value >= 0.40000000596046448 && ((double) hue <= 0.5 || (double) saturation <= 0.5))
      GUI.color = ColorPicker.Blackist;
    Widgets.DrawTextureFitted(rect3, (Texture) VehicleTex.ColorPicker, 1f, 1f);
    GUI.color = Color.white;
    GUI.EndClip();
    return rect1;
  }

  public delegate void SetColor(float h, float s, float v);
}
