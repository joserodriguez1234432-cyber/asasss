// Decompiled with JetBrains decompiler
// Type: SmashTools.WidgetRowButBetter
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools;

public class WidgetRowButBetter
{
  public const float IconSize = 24f;
  public const float DefaultGap = 4f;
  private const float DefaultMaxWidth = 99999f;
  public const float LabelGap = 2f;
  public const float ButtonExtraSpace = 16f;
  private float startX;
  private float startY;
  private int rows = 1;
  private float maxWidth = 99999f;
  private UIDirection growDirection = (UIDirection) 2;

  public virtual float CurX { get; protected set; }

  public virtual float CurY { get; protected set; }

  public virtual float CellGap { get; protected set; }

  public WidgetRowButBetter()
  {
  }

  public WidgetRowButBetter(
    float x,
    float y,
    UIDirection growDirection = 2,
    float maxWidth = 99999f,
    float gap = 4f)
  {
    this.Init(x, y, growDirection, maxWidth, gap);
  }

  public virtual void Init(
    float x,
    float y,
    UIDirection growDirection = 2,
    float maxWidth = 99999f,
    float gap = 4f)
  {
    this.growDirection = growDirection;
    this.startX = x;
    this.startY = y;
    this.CurX = x;
    this.CurY = y;
    this.maxWidth = maxWidth;
    this.CellGap = gap;
  }

  public float LeftX(float elementWidth)
  {
    return this.growDirection == 2 || this.growDirection == 3 ? this.CurX : this.CurX - elementWidth;
  }

  public void IncrementPosition(float amount)
  {
    if (this.growDirection == 2 || this.growDirection == 3)
      this.CurX += amount;
    else
      this.CurX -= amount;
    if ((double) Mathf.Abs(this.CurX - this.startX) <= (double) this.maxWidth)
      return;
    this.IncrementY();
  }

  public void IncrementY()
  {
    if (this.growDirection == 2 || this.growDirection == null)
      this.CurY -= 24f + this.CellGap;
    else
      this.CurY += 24f + this.CellGap;
    ++this.rows;
    this.CurX = this.startX;
  }

  public void IncrementYIfWillExceedMaxWidth(float width)
  {
    if ((double) Mathf.Abs(this.CurX - this.startX) + (double) Mathf.Abs(width) <= (double) this.maxWidth)
      return;
    this.IncrementY();
  }

  public void Gap(float width)
  {
    if ((double) this.CurX == (double) this.startX)
      return;
    this.IncrementPosition(width);
  }

  public bool RowSelect(
    bool selected,
    string tooltip = null,
    Color? mouseoverBackgroundColor = null,
    Color? backgroundColor = null,
    bool doMouseoverSound = true,
    float fixedWidth = -1f)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.startX, this.startY, (double) fixedWidth > 0.0 ? fixedWidth : this.maxWidth, (float) this.rows * 24f);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    if (doMouseoverSound)
      MouseoverSounds.DoRegion(rect);
    if (Mouse.IsOver(rect) | selected)
      Widgets.DrawRectFast(rect, mouseoverBackgroundColor ?? ListingExtension.LightHighlightColor, (GUIContent) null);
    else if (backgroundColor.HasValue && !Mouse.IsOver(rect))
      Widgets.DrawRectFast(rect, backgroundColor.Value, (GUIContent) null);
    return Widgets.ButtonInvisible(rect, true);
  }

  public bool ButtonIcon(
    Texture2D tex,
    string tooltip = null,
    Color? mouseoverColor = null,
    Color? backgroundColor = null,
    Color? mouseoverBackgroundColor = null,
    bool doMouseoverSound = true,
    float overrideSize = -1f)
  {
    float num1 = (double) overrideSize > 0.0 ? overrideSize : 24f;
    float num2 = (float) ((24.0 - (double) num1) / 2.0);
    this.IncrementYIfWillExceedMaxWidth(num1);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(num1) + num2, this.CurY + num2, num1, num1);
    if (doMouseoverSound)
      MouseoverSounds.DoRegion(rect);
    if (mouseoverBackgroundColor.HasValue && Mouse.IsOver(rect))
      Widgets.DrawRectFast(rect, mouseoverBackgroundColor.Value, (GUIContent) null);
    else if (backgroundColor.HasValue && !Mouse.IsOver(rect))
      Widgets.DrawRectFast(rect, backgroundColor.Value, (GUIContent) null);
    bool flag = Widgets.ButtonImage(rect, tex, Color.white, mouseoverColor ?? GenUI.MouseoverColor, true, (string) null);
    this.IncrementPosition(num1);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    return flag;
  }

  public bool ButtonIconWithBG(
    Texture2D texture,
    float width = -1f,
    string tooltip = null,
    bool doMouseoverSound = true)
  {
    if ((double) width < 0.0)
      width = 24f;
    width += 16f;
    this.IncrementYIfWillExceedMaxWidth(width);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(width), this.CurY, width, 26f);
    if (doMouseoverSound)
      MouseoverSounds.DoRegion(rect);
    bool flag = Widgets.ButtonImageWithBG(rect, texture, new Vector2?(Vector2.op_Multiply(Vector2.one, 24f)));
    this.IncrementPosition(width + this.CellGap);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    return flag;
  }

  public void ToggleableIcon(
    ref bool toggleable,
    Texture2D tex,
    string tooltip,
    SoundDef mouseoverSound = null,
    string tutorTag = null)
  {
    this.IncrementYIfWillExceedMaxWidth(24f);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(this.LeftX(24f), this.CurY, 24f, 24f);
    bool flag = Widgets.ButtonImage(rect1, tex, true, (string) null);
    this.IncrementPosition(24f + this.CellGap);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(tooltip));
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x + ((Rect) ref rect1).width / 2f, ((Rect) ref rect1).y, ((Rect) ref rect1).height / 2f, ((Rect) ref rect1).height / 2f);
    Texture2D texture2D = toggleable ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
    GUI.DrawTexture(rect2, (Texture) texture2D);
    if (mouseoverSound != null)
      MouseoverSounds.DoRegion(rect1, mouseoverSound);
    if (flag)
    {
      toggleable = !toggleable;
      if (toggleable)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
    }
    if (tutorTag == null)
      return;
    UIHighlighter.HighlightOpportunity(rect1, tutorTag);
  }

  public Rect Icon(Texture tex, string tooltip = null)
  {
    this.IncrementYIfWillExceedMaxWidth(24f);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(24f), this.CurY, 24f, 24f);
    GUI.DrawTexture(rect, tex);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    this.IncrementPosition(24f + this.CellGap);
    return rect;
  }

  public Rect DefIcon(ThingDef def, string tooltip = null)
  {
    this.IncrementYIfWillExceedMaxWidth(24f);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(24f), this.CurY, 24f, 24f);
    Widgets.DefIcon(rect, (Def) def, (ThingDef) null, 1f, (ThingStyleDef) null, false, new Color?(), (Material) null, new int?(), 1f);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    this.IncrementPosition(24f + this.CellGap);
    return rect;
  }

  public bool ButtonText(
    string label,
    string tooltip = null,
    bool drawBackground = true,
    bool doMouseoverSound = true,
    bool active = true,
    float? fixedWidth = null)
  {
    Rect rect = this.ButtonRect(label, fixedWidth);
    bool flag = Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, active, new TextAnchor?());
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    return flag;
  }

  public Rect ButtonRect(string label, float? fixedWidth = null)
  {
    Vector2 vector2 = fixedWidth.HasValue ? new Vector2(fixedWidth.Value, 24f) : Text.CalcSize(label);
    vector2.x += 16f;
    vector2.y += 2f;
    this.IncrementYIfWillExceedMaxWidth(vector2.x);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(vector2.x), this.CurY, vector2.x, vector2.y);
    this.IncrementPosition(((Rect) ref rect).width + this.CellGap);
    return rect;
  }

  public bool Checkbox(ref bool checkOn, string tooltip = null)
  {
    this.IncrementYIfWillExceedMaxWidth(24f);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(24f), this.CurY, 24f, 24f);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    bool flag = checkOn;
    Widgets.Checkbox(((Rect) ref rect).position, ref checkOn, 24f, false, false, (Texture2D) null, (Texture2D) null);
    this.IncrementPosition(24f + this.CellGap);
    return flag != checkOn;
  }

  public Rect Label(string text, float width = -1f, string tooltip = null, float height = -1f)
  {
    if ((double) height < 0.0)
      height = 24f;
    if ((double) width < 0.0)
      width = Text.CalcSize(text).x;
    this.IncrementYIfWillExceedMaxWidth(width + 2f);
    this.IncrementPosition(2f);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(width), this.CurY + this.CellGap / 2f, width, height);
    Widgets.Label(rect, text);
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    this.IncrementPosition(2f);
    this.IncrementPosition(((Rect) ref rect).width);
    return rect;
  }

  public Rect TextFieldNumeric(ref int val, ref string buffer, float width = -1f)
  {
    if ((double) width < 0.0)
      width = Text.CalcSize(val.ToString()).x;
    this.IncrementYIfWillExceedMaxWidth(width + 2f);
    this.IncrementPosition(2f);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(this.LeftX(width), this.CurY, width, 24f);
    Widgets.TextFieldNumeric<int>(rect, ref val, ref buffer, 0.0f, 1E+09f);
    this.IncrementPosition(2f);
    this.IncrementPosition(((Rect) ref rect).width);
    return rect;
  }

  public Rect FillableBar(
    float width,
    float height,
    float fillPct,
    string label,
    Texture2D fillTex,
    Texture2D bgTex = null)
  {
    this.IncrementYIfWillExceedMaxWidth(width);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(this.LeftX(width), this.CurY, width, height);
    Widgets.FillableBar(rect1, fillPct, fillTex, bgTex, false);
    if (!label.NullOrEmpty<char>())
    {
      Rect rect2 = rect1;
      ref Rect local1 = ref rect2;
      ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + 2f;
      ref Rect local2 = ref rect2;
      ((Rect) ref local2).xMax = ((Rect) ref local2).xMax - 2f;
      if (Text.Anchor >= 0)
      {
        ref Rect local3 = ref rect2;
        ((Rect) ref local3).height = ((Rect) ref local3).height + 14f;
      }
      Text.Font = (GameFont) 0;
      Text.WordWrap = false;
      Widgets.Label(rect2, label);
      Text.WordWrap = true;
    }
    this.IncrementPosition(width);
    return rect1;
  }
}
