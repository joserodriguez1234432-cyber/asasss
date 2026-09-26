// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleOrientationController
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class VehicleOrientationController : BaseTargeter
{
  private const int RecomputeDestinationsFrequency = 15;
  private const float CameraViewerZoomRate = 0.55f;
  private const float DragThreshold = 1f;
  private const float HoldTimeThreshold = 0.5f;
  private static readonly Color FeedbackColor = GenColor.FromBytes(153, 207, 135, (int) byte.MaxValue);
  private static readonly Color GotoBetweenLineColor = Color.op_Multiply(VehicleOrientationController.FeedbackColor, new Color(1f, 1f, 1f, 0.18f));
  private static readonly Color GotoCircleColor = Color.op_Multiply(VehicleOrientationController.FeedbackColor, new Color(1f, 1f, 1f, 0.2f));
  private static readonly Material GotoBetweenLineMaterial = MaterialPool.MatFrom("UI/Overlays/ThickLine", ShaderDatabase.Transparent, VehicleOrientationController.GotoBetweenLineColor);
  private static readonly Material GotoCircleMaterial = MaterialPool.MatFrom("UI/Overlays/Circle75Solid", ShaderDatabase.Transparent, VehicleOrientationController.GotoCircleColor);
  private Vector3 clickPos;
  private IntVec3 start;
  private IntVec3 end;
  private float timeHeldDown;
  private int lastUpdatedTick;
  private float[,] costMatrix;
  private KuhnMunkres assigner;
  private readonly List<IntVec3> potentialDests = new List<IntVec3>();
  private readonly List<IntVec3> dests = new List<IntVec3>();
  private readonly List<VehiclePawn> vehicles = new List<VehiclePawn>();

  private Rot8 Rotation { get; set; }

  private bool IsDragging { get; set; }

  private bool IsMultiSelect => this.vehicles.Count > 1;

  public override bool IsTargeting
  {
    get
    {
      VehiclePawn vehicle = this.vehicle;
      return vehicle != null && ((Thing) vehicle).Spawned;
    }
  }

  private Rot8 MouseTargetedRotation => Rot8.FromAngle(this.end.AngleToCell(UI.MouseCell()));

  private static VehicleOrientationController Instance { get; set; }

  public static void StartOrienting(VehiclePawn dragging, IntVec3 cell, IntVec3 clickCell)
  {
    VehicleOrientationController.Instance.StopTargeting();
    VehicleOrientationController instance = VehicleOrientationController.Instance;
    List<VehiclePawn> vehicles = new List<VehiclePawn>(1);
    vehicles.Add(dragging);
    IntVec3 cell1 = cell;
    IntVec3 clickCell1 = clickCell;
    instance.Init(vehicles, cell1, clickCell1);
  }

  public static void StartOrienting(List<VehiclePawn> dragging, IntVec3 cell, IntVec3 clickCell)
  {
    VehicleOrientationController.Instance.StopTargeting();
    VehicleOrientationController.Instance.Init(dragging, cell, clickCell);
  }

  private void Init(List<VehiclePawn> vehicles, IntVec3 cell, IntVec3 clickCell)
  {
    this.vehicle = vehicles.FirstOrDefault<VehiclePawn>();
    this.vehicles.AddRange((IEnumerable<VehiclePawn>) vehicles);
    this.dests.Populate<IntVec3>(IntVec3.Invalid, vehicles.Count);
    this.potentialDests.Populate<IntVec3>(IntVec3.Invalid, vehicles.Count);
    this.dests[0] = cell;
    this.Rotation = this.vehicle.FullRotation;
    this.start = clickCell;
    this.end = cell;
    this.clickPos = UI.MouseMapPosition();
    if (this.IsMultiSelect)
    {
      int count = vehicles.Count;
      this.costMatrix = new float[count, count];
      this.assigner = new KuhnMunkres(count);
    }
    this.OnStart();
  }

  protected override void OnStart()
  {
    base.OnStart();
    if (!this.IsMultiSelect)
      return;
    this.IsDragging = true;
  }

  private void ConfirmOrientation()
  {
    if (this.IsMultiSelect)
      this.RecomputeDestinations();
    for (int index = 0; index < this.vehicles.Count; ++index)
    {
      VehiclePawn vehicle = this.vehicles[index];
      IntVec3 dest = this.dests[index];
      if (((Thing) vehicle).Spawned && ((IntVec3) ref dest).IsValid)
        FloatMenuOptionProvider_OrderVehicle.PawnGotoAction(this.end, vehicle, dest, this.Rotation);
    }
    if (IntVec3.op_Equality(this.start, this.end))
      LessonAutoActivator.TeachOpportunity(ConceptDefOf.GroupGotoHereDragging, (OpportunityType) 0);
    else if ((double) IntVec3Utility.DistanceToSquared(this.start, this.end) > 1.8999999761581421)
      PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GroupGotoHereDragging, (KnowledgeAmount) 5);
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.ColonistOrdered, (Map) null);
    this.StopTargeting();
  }

  private void RecomputeDestinations()
  {
    this.dests.Populate<IntVec3>(IntVec3.Invalid, this.dests.Count);
    int count = this.vehicles.Count;
    float num1 = count > 1 ? (float) (count - 1) : 1f;
    for (int index = 0; index < count; ++index)
    {
      VehiclePawn vehicle = this.vehicles[index];
      if (((Thing) vehicle).Spawned)
      {
        IntVec3 cell;
        if (((Thing) vehicle).Map.exitMapGrid.IsExitCell(this.end))
        {
          cell = this.end;
        }
        else
        {
          float num2 = (float) index / num1;
          cell = IntVec3Utility.ToIntVec3(Vector3.op_Addition(((IntVec3) ref this.start).ToVector3(), Vector3.op_Multiply(Vector3.op_Subtraction(((IntVec3) ref this.end).ToVector3(), ((IntVec3) ref this.start).ToVector3()), num2)));
        }
        IntVec3 result;
        if (!PathingHelper.TryFindNearestStandableCell(vehicle, cell, out result))
          result = IntVec3.Invalid;
        this.potentialDests[index] = result;
      }
    }
    for (int index1 = 0; index1 < count; ++index1)
    {
      for (int index2 = 0; index2 < count; ++index2)
        this.costMatrix[index1, index2] = IntVec3Utility.DistanceTo(((Thing) this.vehicles[index1]).Position, this.dests[index2]);
    }
    int[] numArray = this.assigner.Compute(this.costMatrix);
    for (int index = 0; index < count; ++index)
    {
      if (((Thing) this.vehicles[index]).Spawned)
        this.dests[index] = this.potentialDests[numArray[index]];
    }
  }

  public override void ProcessInputEvents()
  {
    if (KeyBindingDefOf.Cancel.KeyDownEvent)
    {
      this.StopTargeting();
    }
    else
    {
      if (!Input.GetMouseButton(1))
        this.ConfirmOrientation();
      else if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
        this.Rotation = this.Rotation.Rotated((RotationDirection) 3);
      }
      else if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
        this.Rotation = this.Rotation.Rotated((RotationDirection) 1);
      }
      if (!this.IsMultiSelect)
        return;
      IntVec3 intVec3 = UI.MouseCell();
      int ticksGame = Find.TickManager.TicksGame;
      if (!IntVec3.op_Inequality(intVec3, this.end) && ticksGame <= this.lastUpdatedTick + 15)
        return;
      if (IntVec3.op_Inequality(intVec3, this.end))
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragGoto, (Map) null);
      this.end = intVec3;
      this.lastUpdatedTick = ticksGame;
      this.RecomputeDestinations();
    }
  }

  public override void TargeterUpdate()
  {
    if (!this.IsDragging)
    {
      this.Rotation = Rot8.Invalid;
      this.timeHeldDown += Time.deltaTime;
      float num = Vector3.Distance(this.clickPos, UI.MouseMapPosition());
      if ((double) this.timeHeldDown >= 0.5 || (double) num >= 1.0 || (double) num <= -1.0)
        this.IsDragging = true;
    }
    if (!this.IsDragging)
      return;
    if (!this.IsMultiSelect)
    {
      Rot8 targetedRotation = this.MouseTargetedRotation;
      if (this.Rotation != targetedRotation)
      {
        this.Rotation = targetedRotation;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragGoto, (Map) null);
      }
    }
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(1.7f, 1f, 1.7f);
    float num1 = Altitudes.AltitudeFor((AltitudeLayer) 39);
    float num2 = num1 + 0.03658537f;
    float num3 = num1 - 0.03658537f;
    if (this.IsMultiSelect)
    {
      GenDraw.DrawLineBetween(((IntVec3) ref this.start).ToVector3ShiftedWithAltitude(num3), ((IntVec3) ref this.end).ToVector3ShiftedWithAltitude(num3), VehicleOrientationController.GotoBetweenLineMaterial, 0.9f);
      for (int index = 0; index < this.vehicles.Count; ++index)
      {
        VehiclePawn vehicle = this.vehicles[index];
        IntVec3 dest = this.dests[index];
        if (((Thing) vehicle).Spawned && ((IntVec3) ref dest).IsValid && !GridsUtility.Fogged(dest, ((Thing) vehicle).Map))
        {
          IntVec2 size = ((BuildableDef) vehicle.VehicleDef).Size;
          Vector3 shiftedWithAltitude = ((IntVec3) ref dest).ToVector3ShiftedWithAltitude(num2);
          if (IntVec2.op_Equality(size, IntVec2.One))
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(shiftedWithAltitude, Quaternion.identity, vector3), VehicleOrientationController.GotoCircleMaterial, 0);
          vehicle.DrawAt(((IntVec3) ref dest).ToVector3ShiftedWithAltitude(num1), this.Rotation, 0.0f);
        }
      }
    }
    else
      VehicleGhostUtility.DrawGhostVehicleDef(this.dests[0], this.Rotation, this.vehicle.VehicleDef, VehicleGhostUtility.whiteGhostColor, (AltitudeLayer) 39, this.vehicle);
  }

  public override void StopTargeting()
  {
    this.IsDragging = false;
    this.vehicle = (VehiclePawn) null;
    this.vehicles.Clear();
    this.dests.Clear();
    this.start = IntVec3.Invalid;
    this.end = IntVec3.Invalid;
    this.clickPos = Vector3.zero;
    this.timeHeldDown = 0.0f;
    this.lastUpdatedTick = 0;
  }

  public override void TargeterOnGUI()
  {
    if (!this.IsMultiSelect)
      return;
    VehicleOrientationController.GUIShowRotationControls(0.0f, ((MainTabWindow_Inspect) MainButtonDefOf.Inspect.TabWindow).PaneTopY);
  }

  private static void GUIShowRotationControls(float leftX, float bottomY)
  {
    Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
    Find.WindowStack.ImmediateWindow(nameof (VehicleOrientationController).GetHashCode(), winRect, (WindowLayer) 0, (Action) (() =>
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector((GameFont) 2, (TextAnchor) 4);
      try
      {
        Rect rect1 = new Rect((float) ((double) ((Rect) ref winRect).width / 2.0 - 64.0 - 5.0), 15f, 64f, 64f);
        GUI.DrawTexture(rect1, (Texture) TexUI.RotLeftTex);
        if (!SteamDeck.IsSteamDeck)
          Widgets.Label(rect1, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel);
        Rect rect2 = new Rect((float) ((double) ((Rect) ref winRect).width / 2.0 + 5.0), 15f, 64f, 64f);
        GUI.DrawTexture(rect2, (Texture) TexUI.RotRightTex);
        if (SteamDeck.IsSteamDeck)
          return;
        Widgets.Label(rect2, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel);
      }
      finally
      {
        textBlock.Dispose();
      }
    }), true, false, 1f, (Action) null, false);
  }

  public override void PostInit() => VehicleOrientationController.Instance = this;
}
