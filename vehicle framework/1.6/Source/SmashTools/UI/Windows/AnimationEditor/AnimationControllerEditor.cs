// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationControllerEditor
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Animations;

public class AnimationControllerEditor : AnimationEditor
{
  private const float MinLeftWindowSize = 244f;
  private const float MinRightWindowSize = 300f;
  private const int GridSize = 150;
  private const float WidgetBarHeight = 24f;
  private const float LayerItemHeight = 38f;
  private const float LayerInputWidth = 75f;
  private const float ParameterItemHeight = 32f;
  private const float ParameterInputWidth = 100f;
  private const float TransitionLineWidth = 2f;
  private const float TransitionAnchorSpacing = 5f;
  private const float TabWidth = 110f;
  public const int GridSquareSize = 20;
  private const float TopBarIconSize = 24f;
  private const float HighlightPadding = 2f;
  public const int StateWidth = 16 /*0x10*/;
  private const int StateHeight = 4;
  private const int StateHeightSmall = 3;
  private const float ZoomRate = 0.03f;
  private readonly Texture2D stateTex;
  private readonly Texture2D stateMachineTex;
  private readonly Texture2D eyeTex;
  private readonly Texture2D eyeStrikedTex;
  private readonly Color lineDarkColor;
  private readonly Color lineLightColor;
  private readonly Color topBarFadeColor;
  private readonly Color entryStateColor;
  private readonly Color defaultStateColor;
  private readonly Color exitStateColor;
  private readonly Color anyStateColor;
  private readonly Color stateColor;
  private readonly Color highlightColor;
  private readonly AnimationControllerEditor.Selector selector;
  private readonly AnimationControllerEditor.Clipboard clipboard;
  private readonly QuickSearchFilter parameterFilter;
  private readonly Vector2 gridSize;
  private bool hideLeftWindow;
  private float leftWindowSize;
  private bool initialized;
  private float zoom;
  private Vector2 scrollPos;
  private Vector2 dragPos;
  private AnimationControllerEditor.DragItem dragging;
  private AnimationControllerEditor.LeftSection leftSectionTab;
  private IntVec2 mouseGridPos;
  private IntVec2 clickedGridPos;
  private IntVec2 draggingStateOrigPos;
  private bool draggingState;
  private AnimationState makingTransitionFrom;
  private string motionSpeedBuffer;
  private string cycleOffsetBuffer;

  public AnimationControllerEditor(Dialog_AnimationEditor parent)
  {
    ColorInt colorInt1 = new ColorInt(25, 25, 25);
    this.lineDarkColor = ((ColorInt) ref colorInt1).ToColor;
    ColorInt colorInt2 = new ColorInt(35, 35, 35);
    this.lineLightColor = ((ColorInt) ref colorInt2).ToColor;
    ColorInt colorInt3 = new ColorInt(40, 40, 40);
    this.topBarFadeColor = ((ColorInt) ref colorInt3).ToColor;
    ColorInt colorInt4 = new ColorInt(20, 110, 50);
    this.entryStateColor = ((ColorInt) ref colorInt4).ToColor;
    ColorInt colorInt5 = new ColorInt(185, 105, 25);
    this.defaultStateColor = ((ColorInt) ref colorInt5).ToColor;
    ColorInt colorInt6 = new ColorInt(150, 25, 25);
    this.exitStateColor = ((ColorInt) ref colorInt6).ToColor;
    ColorInt colorInt7 = new ColorInt(90, 160 /*0xA0*/, 140);
    this.anyStateColor = ((ColorInt) ref colorInt7).ToColor;
    ColorInt colorInt8 = new ColorInt(75, 75, 75);
    this.stateColor = ((ColorInt) ref colorInt8).ToColor;
    this.highlightColor = ColorExtension.ToTransparent(Widgets.HighlightStrongBgColor, 0.75f);
    this.selector = new AnimationControllerEditor.Selector();
    this.clipboard = new AnimationControllerEditor.Clipboard();
    this.parameterFilter = new QuickSearchFilter();
    this.gridSize = new Vector2(3000f, 3000f);
    this.leftWindowSize = 244f;
    this.zoom = 2f;
    // ISSUE: explicit constructor call
    base.\u002Ector(parent);
    this.scrollPos = new Vector2(this.gridSize.x / 2f, this.gridSize.y / 2f);
  }

  private bool UnsavedChanges { get; set; }

  private AnimationLayer EditingLayer { get; set; }

  private AnimationParameter EditingParameter { get; set; }

  private bool HideLeftWindow
  {
    get => this.hideLeftWindow;
    set
    {
      if (value == this.hideLeftWindow)
        return;
      if (value)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
      this.hideLeftWindow = value;
    }
  }

  private Vector2 MousePosLeftWindowAdjust
  {
    get
    {
      Vector2 leftWindowAdjust;
      ((Vector2) ref leftWindowAdjust).\u002Ector(this.HideLeftWindow ? 0.0f : this.leftWindowSize + 8f, 56f);
      return leftWindowAdjust;
    }
  }

  private GameFont Font
  {
    get
    {
      if ((double) this.zoom < 1.5)
        return (GameFont) 2;
      return (double) this.zoom < 2.0 ? (GameFont) 1 : (GameFont) 0;
    }
  }

  private float MaxZoom(Rect rect) => this.gridSize.y / ((Rect) ref rect).size.y;

  public override void OnTabOpen() => base.OnTabOpen();

  public override void ResetToCenter()
  {
    base.ResetToCenter();
    this.initialized = false;
    this.zoom = 2f;
  }

  private void ChangeMade()
  {
    this.parent.ChangeMade();
    this.UnsavedChanges = true;
  }

  public override void Save()
  {
    if (!(bool) this.parent.controller)
      return;
    AnimationLoader.Save<AnimationController>(this.parent.controller);
  }

  public override void CopyToClipboard()
  {
    if (!this.selector.AnyStatesSelected)
      return;
    this.CopySelectedStates();
  }

  public override void Paste()
  {
    if (this.clipboard.IsEmpty)
      return;
    this.PasteState();
  }

  public override void Delete()
  {
    if (!this.selector.AnySelected)
      return;
    this.DeleteSelection();
  }

  public override void Escape() => this.StopMakingTransition();

  public override void Draw(Rect rect)
  {
    if (this.parent.animLayer == null)
      this.parent.animLayer = this.parent.controller.layers.FirstOrDefault<AnimationLayer>();
    if (this.HideLeftWindow)
    {
      this.DrawControllerSectionRight(rect);
    }
    else
    {
      Rect rect1;
      Rect rect2;
      GenUI.SplitVertically(rect, this.leftWindowSize, ref rect1, ref rect2);
      this.DrawControllerSectionLeft(rect1);
      this.DrawControllerSectionRight(rect2);
    }
  }

  private void DrawControllerSectionLeft(Rect rect)
  {
    this.DrawBackground(rect);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, 110f, 24f);
    if (this.ToggleText(rect1, TaggedString.op_Implicit(Translator.Translate("ST_Layers")), (string) null, this.leftSectionTab == AnimationControllerEditor.LeftSection.Layers))
      this.leftSectionTab = AnimationControllerEditor.LeftSection.Layers;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).x = ((Rect) ref local1).x + ((Rect) ref rect1).width;
    Rect rect2 = GenUI.ContractedBy(new Rect(((Rect) ref rect).xMax - 24f, ((Rect) ref rect).y, 24f, 24f), 2f);
    if (!this.HideLeftWindow && Widgets.ButtonImage(rect2, this.eyeTex, true, (string) null))
      this.HideLeftWindow = true;
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width);
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect1).yMax, ((Rect) ref rect).width);
    switch (this.leftSectionTab)
    {
      case AnimationControllerEditor.LeftSection.Layers:
        this.DrawLayersTab(rect);
        break;
      case AnimationControllerEditor.LeftSection.Parameters:
        this.DrawParametersTab(rect);
        break;
    }
    if (this.selector.AnyStatesSelected)
    {
      float num = ((Rect) ref rect).height * 0.65f;
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).yMax - num, ((Rect) ref rect).width, num);
      this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect3).y - 1f, ((Rect) ref rect).width);
      this.DrawBackground(rect3);
      this.DrawStateProperties(rect3);
    }
    if (this.selector.AnyTransitionsSelected)
    {
      float num = ((Rect) ref rect).height * 0.65f;
      Rect rect4;
      // ISSUE: explicit constructor call
      ((Rect) ref rect4).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).yMax - num, ((Rect) ref rect).width, num);
      this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect4).y - 1f, ((Rect) ref rect).width);
      this.DrawBackground(rect4);
      this.DrawTransitionProperties(rect4);
    }
    ref Rect local2 = ref rect;
    ((Rect) ref local2).yMin = ((Rect) ref local2).yMin + ((Rect) ref rect1).height;
    this.DoResizerButton(rect, ref this.leftWindowSize, 244f, 300f);
  }

  private void DrawStateProperties(Rect rect)
  {
    rect = GenUI.ContractedBy(rect, 4f);
    AnimationState state = this.selector.SelectedStates.FirstOrDefault<AnimationState>();
    if (state.Type == AnimationState.StateType.Entry || state.Type == AnimationState.StateType.Exit)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y + 24f, ((Rect) ref rect).width, 24f);
    state.name = Widgets.TextField(rect1, state.name);
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).y = ((Rect) ref local1).y + 5f;
    this.DoSeparatorHorizontal(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width);
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).y = ((Rect) ref local2).y + 24f;
    Rect rect2;
    Rect rect3;
    GenUI.SplitVertically(rect1, ((Rect) ref rect1).width * 0.45f, ref rect2, ref rect3);
    GUI.enabled = AnimationLoader.Cache<AnimationClip>.Count > 0;
    Widgets.Label(rect2, Translator.Translate("ST_MotionFile"));
    if (AnimationEditor.Dropdown(rect3, state.clip?.FileName ?? string.Empty, state.clip?.FilePath))
    {
      Rect rect4;
      // ISSUE: explicit constructor call
      ((Rect) ref rect4).\u002Ector(((Rect) ref this.parent.windowRect).x + this.parent.EditorMargin + ((Rect) ref rect1).x, ((Rect) ref this.parent.windowRect).y + this.parent.EditorMargin + ((Rect) ref rect3).yMax, 300f, 500f);
      Find.WindowStack.Add((Window) new Dialog_AnimationClipLister(this.parent.animator, rect4, state.clip, new Dialog_ItemDropdown<AnimationClip>.CreateItemButton("None", (Func<AnimationClip>) (() => (AnimationClip) null)), (Action<AnimationClip>) (clip => state.clip = clip)));
    }
    GUI.enabled = true;
    ref Rect local3 = ref rect1;
    ((Rect) ref local3).y = ((Rect) ref local3).y + 24f;
    Rect rect5;
    Rect rect6;
    GenUI.SplitVertically(rect1, ((Rect) ref rect1).width * 0.65f, ref rect5, ref rect6);
    Widgets.Label(rect5, Translator.Translate("ST_MotionSpeed"));
    Widgets.TextFieldNumeric<float>(rect6, ref state.speed, ref this.motionSpeedBuffer, 0.0f, 1E+09f);
    ref Rect local4 = ref rect1;
    ((Rect) ref local4).y = ((Rect) ref local4).y + 24f;
    if (state.clip == null)
      return;
    this.DoSeparatorHorizontal(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width);
    ref Rect local5 = ref rect1;
    ((Rect) ref local5).y = ((Rect) ref local5).y + 2f;
    UIElements.CheckboxLabeled(rect1, TaggedString.op_Implicit(Translator.Translate("ST_Loop")), ref state.loop);
    ref Rect local6 = ref rect1;
    ((Rect) ref local6).y = ((Rect) ref local6).y + 24f;
    GUI.enabled = state.loop;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(GUI.enabled ? Color.white : Widgets.InactiveColor);
    try
    {
      Widgets.TextFieldNumericLabeled<int>(rect1, TaggedString.op_Implicit(Translator.Translate("ST_CycleOffset")), ref state.clip.cycleOffset, ref this.cycleOffsetBuffer, 0.0f, 1E+09f);
      ref Rect local7 = ref rect1;
      ((Rect) ref local7).y = ((Rect) ref local7).y + 24f;
    }
    finally
    {
      textBlock.Dispose();
    }
    GUI.enabled = true;
    this.DoSeparatorHorizontal(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width);
    ref Rect local8 = ref rect1;
    ((Rect) ref local8).y = ((Rect) ref local8).y + 2f;
  }

  private void DrawTransitionProperties(Rect rect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 1), new TextAnchor?((TextAnchor) 3), new bool?(false));
    try
    {
      rect = GenUI.ContractedBy(rect, 4f);
      Rect rect1 = new Rect(((Rect) ref rect).x, ((Rect) ref rect).y + 24f, ((Rect) ref rect).width, 24f);
      AnimationTransition animationTransition = this.selector.SelectedTransitions.LastOrDefault<AnimationTransition>();
      if (animationTransition.FromState.Type == AnimationState.StateType.Entry || animationTransition.ToState.Type == AnimationState.StateType.Exit)
      {
        Widgets.Label(rect1, Translator.Translate("ST_NoControllerParameters"));
      }
      else
      {
        Widgets.Label(rect1, Translator.Translate("ST_Conditions"));
        this.DoSeparatorHorizontal(((Rect) ref rect1).x, ((Rect) ref rect1).yMax, ((Rect) ref rect1).width);
        ref Rect local1 = ref rect1;
        ((Rect) ref local1).y = ((Rect) ref local1).y + 5f;
        foreach (AnimationCondition condition in animationTransition.conditions)
        {
          ref Rect local2 = ref rect1;
          ((Rect) ref local2).y = ((Rect) ref local2).y + ((Rect) ref rect1).height;
          condition.DrawConditionInput(rect1);
        }
        ref Rect local3 = ref rect1;
        ((Rect) ref local3).y = ((Rect) ref local3).y + ((Rect) ref rect1).height;
        Color color1 = GUI.enabled ? Color.white : Widgets.InactiveColor;
        Color color2 = GUI.enabled ? GenUI.MouseoverColor : Widgets.InactiveColor;
        if (!Widgets.ButtonImage(GenUI.ContractedBy(new Rect((float) ((double) ((Rect) ref rect1).xMax - 24.0 - 5.0), ((Rect) ref rect1).y, 24f, 24f), 2f), TexButton.Plus, color1, color2, GUI.enabled, (string) null))
          return;
        animationTransition.AddCondition();
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawLayersTab(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y + 24f, ((Rect) ref rect).width, 24f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector((float) ((double) ((Rect) ref rect1).xMax - 24.0 - 5.0), ((Rect) ref rect1).y, 24f, 24f);
    if (Widgets.ButtonImage(GenUI.ContractedBy(rect2, 2f), TexButton.Plus, true, (string) null))
      this.parent.controller.AddLayer("New Layer");
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect1).yMax, ((Rect) ref rect).width);
    Widgets.DrawBoxSolid(GenUI.ContractedBy(new Rect(((Rect) ref rect).x, ((Rect) ref rect1).yMax, ((Rect) ref rect).width, 5f), 1f), this.backgroundDopesheetColor.Subtract255NoAlpha(5, 5, 5));
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect1).yMax, ((Rect) ref rect).width, 38f);
    foreach (AnimationLayer layer in this.parent.controller.layers)
    {
      if (this.parent.animLayer == layer)
        Widgets.DrawBoxSolid(rect3, this.backgroundDopesheetColor.Add255NoAlpha(10, 10, 10));
      Rect rect4;
      // ISSUE: explicit constructor call
      ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).x, ((Rect) ref rect3).y, ((Rect) ref rect3).height, ((Rect) ref rect3).height);
      Rect rect5;
      // ISSUE: explicit constructor call
      ((Rect) ref rect5).\u002Ector(((Rect) ref rect4).xMax, ((Rect) ref rect3).y + 5f, (float) ((double) ((Rect) ref rect3).width - (double) ((Rect) ref rect4).width - 75.0), 24f);
      Text.Anchor = (TextAnchor) 3;
      if (this.EditingLayer == layer)
      {
        layer.name = Widgets.TextField(rect5, layer.name);
        if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(rect5))
          this.ConfirmLayerEdit();
        if (Input.GetKeyDown((KeyCode) 13) || Input.GetKeyDown((KeyCode) 271))
          this.ConfirmLayerEdit();
      }
      else
      {
        Widgets.Label(rect5, layer.name);
        if (Widgets.ButtonInvisible(rect3, true))
        {
          if (this.parent.animLayer == layer)
            this.EditingLayer = layer;
          this.parent.animLayer = layer;
        }
      }
      this.DoSeparatorHorizontal(((Rect) ref rect3).x, ((Rect) ref rect3).yMax, ((Rect) ref rect3).width);
      ref Rect local = ref rect3;
      ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect3).height;
    }
  }

  private void ConfirmLayerEdit()
  {
    this.EditingLayer.name = AnimationLoader.GetAvailableName(this.parent.controller.layers.Where<AnimationLayer>((Func<AnimationLayer, bool>) (layer => layer != this.EditingLayer)).Select<AnimationLayer, string>((Func<AnimationLayer, string>) (layer => layer.name)), this.EditingLayer.name);
    this.EditingLayer = (AnimationLayer) null;
  }

  private void ConfirmParameterEdit() => this.EditingParameter = (AnimationParameter) null;

  private void DrawParametersTab(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y + 24f, ((Rect) ref rect).width, 24f);
    Rect rect2 = GenUI.ContractedBy(new Rect(((Rect) ref rect1).x + 10f, ((Rect) ref rect1).y, (float) ((double) ((Rect) ref rect1).width - 48.0 - 20.0), 24f), 2f);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector((float) ((double) ((Rect) ref rect1).xMax - 24.0 - 5.0), ((Rect) ref rect1).y, 24f, 24f);
    this.parameterFilter.Text = Widgets.TextField(rect2, this.parameterFilter.Text);
    GUIState.Disable();
    GUI.DrawTexture(GenUI.ContractedBy(rect3, 2f), (Texture) TexButton.Plus);
    Widgets.ButtonInvisible(rect3, true);
    GUIState.Enable();
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect1).yMax, ((Rect) ref rect).width);
    Text.Anchor = (TextAnchor) 3;
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect1).yMax, ((Rect) ref rect).width, 32f);
    foreach (AnimationParameter parameter in this.parent.controller.parameters)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 1), new TextAnchor?((TextAnchor) 3), new bool?(false));
      try
      {
        Rect rect5 = new Rect(((Rect) ref rect4).x, ((Rect) ref rect4).y, ((Rect) ref rect4).height, ((Rect) ref rect4).height);
        Rect rect6 = GenUI.ContractedBy(new Rect((float) ((double) ((Rect) ref rect4).xMax - 5.0 - 100.0), ((Rect) ref rect4).y, 100f, ((Rect) ref rect4).height), 2f);
        Rect rect7 = GenUI.ContractedBy(new Rect(((Rect) ref rect5).xMax, ((Rect) ref rect4).y, (float) ((double) ((Rect) ref rect4).width - (double) ((Rect) ref rect5).width - (double) ((Rect) ref rect6).width - 5.0), ((Rect) ref rect4).height), 2f);
        Text.Anchor = (TextAnchor) 3;
        if (this.EditingParameter == parameter)
        {
          if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(rect7))
            this.ConfirmParameterEdit();
          if (Input.GetKeyDown((KeyCode) 13) || Input.GetKeyDown((KeyCode) 271))
            this.ConfirmParameterEdit();
        }
        else
        {
          Widgets.Label(rect7, parameter.Name);
          parameter.DrawInput(rect6);
        }
        ref Rect local = ref rect4;
        ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect4).height;
      }
      finally
      {
        textBlock.Dispose();
      }
    }
  }

  private void DrawControllerSectionRight(Rect rect)
  {
    Rect rect1 = rect;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).yMin = ((Rect) ref local1).yMin + 24f;
    Widgets.BeginGroup(rect1);
    Rect rect2 = GenUI.AtZero(rect1);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).x, ((Rect) ref rect2).y, this.gridSize.x, this.gridSize.y);
    Vector2 pos = Vector2.op_Subtraction(this.MouseUIPos(((Rect) ref rect2).position), this.MousePosLeftWindowAdjust);
    this.mouseGridPos = this.GridPosition(rect2, rect3, pos);
    if (!this.initialized)
    {
      this.SetScrollPosNormalized(rect2, ref this.scrollPos, rect3, new Vector2(0.5f, 0.5f));
      this.initialized = true;
    }
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(this.HideLeftWindow ? 0.0f : this.leftWindowSize, 56f);
    Rect visibleRect = this.GetVisibleRect(rect2, this.scrollPos, rect3);
    ref Rect local2 = ref visibleRect;
    ((Rect) ref local2).position = Vector2.op_Subtraction(((Rect) ref local2).position, this.MousePosLeftWindowAdjust);
    Vector2 position = ((Rect) ref rect2).position;
    UIElements.BeginScrollView(rect2, ref this.scrollPos, rect3, false, false);
    this.DrawBackgroundDark(rect3);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect2).x, ((Rect) ref rect2).yMax, ((Rect) ref rect2).width, 24f);
    double num = (double) AnimationEditor.DrawBlend(rect4, this.topBarFadeColor, this.backgroundCurvesColor);
    this.DrawGrid(rect3);
    Rect dragRect = this.DragRect(Vector2.op_Subtraction(((Rect) ref rect2).position, ((Rect) ref visibleRect).position));
    this.DrawAnimationStates(rect2, rect3, dragRect);
    if (this.RightClickUp && Mouse.IsOver(visibleRect))
    {
      this.clickedGridPos = this.mouseGridPos;
      Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
      {
        new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_CreateState")), new Action(this.CreateNewState), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
        new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_CreateSubState")), new Action(this.CreateNewState), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        {
          Disabled = true
        },
        new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_Paste")), new Action(this.PasteState), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        {
          Disabled = this.clipboard.IsEmpty
        }
      }));
    }
    if (!this.draggingState)
      this.SelectionBox(position, visibleRect, rect3, out Rect _);
    UIElements.EndScrollView(false);
    Widgets.EndGroup();
    this.DrawTopBar(rect);
    if (Mouse.IsOver(rect2) && Event.current.type == 6)
    {
      this.zoom = Mathf.Clamp(this.zoom + Event.current.delta.y * 0.03f, 1f, this.MaxZoom(rect2));
      Event.current.Use();
    }
    if (this.draggingSelectionBox || !this.DragWindow(rect2, AnimationControllerEditor.DragItem.Grid, 2))
      return;
    SetDragPos();

    void SetDragPos()
    {
      Vector2 vector2_1;
      // ISSUE: explicit constructor call
      ((Vector2) ref vector2_1).\u002Ector(UI.MousePositionOnUI.x, UI.MousePositionOnUI.y);
      Vector2 vector2_2 = Vector2.op_Subtraction(this.dragPos, vector2_1);
      this.dragPos = vector2_1;
      this.scrollPos = Vector2.op_Addition(this.scrollPos, new Vector2(vector2_2.x, -vector2_2.y));
    }
  }

  private void DrawTopBar(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width, 24f);
    this.DrawBackground(rect1);
    if (!this.HideLeftWindow || !Widgets.ButtonImage(GenUI.ContractedBy(new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, 24f, 24f), 2f), this.eyeStrikedTex, true, (string) null))
      return;
    this.HideLeftWindow = false;
  }

  private void DrawGrid(Rect viewRect)
  {
    int num1 = 1;
    float x = ((Rect) ref viewRect).x + ((Rect) ref viewRect).width / 2f;
    float y = ((Rect) ref viewRect).y + ((Rect) ref viewRect).height / 2f;
    UIElements.DrawLineVertical(x, ((Rect) ref viewRect).y, ((Rect) ref viewRect).height, GetColor(0));
    int num2 = Mathf.RoundToInt((float) ((double) ((Rect) ref viewRect).width / (20.0 / (double) this.zoom) / 2.0));
    for (int lineNumber = 1; lineNumber < num2; lineNumber += num1)
    {
      UIElements.DrawLineVertical(x + (float) (20 * lineNumber) * (1f / this.zoom), ((Rect) ref viewRect).y, ((Rect) ref viewRect).height, GetColor(lineNumber));
      UIElements.DrawLineVertical(x - (float) (20 * lineNumber) * (1f / this.zoom), ((Rect) ref viewRect).y, ((Rect) ref viewRect).height, GetColor(lineNumber));
    }
    UIElements.DrawLineHorizontal(((Rect) ref viewRect).x, y, ((Rect) ref viewRect).width, GetColor(0));
    int num3 = Mathf.RoundToInt((float) ((double) ((Rect) ref viewRect).height / (20.0 / (double) this.zoom) / 2.0));
    for (int lineNumber = 1; lineNumber < num3; lineNumber += num1)
    {
      UIElements.DrawLineHorizontal(((Rect) ref viewRect).x, y + (float) (20 * lineNumber) * (1f / this.zoom), ((Rect) ref viewRect).width, GetColor(lineNumber));
      UIElements.DrawLineHorizontal(((Rect) ref viewRect).x, y - (float) (20 * lineNumber) * (1f / this.zoom), ((Rect) ref viewRect).width, GetColor(lineNumber));
    }

    Color GetColor(int lineNumber)
    {
      return lineNumber % 10 != 0 ? this.lineLightColor : this.lineDarkColor;
    }
  }

  private void DrawAnimationStates(Rect outRect, Rect viewRect, Rect dragRect)
  {
    if (!(bool) this.parent.controller || this.parent.animLayer == null || this.parent.animLayer.states.NullOrEmpty<AnimationState>())
      return;
    bool flag = false;
    Vector2 to = this.StatePosition(viewRect, this.mouseGridPos);
    foreach (AnimationState state in this.parent.animLayer.states)
    {
      Vector2 vector2_1 = this.SizeFor(state.Type);
      Vector2 vector2_2 = this.StatePosition(viewRect, state.position);
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(vector2_2, vector2_1);
      if (Mouse.IsOver(rect1))
        to = ((Rect) ref rect1).center;
      foreach (AnimationTransition transition in state.transitions)
      {
        Vector2 vector2_3 = this.SizeFor(transition.ToState.Type);
        Vector2 vector2_4 = this.StatePosition(viewRect, transition.ToState.position);
        Rect rect2;
        // ISSUE: explicit constructor call
        ((Rect) ref rect2).\u002Ector(vector2_4, vector2_3);
        Color color = this.selector.IsSelected(transition) ? this.highlightColor : Color.white;
        Widgets.DrawLine(((Rect) ref rect1).center, ((Rect) ref rect2).center, color, 2f);
        if (TransitionArrows(((Rect) ref rect1).center, ((Rect) ref rect2).center, color))
        {
          flag = true;
          this.selector.Select(transition, !Input.GetKey((KeyCode) 304) && !Input.GetKey((KeyCode) 306));
        }
      }
    }
    if (this.makingTransitionFrom != null)
    {
      Vector2 vector2_5 = this.SizeFor(this.makingTransitionFrom.Type);
      Vector2 vector2_6 = this.StatePosition(viewRect, this.makingTransitionFrom.position);
      Rect rect;
      // ISSUE: explicit constructor call
      ((Rect) ref rect).\u002Ector(vector2_6, vector2_5);
      Widgets.DrawLine(((Rect) ref rect).center, to, Color.white, 3f);
      TransitionArrows(((Rect) ref rect).center, to, Color.white);
    }
    foreach (AnimationState state1 in this.parent.animLayer.states)
    {
      AnimationState state = state1;
      Vector2 vector2_7 = this.SizeFor(state.Type);
      Vector2 vector2_8 = this.StatePosition(viewRect, state.position);
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(vector2_8, vector2_7);
      if (this.selector.IsSelected(state))
      {
        Rect rect4 = GenUI.ExpandedBy(rect3, 2f);
        GUI.color = this.highlightColor;
        GUI.DrawTexture(rect4, (Texture) this.stateTex);
      }
      GUI.color = this.GetStateColor(state);
      GUI.DrawTexture(rect3, (Texture) this.stateTex);
      GUI.color = Color.white;
      if ((double) this.zoom < 5.0)
      {
        Text.Anchor = (TextAnchor) 4;
        Text.Font = this.Font;
        Widgets.Label(rect3, state.name);
      }
      if (Mouse.IsOver(rect3) && (this.LeftClickDown || this.RightClickDown || this.LeftClickUp || this.RightClickUp))
      {
        this.selector.Select(state, !Input.GetKey((KeyCode) 304) && !Input.GetKey((KeyCode) 306));
        flag = true;
        this.clickedGridPos = this.mouseGridPos;
        if (this.LeftClickDown)
        {
          if (this.makingTransitionFrom != null)
            this.ConfirmTransition(state);
          this.draggingState = true;
          this.draggingStateOrigPos = state.position;
        }
        else if (this.RightClickUp)
        {
          this.draggingState = false;
          List<FloatMenuOption> enumerable = new List<FloatMenuOption>();
          if (state.Type != AnimationState.StateType.Exit)
          {
            FloatMenuOption floatMenuOption = new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_MakeTransition")), (Action) (() => this.SetTransition(state)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
            enumerable.Add(floatMenuOption);
          }
          if (!state.IsPermanent)
          {
            enumerable.Add(new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_SetStateDefault")), new Action(this.CreateNewState), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
            {
              Disabled = state.Type == AnimationState.StateType.Default
            });
            FloatMenuOption floatMenuOption1 = new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_Copy")), new Action(this.CopySelectedStates), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
            enumerable.Add(floatMenuOption1);
            FloatMenuOption floatMenuOption2 = new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_Delete")), (Action) (() => this.DeleteState(state)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
            enumerable.Add(floatMenuOption2);
          }
          if (!enumerable.NullOrEmpty<FloatMenuOption>())
            Find.WindowStack.Add((Window) new FloatMenu(enumerable));
        }
        Event.current.Use();
      }
      else if (!this.draggingState && this.draggingSelectionBox)
      {
        if (((Rect) ref dragRect).Overlaps(rect3, true))
        {
          this.selector.Select(state, false);
          flag = true;
          foreach (AnimationTransition transition in state.transitions)
          {
            if (this.selector.IsSelected(transition.ToState))
              this.selector.Select(transition, false);
            else
              this.selector.Unselect(transition);
          }
        }
        else
          this.selector.Unselect(state);
      }
    }
    if (!Input.GetMouseButton(0))
      this.draggingState = false;
    if (this.LeftClickDown || this.RightClickDown)
      this.StopMakingTransition();
    if (this.draggingState)
    {
      foreach (AnimationState selectedState in this.selector.SelectedStates)
        selectedState.position = IntVec2.op_Addition(this.draggingStateOrigPos, IntVec2.op_Subtraction(this.mouseGridPos, this.clickedGridPos));
    }
    if (!this.LeftClickDown || !Mouse.IsOver(viewRect) || flag)
      return;
    this.selector.ClearSelectedStates();
    this.selector.ClearSelectedTransitions();

    static bool TransitionArrows(Vector2 from, Vector2 to, Color color)
    {
      float num1 = 14f;
      Vector2 vector2 = Vector2.Lerp(from, to, 0.5f);
      float num2 = Ext_Math.AngleToPoint(from.x, -from.y, to.x, -to.y) - 90f;
      bool flag = false;
      Rect rect;
      // ISSUE: explicit constructor call
      ((Rect) ref rect).\u002Ector(vector2.x - num1 / 2f, vector2.y - num1 / 2f, num1, num1);
      Matrix4x4 matrix = GUI.matrix;
      UI.RotateAroundPivot(num2, ((Rect) ref rect).center);
      GUI.color = color;
      GUI.DrawTexture(rect, (Texture) TexButton.Play);
      GUI.color = Color.white;
      GUI.matrix = matrix;
      if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect))
        flag = true;
      return flag;
    }
  }

  private Vector2 SizeFor(AnimationState.StateType stateType)
  {
    Vector2 vector2;
    switch (stateType)
    {
      case AnimationState.StateType.None:
        vector2 = new Vector2(320f / this.zoom, 80f / this.zoom);
        break;
      case AnimationState.StateType.Entry:
        vector2 = new Vector2(320f / this.zoom, 60f / this.zoom);
        break;
      case AnimationState.StateType.Default:
        vector2 = new Vector2(320f / this.zoom, 80f / this.zoom);
        break;
      case AnimationState.StateType.Exit:
        vector2 = new Vector2(320f / this.zoom, 60f / this.zoom);
        break;
      case AnimationState.StateType.Any:
        vector2 = new Vector2(320f / this.zoom, 80f / this.zoom);
        break;
      default:
        vector2 = new Vector2(320f / this.zoom, 80f / this.zoom);
        break;
    }
    return vector2;
  }

  private Vector2 StatePosition(Rect viewRect, IntVec2 gridPos)
  {
    return new Vector2((float) ((double) (gridPos.x * 20) / (double) this.zoom + (double) ((Rect) ref viewRect).width / 2.0), (float) ((double) (-gridPos.z * 20) / (double) this.zoom + (double) ((Rect) ref viewRect).height / 2.0));
  }

  private IntVec2 GridPosition(Rect outRect, Rect viewRect, Vector2 pos)
  {
    Rect visibleRect = this.GetVisibleRect(outRect, this.scrollPos, viewRect);
    Vector2 vector2 = Vector2.op_Division(Vector2.op_Addition(((Rect) ref visibleRect).position, pos), ((Rect) ref viewRect).size);
    return new IntVec2(Mathf.RoundToInt(Mathf.Lerp(-75f, 75f, vector2.x) * this.zoom), -Mathf.RoundToInt(Mathf.Lerp(-75f, 75f, vector2.y) * this.zoom));
  }

  private Color GetStateColor(AnimationState state)
  {
    Color stateColor;
    switch (state.Type)
    {
      case AnimationState.StateType.None:
        stateColor = this.stateColor;
        break;
      case AnimationState.StateType.Entry:
        stateColor = this.entryStateColor;
        break;
      case AnimationState.StateType.Default:
        stateColor = this.defaultStateColor;
        break;
      case AnimationState.StateType.Exit:
        stateColor = this.exitStateColor;
        break;
      case AnimationState.StateType.Any:
        stateColor = this.anyStateColor;
        break;
      default:
        stateColor = this.stateColor;
        break;
    }
    return stateColor;
  }

  private bool DragWindow(Rect rect, AnimationControllerEditor.DragItem dragItem, int button = 0)
  {
    return this.DragWindow(rect, new Action(SetDragItem), new Func<bool>(IsDragging), new Action(StartDragging), new Action(StopDragging), button);

    void SetDragItem() => this.dragging = dragItem;

    bool IsDragging() => this.dragging == dragItem;

    void StartDragging()
    {
      this.dragPos = Vector2.op_Implicit(Input.mousePosition);
      this.dragging = dragItem;
    }

    void StopDragging() => this.dragging = AnimationControllerEditor.DragItem.None;
  }

  private void SetTransition(AnimationState from) => this.makingTransitionFrom = from;

  private void ConfirmTransition(AnimationState target)
  {
    if (target.Type == AnimationState.StateType.Entry || target.Type == AnimationState.StateType.Any)
      return;
    this.makingTransitionFrom.AddTransition(target);
    this.StopMakingTransition();
  }

  private void StopMakingTransition() => this.makingTransitionFrom = (AnimationState) null;

  private void DeleteSelection()
  {
    if (this.selector.AnyTransitionsSelected)
    {
      foreach (AnimationTransition selectedTransition in this.selector.SelectedTransitions)
        this.DeleteTransition(selectedTransition);
      this.selector.ClearSelectedTransitions();
    }
    if (!this.selector.AnyStatesSelected)
      return;
    foreach (AnimationState selectedState in this.selector.SelectedStates)
    {
      if (!selectedState.IsPermanent)
        this.DeleteState(selectedState);
    }
    this.selector.ClearSelectedStates();
  }

  private void CreateNewState()
  {
    this.parent.animLayer.AddState(AnimationLoader.GetAvailableName(this.parent.animLayer.states.Select<AnimationState, string>((Func<AnimationState, string>) (state => state.name)), "New State"), this.clickedGridPos);
  }

  private void PasteState()
  {
    foreach (AnimationState copiedState in this.clipboard.GetCopiedStates())
      this.parent.animLayer.states.Add(copiedState.CreateCopy(this.parent.animLayer));
  }

  private void DeleteState(AnimationState state)
  {
    state.Dispose();
    this.parent.animLayer.states.Remove(state);
  }

  private void DeleteTransition(AnimationTransition transition) => transition.Dispose();

  private void CopySelectedStates()
  {
    if (!this.selector.AnyStatesSelected)
      return;
    this.clipboard.CopyToClipboard((IEnumerable<AnimationState>) this.selector.SelectedStates);
  }

  private enum DragItem
  {
    None,
    Grid,
  }

  private enum LeftSection
  {
    Layers,
    Parameters,
  }

  private class Clipboard
  {
    private HashSet<AnimationState> copiedStates = new HashSet<AnimationState>();

    public bool IsEmpty => this.copiedStates.Count == 0;

    public void CopyToClipboard(IEnumerable<AnimationState> states)
    {
      this.ClearClipboard();
      GenCollection.AddRange<AnimationState>(this.copiedStates, states);
    }

    public IEnumerable<AnimationState> GetCopiedStates()
    {
      return (IEnumerable<AnimationState>) this.copiedStates;
    }

    public void ClearClipboard() => this.copiedStates.Clear();
  }

  private class Selector
  {
    private HashSet<AnimationState> selectedStates = new HashSet<AnimationState>();
    private HashSet<AnimationTransition> selectedTransitions = new HashSet<AnimationTransition>();

    public bool AnySelected => this.AnyStatesSelected || this.AnyTransitionsSelected;

    public bool AnyStatesSelected => GenCollection.Any<AnimationState>(this.selectedStates);

    public bool AnyTransitionsSelected
    {
      get => GenCollection.Any<AnimationTransition>(this.selectedTransitions);
    }

    public HashSet<AnimationState> SelectedStates => this.selectedStates;

    public HashSet<AnimationTransition> SelectedTransitions => this.selectedTransitions;

    public bool IsSelected(AnimationState state) => this.selectedStates.Contains(state);

    public bool IsSelected(AnimationTransition transition)
    {
      return this.selectedTransitions.Contains(transition);
    }

    public void Select(AnimationState state, bool clear = true)
    {
      if (clear)
        this.ClearSelectedStates();
      this.selectedStates.Add(state);
    }

    public void Select(AnimationTransition transition, bool clear = true)
    {
      if (clear)
        this.ClearSelectedTransitions();
      this.selectedTransitions.Add(transition);
    }

    public void Unselect(AnimationState state) => this.selectedStates.Remove(state);

    public void Unselect(AnimationTransition transition)
    {
      this.selectedTransitions.Remove(transition);
    }

    public void ClearSelectedStates() => this.selectedStates.Clear();

    public void ClearSelectedTransitions() => this.selectedTransitions.Clear();
  }
}
