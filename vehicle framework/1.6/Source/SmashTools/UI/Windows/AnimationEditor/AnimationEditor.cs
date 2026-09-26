// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationEditor
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Animations;

public abstract class AnimationEditor
{
  public const float DropdownWidth = 300f;
  protected readonly Dialog_AnimationEditor parent;
  protected const float fadeSize = 10f;
  protected const int fadeLines = 10;
  protected const float fadeHeight = 1f;
  protected readonly Color backgroundLightColor;
  protected readonly Color backgroundDopesheetColor;
  protected readonly Color backgroundCurvesColor;
  protected readonly Color separatorColor;
  protected readonly Color buttonColor;
  protected readonly Color buttonPressedColor;
  protected readonly Color selectBoxFillColor;
  protected readonly Color selectBoxBorderColor;
  protected bool draggingSelectionBox;
  private bool hardDisabled;
  private bool resizing;
  private float startingWidth;
  private Vector2 selectionBoxPos;

  public AnimationEditor(Dialog_AnimationEditor parent)
  {
    ColorInt colorInt1 = new ColorInt(63 /*0x3F*/, 63 /*0x3F*/, 63 /*0x3F*/);
    this.backgroundLightColor = ((ColorInt) ref colorInt1).ToColor;
    ColorInt colorInt2 = new ColorInt(56, 56, 56);
    this.backgroundDopesheetColor = ((ColorInt) ref colorInt2).ToColor;
    ColorInt colorInt3 = new ColorInt(40, 40, 40);
    this.backgroundCurvesColor = ((ColorInt) ref colorInt3).ToColor;
    ColorInt colorInt4 = new ColorInt(35, 35, 35);
    this.separatorColor = ((ColorInt) ref colorInt4).ToColor;
    ColorInt colorInt5 = new ColorInt(88, 88, 88);
    this.buttonColor = ((ColorInt) ref colorInt5).ToColor;
    ColorInt colorInt6 = new ColorInt(70, 96 /*0x60*/, 124);
    this.buttonPressedColor = ((ColorInt) ref colorInt6).ToColor;
    ColorInt colorInt7 = new ColorInt(85, 145, 245, 15);
    this.selectBoxFillColor = ((ColorInt) ref colorInt7).ToColor;
    ColorInt colorInt8 = new ColorInt(125, 175, 245, 75);
    this.selectBoxBorderColor = ((ColorInt) ref colorInt8).ToColor;
    // ISSUE: explicit constructor call
    base.\u002Ector();
    this.parent = parent;
  }

  public bool SingleSelect
  {
    get
    {
      return Input.GetMouseButtonDown(0) && !Input.GetKey((KeyCode) 306) && !Input.GetKey((KeyCode) 304);
    }
  }

  public bool LeftClickDown
  {
    get => Event.current != null && Event.current.type == null && Event.current.button == 0;
  }

  public bool RightClickDown
  {
    get => Event.current != null && Event.current.type == null && Event.current.button == 1;
  }

  public bool LeftClickUp
  {
    get => Event.current != null && Event.current.type == 1 && Event.current.button == 0;
  }

  public bool RightClickUp
  {
    get => Event.current != null && Event.current.type == 1 && Event.current.button == 1;
  }

  public bool ShiftClick => Input.GetKey((KeyCode) 304);

  public bool ControlClick => Input.GetKey((KeyCode) 306) || Input.GetKey((KeyCode) 310);

  public abstract void Draw(Rect rect);

  public virtual void Update()
  {
  }

  public virtual void OnClose()
  {
  }

  public virtual void OnGUIHighPriority()
  {
    if (Event.current == null || Event.current.type != 4)
      return;
    if (Event.current.keyCode == 115 && this.ControlClick)
    {
      this.Save();
      Event.current.Use();
    }
    if (Event.current.keyCode == 99 && this.ControlClick)
    {
      this.CopyToClipboard();
      Event.current.Use();
    }
    if (Event.current.keyCode == 118 && this.ControlClick)
    {
      this.Paste();
      Event.current.Use();
    }
    if (Event.current.keyCode == (int) sbyte.MaxValue || Event.current.keyCode == 8)
    {
      this.Delete();
      Event.current.Use();
    }
    if (Event.current.keyCode != 27)
      return;
    this.Escape();
    Event.current.Use();
  }

  public virtual void Save()
  {
  }

  public virtual void CopyToClipboard()
  {
  }

  public virtual void Paste()
  {
  }

  public virtual void Delete()
  {
  }

  public virtual void Escape()
  {
  }

  public virtual void AnimatorLoaded(IAnimator animator)
  {
  }

  public virtual void OnTabOpen()
  {
  }

  public virtual void ResetToCenter()
  {
  }

  protected void DrawBackground(Rect rect)
  {
    Widgets.DrawBoxSolidWithOutline(rect, this.backgroundDopesheetColor, this.separatorColor, 1);
  }

  protected void DrawBackgroundDark(Rect rect)
  {
    Widgets.DrawBoxSolidWithOutline(rect, this.backgroundCurvesColor, this.separatorColor, 1);
  }

  protected void DoSeparatorHorizontal(float x, float y, float length)
  {
    UIElements.DrawLineHorizontal(x, y, length, this.separatorColor);
  }

  protected void DoSeparatorVertical(float x, float y, float height)
  {
    UIElements.DrawLineVertical(x, y, height, this.separatorColor);
  }

  protected void EnableGUI(bool hardEnable = false)
  {
    if (this.hardDisabled && !hardEnable)
      return;
    GUIState.Enable();
  }

  protected void DisableGUI(bool hardDisable = false)
  {
    GUIState.Disable();
    this.hardDisabled = hardDisable;
  }

  protected bool AnimationButton(Rect rect, Texture2D texture, string tooltip)
  {
    Color color = GUI.color;
    if (GUI.enabled && Mouse.IsOver(rect))
      GUI.color = GenUI.MouseoverColor;
    GUI.DrawTexture(GenUI.ContractedBy(rect, 3f), (Texture) texture);
    GUI.color = color;
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    bool flag = Widgets.ButtonInvisible(rect, true);
    if (flag)
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    return flag;
  }

  protected bool ToggleText(Rect rect, string label, string tooltip, bool enabled)
  {
    bool flag = false;
    TextAnchor anchor = Text.Anchor;
    Text.Anchor = (TextAnchor) 4;
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width);
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect).yMax, ((Rect) ref rect).width);
    this.DoSeparatorVertical(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).height);
    this.DoSeparatorVertical(((Rect) ref rect).xMax, ((Rect) ref rect).y, ((Rect) ref rect).height);
    Color color = GUI.color;
    if (GUI.enabled && Mouse.IsOver(rect))
      GUI.color = new Color(0.75f, 0.75f, 0.75f);
    Widgets.Label(rect, label);
    if (enabled)
      Widgets.DrawBoxSolid(GenUI.ContractedBy(rect, 1f), new Color(0.75f, 0.75f, 0.75f, 0.25f));
    if (!tooltip.NullOrEmpty<char>())
      TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
    if (Widgets.ButtonInvisible(rect, true))
    {
      flag = true;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    GUI.color = color;
    Text.Anchor = anchor;
    return flag;
  }

  protected bool ButtonText(Rect rect, string label)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 4);
    try
    {
      bool flag = false;
      Color color = this.buttonColor;
      if (GUI.enabled && Mouse.IsOver(rect))
      {
        GUI.color = new Color(0.75f, 0.75f, 0.75f);
        if (Input.GetMouseButton(0))
          color = this.buttonPressedColor;
      }
      Widgets.DrawBoxSolidWithOutline(rect, color, this.separatorColor, 1);
      Widgets.Label(rect, label);
      if (Widgets.ButtonInvisible(rect, true))
      {
        flag = true;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      }
      return flag;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static bool Dropdown(Rect rect, string label, string tooltip)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 4);
    try
    {
      bool flag = false;
      if (GUI.enabled && Mouse.IsOver(rect))
        GUI.color = new Color(0.75f, 0.75f, 0.75f);
      float height = ((Rect) ref rect).height;
      Rect rect1;
      Rect rect2;
      GenUI.SplitVertically(rect, ((Rect) ref rect).width - height, ref rect1, ref rect2);
      if ((double) Text.CalcSize(label).x > (double) ((Rect) ref rect1).width)
        Text.Font = (GameFont) (int) (byte) (Text.Font - 1);
      Widgets.Label(rect1, GenText.Truncate(label, ((Rect) ref rect1).width, (Dictionary<string, string>) null));
      Matrix4x4 matrix = GUI.matrix;
      UI.RotateAroundPivot(90f, ((Rect) ref rect2).center);
      GUI.DrawTexture(rect2, (Texture) TexButton.Reveal);
      GUI.matrix = matrix;
      if (!tooltip.NullOrEmpty<char>())
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      if (Widgets.ButtonInvisible(rect, true))
      {
        flag = true;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      }
      return flag;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  protected void DoResizerButton(
    Rect rect,
    ref float leftWindowSize,
    float minLeft,
    float minRight)
  {
    float width = ((Rect) ref rect).width;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).xMax - 24f, ((Rect) ref rect).yMax - 24f, 24f, 24f);
    Vector2 mousePosition = Event.current.mousePosition;
    if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect1))
    {
      this.resizing = true;
      this.startingWidth = mousePosition.x;
    }
    if (this.resizing)
    {
      ((Rect) ref rect).width = this.startingWidth + (mousePosition.x - this.startingWidth);
      ((Rect) ref rect).width = Mathf.Clamp(((Rect) ref rect).width, minLeft, ((Rect) ref this.parent.windowRect).width - minRight);
      if (!Input.GetMouseButton(0))
        this.resizing = false;
    }
    Widgets.ButtonImage(rect1, TexUI.WinExpandWidget, true, (string) null);
    if ((double) ((Rect) ref rect).width == (double) width)
      return;
    leftWindowSize = ((Rect) ref rect).width;
  }

  protected void CheckTextFieldControlFocus(Rect rect)
  {
    string str = $"TextField{((Rect) ref rect).y:F0}{((Rect) ref rect).x:F0}";
    if (!(GUI.GetNameOfFocusedControl() == str) || !Input.GetMouseButtonDown(0) || Mouse.IsOver(rect))
      return;
    UI.UnfocusCurrentControl();
  }

  protected bool DragWindow(
    Rect rect,
    Action dragAction,
    Func<bool> isDragging,
    Action dragStarted = null,
    Action dragStopped = null,
    int button = 0)
  {
    if (!GUI.enabled)
      return false;
    if (Event.current != null && Event.current.type == null && Event.current.button == button && Mouse.IsOver(rect))
    {
      if (dragStarted != null)
        dragStarted();
      Event.current.Use();
    }
    if (!isDragging())
      return false;
    bool flag = Event.current != null && Event.current.type == 1 && Event.current.button == button;
    dragAction();
    if (Input.GetMouseButton(button))
    {
      if (UnityGUIBugsFixer.MouseDrag(button))
        Event.current.Use();
    }
    else
    {
      if (dragStopped != null)
        dragStopped();
      if (flag)
        Event.current.Use();
    }
    return true;
  }

  protected bool SelectionBox(
    Vector2 groupPos,
    Rect visibleRect,
    Rect clickArea,
    out Rect dragRect,
    float snapX = 0.0f,
    float snapY = 0.0f,
    float snapPaddingX = 0.0f,
    float snapPaddingY = 0.0f)
  {
    dragRect = Rect.zero;
    if (Input.GetMouseButtonDown(0) && Mouse.IsOver(clickArea))
    {
      this.draggingSelectionBox = true;
      this.selectionBoxPos = this.MouseUIPos(Vector2.op_Subtraction(groupPos, ((Rect) ref visibleRect).position));
    }
    if (this.draggingSelectionBox)
    {
      dragRect = this.DragRect(Vector2.op_Subtraction(groupPos, ((Rect) ref visibleRect).position), snapX, snapY, snapPaddingX, snapPaddingY);
      Widgets.DrawBoxSolidWithOutline(dragRect, this.selectBoxFillColor, this.selectBoxBorderColor, 1);
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 4);
      try
      {
        Vector2 vector2 = Text.CalcSize(dragRect.ToString());
        Rect rect;
        // ISSUE: explicit constructor call
        ((Rect) ref rect).\u002Ector(((Rect) ref dragRect).xMax - vector2.x, ((Rect) ref dragRect).yMax + vector2.y, vector2.x, vector2.y);
        Widgets.DrawBoxSolid(rect, new Color(0.0f, 0.0f, 0.0f, 0.25f));
        Widgets.Label(rect, dragRect.ToString());
      }
      finally
      {
        textBlock.Dispose();
      }
      if (Input.GetMouseButton(0))
      {
        if (UnityGUIBugsFixer.MouseDrag(0))
          Event.current.Use();
      }
      else
      {
        if (Input.GetMouseButtonUp(0))
          Event.current.Use();
        this.draggingSelectionBox = false;
        return true;
      }
    }
    return false;
  }

  protected void InputBox(Rect rect, Type type, ref object value, ref string buffer)
  {
    if (buffer == null)
      buffer = value?.ToString() ?? type.GetDefaultValue().ToString();
    string str = $"InputBox{((Rect) ref rect).y.ToString("F0")}{((Rect) ref rect).x.ToString("F0")}";
    GUI.SetNextControlName(str);
    string edited = Widgets.TextField(rect, buffer);
    if (GUI.GetNameOfFocusedControl() != str)
    {
      AnimationEditor.ResolveParseNow(type, buffer, ref value, ref buffer);
    }
    else
    {
      if (!(edited != buffer))
        return;
      buffer = edited;
      AnimationEditor.ResolveParseNow(type, edited, ref value, ref buffer);
    }
  }

  private static void ResolveParseNow(
    Type type,
    string edited,
    ref object value,
    ref string buffer)
  {
    if (!ParseHelper.CanParse(type, edited))
      buffer = (string) null;
    else
      value = ParseHelper.FromString(edited, type);
  }

  protected Rect DragRect(
    Vector2 groupPos,
    float snapX = 0.0f,
    float snapY = 0.0f,
    float snapPaddingX = 0.0f,
    float snapPaddingY = 0.0f)
  {
    Vector2 vector2_1 = this.MouseUIPos(groupPos);
    if ((double) snapX > 0.0)
      vector2_1.x = vector2_1.x.RoundTo(snapX) + snapPaddingX;
    if ((double) snapY > 0.0)
      vector2_1.y = vector2_1.y.RoundTo(snapY) + snapPaddingY;
    Vector2 selectionBoxPos = this.selectionBoxPos;
    if ((double) snapX > 0.0)
      selectionBoxPos.x = selectionBoxPos.x.RoundTo(snapX) + snapPaddingX;
    if ((double) snapY > 0.0)
      selectionBoxPos.y = selectionBoxPos.y.RoundTo(snapY) + snapPaddingY;
    Vector2 vector2_2 = Vector2.op_Subtraction(new Vector2(vector2_1.x, vector2_1.y), selectionBoxPos);
    return new Rect(selectionBoxPos, vector2_2);
  }

  protected static float DrawBlend(Rect rect, Color colorOne, Color colorTwo)
  {
    for (int index = 0; index < 10; ++index)
    {
      float num1 = (float) index / 10f;
      float num2 = Mathf.Lerp(colorOne.r, colorTwo.r, num1);
      float num3 = Mathf.Lerp(colorOne.g, colorTwo.g, num1);
      float num4 = Mathf.Lerp(colorOne.b, colorTwo.b, num1);
      float num5 = colorOne.a;
      if ((double) colorOne.a != (double) colorTwo.a)
        num5 = Mathf.Lerp(colorOne.a, colorTwo.a, num1);
      Color color;
      // ISSUE: explicit constructor call
      ((Color) ref color).\u002Ector(num2, num3, num4, num5);
      Widgets.DrawBoxSolid(rect, color);
      ref Rect local = ref rect;
      ((Rect) ref local).y = ((Rect) ref local).y + 1f;
    }
    return ((Rect) ref rect).y;
  }

  protected Vector2 MouseUIPos(Vector2 groupPos)
  {
    Vector2 vector2_1;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_1).\u002Ector(UI.MousePositionOnUIInverted.x, UI.MousePositionOnUIInverted.y);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_2).\u002Ector(this.parent.EditorMargin, this.parent.EditorMargin);
    return Vector2.op_Subtraction(Vector2.op_Subtraction(Vector2.op_Subtraction(vector2_1, ((Rect) ref this.parent.windowRect).position), groupPos), vector2_2);
  }

  protected Vector2 GetScrollPosNormalized(Rect outRect, Vector2 scrollPos, Rect viewRect)
  {
    float num1 = ((Rect) ref viewRect).width - ((Rect) ref outRect).width;
    if ((double) ((Rect) ref viewRect).height > (double) ((Rect) ref outRect).height)
      num1 -= 16f;
    float num2 = ((Rect) ref viewRect).height - ((Rect) ref outRect).height;
    double width1 = (double) ((Rect) ref viewRect).width;
    double width2 = (double) ((Rect) ref outRect).width;
    return new Vector2((double) num1 <= 0.0 ? 1f : scrollPos.x / num1, (double) num2 <= 0.0 ? 1f : scrollPos.y / num2);
  }

  protected void SetScrollPosNormalized(
    Rect outRect,
    ref Vector2 scrollPos,
    Rect viewRect,
    Vector2 normalizedScrollPos)
  {
    float num1 = (float) ((double) ((Rect) ref viewRect).width - (double) ((Rect) ref outRect).width + 16.0);
    float num2 = (float) ((double) ((Rect) ref viewRect).height - (double) ((Rect) ref outRect).height + 16.0);
    scrollPos = new Vector2(normalizedScrollPos.x * num1, normalizedScrollPos.y * num2);
  }

  protected void TryResetExtraScrollSize(
    Rect outRect,
    Vector2 scrollPos,
    Rect viewRect,
    ref Vector2 extraSize)
  {
    this.GetScrollPosNormalized(outRect, scrollPos, viewRect);
    throw new NotImplementedException();
  }

  protected Rect GetVisibleRect(Rect outRect, Vector2 scrollPos, Rect viewRect)
  {
    Vector2 scrollPosNormalized = this.GetScrollPosNormalized(outRect, scrollPos, viewRect);
    return new Rect(Mathf.Lerp(0.0f, ((Rect) ref viewRect).width - ((Rect) ref outRect).width, scrollPosNormalized.x), Mathf.Lerp(0.0f, ((Rect) ref viewRect).height - ((Rect) ref outRect).height, scrollPosNormalized.y), ((Rect) ref outRect).width, ((Rect) ref outRect).height);
  }
}
