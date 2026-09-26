// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationClipEditor
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Animations;

public class AnimationClipEditor : AnimationEditor
{
  private const float MinLeftWindowSize = 300f;
  private const float MinRightWindowSize = 250f;
  private const float WidgetBarHeight = 24f;
  private const float KeyframeSize = 20f;
  private const float FrameInputWidth = 50f;
  private const float TabWidth = 110f;
  private const float PropertyEntryHeight = 24f;
  private const float PropertyBtnWidth = 180f;
  private const float ParamCheckboxSize = 20f;
  private const float CurveNoWeightDist = 50f;
  private const float TangentSlopeMargin = 0.25f;
  private const float FrameBarPadding = 40f;
  private const float FrameBarTickRenderingPadding = 130f;
  private const float CollapseFrameDistance = 100f;
  private const float MaxFrameZoom = 1000f;
  private const float ZoomRate = 0.01f;
  private const int InitialTickInterval = 1;
  private const float InitialCurveTickInterval = 0.1f;
  private const float MaxCurveSpacing = 400f;
  private const int DefaultFrameCount = 60;
  private const float SecondsPerFrame = 0.0166666675f;
  private const float DefaultAxisCount = 100f;
  private const float MaxExtraScrollDistance = 5000f;
  private readonly Texture2D skipToBeginningTexture;
  private readonly Texture2D skipToPreviousTexture;
  private readonly Texture2D skipToNextTexture;
  private readonly Texture2D skipToEndTexture;
  private readonly Texture2D animationEventTexture;
  private readonly Texture2D keyFrameTexture;
  private readonly Texture2D addAnimationEventTexture;
  private readonly Texture2D addKeyFrameTexture;
  private readonly Color propertyExpandedNameColor;
  private readonly Color propertyLabelHighlightColor;
  private readonly Color itemSelectedColor;
  private readonly Color animationEventBarColor;
  private readonly Color animationKeyFrameBarColor;
  private readonly Color animationKeyFrameBarFadeColor;
  private readonly Color curveTopColor;
  private readonly Color curveTopFadeColor;
  private readonly Color frameTimeBarColor;
  private readonly Color frameTimeBarColorDisabled;
  private readonly Color frameTickColor;
  private readonly Color frameBarHighlightColor;
  private readonly Color frameBarHighlightMinorColor;
  private readonly Color frameBarHighlightOutlineColor;
  private readonly Color frameBarCurveColor;
  private readonly Color curveAxisColor;
  private readonly Color keyFrameColor;
  private readonly Color keyFrameTopColor;
  private readonly Color keyFrameHighlightColor;
  private readonly Color frameLineMajorDopesheetColor;
  private readonly Color frameLineMinorDopesheetColor;
  private readonly Color frameLineCurvesColor;
  private readonly AnimationClipEditor.KeyFrameSelector keyFrameSelector;
  private readonly Selector selector;
  private readonly List<AnimationPropertyParent> propertiesToRemove;
  private readonly HashSet<(int index, int frame)> framesToDraw;
  private readonly HashSet<(int index, int frame)> parentFramesToDraw;
  private AnimationClip animation;
  private float zoomX;
  private float zoomY;
  private int frame;
  private bool isPlaying;
  private int tickInterval;
  private float curveTickInterval;
  private readonly Dictionary<AnimationPropertyParent, bool> propertyExpanded;
  private readonly Dictionary<ParameterInfo, string> inputBuffers;
  private Dialog_CameraView previewWindow;
  private float leftWindowSize;
  private Vector2 panelScrollPos;
  private float extraPanelWidth;
  private Vector2 frameScrollPos;
  private float extraFrameHeight;
  private float realTimeToTick;
  private AnimationClipEditor.KeyFrameDragHandler keyFrameDragger;
  private Vector2 dragPos;
  private AnimationClipEditor.DragItem dragging;
  private AnimationClipEditor.EditTab tab;

  public AnimationClipEditor(Dialog_AnimationEditor parent)
  {
    ColorInt colorInt1 = new ColorInt(123, 123, 123);
    this.propertyExpandedNameColor = ((ColorInt) ref colorInt1).ToColor;
    ColorInt colorInt2 = new ColorInt((int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue, 10);
    this.propertyLabelHighlightColor = ((ColorInt) ref colorInt2).ToColor;
    ColorInt colorInt3 = new ColorInt(87, 133, 217);
    this.itemSelectedColor = ((ColorInt) ref colorInt3).ToColor;
    ColorInt colorInt4 = new ColorInt(49, 49, 49);
    this.animationEventBarColor = ((ColorInt) ref colorInt4).ToColor;
    ColorInt colorInt5 = new ColorInt(47, 47, 47);
    this.animationKeyFrameBarColor = ((ColorInt) ref colorInt5).ToColor;
    ColorInt colorInt6 = new ColorInt(40, 40, 40);
    this.animationKeyFrameBarFadeColor = ((ColorInt) ref colorInt6).ToColor;
    ColorInt colorInt7 = new ColorInt(40, 40, 40, 25);
    this.curveTopColor = ((ColorInt) ref colorInt7).ToColor;
    ColorInt colorInt8 = new ColorInt(0, 0, 0, 25);
    this.curveTopFadeColor = ((ColorInt) ref colorInt8).ToColor;
    ColorInt colorInt9 = new ColorInt(40, 64 /*0x40*/, 75);
    this.frameTimeBarColor = ((ColorInt) ref colorInt9).ToColor;
    ColorInt colorInt10 = new ColorInt(10, 10, 10, 100);
    this.frameTimeBarColorDisabled = ((ColorInt) ref colorInt10).ToColor;
    ColorInt colorInt11 = new ColorInt(140, 140, 140);
    this.frameTickColor = ((ColorInt) ref colorInt11).ToColor;
    ColorInt colorInt12 = new ColorInt((int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue, 5);
    this.frameBarHighlightColor = ((ColorInt) ref colorInt12).ToColor;
    ColorInt colorInt13 = new ColorInt((int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue, 2);
    this.frameBarHighlightMinorColor = ((ColorInt) ref colorInt13).ToColor;
    ColorInt colorInt14 = new ColorInt(68, 68, 68);
    this.frameBarHighlightOutlineColor = ((ColorInt) ref colorInt14).ToColor;
    ColorInt colorInt15 = new ColorInt(73, 73, 73);
    this.frameBarCurveColor = ((ColorInt) ref colorInt15).ToColor;
    ColorInt colorInt16 = new ColorInt(93, 93, 93);
    this.curveAxisColor = ((ColorInt) ref colorInt16).ToColor;
    ColorInt colorInt17 = new ColorInt(153, 153, 153);
    this.keyFrameColor = ((ColorInt) ref colorInt17).ToColor;
    ColorInt colorInt18 = new ColorInt(108, 108, 108);
    this.keyFrameTopColor = ((ColorInt) ref colorInt18).ToColor;
    ColorInt colorInt19 = new ColorInt(200, 200, 200);
    this.keyFrameHighlightColor = ((ColorInt) ref colorInt19).ToColor;
    ColorInt colorInt20 = new ColorInt(75, 75, 75);
    this.frameLineMajorDopesheetColor = ((ColorInt) ref colorInt20).ToColor;
    ColorInt colorInt21 = new ColorInt(66, 66, 66);
    this.frameLineMinorDopesheetColor = ((ColorInt) ref colorInt21).ToColor;
    ColorInt colorInt22 = new ColorInt(51, 51, 51);
    this.frameLineCurvesColor = ((ColorInt) ref colorInt22).ToColor;
    this.keyFrameSelector = new AnimationClipEditor.KeyFrameSelector();
    this.selector = new Selector();
    this.propertiesToRemove = new List<AnimationPropertyParent>();
    this.framesToDraw = new HashSet<(int, int)>();
    this.parentFramesToDraw = new HashSet<(int, int)>();
    this.zoomX = 1f;
    this.zoomY = 1f;
    this.tickInterval = 1;
    this.curveTickInterval = 0.1f;
    this.propertyExpanded = new Dictionary<AnimationPropertyParent, bool>();
    this.inputBuffers = new Dictionary<ParameterInfo, string>();
    this.leftWindowSize = 300f;
    this.keyFrameDragger = new AnimationClipEditor.KeyFrameDragHandler();
    // ISSUE: explicit constructor call
    base.\u002Ector(parent);
  }

  private bool UnsavedChanges { get; set; }

  private bool MouseOverSelectableArea { get; set; }

  private float ExtraPadding { get; set; }

  public float FrameBarWidth => this.EditorWidth - 80f;

  private int FrameCountShown { get; set; }

  private int Frame
  {
    get => this.frame;
    set
    {
      if (this.frame == value)
        return;
      this.frame = value;
    }
  }

  private int FrameCount
  {
    get
    {
      return this.animation == null || this.animation.frameCount <= 0 ? 60 : this.animation.frameCount;
    }
  }

  private float FrameTickMarkSpacing
  {
    get
    {
      float frameTickMarkSpacing = 100f / Mathf.Lerp((float) this.TickInterval / 2f, (float) this.TickInterval, this.ZoomFrames % 1f);
      if (this.TickInterval == 1)
        frameTickMarkSpacing /= 2.5f;
      return frameTickMarkSpacing;
    }
  }

  private float CurveAxisSpacing
  {
    get
    {
      float curveAxisSpacing = 100f / Mathf.Lerp(this.CurveTickInterval / 2f, this.CurveTickInterval, this.ZoomCurve % 1f);
      if ((double) this.CurveTickInterval == 0.10000000149011612)
        curveAxisSpacing /= 2.5f;
      return curveAxisSpacing;
    }
  }

  private int TickInterval => this.tickInterval;

  private float CurveTickInterval => this.curveTickInterval;

  private float ZoomFrames
  {
    get => this.zoomX;
    set
    {
      if ((double) this.zoomX == (double) value)
        return;
      this.zoomX = Mathf.Clamp(value, 1f, 1000f);
      this.RecalculateTickInterval();
    }
  }

  private float ZoomCurve
  {
    get => this.zoomY;
    set
    {
      if ((double) this.zoomY == (double) value)
        return;
      this.zoomY = Mathf.Clamp(value, 1f, 1000f);
      this.RecalculateCurveTickInterval();
    }
  }

  public float EditorWidth
  {
    get => (float) ((double) this.FrameTickMarkSpacing * (double) this.FrameCount + 80.0);
  }

  public bool IsPlaying
  {
    get => this.isPlaying;
    private set
    {
      if (this.isPlaying == value)
        return;
      this.isPlaying = value;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Clock_Stop, (Map) null);
    }
  }

  private void ChangeMade()
  {
    this.parent.ChangeMade();
    this.UnsavedChanges = true;
  }

  public override void AnimatorLoaded(IAnimator animator)
  {
    base.AnimatorLoaded(animator);
    if (this.previewWindow != null && this.previewWindow.IsOpen)
      this.previewWindow.Close(true);
    if (this.parent?.animator?.Manager == null)
      return;
    this.previewWindow = new Dialog_CameraView(new Func<bool>(this.DisableCameraView), (Func<Vector3>) (() => animator.DrawPos), new Vector2(((Rect) ref this.parent.windowRect).xMax - 50f, ((Rect) ref this.parent.windowRect).yMax - 50f));
    if (animator is Thing thing)
      CameraJumper.TryJump(GlobalTargetInfo.op_Implicit(thing), (CameraJumper.MovementMode) 1);
    CameraView.Start(CameraView.animationSettings.orthographicSize);
    Find.Selector.ClearSelection();
  }

  public override void Update()
  {
    if (this.IsPlaying)
    {
      if ((double) Mathf.Abs(Time.deltaTime - 0.0166666675f) < 0.0016666668234393)
        this.realTimeToTick += 0.0166666675f;
      else
        this.realTimeToTick += Time.deltaTime;
      if ((double) this.realTimeToTick >= 0.01666666753590107)
      {
        ++this.Frame;
        this.realTimeToTick -= 0.0166666675f;
        if (this.Frame >= this.FrameCount)
          this.Frame = 0;
      }
    }
    if (this.previewWindow == null || !this.previewWindow.IsOpen)
      return;
    this.parent.animator.Manager?.SetFrame(this.animation, this.frame);
  }

  public override void OnClose()
  {
    if (this.previewWindow == null || !this.previewWindow.IsOpen)
      return;
    this.previewWindow.Close(true);
  }

  public override void OnGUIHighPriority()
  {
    base.OnGUIHighPriority();
    if (KeyBindingDefOf.TogglePause.KeyDownEvent)
    {
      Event.current.Use();
      this.IsPlaying = !this.IsPlaying;
    }
    if (this.IsPlaying || Event.current == null || Event.current.type != 4)
      return;
    int num = 1;
    if (this.ShiftClick)
      num = 10;
    if (this.ControlClick)
      num = 100;
    if (Event.current.keyCode == 276)
      this.frame -= num;
    if (Event.current.keyCode != 275)
      return;
    this.frame += num;
  }

  public override void Save()
  {
    if (!(bool) this.animation)
      return;
    AnimationLoader.Save<AnimationClip>(this.animation);
  }

  public override void CopyToClipboard()
  {
  }

  public override void Paste()
  {
  }

  public override void Escape()
  {
  }

  public override void Delete()
  {
    if (this.keyFrameSelector.AnyKeyFrameSelected)
    {
      foreach ((AnimationProperty property, int frame) selPropKeyFrame in this.keyFrameSelector.selPropKeyFrames)
        selPropKeyFrame.property.curve.Remove(selPropKeyFrame.frame);
      this.keyFrameSelector.ClearSelectedKeyFrames();
    }
    if (!this.selector.AnySelected<AnimationEvent>())
      return;
    this.animation.events.RemoveAll(new Predicate<AnimationEvent>(this.selector.IsSelected));
    this.animation.ValidateEventOrder();
    this.selector.DeselectAll<AnimationEvent>();
  }

  public override void Draw(Rect rect)
  {
    Rect rect1;
    Rect rect2;
    GenUI.SplitVertically(rect, this.leftWindowSize, ref rect1, ref rect2);
    this.DrawAnimatorSectionLeft(rect1);
    this.DrawAnimatorSectionRight(rect2);
  }

  private void DrawAnimatorSectionLeft(Rect rect)
  {
    this.DrawBackground(rect);
    if (this.parent.animator == null)
      this.DisableGUI(true);
    if (this.animation == null)
      this.DisableGUI();
    bool enabled = this.previewWindow != null && this.previewWindow.IsOpen;
    string label1 = TaggedString.op_Implicit(Translator.Translate("ST_PreviewAnimation"));
    float x = Text.CalcSize(label1).x;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, x + 20f, 24f);
    if (this.previewWindow == null)
      GUIState.Disable();
    if (this.ToggleText(rect1, label1, TaggedString.op_Implicit(Translator.Translate("ST_PreviewAnimationTooltip")), enabled))
    {
      if (enabled)
      {
        this.previewWindow.Close(true);
      }
      else
      {
        CameraView.ResetSize();
        Find.WindowStack.Add((Window) this.previewWindow);
      }
    }
    GUIState.Enable();
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect).y + 24f, ((Rect) ref rect).width);
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect).yMax, ((Rect) ref rect).width);
    this.DoSeparatorVertical(((Rect) ref rect).xMax, ((Rect) ref rect).y, ((Rect) ref rect).height);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect).y, 24f, 24f);
    if (this.AnimationButton(rect2, this.skipToBeginningTexture, TaggedString.op_Implicit(Translator.Translate("ST_SkipFrameBeginningTooltip"))))
      this.Frame = 0;
    this.DoSeparatorVertical(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    ref Rect local1 = ref rect2;
    ((Rect) ref local1).x = ((Rect) ref local1).x + 1f;
    ref Rect local2 = ref rect2;
    ((Rect) ref local2).x = ((Rect) ref local2).x + ((Rect) ref rect2).width;
    if (this.AnimationButton(rect2, this.skipToPreviousTexture, TaggedString.op_Implicit(Translator.Translate("ST_SkipFramePreviousTooltip"))))
      this.SkipKeyFrame(-1);
    this.DoSeparatorVertical(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    ref Rect local3 = ref rect2;
    ((Rect) ref local3).x = ((Rect) ref local3).x + 1f;
    ref Rect local4 = ref rect2;
    ((Rect) ref local4).x = ((Rect) ref local4).x + ((Rect) ref rect2).width;
    if (this.AnimationButton(rect2, this.IsPlaying ? CameraView.pauseTexture : CameraView.playTexture, TaggedString.op_Implicit(this.IsPlaying ? Translator.Translate("ST_PauseAnimationTooltip") : Translator.Translate("ST_PlayAnimationTooltip"))))
      this.IsPlaying = !this.IsPlaying;
    this.DoSeparatorVertical(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    ref Rect local5 = ref rect2;
    ((Rect) ref local5).x = ((Rect) ref local5).x + 1f;
    ref Rect local6 = ref rect2;
    ((Rect) ref local6).x = ((Rect) ref local6).x + ((Rect) ref rect2).width;
    if (this.AnimationButton(rect2, this.skipToNextTexture, TaggedString.op_Implicit(Translator.Translate("ST_SkipFrameNextTooltip"))))
      this.SkipKeyFrame(1);
    this.DoSeparatorVertical(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    ref Rect local7 = ref rect2;
    ((Rect) ref local7).x = ((Rect) ref local7).x + 1f;
    ref Rect local8 = ref rect2;
    ((Rect) ref local8).x = ((Rect) ref local8).x + ((Rect) ref rect2).width;
    if (this.AnimationButton(rect2, this.skipToEndTexture, TaggedString.op_Implicit(Translator.Translate("ST_SkipFrameEndTooltip"))))
      this.Frame = this.FrameCount;
    this.DoSeparatorVertical(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    this.DoSeparatorVertical(((Rect) ref rect2).xMax, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    Rect rect3 = GenUI.ContractedBy(new Rect(((Rect) ref rect).xMax - 50f, ((Rect) ref rect).y, 50f, ((Rect) ref rect2).height), 2f);
    string str = (string) null;
    int frame = this.Frame;
    Widgets.TextFieldNumeric<int>(rect3, ref frame, ref str, 0.0f, 1E+09f);
    this.Frame = frame;
    this.CheckTextFieldControlFocus(rect3);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect).xMax - ((Rect) ref rect2).height, ((Rect) ref rect2).yMax, ((Rect) ref rect2).height, ((Rect) ref rect2).height);
    if (this.AnimationButton(rect4, this.addAnimationEventTexture, TaggedString.op_Implicit(Translator.Translate("ST_AddAnimationEvent"))))
    {
      this.animation.events.Add(new AnimationEvent()
      {
        frame = this.Frame
      });
      this.animation.ValidateEventOrder();
      this.ChangeMade();
    }
    this.DoSeparatorVertical(((Rect) ref rect4).x, ((Rect) ref rect4).y, ((Rect) ref rect4).height);
    ref Rect local9 = ref rect4;
    ((Rect) ref local9).x = ((Rect) ref local9).x - 1f;
    ref Rect local10 = ref rect4;
    ((Rect) ref local10).x = ((Rect) ref local10).x - ((Rect) ref rect4).height;
    if (this.AnimationButton(rect4, this.addKeyFrameTexture, TaggedString.op_Implicit(Translator.Translate("ST_AddKeyFrame"))))
    {
      foreach (AnimationPropertyParent property in this.animation.properties)
        this.AddKeyFramesForParent(property);
      this.animation.RecacheFrameCount();
      this.ChangeMade();
    }
    this.DoSeparatorVertical(((Rect) ref rect4).x, ((Rect) ref rect4).y, ((Rect) ref rect4).height);
    ref Rect local11 = ref rect4;
    ((Rect) ref local11).x = ((Rect) ref local11).x - 1f;
    this.EnableGUI();
    Rect rect5;
    // ISSUE: explicit constructor call
    ((Rect) ref rect5).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect4).y, 200f, ((Rect) ref rect2).height);
    Rect rect6;
    // ISSUE: explicit constructor call
    ((Rect) ref rect6).\u002Ector(((Rect) ref this.parent.windowRect).x + this.parent.EditorMargin + ((Rect) ref rect5).x, ((Rect) ref this.parent.windowRect).y + this.parent.EditorMargin + ((Rect) ref rect5).yMax, 300f, 500f);
    string label2 = this.animation?.FileName ?? "[No Clip]";
    string tooltip = this.animation?.FilePath ?? string.Empty;
    if (AnimationEditor.Dropdown(rect5, label2, tooltip))
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      Find.WindowStack.Add((Window) new Dialog_AnimationClipLister(this.parent.animator, rect6, this.animation, (Dialog_ItemDropdown<AnimationClip>.CreateItemButton) ("ST_CreateNewClip", AnimationClipEditor.\u003C\u003EO.\u003C0\u003E__CreateEmpty ?? (AnimationClipEditor.\u003C\u003EO.\u003C0\u003E__CreateEmpty = new Func<AnimationClip>(AnimationClip.CreateEmpty))), new Action<AnimationClip>(this.LoadAnimation)));
    }
    this.DoSeparatorVertical(((Rect) ref rect5).xMax, ((Rect) ref rect5).y, ((Rect) ref rect5).height);
    this.DoSeparatorHorizontal(((Rect) ref rect5).x, ((Rect) ref rect5).yMax, ((Rect) ref rect).width);
    Rect rect7;
    // ISSUE: explicit constructor call
    ((Rect) ref rect7).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect5).yMax, ((Rect) ref rect).width, ((Rect) ref rect).height - ((Rect) ref rect5).yMax);
    if (this.selector.AnySelected<AnimationEvent>())
      this.DrawAnimationEventFields(rect7);
    else
      this.DrawPropertyEntries(rect7);
    if (this.animation == null)
      this.DisableGUI();
    Rect rect8;
    // ISSUE: explicit constructor call
    ((Rect) ref rect8).\u002Ector((float) ((double) ((Rect) ref rect).xMax - 110.0 - 24.0), ((Rect) ref rect).yMax - 24f, 110f, 24f);
    this.DoSeparatorHorizontal(((Rect) ref rect8).xMax, ((Rect) ref rect8).y, 24f);
    if (this.ToggleText(rect8, TaggedString.op_Implicit(Translator.Translate("ST_CurvesTab")), (string) null, this.tab == AnimationClipEditor.EditTab.Curves))
      FlipTab();
    ref Rect local12 = ref rect8;
    ((Rect) ref local12).x = ((Rect) ref local12).x - ((Rect) ref rect8).width;
    if (this.ToggleText(rect8, TaggedString.op_Implicit(Translator.Translate("ST_DopesheetTab")), (string) null, this.tab == AnimationClipEditor.EditTab.Dopesheet))
      FlipTab();
    this.DoSeparatorHorizontal(((Rect) ref rect).x, ((Rect) ref rect8).y, ((Rect) ref rect).x + ((Rect) ref rect8).x);
    this.EnableGUI(true);
    this.DoResizerButton(rect, ref this.leftWindowSize, 300f, 250f);

    void FlipTab()
    {
      AnimationClipEditor.EditTab editTab;
      switch (this.tab)
      {
        case AnimationClipEditor.EditTab.Dopesheet:
          editTab = AnimationClipEditor.EditTab.Curves;
          break;
        case AnimationClipEditor.EditTab.Curves:
          editTab = AnimationClipEditor.EditTab.Dopesheet;
          break;
        default:
          throw new NotImplementedException();
      }
      this.tab = editTab;
      this.keyFrameDragger.Configure(this.tab == AnimationClipEditor.EditTab.Dopesheet ? 20f : 0.0f);
    }
  }

  private void SkipKeyFrame(int offset)
  {
    if (this.animation == null || offset == 0)
      return;
    int num1 = -1;
    float num2 = float.MaxValue;
    foreach (AnimationPropertyParent property in this.animation.properties)
    {
      foreach (AnimationProperty animationProperty in property)
      {
        foreach (KeyFrame point in animationProperty.curve.points)
        {
          if ((offset <= 0 || point.frame > this.Frame) && (offset >= 0 || point.frame < this.Frame))
          {
            float num3 = (float) Mathf.Abs(point.frame - this.Frame + offset);
            if ((double) num3 < (double) num2)
            {
              num1 = point.frame;
              num2 = num3;
            }
          }
        }
      }
    }
    if (num1 < 0)
      return;
    this.Frame = num1;
  }

  private void DrawAnimationEventFields(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + 5f, ((Rect) ref rect).y + 5f, ((Rect) ref rect).width - 10f, 24f);
    if (!(this.selector.GetSelected<AnimationEvent>().FirstOrDefault<ISelectableUI>() is AnimationEvent animationEvent))
      return;
    string tooltip = string.Empty;
    string label;
    if (animationEvent.method?.method == (MethodInfo) null)
    {
      label = $"[{Translator.Translate("ST_NoFunctionSelected")}]";
    }
    else
    {
      label = Dialog_MethodSelector.MethodName(animationEvent.method.method);
      tooltip = Dialog_MethodSelector.FullMethodSignature(animationEvent.method.method);
    }
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width, Mathf.Max(((Rect) ref rect1).height, Text.CalcHeight(label, ((Rect) ref rect1).width)));
    if (AnimationEditor.Dropdown(rect2, label, tooltip))
    {
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(((Rect) ref this.parent.windowRect).x + this.parent.EditorMargin + ((Rect) ref rect1).x, ((Rect) ref this.parent.windowRect).y + this.parent.EditorMargin + ((Rect) ref rect1).yMax, 300f, 500f);
      Find.WindowStack.Add((Window) new Dialog_MethodSelector(this.parent.animator, rect3, animationEvent, new Action<MethodInfo>(this.AddMethodToEvent)));
    }
    ((Rect) ref rect1).y = ((Rect) ref rect2).yMax;
    if (!(animationEvent.method?.method != (MethodInfo) null))
      return;
    ParameterInfo[] parameters = animationEvent.method.method.GetParameters();
    DynamicDelegate method = animationEvent.method;
    if (method.args == null)
      method.args = new object[parameters.Length];
    for (int index = 0; index < parameters.Length; ++index)
    {
      ParameterInfo parameter = parameters[index];
      bool flag = animationEvent.method.InjectedCount > index;
      object obj = (object) null;
      if (!((IList<object>) animationEvent.method.args).OutOfBounds<object>(index))
        obj = animationEvent.method.args[index];
      Text.Anchor = (TextAnchor) 3;
      Rect rect4;
      Rect rect5;
      GenUI.SplitVertically(rect1, ((Rect) ref rect).width * 0.3f, ref rect4, ref rect5);
      ref Rect local1 = ref rect5;
      ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + ((Rect) ref rect).width * 0.2f;
      Widgets.Label(rect4, parameter.Name);
      if (flag)
      {
        Text.Anchor = (TextAnchor) 5;
        Widgets.Label(rect5, Translator.Translate("ST_Injected"));
      }
      else
      {
        this.ParameterInput(rect5, animationEvent, parameter, ref obj);
        animationEvent.method.args[index] = obj;
      }
      Text.Anchor = (TextAnchor) 3;
      ref Rect local2 = ref rect1;
      ((Rect) ref local2).y = ((Rect) ref local2).y + 29f;
    }
  }

  private void ParameterInput(
    Rect rect,
    AnimationEvent animationEvent,
    ParameterInfo parameter,
    ref object value)
  {
    if (parameter.ParameterType == typeof (bool))
    {
      Rect rect1 = GenUI.ContractedBy(new Rect(((Rect) ref rect).xMax - ((Rect) ref rect).height, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height), 2f);
      bool flag = value != null && (bool) value;
      Widgets.Checkbox(((Rect) ref rect1).position, ref flag, 20f, false, false, (Texture2D) null, (Texture2D) null);
      value = (object) flag;
    }
    else if (ParseHelper.HandlesType(parameter.ParameterType))
    {
      if (!this.inputBuffers.ContainsKey(parameter))
        this.inputBuffers[parameter] = (string) null;
      string inputBuffer = this.inputBuffers[parameter];
      this.InputBox(rect, parameter.ParameterType, ref value, ref inputBuffer);
      this.inputBuffers[parameter] = inputBuffer;
    }
    else if (parameter.ParameterType.IsSubclassOf(typeof (Def)))
    {
      Text.Anchor = (TextAnchor) 4;
      Def selectedDef = value as Def;
      string label = selectedDef?.defName ?? "NULL";
      if (!AnimationEditor.Dropdown(rect, label, string.Empty))
        return;
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref this.parent.windowRect).x + this.parent.EditorMargin + ((Rect) ref rect).x, ((Rect) ref this.parent.windowRect).y + this.parent.EditorMargin + ((Rect) ref rect).yMax, 300f, 500f);
      Find.WindowStack.Add((Window) new Dialog_DefDropdown(rect2, parameter.ParameterType, (Action<Def>) (def => this.SetDefParameter(animationEvent, parameter, def)), (Func<Def, bool>) (def => selectedDef == def)));
    }
    else
    {
      Text.Anchor = (TextAnchor) 4;
      Widgets.Label(rect, Translator.Translate("ST_UnsupportedType"));
    }
  }

  private void SetDefParameter(
    AnimationEvent animationEvent,
    ParameterInfo settingParameter,
    Def def)
  {
    ParameterInfo[] parameters = animationEvent.method.method.GetParameters();
    for (int index = 0; index < parameters.Length; ++index)
    {
      if (parameters[index] == settingParameter)
        animationEvent.method.args[index] = (object) def;
    }
  }

  private void AddMethodToEvent(MethodInfo method)
  {
    if (!(this.selector.GetSelected<AnimationEvent>().FirstOrDefault<ISelectableUI>() is AnimationEvent animationEvent))
      return;
    animationEvent.method = new DynamicDelegate(method);
  }

  private void DrawPropertyEntries(Rect rect)
  {
    if (this.animation == null)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + 5f, ((Rect) ref rect).y + 20f, ((Rect) ref rect).width - 10f, 24f);
    Rect rect2 = rect1;
    float height1 = ((Rect) ref rect1).height;
    float height2 = ((Rect) ref rect1).height;
    float num1 = height1;
    bool flag = false;
    foreach (AnimationPropertyParent property1 in this.animation.properties)
    {
      AnimationPropertyParent propertyParent = property1;
      if (this.selector.IsSelected((ISelectableUI) propertyParent))
        Widgets.DrawBoxSolid(rect1, this.itemSelectedColor);
      else if (flag)
        Widgets.DrawBoxSolid(rect1, this.backgroundLightColor);
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(((Rect) ref rect1).x + height1, ((Rect) ref rect1).y, (float) ((double) ((Rect) ref rect1).width - 72.0 - (double) height2 * 2.0) - height1, 24f);
      if (Widgets.ButtonInvisible(rect3, false))
        this.selector.Select((ISelectableUI) propertyParent, !Input.GetKey((KeyCode) 306));
      Rect rect4 = GenUI.ContractedBy(new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y, height1, height1), 3f);
      bool expanded = GenCollection.TryGetValue<AnimationPropertyParent, bool>((IReadOnlyDictionary<AnimationPropertyParent, bool>) this.propertyExpanded, propertyParent, false);
      if (!propertyParent.IsSingle && UIElements.CollapseButton(rect4, ref expanded, this.keyFrameColor, this.keyFrameHighlightColor))
      {
        this.propertyExpanded[propertyParent] = expanded;
        if (expanded)
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
        else
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
      }
      else if (propertyParent.IsSingle)
      {
        float num2 = 72f;
        Rect inputRect;
        // ISSUE: explicit constructor call
        ((Rect) ref inputRect).\u002Ector(((Rect) ref rect1).xMax - height1 - num2, ((Rect) ref rect1).y, num2, ((Rect) ref rect1).height);
        this.KeyFrameInput(inputRect, propertyParent.Properties[0]);
      }
      if (Widgets.ButtonImage(GenUI.ContractedBy(new Rect(((Rect) ref rect1).xMax - ((Rect) ref rect4).width, ((Rect) ref rect1).y, height2, height2), 6f), this.keyFrameTexture, !propertyParent.IsSingle || this.tab != AnimationClipEditor.EditTab.Curves ? this.keyFrameColor : propertyParent.Properties[0].Color, this.keyFrameHighlightColor, true, (string) null))
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_RemoveProperties")), (Action) (() =>
          {
            this.propertiesToRemove.Add(propertyParent);
            this.ChangeMade();
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_AddKey")), (Action) (() =>
          {
            this.AddKeyFramesForParent(propertyParent);
            this.ChangeMade();
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
          {
            Disabled = propertyParent.AllKeyFramesAt(this.Frame)
          },
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_RemoveKey")), (Action) (() =>
          {
            this.RemoveKeyFramesForParent(propertyParent);
            this.ChangeMade();
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
          {
            Disabled = !propertyParent.AnyKeyFrameAt(this.Frame)
          }
        }));
      Rect rect5;
      // ISSUE: explicit constructor call
      ((Rect) ref rect5).\u002Ector(((Rect) ref rect4).xMax, ((Rect) ref rect1).y, ((Rect) ref rect1).width - height1 - height2, ((Rect) ref rect1).height);
      if (Mouse.IsOver(rect5))
        Widgets.DrawBoxSolid(rect5, this.propertyLabelHighlightColor);
      Widgets.Label(rect5, propertyParent.IsIndexer ? propertyParent.LabelWithIdentifier : propertyParent.Label);
      if (expanded)
      {
        foreach (AnimationProperty property2 in propertyParent.Properties)
        {
          AnimationProperty property = property2;
          flag = !flag;
          ref Rect local1 = ref rect1;
          ((Rect) ref local1).y = ((Rect) ref local1).y + ((Rect) ref rect1).height;
          ref Rect local2 = ref rect2;
          ((Rect) ref local2).height = ((Rect) ref local2).height + ((Rect) ref rect1).height;
          if (this.selector.IsSelected((ISelectableUI) property))
            Widgets.DrawBoxSolid(rect1, this.itemSelectedColor);
          else if (flag)
            Widgets.DrawBoxSolid(rect1, this.backgroundLightColor);
          Rect rect6;
          // ISSUE: explicit constructor call
          ((Rect) ref rect6).\u002Ector(((Rect) ref rect1).x + height1, ((Rect) ref rect1).y, (float) ((double) ((Rect) ref rect1).width - 72.0 - (double) height2 * 2.0) - height1, 24f);
          if (Widgets.ButtonInvisible(rect6, false))
            this.selector.Select((ISelectableUI) property, !Input.GetKey((KeyCode) 306));
          if (Widgets.ButtonImage(GenUI.ContractedBy(new Rect(((Rect) ref rect1).xMax - ((Rect) ref rect4).width, ((Rect) ref rect1).y, height2, height2), 6f), this.keyFrameTexture, this.tab == AnimationClipEditor.EditTab.Curves ? property.Color : this.keyFrameColor, this.keyFrameHighlightColor, true, (string) null))
            Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
            {
              new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_RemoveProperties")), (Action) (() =>
              {
                this.propertiesToRemove.Add(propertyParent);
                this.ChangeMade();
              }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
              new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_AddKey")), (Action) (() =>
              {
                property.curve.Add(this.Frame, property.curve[this.Frame]);
                this.ChangeMade();
              }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
              {
                Disabled = property.curve.KeyFrameAt((float) this.Frame)
              },
              new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ST_RemoveKey")), (Action) (() =>
              {
                property.curve.Remove(this.Frame);
                this.ChangeMade();
              }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
              {
                Disabled = !property.curve.KeyFrameAt((float) this.Frame)
              }
            }));
          float num3 = 72f;
          Rect inputRect;
          // ISSUE: explicit constructor call
          ((Rect) ref inputRect).\u002Ector(((Rect) ref rect1).xMax - height1 - num3, ((Rect) ref rect1).y, num3, ((Rect) ref rect1).height);
          this.KeyFrameInput(inputRect, property);
          GUI.color = this.propertyExpandedNameColor;
          Rect rect7;
          // ISSUE: explicit constructor call
          ((Rect) ref rect7).\u002Ector(((Rect) ref rect5).x + num1, ((Rect) ref rect1).y, ((Rect) ref rect1).width - height1 - height2, ((Rect) ref rect1).height);
          if (Mouse.IsOver(rect7))
            Widgets.DrawBoxSolid(rect7, this.propertyLabelHighlightColor);
          Widgets.Label(rect7, $"{propertyParent.Label}.{property.Label}");
          GUI.color = Color.white;
        }
      }
      flag = !flag;
      ref Rect local3 = ref rect1;
      ((Rect) ref local3).y = ((Rect) ref local3).y + ((Rect) ref rect1).height;
      ref Rect local4 = ref rect2;
      ((Rect) ref local4).height = ((Rect) ref local4).height + ((Rect) ref rect1).height;
    }
    ref Rect local = ref rect1;
    ((Rect) ref local).y = ((Rect) ref local).y + 12f;
    this.RemoveFlaggedProperties();
    Rect rect8;
    // ISSUE: explicit constructor call
    ((Rect) ref rect8).\u002Ector((float) ((double) ((Rect) ref rect).xMax / 2.0 - 90.0), ((Rect) ref rect1).yMax, 180f, 24f);
    if (this.ButtonText(rect8, TaggedString.op_Implicit(Translator.Translate("ST_AddProperty"))))
    {
      Vector2 position;
      // ISSUE: explicit constructor call
      ((Vector2) ref position).\u002Ector((float) ((double) ((Rect) ref this.parent.windowRect).x + (double) this.parent.EditorMargin + (double) ((Rect) ref rect8).xMax + 2.0), (float) ((double) ((Rect) ref this.parent.windowRect).y + (double) this.parent.EditorMargin + (double) ((Rect) ref rect8).y + 1.0));
      Find.WindowStack.Add((Window) new Dialog_PropertySelect(this.parent.animator, this.animation, position, new Action<AnimationPropertyParent>(this.InjectKeyFramesNewProperty)));
    }
    if (!Input.GetMouseButton(0) || Mouse.IsOver(rect2) || Mouse.IsOver(rect8) || !Mouse.IsOver(rect))
      return;
    this.selector.DeselectAll<AnimationPropertyParent>();
    this.selector.DeselectAll<AnimationProperty>();
  }

  private void KeyFrameInput(Rect inputRect, AnimationProperty property)
  {
    inputRect = GenUI.ContractedBy(inputRect, 2f);
    string str = (string) null;
    switch (property.PropType)
    {
      case AnimationProperty.PropertyType.Float:
        float num1 = property.curve[this.Frame];
        float num2 = num1;
        Widgets.TextFieldNumeric<float>(inputRect, ref num1, ref str, float.MinValue, float.MaxValue);
        if (!Mathf.Approximately(num1, num2))
        {
          property.curve.Set(this.Frame, num1);
          this.animation.RecacheFrameCount();
          this.ChangeMade();
          break;
        }
        break;
      case AnimationProperty.PropertyType.Int:
        int num3 = Mathf.RoundToInt(property.curve[this.Frame]);
        int num4 = num3;
        Widgets.TextFieldNumeric<int>(inputRect, ref num3, ref str, float.MinValue, float.MaxValue);
        if (num3 != num4)
        {
          property.curve.Set(this.Frame, (float) num3);
          this.animation.RecacheFrameCount();
          this.ChangeMade();
          break;
        }
        break;
      case AnimationProperty.PropertyType.Bool:
        this.animation.RecacheFrameCount();
        this.ChangeMade();
        break;
    }
    this.CheckTextFieldControlFocus(inputRect);
  }

  private void DrawAnimatorSectionRight(Rect rect)
  {
    if (this.parent.animator == null)
      this.DisableGUI(true);
    Widgets.BeginGroup(rect);
    Rect outRect = GenUI.AtZero(rect);
    this.ExtraPadding = 0.0f;
    if ((double) this.EditorWidth < (double) ((Rect) ref outRect).width)
      this.ExtraPadding = ((Rect) ref outRect).width - this.EditorWidth;
    this.FrameCountShown = Mathf.CeilToInt((float) ((double) this.FrameBarWidth + (double) this.ExtraPadding + (double) this.extraPanelWidth + 40.0) / this.FrameTickMarkSpacing);
    if (this.animation == null)
      this.DisableGUI();
    Rect editorOutRect = new Rect(((Rect) ref outRect).x, ((Rect) ref outRect).y, ((Rect) ref outRect).width, ((Rect) ref outRect).height);
    int dragging = (int) this.dragging;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref editorOutRect).x, ((Rect) ref outRect).y + 24f, ((Rect) ref editorOutRect).width, (float) ((double) ((Rect) ref editorOutRect).height - 24.0 - 16.0));
    this.MouseOverSelectableArea = Mouse.IsOver(rect1);
    float num1 = Mathf.Clamp(this.EditorWidth, ((Rect) ref outRect).width, this.EditorWidth);
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(num1 + this.extraPanelWidth, ((Rect) ref editorOutRect).height - 16f);
    Rect editorViewRect = new Rect(((Rect) ref editorOutRect).position, vector2);
    Rect visibleRect1 = this.GetVisibleRect(editorOutRect, this.panelScrollPos, editorViewRect);
    UIElements.BeginScrollView(editorOutRect, ref this.panelScrollPos, editorViewRect, GUI.enabled, false);
    Vector2 groupPos = Vector2.op_Subtraction(Vector2.op_Subtraction(((Rect) ref rect).position, ((Rect) ref visibleRect1).position), new Vector2(0.0f, 32f));
    ref Rect local1 = ref editorViewRect;
    ((Rect) ref local1).height = ((Rect) ref local1).height + 16f;
    Vector2 scrollPosNormalized = this.GetScrollPosNormalized(editorOutRect, this.panelScrollPos, editorViewRect);
    Rect rect2 = this.DrawFrameBar(visibleRect1, editorViewRect, scrollPosNormalized);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref editorViewRect).x, ((Rect) ref rect2).yMax, ((Rect) ref editorViewRect).width, 24f);
    this.DrawAnimationEventMarkers(rect3);
    Rect frameOutRect = new Rect(((Rect) ref editorOutRect).x, ((Rect) ref rect3).yMax, ((Rect) ref editorViewRect).width, ((Rect) ref editorOutRect).height);
    Rect frameViewRect = new Rect(((Rect) ref frameOutRect).x, ((Rect) ref frameOutRect).y, ((Rect) ref frameOutRect).width, ((Rect) ref frameOutRect).height + this.extraFrameHeight);
    Rect visibleRect2 = this.GetVisibleRect(frameOutRect, this.frameScrollPos, frameViewRect);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref visibleRect1).xMax + (float) GUI.skin.horizontalScrollbar.margin.left, ((Rect) ref frameOutRect).y, GUI.skin.verticalScrollbar.fixedWidth, ((Rect) ref frameViewRect).height);
    Mathf.Min(((Rect) ref visibleRect1).height, ((Rect) ref frameViewRect).height);
    UIElements.BeginScrollView(frameOutRect, ref this.frameScrollPos, frameViewRect, false);
    ref Rect local2 = ref frameViewRect;
    ((Rect) ref local2).width = ((Rect) ref local2).width + 16f;
    Rect rect5;
    // ISSUE: explicit constructor call
    ((Rect) ref rect5).\u002Ector(((Rect) ref editorViewRect).x, ((Rect) ref rect3).yMax + ((Rect) ref visibleRect2).y, ((Rect) ref editorViewRect).width, 1f);
    switch (this.tab)
    {
      case AnimationClipEditor.EditTab.Dopesheet:
        float num2 = AnimationEditor.DrawBlend(rect5, this.animationKeyFrameBarFadeColor, this.animationKeyFrameBarColor);
        Rect rect6;
        // ISSUE: explicit constructor call
        ((Rect) ref rect6).\u002Ector(((Rect) ref frameViewRect).x, num2, ((Rect) ref frameViewRect).width, 10f);
        Widgets.DrawBoxSolid(rect6, this.animationKeyFrameBarColor);
        Rect rect7;
        // ISSUE: explicit constructor call
        ((Rect) ref rect7).\u002Ector(((Rect) ref frameViewRect).x, ((Rect) ref rect6).yMax, ((Rect) ref frameViewRect).width, ((Rect) ref frameViewRect).height - ((Rect) ref rect6).yMax);
        this.DrawBackground(rect7);
        this.DrawDopesheetFrameTicks(rect7);
        bool keyFrameSelected1;
        this.DrawKeyFrameMarkers(rect7, out keyFrameSelected1);
        if (this.DragWindow(rect7, AnimationClipEditor.DragItem.KeyFrameWindow, 2))
          SetDragPos(expandVertical: false);
        if (this.dragging != AnimationClipEditor.DragItem.None || keyFrameSelected1 || !this.SelectionBox(groupPos, visibleRect1, rect7, out Rect _, this.FrameTickMarkSpacing, 24f))
          break;
        break;
      case AnimationClipEditor.EditTab.Curves:
        Rect rect8;
        // ISSUE: explicit constructor call
        ((Rect) ref rect8).\u002Ector(((Rect) ref frameViewRect).x, ((Rect) ref rect3).yMax, ((Rect) ref frameViewRect).width, ((Rect) ref frameViewRect).height - ((Rect) ref rect3).height);
        this.DrawBackgroundDark(rect8);
        this.DrawCurvesFrameTicks(rect8);
        double num3 = (double) AnimationEditor.DrawBlend(rect5, this.curveTopFadeColor, this.curveTopColor);
        float y = this.GetScrollPosNormalized(outRect, this.frameScrollPos, frameViewRect).y;
        Rect rect9;
        // ISSUE: explicit constructor call
        ((Rect) ref rect9).\u002Ector(((Rect) ref visibleRect1).x, ((Rect) ref rect8).y, 40f, ((Rect) ref rect8).height);
        this.DrawAxis(rect9, y, visibleRect1);
        Rect rect10;
        // ISSUE: explicit constructor call
        ((Rect) ref rect10).\u002Ector(((Rect) ref rect2).x, 0.0f, this.FrameBarWidth, ((Rect) ref rect8).height);
        bool keyFrameSelected2;
        this.DrawCurves(rect10, visibleRect1, groupPos, out keyFrameSelected2);
        if (this.DragWindow(rect8, AnimationClipEditor.DragItem.KeyFrameWindow, 2))
          SetDragPos();
        if (this.dragging == AnimationClipEditor.DragItem.None && !keyFrameSelected2)
        {
          this.SelectionBox(groupPos, visibleRect1, rect8, out Rect _);
          break;
        }
        break;
    }
    UIElements.EndScrollView(false);
    if (GUI.enabled)
      UIElements.DrawLineVertical(((Rect) ref rect2).x + (float) this.Frame * this.FrameTickMarkSpacing, ((Rect) ref rect2).y, 2000f, Color.white);
    UIElements.EndScrollView(false);
    Widgets.EndGroup();
    this.frameScrollPos.x = this.panelScrollPos.x;
    this.panelScrollPos.y = this.frameScrollPos.y;
    if (GUI.enabled && Mouse.IsOver(rect) && Event.current.type == 6)
    {
      float num4 = Event.current.delta.y * 0.01f;
      bool key1 = Input.GetKey((KeyCode) 306);
      bool key2 = Input.GetKey((KeyCode) 304);
      if (!key1 && !key2)
      {
        this.ZoomFrames += num4;
        this.ZoomCurve += num4;
        Event.current.Use();
      }
      else if (key1)
      {
        this.ZoomFrames += num4;
        Event.current.Use();
      }
      else if (key2)
      {
        this.ZoomCurve += num4;
        Event.current.Use();
      }
    }
    this.EnableGUI(true);

    void SetDragPos(bool horizontal = true, bool vertical = true, bool expandHorizontal = true, bool expandVertical = true)
    {
      Vector2 vector2_1;
      // ISSUE: explicit constructor call
      ((Vector2) ref vector2_1).\u002Ector(Input.mousePosition.x, Input.mousePosition.y);
      Vector2 vector2_2 = Vector2.op_Subtraction(this.dragPos, vector2_1);
      this.dragPos = vector2_1;
      if (horizontal)
      {
        float x = this.GetScrollPosNormalized(editorOutRect, this.panelScrollPos, editorViewRect).x;
        this.panelScrollPos.x += vector2_2.x;
        if (expandHorizontal && Mathf.Approximately(x, 1f))
          this.extraPanelWidth += vector2_2.x;
      }
      if (vertical)
      {
        float y = this.GetScrollPosNormalized(frameOutRect, this.frameScrollPos, frameViewRect).y;
        bool flag1 = Mathf.Approximately(y, 0.0f);
        bool flag2 = Mathf.Approximately(y, 1f);
        if (expandVertical && (flag1 || flag2 && (double) vector2_2.y > 0.0))
        {
          this.extraFrameHeight += vector2_2.y;
        }
        else
        {
          this.frameScrollPos.y -= vector2_2.y;
          if (expandVertical & flag2)
            this.extraFrameHeight -= vector2_2.y;
        }
      }
      this.extraPanelWidth = Mathf.Clamp(this.extraPanelWidth, 0.0f, 5000f);
      this.extraFrameHeight = Mathf.Clamp(this.extraFrameHeight, 0.0f, 5000f);
    }
  }

  private Rect DrawFrameBar(Rect visibleRect, Rect viewRect, Vector2 scrollT)
  {
    Color color1 = this.frameTimeBarColor;
    Color color2 = this.frameTimeBarColorDisabled;
    if (!GUI.enabled)
    {
      color1 = this.backgroundDopesheetColor;
      color2 = this.backgroundDopesheetColor;
    }
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref viewRect).x, ((Rect) ref viewRect).y, 40f, 24f);
    Widgets.DrawBoxSolid(rect1, color1);
    Widgets.DrawBoxSolid(rect1, color2);
    UIElements.DrawLineVertical(((Rect) ref rect1).xMax - 1f, ((Rect) ref rect1).y, ((Rect) ref rect1).height, this.frameTickColor);
    float num = 40f + this.ExtraPadding + this.extraPanelWidth;
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref viewRect).y, this.FrameBarWidth + num, 24f);
    this.DoFrameSlider(visibleRect, rect2, scrollT);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(this.FrameBarWidth + 40f, ((Rect) ref viewRect).y, num, 24f);
    Widgets.DrawBoxSolid(rect3, this.frameTimeBarColorDisabled);
    UIElements.DrawLineVertical(((Rect) ref rect3).x, ((Rect) ref rect3).y, ((Rect) ref rect3).height, this.frameTickColor);
    this.DoFrameSliderHandle(rect2);
    this.DoSeparatorHorizontal(((Rect) ref viewRect).x, ((Rect) ref rect2).yMax, ((Rect) ref viewRect).width);
    ref Rect local = ref rect2;
    ((Rect) ref local).yMax = ((Rect) ref local).yMax + 1f;
    return rect2;
  }

  private void DoFrameSlider(Rect visibleRect, Rect viewRect, Vector2 scrollT)
  {
    Widgets.DrawBoxSolid(viewRect, this.frameTimeBarColor);
    Widgets.BeginGroup(viewRect);
    Text.Anchor = (TextAnchor) 3;
    Text.Font = (GameFont) 0;
    Color color = GUI.color;
    GUI.color = this.frameTickColor;
    float num1 = ((Rect) ref viewRect).height * 0.65f;
    int num2 = Mathf.RoundToInt((((Rect) ref visibleRect).width + 130f) / this.FrameTickMarkSpacing);
    int num3 = Mathf.RoundToInt(Mathf.Clamp(Mathf.Lerp(-1f, (float) (this.FrameCountShown - num2), scrollT.x), 0.0f, (float) this.FrameCountShown)).RoundTo(this.TickInterval);
    int num4 = num3 + num2 + this.TickInterval;
    for (int frame = num3; frame <= num4; frame += this.TickInterval)
    {
      float x = (float) frame * this.FrameTickMarkSpacing;
      float num5 = frame % this.NextTickInterval() != 0 ? (frame % this.TickInterval != 0 ? Mathf.Lerp(num1 / 2f, num1 / 4f, this.ZoomFrames % 1f) : Mathf.Lerp(num1, num1 / 2f, this.ZoomFrames % 1f)) : num1;
      UIElements.DrawLineVertical(x, ((Rect) ref viewRect).yMax, -num5, this.frameTickColor);
      if (frame > 0 && this.TickInterval > 1)
      {
        float num6 = num1 / 4f;
        int num7 = this.SubTickCount();
        for (int index = 1; index <= num7; ++index)
          UIElements.DrawLineVertical(x - (float) index / (float) (num7 + 1) * (float) this.TickInterval * this.FrameTickMarkSpacing, ((Rect) ref viewRect).yMax, -num6, this.frameTickColor);
      }
      if (frame % this.TickInterval == 0)
        Widgets.Label(GenUI.ContractedBy(new Rect(x, ((Rect) ref viewRect).y, this.FrameTickMarkSpacing * (float) this.TickInterval, ((Rect) ref viewRect).height), 3f), this.TimeStamp(frame));
    }
    GUI.color = color;
    Text.Font = (GameFont) 1;
    Widgets.EndGroup();
    this.DoSeparatorVertical(((Rect) ref viewRect).x, ((Rect) ref viewRect).y, ((Rect) ref viewRect).height);
    this.DoSeparatorVertical(((Rect) ref viewRect).xMax, ((Rect) ref viewRect).y, ((Rect) ref viewRect).height);
  }

  private void DoFrameSliderHandle(Rect rect)
  {
    if (!this.DragWindow(rect, AnimationClipEditor.DragItem.FrameBar))
      return;
    this.Frame = this.FrameAtMousePos(rect);
  }

  private void DrawDopesheetFrameTicks(Rect rect)
  {
    if (!GUI.enabled)
      return;
    Widgets.BeginGroup(rect);
    GUI.color = this.frameLineMajorDopesheetColor;
    for (int index1 = 0; index1 <= this.FrameCountShown; index1 += this.TickInterval)
    {
      float x = (float) (40.0 + (double) index1 * (double) this.FrameTickMarkSpacing);
      UIElements.DrawLineVertical(x, 0.0f, ((Rect) ref rect).height - 1f, this.frameLineMajorDopesheetColor);
      int num = this.SubTickCount();
      for (int index2 = 1; index2 <= num; ++index2)
        UIElements.DrawLineVertical(x + (float) index2 / (float) (num + 1) * (float) this.TickInterval * this.FrameTickMarkSpacing, 0.0f, ((Rect) ref rect).height - 1f, this.frameLineMinorDopesheetColor);
    }
    GUI.color = Color.white;
    Widgets.EndGroup();
  }

  private bool DisableCameraView()
  {
    return this.parent.animator is Thing animator ? !animator.Spawned : this.parent.animator == null;
  }

  private void RemoveFlaggedProperties()
  {
    foreach (AnimationPropertyParent animationPropertyParent in this.propertiesToRemove)
      this.animation.properties.Remove(animationPropertyParent);
    this.propertiesToRemove.Clear();
  }

  private int FrameAtMousePos(Rect rect)
  {
    return Mathf.RoundToInt(Mathf.Clamp((Event.current.mousePosition.x - ((Rect) ref rect).x) / ((Rect) ref rect).width * (float) this.FrameCountShown, 0.0f, (float) this.FrameCountShown));
  }

  private void RecalculateTickInterval()
  {
    int n = Mathf.FloorToInt(this.ZoomFrames) - 2;
    if (n < 0)
      this.tickInterval = 1;
    else
      this.tickInterval = Mathf.Clamp(5 * Ext_Math.PowTwo(n), 5, int.MaxValue);
  }

  private void RecalculateCurveTickInterval()
  {
    int n = Mathf.FloorToInt(this.ZoomCurve) - 2;
    if (n < 0)
      this.curveTickInterval = 0.1f;
    else
      this.curveTickInterval = Mathf.Clamp(0.5f * (float) Ext_Math.PowTwo(n), 0.5f, float.MaxValue);
  }

  private void LoadAnimation(AnimationClip animationClip)
  {
    this.propertyExpanded.Clear();
    this.animation = animationClip;
    if ((bool) animationClip)
      return;
    Messages.Message("Unable to load animation file.", MessageTypeDefOf.RejectInput, true);
  }

  private string TimeStamp(int frame) => $"{frame / 60}:{frame % 60:00}";

  private string AxisStamp(float axis) => $"{axis:####0.###}";

  private void DrawCurvesFrameTicks(Rect rect)
  {
    if (!GUI.enabled)
      return;
    Widgets.BeginGroup(rect);
    GUI.color = this.frameLineMajorDopesheetColor;
    for (int index = 0; index <= this.FrameCountShown; index += this.TickInterval)
      UIElements.DrawLineVertical((float) (40.0 + (double) index * (double) this.FrameTickMarkSpacing), 0.0f, ((Rect) ref rect).height - 1f, this.frameLineCurvesColor);
    GUI.color = Color.white;
    Widgets.EndGroup();
  }

  private void DrawAnimationEventMarkers(Rect rect)
  {
    Widgets.DrawBoxSolid(rect, this.animationEventBarColor);
    if (!GUI.enabled)
      return;
    bool flag = Input.GetMouseButtonDown(0);
    foreach (AnimationEvent animationEvent in this.animation.events)
    {
      GUI.color = this.selector.IsSelected((ISelectableUI) animationEvent) ? this.itemSelectedColor : Color.white;
      Rect rect1 = GenUI.ContractedBy(this.DopesheetIconRect(((Rect) ref rect).y, animationEvent.frame), 5f, 3f);
      ref Rect local = ref rect1;
      ((Rect) ref local).y = ((Rect) ref local).y - 3f;
      GUI.DrawTexture(rect1, (Texture) this.animationEventTexture);
      if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect1))
      {
        flag = false;
        this.selector.Select((ISelectableUI) animationEvent, this.SingleSelect);
        this.inputBuffers.Clear();
      }
      GUI.color = Color.white;
    }
    if (!flag || !this.MouseOverSelectableArea)
      return;
    this.selector.DeselectAll<AnimationEvent>();
  }

  private void DrawKeyFrameMarkers(Rect rect, out bool keyFrameSelected)
  {
    keyFrameSelected = false;
    if (!GUI.enabled)
      return;
    bool clickedOutside = Input.GetMouseButtonDown(0);
    Vector4 selectRectBounds = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
    this.framesToDraw.Clear();
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width, 24f);
    float y1 = ((Rect) ref rect1).y - 20f;
    foreach (AnimationPropertyParent property1 in this.animation.properties)
    {
      float y2 = ((Rect) ref rect1).y;
      this.parentFramesToDraw.Clear();
      Widgets.DrawBoxSolidWithOutline(GenUI.ContractedBy(rect1, 1f), this.frameBarHighlightColor, this.frameBarHighlightOutlineColor, 1);
      if (property1.IsSingle)
      {
        AnimationCurve curve = property1.Properties[0].curve;
        if (curve != null && !curve.points.NullOrEmpty<KeyFrame>())
        {
          for (int index = 0; index < curve.points.Count; ++index)
          {
            KeyFrame point = curve.points[index];
            this.framesToDraw.Add((index, point.frame));
            if (KeyFrameButton(((Rect) ref rect1).y, point.frame, this.keyFrameColor, this.keyFrameSelector.IsSelected(property1.Properties[0], point.frame)))
              this.keyFrameSelector.SelectFrame(property1, point.frame);
          }
        }
      }
      else
      {
        Widgets.DrawBoxSolidWithOutline(GenUI.ContractedBy(rect1, 2f), this.frameBarHighlightColor, this.frameBarHighlightOutlineColor, 1);
        bool flag = GenCollection.TryGetValue<AnimationPropertyParent, bool>((IReadOnlyDictionary<AnimationPropertyParent, bool>) this.propertyExpanded, property1, false);
        foreach (AnimationProperty property2 in property1.Properties)
        {
          if (flag)
          {
            ref Rect local = ref rect1;
            ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height;
            Widgets.DrawBoxSolidWithOutline(GenUI.ContractedBy(rect1, 1f), this.frameBarHighlightMinorColor, this.frameBarHighlightOutlineColor, 1);
          }
          for (int index = 0; index < property2.curve.points.Count; ++index)
          {
            KeyFrame point = property2.curve.points[index];
            this.framesToDraw.Add((index, point.frame));
            this.parentFramesToDraw.Add((index, point.frame));
            if (flag && KeyFrameButton(((Rect) ref rect1).y, point.frame, this.keyFrameColor, this.keyFrameSelector.IsSelected(property2, point.frame)))
            {
              keyFrameSelected = true;
              this.keyFrameSelector.SelectFrame(property2, point.frame);
            }
          }
        }
      }
      foreach ((int _, int frame) in this.parentFramesToDraw)
      {
        if (KeyFrameButton(y2, frame, this.keyFrameColor, this.keyFrameSelector.IsSelected(property1, frame)))
        {
          keyFrameSelected = true;
          this.keyFrameSelector.SelectFrame(property1, frame);
        }
      }
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).y = ((Rect) ref local1).y + ((Rect) ref rect1).height;
    }
    foreach ((int _, int frame) in this.framesToDraw)
    {
      if (KeyFrameButton(y1, frame, this.keyFrameTopColor, this.keyFrameSelector.IsSelected(frame)))
      {
        keyFrameSelected = true;
        this.keyFrameSelector.SelectAll(this.animation, frame);
      }
    }
    if (clickedOutside && this.MouseOverSelectableArea)
      this.keyFrameSelector.ClearSelectedKeyFrames();
    if (!this.keyFrameSelector.AnyKeyFrameSelected)
      return;
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(selectRectBounds.x, selectRectBounds.y, selectRectBounds.z - selectRectBounds.x, selectRectBounds.w - selectRectBounds.y);
    if (this.keyFrameSelector.selPropKeyFrames.Count > 1 && GenCollection.Any<(AnimationProperty, int)>(this.keyFrameSelector.selPropKeyFrames, (Predicate<(AnimationProperty, int)>) (pair => pair.property != this.keyFrameSelector.selPropKeyFrames[0].property)))
      Widgets.DrawBoxSolid(rect2, this.selectBoxFillColor);
    if (!this.DragWindow(rect2, new Action(SetDragItem), new Func<bool>(IsDragging), new Action(StartDragging), new Action(StopDragging)))
      return;
    Vector2 mouseDiff = this.keyFrameDragger.MouseDiff;
    foreach ((AnimationProperty _, int _) in this.keyFrameSelector.selPropKeyFrames)
      ;

    static void SetDragItem()
    {
    }

    static bool IsDragging() => false;

    static void StopDragging()
    {
    }

    void StartDragging()
    {
      this.keyFrameDragger.Start();
      this.dragging = AnimationClipEditor.DragItem.KeyFrameHandle;
      this.dragPos = Vector2.op_Implicit(Input.mousePosition);
    }

    bool KeyFrameButton(float y, int frame, Color color, bool selected)
    {
      bool flag = false;
      GUI.color = selected ? this.itemSelectedColor : color;
      Rect rect = GenUI.ContractedBy(this.DopesheetIconRect(y, frame), 4f);
      GUI.DrawTexture(rect, (Texture) this.keyFrameTexture);
      if (selected)
      {
        if ((double) ((Rect) ref rect).xMin < (double) selectRectBounds.x)
          selectRectBounds.x = ((Rect) ref rect).xMin;
        if ((double) ((Rect) ref rect).yMin < (double) selectRectBounds.y)
          selectRectBounds.y = ((Rect) ref rect).yMin;
        if ((double) ((Rect) ref rect).xMax > (double) selectRectBounds.z)
          selectRectBounds.z = ((Rect) ref rect).xMax;
        if ((double) ((Rect) ref rect).yMax > (double) selectRectBounds.w)
          selectRectBounds.w = ((Rect) ref rect).yMax;
      }
      if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect))
      {
        flag = true;
        clickedOutside = false;
        if (this.SingleSelect)
          this.keyFrameSelector.ClearSelectedKeyFrames();
      }
      GUI.color = Color.white;
      return flag;
    }
  }

  private Rect DopesheetIconRect(float y, int frame)
  {
    return new Rect((float) (40.0 + (double) frame * (double) this.FrameTickMarkSpacing - 12.0 + 0.5), y, 24f, 24f);
  }

  private void DrawCurves(
    Rect rect,
    Rect visibleRect,
    Vector2 groupPos,
    out bool keyFrameSelected)
  {
    keyFrameSelected = false;
    if (!GUI.enabled)
      return;
    if (!this.selector.AnySelected<AnimationPropertyParent>() && !this.selector.AnySelected<AnimationProperty>())
    {
      foreach (AnimationPropertyParent property in this.animation.properties)
        DrawPropertyParent(property, ref keyFrameSelected);
    }
    else
    {
      foreach (AnimationPropertyParent propertyParent in this.selector.GetSelected<AnimationPropertyParent>())
        DrawPropertyParent(propertyParent, ref keyFrameSelected);
      foreach (AnimationProperty property in this.selector.GetSelected<AnimationProperty>())
        DrawProperty(property, ref keyFrameSelected);
    }

    void DrawPropertyParent(AnimationPropertyParent propertyParent, ref bool keyFrameSelected)
    {
      if (propertyParent.IsSingle)
      {
        DrawProperty(propertyParent.Properties[0], ref keyFrameSelected);
      }
      else
      {
        if (propertyParent.Properties.NullOrEmpty<AnimationProperty>())
          return;
        foreach (AnimationProperty property in propertyParent.Properties)
        {
          if (!this.selector.GetSelected<AnimationPropertyParent>().Contains((ISelectableUI) property))
            DrawProperty(property, ref keyFrameSelected);
        }
      }
    }

    void DrawProperty(AnimationProperty property, ref bool keyFrameSelected)
    {
      AnimationGraph.DrawAnimationCurve(rect, visibleRect, property.curve, property.Color, this.CurveAxisSpacing);
      this.DragHandle(rect, visibleRect, groupPos, property, ref keyFrameSelected);
    }
  }

  private void DrawAxis(Rect rect, float yT, Rect visibleRect)
  {
    if (!GUI.enabled)
      return;
    Text.Anchor = (TextAnchor) 8;
    Text.Font = (GameFont) 0;
    GUI.color = this.curveAxisColor;
    DrawAxisTick(0.0f, 0.0f);
    float curveTickInterval = this.CurveTickInterval;
    int num = this.CurveTickMarks(rect);
    for (int index = 1; index < num; ++index)
    {
      DrawAxisTick(this.CurveTickInterval * (float) index, -curveTickInterval);
      DrawAxisTick(-this.CurveTickInterval * (float) index, curveTickInterval);
      curveTickInterval += this.CurveTickInterval;
    }
    GUI.color = Color.white;
    Text.Font = (GameFont) 1;
    Text.Anchor = (TextAnchor) 0;

    void DrawAxisTick(float value, float tick)
    {
      float num = this.CurveAxisSpacing * this.CurveTickInterval;
      float y = (float) ((double) ((Rect) ref rect).height / 2.0 + (double) tick * (double) this.CurveAxisSpacing);
      UIElements.DrawLineHorizontal(((Rect) ref rect).x, y, ((Rect) ref rect).width, this.frameBarCurveColor);
      Widgets.Label(GenUI.ContractedBy(new Rect(((Rect) ref rect).x, y - num, ((Rect) ref rect).width, num), 3f), this.AxisStamp(value));
    }
  }

  private int CurveTickMarks(Rect rect)
  {
    return Mathf.CeilToInt(((Rect) ref rect).height / (this.CurveAxisSpacing * this.CurveTickInterval));
  }

  private void DragHandle(
    Rect rect,
    Rect visibleRect,
    Vector2 groupPos,
    AnimationProperty property,
    ref bool keyFrameSelected)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      bool flag = false;
      List<KeyFrame> points = property.curve.points;
      for (int index = 0; index < points.Count; ++index)
      {
        KeyFrame keyFrame = points[index];
        int frame = keyFrame.frame;
        float num1 = property.curve[frame];
        Vector2 screenPos = AnimationGraph.GraphCoordToScreenPos(rect, new Vector2((float) frame, num1), property.curve.RangeX, this.CurveAxisSpacing);
        float num2 = this.keyFrameSelector.IsSelected(property, frame) ? 15f : 12f;
        Rect texRect;
        // ISSUE: explicit constructor call
        ((Rect) ref texRect).\u002Ector(screenPos.x - num2 / 2f, screenPos.y - num2 / 2f, num2, num2);
        GUI.color = Mouse.IsOver(texRect) ? property.Color.AddNoAlpha(0.1f, 0.1f, 0.1f) : property.Color;
        GUI.DrawTexture(texRect, (Texture) this.keyFrameTexture);
        GUI.color = this.keyFrameSelector.IsSelected(property, frame) ? Color.white : Color.black;
        GUI.DrawTexture(GenUI.ContractedBy(texRect, 3f), (Texture) this.keyFrameTexture);
        GUI.color = Color.white;
        Rect rect1 = GenUI.ExpandedBy(new Rect(((Rect) ref texRect).center.x, ((Rect) ref texRect).center.y, 0.0f, 0.0f), 10f);
        if (this.LeftClickDown && Mouse.IsOver(rect1))
        {
          this.keyFrameSelector.SelectFrame(property, keyFrame.frame);
          keyFrameSelected = true;
          Event.current.Use();
        }
        if (this.keyFrameSelector.IsSelected(property, keyFrame.frame))
        {
          if (!points.OutOfBounds<KeyFrame>(index - 1))
            DrawTangentHandle(texRect, screenPos, property, index, false);
          if (!points.OutOfBounds<KeyFrame>(index + 1))
            DrawTangentHandle(texRect, screenPos, property, index, true);
        }
      }
      if (!flag)
        return;
      property.curve.points.Sort();

      void DrawTangentHandle(
        Rect texRect,
        Vector2 dragHandlePos,
        AnimationProperty property,
        int i,
        bool forward)
      {
        KeyFrame keyFrame = points[i];
        float num1 = forward ? keyFrame.outWeight : keyFrame.inWeight;
        float num2 = forward ? keyFrame.outTangent : keyFrame.inTangent;
        float num3 = property.curve[keyFrame.frame];
        Vector2 screenPos = AnimationGraph.GraphCoordToScreenPos(rect, new Vector2((float) keyFrame.frame, num3), property.curve.RangeX, this.CurveAxisSpacing);
        Rect rect1;
        // ISSUE: explicit constructor call
        ((Rect) ref rect1).\u002Ector(screenPos.x - 6f, screenPos.y - 6f, 12f, 12f);
        float num4 = forward ? 50f : -50f;
        if (keyFrame.weightedMode == 1 && !forward || keyFrame.weightedMode == 2 & forward || keyFrame.weightedMode == 3)
          num4 = Mathf.Lerp(dragHandlePos.x, screenPos.x, num1);
        float num5 = num2 * num2;
        float num6 = num4 / Mathf.Sqrt(1f + num5);
        Vector2 vector2_1;
        // ISSUE: explicit constructor call
        ((Vector2) ref vector2_1).\u002Ector(num6 + dragHandlePos.x, -num2 * num6 + dragHandlePos.y);
        Rect rect2;
        // ISSUE: explicit constructor call
        ((Rect) ref rect2).\u002Ector(Vector2.op_Subtraction(vector2_1, Vector2.op_Division(((Rect) ref texRect).size, 2f)), ((Rect) ref texRect).size);
        Widgets.DrawLine(((Rect) ref texRect).center, ((Rect) ref rect2).center, Color.white, 0.5f);
        GUI.DrawTexture(GenUI.ContractedBy(rect2, 2f), (Texture) this.keyFrameTexture);
        if (!this.DragWindow(GenUI.ExpandedBy(rect2, 2f), new Action(OnTangentDrag), new Func<bool>(IsTangentDrag), new Action(OnStartTangentDrag), new Action(OnStopTangentDrag)))
          return;
        Vector2 vector2_2 = this.MouseUIPos(Vector2.op_Subtraction(groupPos, ((Rect) ref visibleRect).position));
        float num7 = vector2_2.y - ((Rect) ref texRect).center.y;
        float num8 = vector2_2.x - ((Rect) ref texRect).center.x;
        float num9 = (double) Mathf.Abs(num8) >= 0.25 ? num7 / num8 : ((double) num7 > 0.0 ? float.PositiveInfinity : float.NegativeInfinity);
        points[i] = new KeyFrame(keyFrame.frame, keyFrame.value, -num9, -num9);

        bool IsTangentDrag()
        {
          return this.keyFrameSelector.draggingTangent.property == property && this.keyFrameSelector.draggingTangent.index == i && this.keyFrameSelector.draggingTangent.forward == forward;
        }

        void OnStartTangentDrag()
        {
          this.dragging = AnimationClipEditor.DragItem.TangentHandle;
          this.keyFrameSelector.draggingTangent = (property, i, forward);
        }
      }
    }
    finally
    {
      textBlock.Dispose();
    }

    void OnTangentDrag()
    {
      Vector2 vector2;
      // ISSUE: explicit constructor call
      ((Vector2) ref vector2).\u002Ector(Input.mousePosition.x, Input.mousePosition.y);
      Vector2.op_Subtraction(this.dragPos, vector2);
      this.dragPos = vector2;
    }

    void OnStopTangentDrag()
    {
      this.dragging = AnimationClipEditor.DragItem.None;
      this.keyFrameSelector.draggingTangent = ((AnimationProperty) null, 0, false);
    }
  }

  private bool DragWindow(Rect rect, AnimationClipEditor.DragItem dragItem, int button = 0)
  {
    return this.DragWindow(rect, new Action(SetDragItem), new Func<bool>(IsDragging), new Action(StartDragging), new Action(StopDragging), button);

    void SetDragItem() => this.dragging = dragItem;

    bool IsDragging() => this.dragging == dragItem;

    void StartDragging()
    {
      this.dragging = dragItem;
      this.dragPos = Vector2.op_Implicit(Input.mousePosition);
    }

    void StopDragging() => this.dragging = AnimationClipEditor.DragItem.None;
  }

  private void InjectKeyFramesNewProperty(AnimationPropertyParent propertyParent)
  {
    foreach (AnimationProperty property in propertyParent.Properties)
      Inject(property);

    void Inject(AnimationProperty property)
    {
      property.curve.Set(0, 0.0f);
      property.curve.Set(this.FrameCount, 0.0f);
      this.ChangeMade();
    }
  }

  private void AddKeyFramesForParent(AnimationPropertyParent propertyParent)
  {
    foreach (AnimationProperty animationProperty in propertyParent)
    {
      float num = animationProperty.curve[this.Frame];
      animationProperty.curve.Add(this.Frame, num);
    }
    this.ChangeMade();
  }

  private void RemoveKeyFramesForParent(AnimationPropertyParent propertyParent)
  {
    foreach (AnimationProperty property in propertyParent.Properties)
      property.curve.Remove(this.Frame);
    this.ChangeMade();
  }

  private int SubTickCount()
  {
    if (this.TickInterval == 1)
      return 0;
    return this.TickInterval == 5 ? 4 : 9;
  }

  private int NextTickInterval() => this.TickInterval == 1 ? 5 : this.TickInterval * 2;

  private enum DragItem
  {
    None,
    FrameBar,
    KeyFrameWindow,
    KeyFrameHandle,
    TangentHandle,
  }

  private enum EditTab
  {
    Dopesheet,
    Curves,
  }

  private class KeyFrameSelector
  {
    public (AnimationProperty property, int index, bool forward) draggingTangent;
    public List<(AnimationProperty property, int frame)> selPropKeyFrames = new List<(AnimationProperty, int)>();

    public bool AnyKeyFrameSelected => this.selPropKeyFrames.Count > 0;

    public bool IsSelected(int frame)
    {
      return GenCollection.Any<(AnimationProperty, int)>(this.selPropKeyFrames, (Predicate<(AnimationProperty, int)>) (selection => selection.frame == frame));
    }

    public bool IsSelected(AnimationPropertyParent propertyParent, int frame)
    {
      if (propertyParent == null || !propertyParent.IsValid)
        return false;
      foreach (AnimationProperty property in propertyParent.Properties)
      {
        if (this.IsSelected(property, frame))
          return true;
      }
      return false;
    }

    public bool IsSelected(AnimationProperty property, int frame)
    {
      return property != null && property.IsValid && this.selPropKeyFrames.Contains((property, frame));
    }

    public void SelectAll(AnimationClip clip, int frame)
    {
      if (!Input.GetKey((KeyCode) 306))
        this.ClearSelectedKeyFrames();
      foreach (AnimationPropertyParent property in clip.properties)
        this.SelectFrame(property, frame, false);
    }

    public void SelectFrame(AnimationPropertyParent propertyParent, int frame, bool clear = true)
    {
      if (clear && !Input.GetKey((KeyCode) 306))
        this.ClearSelectedKeyFrames();
      foreach (AnimationProperty property in propertyParent.Properties)
        this.SelectFrame(property, frame, false, false);
      GenCollection.SortBy<(AnimationProperty, int), int>(this.selPropKeyFrames, (Func<(AnimationProperty, int), int>) (selProp => selProp.frame));
    }

    public void SelectFrame(AnimationProperty property, int frame, bool clear = true, bool sort = true)
    {
      if (clear && !Input.GetKey((KeyCode) 306))
        this.ClearSelectedKeyFrames();
      if (this.selPropKeyFrames.Contains((property, frame)))
        return;
      this.selPropKeyFrames.Add((property, frame));
    }

    public void ClearSelectedKeyFrames() => this.selPropKeyFrames.Clear();
  }

  private class KeyFrameDragHandler
  {
    private readonly Dictionary<(AnimationProperty property, int index), (int frame, float value)> innerContainer = new Dictionary<(AnimationProperty, int), (int, float)>();
    private Vector2 clickPos;
    private float dragThreshold;

    public Vector2 ClickPos => this.clickPos;

    public Vector2 MouseDiff
    {
      get
      {
        return Vector2.op_Subtraction(new Vector2(Input.mousePosition.x, Input.mousePosition.y), this.ClickPos);
      }
    }

    public bool ReachedThreshold { get; private set; }

    private bool Dragging { get; set; }

    public void Update()
    {
      if (this.Dragging)
        return;
      this.ReachedThreshold = false;
      if (Input.GetMouseButtonDown(0))
        this.clickPos = Vector2.op_Implicit(Input.mousePosition);
      if (!Input.GetMouseButton(0))
        return;
      double magnitude1 = (double) ((Vector2) ref this.clickPos).magnitude;
      Vector3 mousePosition = Input.mousePosition;
      double magnitude2 = (double) ((Vector3) ref mousePosition).magnitude;
      if ((double) Mathf.Abs((float) (magnitude1 - magnitude2)) < (double) this.dragThreshold)
        return;
      this.ReachedThreshold = true;
    }

    public void Configure(float dragThreshold) => this.dragThreshold = dragThreshold;

    public void Start() => this.Dragging = true;

    public void Clear()
    {
      this.clickPos = Vector2.zero;
      this.innerContainer.Clear();
      this.ReachedThreshold = false;
      this.Dragging = false;
    }

    public void Record(AnimationProperty property, int index, int frame, float originalValue)
    {
      this.innerContainer[(property, index)] = (frame, originalValue);
    }

    public bool Contains(AnimationProperty property, int index)
    {
      return this.innerContainer.ContainsKey((property, index));
    }

    public bool TryGetOriginalValue(
      AnimationProperty property,
      int index,
      out int frame,
      out float originalValue)
    {
      frame = -1;
      originalValue = float.MinValue;
      (int frame, float value) tuple;
      if (!this.innerContainer.TryGetValue((property, index), out tuple))
        return false;
      frame = tuple.frame;
      originalValue = tuple.value;
      return true;
    }
  }
}
