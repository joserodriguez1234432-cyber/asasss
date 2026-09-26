// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_GraphEditor
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public class Dialog_GraphEditor : Dialog_Graph
{
  public const float DefaultMinX = 0.0f;
  public const float DefaultMaxX = 1f;
  public const float CoordinatesListButtonSize = 24f;
  public const float PercentCameraBoxWidth = 0.4f;
  public const float PlaybackBarHeight = 3f;
  public const float PlaybackRectHeight = 25f;
  public const float PlaybackHandleSize = 12f;
  public const float PlayButtonFadeTime = 1.5f;
  public static readonly FloatRange ZoomRange = new FloatRange(2f, 16f);
  private static readonly string filePath = Path.Combine(Application.persistentDataPath, "GraphEditorExport.xml");
  private static readonly Texture2D[] viewerButtonTextures = new Texture2D[3]
  {
    ContentFinder<Texture2D>.Get("SmashTools/VideoPause", false) ?? ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause", true),
    ContentFinder<Texture2D>.Get("SmashTools/VideoPlay", false) ?? ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Normal", true),
    ContentFinder<Texture2D>.Get("SmashTools/VideoSkiptoNext", false) ?? ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Fast", true)
  };
  private static readonly Texture2D dragHandleIcon = ContentFinder<Texture2D>.Get("SmashTools/ViewHandle", false) ?? ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult", true);
  private Graph.GraphType graphType;
  private LinearCurve curve;
  private IAnimationTarget animationTarget;
  private List<AnimatorObject> animators;
  private AnimatorObject curAnimator;
  private bool drawCoordLabels = true;
  private Vector2 scrollPos;
  private Listing_SplitColumns lister = new Listing_SplitColumns();
  private List<IAnimationTarget> potentialAnimationTargets = new List<IAnimationTarget>();
  private static bool dragging = false;
  private static float timeFading = 0.0f;

  public Dialog_GraphEditor()
    : base((Graph.Function) null, new FloatRange(0.0f, 1f), new List<CurvePoint>())
  {
    this.doCloseX = true;
    this.forcePause = true;
    this.GraphType = Graph.GraphType.Linear;
    this.ReinstantiateCurve();
    this.ValidateLimits(true);
  }

  public Dialog_GraphEditor(Graph.Function function, FloatRange range, bool vectorEvaluation = false)
    : base(function, range, new List<CurvePoint>(), vectorEvaluation)
  {
    this.doCloseX = true;
    this.forcePause = true;
    this.GraphType = Graph.GraphType.Linear;
    this.ReinstantiateCurve();
    this.ValidateLimits(true);
  }

  public Dialog_GraphEditor(
    Graph.Function function,
    FloatRange range,
    List<CurvePoint> plotPoints,
    bool vectorEvaluation = false)
    : base(function, range, plotPoints, vectorEvaluation)
  {
    this.doCloseX = true;
    this.forcePause = true;
    this.GraphType = Graph.GraphType.Linear;
    this.ReinstantiateCurve();
    this.ValidateLimits(true);
  }

  public Dialog_GraphEditor(IAnimationTarget animationTarget = null, bool vectorEvaluation = false)
    : base((Graph.Function) null, new FloatRange(0.0f, 1f), new List<CurvePoint>(), vectorEvaluation)
  {
    this.doCloseX = true;
    this.forcePause = true;
    this.GraphType = Graph.GraphType.Linear;
    this.animationTarget = animationTarget;
    this.ReinstantiateCurve();
    this.ValidateLimits(true);
  }

  public override List<CurvePoint> CurvePoints => this.curve.points;

  protected override bool Editable => true;

  protected override bool DrawCoordLabels => this.drawCoordLabels;

  public bool DisableCameraView => this.animationTarget == null;

  public bool LogReport { get; set; }

  private float StartingAnimationDriverTick { get; set; }

  public override Vector2 InitialSize
  {
    get
    {
      float num = (float) Mathf.Min(UI.screenWidth, UI.screenHeight);
      return new Vector2(num * 1.5f, num);
    }
  }

  public Graph.GraphType GraphType
  {
    get => this.graphType;
    set
    {
      this.graphType = value;
      this.ReinstantiateCurve();
      this.ValidateLimits(true);
    }
  }

  public Type CurveType
  {
    get
    {
      Type curveType;
      switch (this.GraphType)
      {
        case Graph.GraphType.Linear:
          curveType = typeof (LinearCurve);
          break;
        case Graph.GraphType.Bezier:
          curveType = typeof (BezierCurve);
          break;
        case Graph.GraphType.Lagrange:
          curveType = typeof (LagrangeCurve);
          break;
        case Graph.GraphType.Staircase:
          curveType = typeof (StaircaseCurve);
          break;
        default:
          curveType = (Type) null;
          break;
      }
      return curveType;
    }
  }

  protected override float Progress => -1f;

  public virtual void PostOpen()
  {
    base.PostOpen();
    if (!Find.Maps.NullOrEmpty<Map>())
      this.potentialAnimationTargets = Find.Maps.SelectMany<Map, Thing>((Func<Map, IEnumerable<Thing>>) (map => ((IEnumerable<Thing>) map.spawnedThings).Where<Thing>((Func<Thing, bool>) (thing => thing is IAnimationTarget)))).Cast<IAnimationTarget>().ToList<IAnimationTarget>();
    if (this.DisableCameraView || !AnimationSimulator.Reserve(this.animationTarget, new AnimationSimulator.Ticker(this.AnimationTick)))
      return;
    this.TryStartCamera(this.animationTarget);
  }

  public virtual void PostClose()
  {
    base.PostClose();
    CameraView.Close();
    AnimationSimulator.Release();
  }

  private void TryStartCamera(IAnimationTarget animationTarget)
  {
    if (CameraView.InUse)
      CameraView.Close();
    this.animationTarget = animationTarget;
    if (this.animationTarget == null)
      return;
    this.RecacheAnimationTargetCurves();
    CameraJumper.TryJump(GlobalTargetInfo.op_Implicit((Thing) animationTarget.Thing), (CameraJumper.MovementMode) 1);
    CameraView.Start(CameraView.animationSettings.orthographicSize);
    Find.Selector.ClearSelection();
    AnimationSimulator.Reset();
    AnimationSimulator.SetDriver((AnimationDriver) null);
    this.RecacheAnimationDriverStartingTick();
  }

  public void RecacheAnimationTargetCurves()
  {
    StringBuilder stringBuilder = this.LogReport ? new StringBuilder() : (StringBuilder) null;
    IAnimationTarget animationTarget = this.animationTarget;
    this.animators = animationTarget != null ? animationTarget.GetAnimators(stringBuilder).OrderBy<AnimatorObject, string>((Func<AnimatorObject, string>) (animObject => animObject.DisplayName)).ToList<AnimatorObject>() : (List<AnimatorObject>) null;
    if (stringBuilder == null)
      return;
    Log.Message($"----- Report: -----\n{stringBuilder}");
  }

  public void ValidateLimits(bool hardSet = false)
  {
    if (this.curve != null && !this.curve.points.NullOrEmpty<CurvePoint>())
    {
      float num1 = this.curve.points.Min<CurvePoint>((Func<CurvePoint, float>) (cp => ((CurvePoint) ref cp).x));
      float num2 = this.curve.points.Max<CurvePoint>((Func<CurvePoint, float>) (cp => ((CurvePoint) ref cp).x));
      this.XRange = new FloatRange(hardSet ? num1 : Mathf.Min(this.XRange.min, num1), hardSet ? num2 : Mathf.Max(this.XRange.max, num2));
      float num3 = this.curve.points.Min<CurvePoint>((Func<CurvePoint, float>) (cp => ((CurvePoint) ref cp).y));
      float num4 = this.curve.points.Max<CurvePoint>((Func<CurvePoint, float>) (cp => ((CurvePoint) ref cp).y));
      this.YRange = new FloatRange(hardSet ? num3 : Mathf.Min(this.YRange.min, num3), hardSet ? num4 : Mathf.Max(this.YRange.max, num4));
    }
    else
    {
      this.XRange = new FloatRange(0.0f, 1f);
      this.YRange = new FloatRange(0.0f, 1f);
    }
  }

  private void SelectAnimator(AnimatorObject animatorObject)
  {
    this.curAnimator = animatorObject;
    if (animatorObject != null)
    {
      this.curve = animatorObject.Curve;
      this.GraphType = this.CurveToGraphType(this.curve.GetType());
      this.Function = new Graph.Function(this.curve.Function);
    }
    else
    {
      this.GraphType = Graph.GraphType.Linear;
      this.curve = (LinearCurve) null;
      this.XRange = new FloatRange(0.0f, 1f);
      this.YRange = new FloatRange(0.0f, 1f);
      this.ReinstantiateCurve();
    }
    this.ValidateLimits(true);
  }

  private void SelectAnimationDriver(AnimationDriver animationDriver)
  {
    if (AnimationSimulator.CurrentDriver != null && AnimationSimulator.CurrentDriver != animationDriver)
      AnimationSimulator.Reset();
    AnimationSimulator.SetDriver(animationDriver);
    this.RecacheAnimationDriverStartingTick();
    this.SelectAnimator((AnimatorObject) null);
  }

  private void RecacheAnimationDriverStartingTick()
  {
    this.StartingAnimationDriverTick = 0.0f;
    if (AnimationSimulator.CurrentDriver == null)
      return;
    int num = 0;
    foreach (AnimationDriver animation in this.animationTarget.Animations)
    {
      if (animation == AnimationSimulator.CurrentDriver)
      {
        this.StartingAnimationDriverTick = (float) num;
        break;
      }
      num += animation.AnimationLength;
    }
  }

  public Graph.GraphType CurveToGraphType(Type type)
  {
    if (type == typeof (LinearCurve))
      return Graph.GraphType.Linear;
    if (type == typeof (BezierCurve))
      return Graph.GraphType.Bezier;
    if (type == typeof (LagrangeCurve))
      return Graph.GraphType.Lagrange;
    if (type == typeof (StaircaseCurve))
      return Graph.GraphType.Staircase;
    throw new NotImplementedException("GraphType");
  }

  public void ReinstantiateCurve()
  {
    this.curve = (LinearCurve) Activator.CreateInstance(this.CurveType, (object) (this.curve?.points ?? new List<CurvePoint>()));
    this.Function = new Graph.Function(this.curve.Function);
  }

  public virtual void WindowUpdate()
  {
    base.WindowUpdate();
    if (!this.DisableCameraView)
    {
      if (CameraView.animationSettings.drawCellGrid)
        CameraView.DrawMapGridInView();
      (Vector3 drawPos, float rotation) = this.animationTarget.DrawData;
      if (AnimationSimulator.CurrentDriver != null)
        drawPos = AnimationSimulator.CurrentDriver.Draw(drawPos, rotation).drawPos;
      CameraView.Update(drawPos);
    }
    AnimationSimulator.Update();
  }

  public void AnimationTick()
  {
    if (AnimationSimulator.CurrentDriver == null)
      return;
    if (AnimationSimulator.TicksPassed >= AnimationSimulator.CurrentDriver.AnimationLength)
    {
      AnimationSimulator.Reset();
      AnimationSimulator.Paused = !CameraView.animationSettings.loop;
    }
    else
      AnimationSimulator.CurrentDriver.Tick(AnimationSimulator.TicksPassed);
  }

  public override void DoWindowContents(Rect inRect)
  {
    AnimationSimulator.OnGUI();
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(inRect);
    ((Rect) ref rect1).width = (float) ((double) base.InitialSize.x - (double) base.InitialSize.y - 10.0);
    Rect rect2 = rect1;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Widgets.DrawMenuSection(rect2);
      Rect rect3 = GenUI.ContractedBy(rect2, 10f);
      this.TopLevelButtons(rect3, (this.graphType.ToString(), new Action(this.SelectCurveType)), (this.VectorEvaluation ? "Vector" : "Simplified", (Action) (() => this.VectorEvaluation = !this.VectorEvaluation)), ("Save", (Action) (() => this.SaveEdits())), ("Export Xml", new Action(this.ExportAnimationXml)));
      string str;
      switch (this.graphType)
      {
        case Graph.GraphType.Linear:
          str = "y = mx + b";
          break;
        case Graph.GraphType.Bezier:
          str = "Σ{n : i=0} nC_i ((1-t) ^ (n-i)) * (t^i) * (P_i)";
          break;
        case Graph.GraphType.Lagrange:
          str = "Σ{n-1 : i=0} y * ∏{n-1 : j=0, j≠1} (x - xj) / (xi - xj)";
          break;
        default:
          str = string.Empty;
          break;
      }
      Vector2 vector2 = Verse.Text.CalcSize(str);
      Rect rect4 = new Rect(((Rect) ref rect3).x + ((Rect) ref rect3).width - vector2.x, ((Rect) ref rect3).y, vector2.x, vector2.y);
      Rect rect5 = new Rect(((Rect) ref rect3).x, ((Rect) ref rect4).yMax + 10f, ((Rect) ref rect3).width / 3f, 30f);
      using (new TextBlock((GameFont) 2))
        Widgets.Label(rect5, "Axis Limits");
      float num1 = this.XRange.min;
      float num2 = this.XRange.max;
      float num3 = this.YRange.min;
      float num4 = this.YRange.max;
      float num5 = ((Rect) ref rect3).width / 6f;
      Rect rect6 = new Rect(((Rect) ref rect3).x, ((Rect) ref rect5).yMax + 3f, num5, 30f);
      UIElements.NumericBox<float>(rect6, ref num1, "xMin", string.Empty, string.Empty, float.MinValue, float.MaxValue, 0.4f);
      ref Rect local1 = ref rect6;
      ((Rect) ref local1).x = ((Rect) ref local1).x + (((Rect) ref rect6).width + 5f);
      UIElements.NumericBox<float>(rect6, ref num2, "xMax", string.Empty, string.Empty, float.MinValue, float.MaxValue, 0.4f);
      Rect rect7 = new Rect(((Rect) ref rect3).x, ((Rect) ref rect6).yMax + 3f, num5, 30f);
      UIElements.NumericBox<float>(rect7, ref num3, "yMin", string.Empty, string.Empty, float.MinValue, float.MaxValue, 0.4f);
      ref Rect local2 = ref rect7;
      ((Rect) ref local2).x = ((Rect) ref local2).x + (((Rect) ref rect7).width + 5f);
      UIElements.NumericBox<float>(rect7, ref num4, "yMax", string.Empty, string.Empty, float.MinValue, float.MaxValue, 0.4f);
      string label = ((Entity) this.animationTarget?.Thing).Label ?? "No Animator";
      if (UIElements.ClickableLabel(new Rect(((Rect) ref rect5).xMax, ((Rect) ref rect5).y, ((Rect) ref rect3).width - ((Rect) ref rect5).width, ((Rect) ref rect5).height), label, GenUI.MouseoverColor, Color.white, (GameFont) 2, (TextAnchor) 5))
      {
        if (!this.potentialAnimationTargets.NullOrEmpty<IAnimationTarget>())
        {
          List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
          floatMenuOptionList.Add(new FloatMenuOption("None", (Action) (() => this.TryStartCamera((IAnimationTarget) null)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          foreach (IAnimationTarget potentialAnimationTarget in this.potentialAnimationTargets)
          {
            IAnimationTarget animationTarget = potentialAnimationTarget;
            floatMenuOptionList.Add(new FloatMenuOption(((Entity) animationTarget.Thing).Label, (Action) (() => this.TryStartCamera(animationTarget)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          }
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        }
        else
          Messages.Message("Map must be loaded with at least 1 IAnimationTarget spawned.", MessageTypeDefOf.RejectInput, true);
      }
      Rect rect8 = new Rect(((Rect) ref rect3).xMax - 250f, ((Rect) ref rect6).y, 250f, 30f);
      if (this.animationTarget != null && !this.animators.NullOrEmpty<AnimatorObject>())
      {
        if (Widgets.ButtonText(rect8, AnimationSimulator.CurrentDriver?.Name ?? "Select Animation Driver", true, true, true, new TextAnchor?()))
        {
          List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
          floatMenuOptionList.Add(new FloatMenuOption("None", (Action) (() => this.SelectAnimationDriver((AnimationDriver) null)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          foreach (AnimationDriver animation in this.animationTarget.Animations)
          {
            AnimationDriver animationDriver = animation;
            floatMenuOptionList.Add(new FloatMenuOption(animationDriver.Name ?? "", (Action) (() => this.SelectAnimationDriver(animationDriver)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          }
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        }
        ((Rect) ref rect8).y = ((Rect) ref rect7).y;
        if (Widgets.ButtonText(rect8, this.curAnimator?.DisplayName ?? "None", true, true, true, new TextAnchor?()))
        {
          List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
          floatMenuOptionList.Add(new FloatMenuOption("None", (Action) (() => this.SelectAnimator((AnimatorObject) null)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
          if (AnimationSimulator.CurrentDriver != null)
          {
            foreach (AnimatorObject animatorObject1 in this.animators.Where<AnimatorObject>((Func<AnimatorObject, bool>) (anim => !anim.category.NullOrEmpty<char>() && anim.category.Split('.', StringSplitOptions.None)[0] == AnimationSimulator.CurrentDriver.Name)))
            {
              AnimatorObject animatorObject = animatorObject1;
              floatMenuOptionList.Add(new FloatMenuOption(animatorObject.DisplayName ?? "", (Action) (() => this.SelectAnimator(animatorObject)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
            }
          }
          Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        }
      }
      if ((double) num1 - (double) num2 > -0.0099999997764825821)
        num1 = num2 - 0.01f;
      if ((double) num2 - (double) num1 < 0.0099999997764825821)
        num2 = num1 + 0.01f;
      if ((double) num3 - (double) num4 > -0.0099999997764825821)
        num3 = num4 - 0.01f;
      if ((double) num4 - (double) num3 < 0.0099999997764825821)
        num4 = num3 + 0.01f;
      this.ValidateLimits();
      this.XRange = new FloatRange(num1, num2);
      this.YRange = new FloatRange(num3, num4);
      float num6 = Verse.Text.CalcHeight("Coordinates", ((Rect) ref rect3).width);
      Rect rect9 = new Rect(((Rect) ref rect3).x, ((Rect) ref rect7).yMax + 10f, ((Rect) ref rect3).width, num6);
      Widgets.Label(rect9, "Coordinates");
      UIElements.CheckboxLabeled(new Rect((float) ((double) ((Rect) ref rect3).x + (double) ((Rect) ref rect3).width - 250.0), ((Rect) ref rect9).y, 250f, 30f), "Draw Coordinate Labels", ref this.drawCoordLabels);
      Rect rect10 = new Rect(((Rect) ref rect3).x, ((Rect) ref rect9).yMax + 5f, ((Rect) ref rect3).width, ((Rect) ref rect3).height / 2f);
      float num7 = (float) ((double) (this.curve.PointsCount + 1) * 24.0 * 2.0 - 10.0);
      Rect rect11 = new Rect(((Rect) ref rect10).x, ((Rect) ref rect10).y, ((Rect) ref rect10).width - 16f, num7);
      Widgets.DrawMenuSection(rect10);
      Widgets.BeginScrollView(rect10, ref this.scrollPos, rect11, true);
      Rect rect12 = GenUI.ContractedBy(new Rect(((Rect) ref rect11).x, ((Rect) ref rect11).y, ((Rect) ref rect11).width, 48f), 5f);
      if (!this.curve.points.NullOrEmpty<CurvePoint>())
      {
        int index1 = -1;
        for (int index2 = 0; index2 < this.curve.PointsCount; ++index2)
        {
          UIHighlighter.HighlightOpportunity(rect12, $"GraphEditor_CurvePoint_{index2}");
          Rect rect13 = new Rect(rect12);
          ((Rect) ref rect13).width = 24f;
          ((Rect) ref rect13).height = ((Rect) ref rect12).height / 2f;
          Rect rect14 = rect13;
          if (index2 == 0)
            GUIState.Disable();
          if (Widgets.ButtonImage(rect14, TexButton.ReorderUp, GUI.enabled ? Color.white : UIElements.InactiveColor, GUI.enabled ? GenUI.MouseoverColor : UIElements.InactiveColor, GUI.enabled, (string) null) && GUI.enabled)
            GenList.Swap<CurvePoint>((IList<CurvePoint>) this.curve.points, index2, index2 - 1);
          GUIState.Enable();
          ref Rect local3 = ref rect14;
          ((Rect) ref local3).y = ((Rect) ref local3).y + ((Rect) ref rect14).height;
          if (index2 == this.curve.PointsCount - 1)
            GUIState.Disable();
          if (Widgets.ButtonImage(rect14, TexButton.ReorderDown, GUI.enabled ? Color.white : UIElements.InactiveColor, GUI.enabled ? GenUI.MouseoverColor : UIElements.InactiveColor, GUI.enabled, (string) null) && GUI.enabled)
            GenList.Swap<CurvePoint>((IList<CurvePoint>) this.curve.points, index2, index2 + 1);
          GUIState.Enable();
          CurvePoint point = this.curve.points[index2];
          rect13 = new Rect(rect12);
          ((Rect) ref rect13).x = (float) ((double) ((Rect) ref rect12).x + (double) ((Rect) ref rect14).width + 10.0);
          ((Rect) ref rect13).width = (float) (((double) ((Rect) ref rect12).width - 24.0) / 7.0);
          Rect rect15 = rect13;
          float num8 = UIElements.NumericBox<float>(rect15, ((CurvePoint) ref point).x, "X", string.Empty, string.Empty, labelProportion: 0.25f);
          ref Rect local4 = ref rect15;
          ((Rect) ref local4).x = ((Rect) ref local4).x + (((Rect) ref rect15).width + 5f);
          float num9 = UIElements.NumericBox<float>(rect15, ((CurvePoint) ref point).y, "Y", string.Empty, string.Empty, labelProportion: 0.25f);
          this.curve.points[index2] = new CurvePoint(num8.RoundTo(0.01f), num9.RoundTo(0.01f));
          Rect rect16;
          // ISSUE: explicit constructor call
          ((Rect) ref rect16).\u002Ector(((Rect) ref rect12).width - 24f, ((Rect) ref rect12).y + (float) (((double) ((Rect) ref rect12).height - 24.0) / 2.0), 24f, 24f);
          if (Widgets.ButtonImage(rect16, TexButton.Minus, true, (string) null))
            index1 = index2;
          ref Rect local5 = ref rect12;
          ((Rect) ref local5).y = ((Rect) ref local5).y + ((Rect) ref rect12).height;
        }
        if (index1 >= 0)
          this.curve.points.RemoveAt(index1);
      }
      Rect rect17;
      // ISSUE: explicit constructor call
      ((Rect) ref rect17).\u002Ector(((Rect) ref rect12).x, ((Rect) ref rect12).y + (float) (((double) ((Rect) ref rect12).height - 24.0) / 2.0), 24f, 24f);
      if (Widgets.ButtonImage(rect17, TexButton.Plus, true, (string) null))
      {
        CurvePoint curvePoint;
        if (!this.curve.points.NullOrEmpty<CurvePoint>())
        {
          FloatRange floatRange = this.XRange;
          double average1 = (double) ((FloatRange) ref floatRange).Average;
          floatRange = this.YRange;
          double average2 = (double) ((FloatRange) ref floatRange).Average;
          curvePoint = new CurvePoint((float) average1, (float) average2);
        }
        else
          curvePoint = new CurvePoint(0.0f, 0.0f);
        this.curve.Add(curvePoint);
      }
      Widgets.EndScrollView();
      float num10 = ((Rect) ref rect3).height - ((Rect) ref rect10).yMax;
      this.DrawPreviewWindow(new Rect(((Rect) ref rect10).x, ((Rect) ref rect10).yMax + 5f, ((Rect) ref rect10).width, num10 - 5f));
      this.DrawGraph(new Rect(((Rect) ref inRect).width - ((Rect) ref inRect).height, 0.0f, ((Rect) ref inRect).height, ((Rect) ref inRect).height));
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void TopLevelButtons(Rect rect, params (string label, Action onClick)[] buttons)
  {
    float num = ((Rect) ref rect).width / (float) buttons.Length;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, num, 30f);
    foreach ((string label, Action onClick) in buttons)
    {
      if (Widgets.ButtonText(rect1, label, true, true, true, new TextAnchor?()))
        onClick();
      ref Rect local = ref rect1;
      ((Rect) ref local).x = ((Rect) ref local).x + num;
    }
  }

  private void DrawPreviewWindow(Rect rect)
  {
    Widgets.DrawMenuSection(rect);
    rect = GenUI.ContractedBy(rect, 1f);
    string header = "Preview Window Disabled";
    bool flag1 = this.animationTarget == null;
    bool flag2 = false;
    if (!flag1)
    {
      try
      {
        float height = ((Rect) ref rect).height;
        Rect rect1;
        // ISSUE: explicit constructor call
        ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + ((Rect) ref rect).width - height, ((Rect) ref rect).y, height, height);
        flag1 = !CameraView.RenderAt(rect1);
        if (!flag1)
        {
          UIElements.DrawLineVertical(((Rect) ref rect1).x - 1f, ((Rect) ref rect1).y, height, UIElements.MenuSectionBgBorderColor);
          Rect rect2 = GenUI.ContractedBy(new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, (float) ((double) ((Rect) ref rect).width - (double) height - 1.0), height), 5f);
          this.lister.columnGap = 0.0f;
          this.lister.Begin(rect2, 1);
          this.lister.Header("Settings", (GameFont) 2, (TextAnchor) 4);
          CameraView.OrthographicSize = this.lister.SliderLabeled("Orthographic Size", CameraView.OrthographicSize, "Camera Zoom", string.Empty, string.Empty, Dialog_GraphEditor.ZoomRange.min, Dialog_GraphEditor.ZoomRange.max, 1);
          if (this.lister.Button($"Playback Speed: {AnimationSimulator.PlaybackSpeed}", highlightTag: "Speed multiplier on animation window"))
          {
            List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
            foreach (float playbackSpeed in AnimationSimulator.playbackSpeeds)
            {
              float speed = playbackSpeed;
              floatMenuOptionList.Add(new FloatMenuOption($"{speed}", (Action) (() => AnimationSimulator.PlaybackSpeed = speed), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
            }
            Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
          }
          this.lister.Gap(4f);
          this.lister.CheckboxLabeled("Loop", ref CameraView.animationSettings.loop, "Loop the viewer when reaching the end of the animation.", string.Empty, false);
          this.lister.CheckboxLabeled("Display Ticks", ref CameraView.animationSettings.displayTicks, "Display the time remaining in ticks, rather than seconds.", string.Empty, false);
          this.lister.CheckboxLabeled("Draw Cell Grid", ref CameraView.animationSettings.drawCellGrid, "Draw lines along edges of the map's cells.", string.Empty, false);
          ((Listing) this.lister).End();
          if (Mouse.IsOver(rect1))
          {
            Widgets.DrawTextureFitted(rect1, (Texture) UIData.TransparentBlackBG, 1f, 1f);
            float num1 = 12.5f;
            Rect rect3 = GenUI.ContractedBy(new Rect(((Rect) ref rect1).x + num1, (float) ((double) ((Rect) ref rect1).yMax - 25.0 - 5.0), 25f, 25f), 2f);
            Texture2D texture2D = AnimationSimulator.Paused ? Dialog_GraphEditor.viewerButtonTextures[1] : Dialog_GraphEditor.viewerButtonTextures[0];
            if (Widgets.ButtonImage(rect3, texture2D, true, (string) null))
            {
              if (AnimationSimulator.CurrentDriver != null && AnimationSimulator.TicksPassed >= AnimationSimulator.CurrentDriver.AnimationLength)
                AnimationSimulator.Reset();
              AnimationSimulator.TogglePause(true);
              SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            }
            Rect rect4;
            // ISSUE: explicit constructor call
            ((Rect) ref rect4).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width, ((Rect) ref rect1).height - ((Rect) ref rect3).height * 2.5f);
            float num2 = ((Rect) ref rect1).width / 5f;
            Rect playButtonAnimated = new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y, num2, num2);
            if (AnimationSimulator.ButtonUpdated(rect4, (Action) (() => Dialog_GraphEditor.timeFading += Time.deltaTime), (Action) (() => Dialog_GraphEditor.ButtonFadeHandler(playButtonAnimated)), (Func<bool>) (() => (double) Dialog_GraphEditor.timeFading <= 0.0), false))
            {
              Dialog_GraphEditor.timeFading = 0.0f;
              AnimationSimulator.TogglePause(true);
              SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
            }
            string str = CameraView.animationSettings.displayTicks ? "0 / 0" : "0:00/0:00";
            if (AnimationSimulator.CurrentDriver != null)
              str = !CameraView.animationSettings.displayTicks ? string.Format("{0:m\\:ss} / {1:m\\:ss}", (object) new TimeSpan(0, 0, Mathf.CeilToInt(GenTicks.TicksToSeconds(AnimationSimulator.TicksPassed))), (object) new TimeSpan(0, 0, Mathf.CeilToInt(GenTicks.TicksToSeconds(AnimationSimulator.CurrentDriver.AnimationLength)))) : $"{AnimationSimulator.TicksPassed} / {AnimationSimulator.CurrentDriver.AnimationLength}";
            using (new TextBlock((GameFont) 1, (TextAnchor) 5))
            {
              Rect rect5;
              // ISSUE: explicit constructor call
              ((Rect) ref rect5).\u002Ector((float) ((double) ((Rect) ref rect1).xMax - (double) ((Rect) ref rect1).width / 2.0 - 10.0), ((Rect) ref rect3).y, ((Rect) ref rect1).width / 2f, ((Rect) ref rect3).height);
              Widgets.Label(rect5, str);
            }
            Rect rect6;
            // ISSUE: explicit constructor call
            ((Rect) ref rect6).\u002Ector(((Rect) ref rect1).x + 8f, (float) ((double) ((Rect) ref rect3).y - 3.0 - 8.3333330154418945), ((Rect) ref rect1).width - 16f, 3f);
            float num3 = 12f;
            if (Mouse.IsOver(GenUI.ExpandedBy(rect6, 2f)))
            {
              ref Rect local1 = ref rect6;
              ((Rect) ref local1).height = ((Rect) ref local1).height * 1.5f;
              num3 *= 1.5f;
              ref Rect local2 = ref rect6;
              ((Rect) ref local2).y = ((Rect) ref local2).y - (float) (((double) ((Rect) ref rect6).height - 3.0) / 2.0);
            }
            float fillPercent = 0.0f;
            if (AnimationSimulator.CurrentDriver != null)
              fillPercent = (float) AnimationSimulator.TicksPassed / (float) AnimationSimulator.CurrentDriver.AnimationLength;
            UIElements.FillableBar(rect6, fillPercent, UIData.FillableBarProgressBar, UIData.FillableBarProgressBarBG);
            Rect rect7;
            // ISSUE: explicit constructor call
            ((Rect) ref rect7).\u002Ector((float) ((double) ((Rect) ref rect6).x + (double) ((Rect) ref rect6).width * (double) fillPercent - (double) num3 / 2.0), (float) ((double) ((Rect) ref rect6).y + (double) ((Rect) ref rect6).height / 2.0 - (double) num3 / 2.0), num3, num3);
            using (new TextBlock(UIData.ProgressBarRed))
              Widgets.DrawTextureFitted(rect7, (Texture) Dialog_GraphEditor.dragHandleIcon, 1f, 1f);
            Widgets.DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(rect6, false);
            if (draggableResult == 2 && AnimationSimulator.CurrentDriver != null)
            {
              Dialog_GraphEditor.dragging = true;
              AnimationSimulator.EditingTicks = true;
            }
            if (!Input.GetMouseButton(0))
            {
              Dialog_GraphEditor.dragging = false;
              AnimationSimulator.EditingTicks = false;
            }
            if (!Dialog_GraphEditor.dragging)
            {
              if (draggableResult != 1)
                goto label_36;
            }
            if (AnimationSimulator.CurrentDriver != null)
              AnimationSimulator.TicksPassed = Mathf.RoundToInt((float) AnimationSimulator.CurrentDriver.AnimationLength * Mathf.Clamp01((Event.current.mousePosition.x - ((Rect) ref rect6).x) / ((Rect) ref rect6).width));
          }
        }
      }
      catch (Exception ex)
      {
        flag1 = true;
        flag2 = true;
        header = ex.ToString();
        Log.ErrorOnce($"Exception thrown in CameraView. Exception = {ex}", "CameraView_GraphEditor".GetHashCode());
      }
    }
label_36:
    if (!flag1)
      return;
    UIElements.Header(rect, header, flag2 ? new Color(0.0f, 0.0f, 0.0f, 0.75f) : ListingExtension.BannerColor, (GameFont) 2, (TextAnchor) 4);
  }

  private void SelectCurveType()
  {
    Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
    {
      new FloatMenuOption(Graph.GraphType.Linear.ToString(), (Action) (() => this.GraphType = Graph.GraphType.Linear), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
      new FloatMenuOption(Graph.GraphType.Bezier.ToString(), (Action) (() => this.GraphType = Graph.GraphType.Bezier), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
      new FloatMenuOption(Graph.GraphType.Lagrange.ToString(), (Action) (() => this.GraphType = Graph.GraphType.Lagrange), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
      new FloatMenuOption(Graph.GraphType.Staircase.ToString(), (Action) (() => this.GraphType = Graph.GraphType.Staircase), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
    }));
  }

  private void SaveEdits(bool reportFail = true)
  {
    if (this.curAnimator != null)
    {
      this.curAnimator.SetCurve(this.curve);
      Messages.Message("Animation data saved to " + this.curAnimator.DisplayName, MessageTypeDefOf.NeutralEvent, true);
    }
    else
    {
      if (!reportFail)
        return;
      Messages.Message("Unable to save animation data. No animator loaded.", MessageTypeDefOf.RejectInput, true);
    }
  }

  private void ExportAnimationXml()
  {
    if (this.animationTarget == null)
      Messages.Message("Unable to export animation data. No animation target loaded.", MessageTypeDefOf.RejectInput, true);
    else if (this.animators.NullOrEmpty<AnimatorObject>())
    {
      Messages.Message("Unable to export animation data. No animations to export.", MessageTypeDefOf.RejectInput, true);
    }
    else
    {
      bool flag = true;
      try
      {
        this.SaveEdits(false);
        XmlExporter.StartDocument(Dialog_GraphEditor.filePath);
        XmlExporter.OpenNode("Animations");
        IOrderedEnumerable<AnimatorObject> source = this.animators.OrderBy<AnimatorObject, string>((Func<AnimatorObject, string>) (anim => anim.category));
        string category = source.FirstOrDefault<AnimatorObject>().category;
        string prefix = source.FirstOrDefault<AnimatorObject>().prefix;
        XmlExporter.OpenNode(category);
        foreach (AnimatorObject animatorObject in (IEnumerable<AnimatorObject>) source)
        {
          if (category != animatorObject.category)
          {
            XmlExporter.CloseNode();
            category = animatorObject.category;
            XmlExporter.OpenNode(category);
          }
          if (!animatorObject.prefix.NullOrEmpty<char>() && prefix != animatorObject.prefix)
          {
            XmlExporter.CloseNode();
            prefix = animatorObject.prefix;
            XmlExporter.OpenNode(prefix);
          }
          Type type = animatorObject.Curve.GetType();
          (string, string) valueTuple = type.IsSubclassOf(typeof (LinearCurve)) ? ("Class", $"{type.Namespace}.{type.Name}") : (string.Empty, string.Empty);
          XmlExporter.OpenNode(animatorObject.fieldInfo.Name, valueTuple);
          if (animatorObject.Curve.PointsCount > 0)
          {
            XmlExporter.OpenNode("points");
            foreach (CurvePoint curvePoint in animatorObject.Curve)
            {
              XmlExporter.OpenNode("li");
              XmlExporter.WriteString($"({((CurvePoint) ref curvePoint).x:0.##}, {((CurvePoint) ref curvePoint).y:0.##})");
              XmlExporter.CloseNode();
            }
            XmlExporter.CloseNode();
          }
          XmlExporter.CloseNode();
        }
        XmlExporter.CloseNode();
        XmlExporter.CloseNode();
        XmlExporter.Export();
      }
      catch (Exception ex)
      {
        flag = false;
        Log.Error($"Unable to export animation data. Exception = {ex}");
      }
      finally
      {
        XmlExporter.Close();
      }
      if (flag)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
  }

  private static void ButtonFadeHandler(Rect rect)
  {
    float width = ((Rect) ref rect).width;
    float num1 = Dialog_GraphEditor.timeFading / 1.5f;
    float num2 = Mathf.Lerp(width, width * 2f, num1);
    ((Rect) ref rect).size = new Vector2(num2, num2);
    float num3 = Mathf.Lerp(0.5f, 0.0f, num1);
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector(new Color(0.0f, 0.0f, 0.0f, num3));
    try
    {
      Widgets.DrawTextureFitted(rect, (Texture) Dialog_GraphEditor.dragHandleIcon, 1f, 1f);
    }
    finally
    {
      textBlock1.Dispose();
    }
    TextBlock textBlock2;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock2).\u002Ector(new Color(1f, 1f, 1f, num3));
    try
    {
      Widgets.DrawTextureFitted(rect, (Texture) Dialog_GraphEditor.viewerButtonTextures[0], 1f, 1f);
    }
    finally
    {
      textBlock2.Dispose();
    }
  }

  public class AnimationSettings
  {
    public float orthographicSize = 4f;
    public bool pauseOnTransition;
    public bool loop;
    public bool displayTicks = true;
    public bool drawCellGrid;
  }
}
