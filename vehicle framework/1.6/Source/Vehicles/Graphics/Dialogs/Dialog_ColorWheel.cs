// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_ColorWheel
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Dialog_ColorWheel : Window
{
  private const int ButtonWidth = 90;
  private const float ButtonHeight = 30f;
  private Color color;
  private readonly Action<Color> onComplete;
  private float hue;
  private float saturation;
  private float value;
  private readonly ColorPicker colorPicker = new ColorPicker();

  public Dialog_ColorWheel(Color color, Action<Color> onComplete)
    : base((IWindowDrawing) null)
  {
    this.color = color;
    this.onComplete = onComplete;
    this.doCloseX = true;
    this.closeOnClickedOutside = true;
  }

  public virtual Vector2 InitialSize => new Vector2(375f, 380f);

  public virtual void DoWindowContents(Rect inRect)
  {
    Rect fullRect = inRect;
    ((Rect) ref fullRect).height = ((Rect) ref inRect).width - 25f;
    this.colorPicker.Draw(fullRect, ref this.hue, ref this.saturation, ref this.value, new ColorPicker.SetColor(this.SetColor));
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(0.0f, ((Rect) ref inRect).height - 30f, 90f, 30f);
    this.DoBottomButtons(rect);
  }

  private void DoBottomButtons(Rect rect)
  {
    if (Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("VF_ApplyButton")), true, true, true, new TextAnchor?()))
    {
      this.onComplete(this.color);
      this.Close(true);
    }
    ref Rect local = ref rect;
    ((Rect) ref local).x = ((Rect) ref local).x + 90f;
    if (!Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, true, true, new TextAnchor?()))
      return;
    this.Close(true);
  }

  private void SetColor(float h, float s, float b)
  {
    ColorInt colorInt = new ColorInt(Color.HSVToRGB(h, s, b));
    this.color = ((ColorInt) ref colorInt).ToColor;
  }
}
