// Decompiled with JetBrains decompiler
// Type: Vehicles.LandingTargeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class LandingTargeter : BaseTargeter
{
  public const int PingPongTickLength = 100;
  public static readonly Color GhostOccupiedColor = new Color(1f, 0.5f, 0.2f, 0.5f);
  private static float middleMouseDownTime;
  private static float framesOpen;
  private Map map;
  private Action<LocalTargetInfo, Rot4> action;
  private Rot4 landingRotation;
  private LocalTargetInfo cachedTarget;
  private Func<LocalTargetInfo, bool> targetValidator;
  private bool allowRotating;
  private Queue<Action> targeterQueue = new Queue<Action>();
  private static (IntVec3 startingCell, Rot4 rotation, bool result) restrictionCached;

  public static LandingTargeter Instance { get; private set; }

  public bool ForcedTargeting { get; private set; }

  private bool Paused { get; set; }

  public override bool IsTargeting => this.action != null;

  public void BeginTargetingAndFocusMap(
    VehiclePawn vehicle,
    [CanBeNull] Map map,
    Action<LocalTargetInfo, Rot4> action,
    Func<LocalTargetInfo, bool> targetValidator = null,
    Action actionWhenFinished = null,
    Texture2D mouseAttachment = null,
    bool allowRotating = false,
    bool forcedTargeting = false)
  {
    Current.Game.CurrentMap = map;
    this.BeginTargeting(vehicle, map, action, targetValidator, (Action) (() => Current.Game.CurrentMap = map), actionWhenFinished, mouseAttachment, allowRotating, forcedTargeting);
  }

  public void BeginTargeting(
    VehiclePawn vehicle,
    [CanBeNull] Map map,
    Action<LocalTargetInfo, Rot4> action,
    Func<LocalTargetInfo, bool> targetValidator = null,
    Action actionOnStart = null,
    Action actionWhenFinished = null,
    Texture2D mouseAttachment = null,
    bool allowRotating = false,
    bool forcedTargeting = false)
  {
    this.vehicle = vehicle;
    this.map = map;
    this.targeterQueue.Enqueue((Action) (() =>
    {
      Action action1 = actionOnStart;
      if (action1 != null)
        action1();
      this.vehicle = vehicle;
      this.action = action;
      this.actionWhenFinished = actionWhenFinished;
      this.mouseAttachment = mouseAttachment;
      this.targetValidator = targetValidator;
      this.allowRotating = allowRotating;
      this.landingRotation = (Rot4?) vehicle.CompVehicleLauncher.launchProtocol.GetProperties(LaunchProtocol.LaunchType.Landing, this.landingRotation)?.forcedRotation ?? Rot4.North;
      this.ForcedTargeting = forcedTargeting;
      LandingTargeter.ResetRestrictionCache();
    }));
    this.TryStartNextTargeter();
    this.OnStart();
  }

  public override void StopTargeting()
  {
    if (this.actionWhenFinished != null)
    {
      this.actionWhenFinished();
      this.actionWhenFinished = (Action) null;
    }
    this.map = (Map) null;
    this.action = (Action<LocalTargetInfo, Rot4>) null;
    this.targetValidator = (Func<LocalTargetInfo, bool>) null;
    LandingTargeter.framesOpen = 0.0f;
    this.ForcedTargeting = false;
    this.TryStartNextTargeter();
  }

  private void TryStartNextTargeter()
  {
    if (this.IsTargeting || this.targeterQueue.Count <= 0)
      return;
    this.targeterQueue.Dequeue()();
  }

  private static void ResetRestrictionCache()
  {
    LandingTargeter.restrictionCached = (IntVec3.Invalid, Rot4.Invalid, true);
  }

  private void CheckStillValid()
  {
    if (this.map == null || Current.Game.CurrentMap == this.map)
      return;
    this.TryCancelTargeter();
  }

  private void TryCancelTargeter()
  {
    if (this.ForcedTargeting)
    {
      this.Paused = true;
      Event.current.Use();
      Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(Translator.Translate("VF_ConfirmationCancelLanding"), (Action) (() =>
      {
        // ISSUE: object of a compiler-generated type is created
        CaravanHelper.MakeVehicleCaravan((IEnumerable<Pawn>) new \u003C\u003Ez__ReadOnlySingleElementList<Pawn>((Pawn) this.vehicle), ((Thing) this.vehicle).Faction, this.map.Tile, true);
        this.StopTargeting();
        this.Paused = false;
      }), (Action) (() =>
      {
        Current.Game.CurrentMap = this.map;
        CameraJumper.TryHideWorld();
        this.Paused = false;
      }), false, (string) null, (WindowLayer) 1));
    }
    else
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
      this.StopTargeting();
      Event.current.Use();
    }
  }

  public LandingTargeter.PositionState GetPosState(
    LocalTargetInfo localTargetInfo,
    bool drawRestriction = false)
  {
    IntVec3 cell1 = ((LocalTargetInfo) ref localTargetInfo).Cell;
    IntVec3 intVec3 = IntVec3Utility.ToIntVec3(new Vector3((float) cell1.x, Altitudes.AltitudeFor((AltitudeLayer) 15), (float) cell1.z));
    ((IntVec3) ref intVec3).ToVector3Shifted();
    Map currentMap = Current.Game.CurrentMap;
    bool flag1 = (!((LocalTargetInfo) ref localTargetInfo).IsValid || this.targetValidator != null && !this.targetValidator(localTargetInfo)) | MapHelper.ImpassableOrVehicleBlocked(this.vehicle, currentMap, ((LocalTargetInfo) ref localTargetInfo).Cell, this.landingRotation);
    bool flag2 = MapHelper.NonStandableOrVehicleBlocked(this.vehicle, currentMap, ((LocalTargetInfo) ref localTargetInfo).Cell, this.landingRotation);
    bool flag3 = false;
    LaunchRestriction restriction = this.vehicle.CompVehicleLauncher.launchProtocol.GetProperties(LaunchProtocol.LaunchType.Landing, this.landingRotation)?.restriction;
    if (restriction != null)
    {
      if (IntVec3.op_Inequality(LandingTargeter.restrictionCached.startingCell, cell1) || Rot4.op_Inequality(LandingTargeter.restrictionCached.rotation, this.landingRotation))
      {
        bool flag4 = !restriction.CanStartProtocol(this.vehicle, Current.Game.CurrentMap, cell1, this.landingRotation);
        LandingTargeter.restrictionCached = (cell1, this.landingRotation, flag4);
      }
      if (drawRestriction)
        restriction.DrawRestrictionsTargeter(this.vehicle, Current.Game.CurrentMap, cell1, this.landingRotation);
      flag3 = LandingTargeter.restrictionCached.result;
    }
    if (flag1 | flag3)
      return LandingTargeter.PositionState.Invalid;
    if (flag2)
      return LandingTargeter.PositionState.Obstructed;
    if (this.vehicle.CompVehicleLauncher.Props.canRoofPunch)
    {
      CellRect cellRect = GenAdj.OccupiedRect(cell1, this.landingRotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
      foreach (IntVec3 cell2 in cellRect)
      {
        int stateInt;
        if (this.RoofPunchOverride(currentMap, cell2, out stateInt))
          return (LandingTargeter.PositionState) stateInt;
        RoofDef roof = GridsUtility.GetRoof(cell2, currentMap);
        if (roof != null)
        {
          RoofDefPositionStateDefModExtension modExtension = ((Def) roof).GetModExtension<RoofDefPositionStateDefModExtension>();
          if (modExtension != null)
            return modExtension.state;
          return roof.isThickRoof ? LandingTargeter.PositionState.Invalid : LandingTargeter.PositionState.Obstructed;
        }
      }
    }
    return LandingTargeter.PositionState.Valid;
  }

  private bool RoofPunchOverride(Map map, IntVec3 cell, out int stateInt)
  {
    stateInt = 0;
    return false;
  }

  public override void ProcessInputEvents()
  {
    this.HandleRotationShortcuts();
    if (Event.current.type == null && Event.current.button == 0)
    {
      LocalTargetInfo localTargetInfo = this.CurrentTargetUnderMouse();
      if (this.action != null && GenGrid.InBounds(((LocalTargetInfo) ref localTargetInfo).Cell, Current.Game.CurrentMap))
      {
        if (this.GetPosState(localTargetInfo) != LandingTargeter.PositionState.Invalid)
        {
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
          this.action(localTargetInfo, this.landingRotation);
          this.StopTargeting();
        }
        else
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
      }
      Event.current.Use();
    }
    if ((Event.current.type != null || Event.current.button != 1) && !KeyBindingDefOf.Cancel.KeyDownEvent)
      return;
    this.TryCancelTargeter();
  }

  public override void TargeterOnGUI()
  {
    if (this.Paused)
      return;
    this.DoExtraGuiControls();
    GenUI.DrawMouseAttachment(this.mouseAttachment ?? CompLaunchable.TargeterMouseAttachment);
  }

  public override void TargeterUpdate()
  {
    if (this.Paused)
      return;
    ++LandingTargeter.framesOpen;
    LocalTargetInfo localTargetInfo = this.CurrentTargetUnderMouse();
    if (((LocalTargetInfo) ref localTargetInfo).IsValid)
    {
      Color color = LandingTargeter.GhostDrawerColor(this.GetPosState(localTargetInfo, true));
      color.a = (float) ((double) Mathf.PingPong(LandingTargeter.framesOpen, 66.6666641f) / 100.0 + 0.25);
      GhostDrawer.DrawGhostThing(((LocalTargetInfo) ref localTargetInfo).Cell, this.landingRotation, (ThingDef) this.vehicle.VehicleDef.buildDef, ((BuildableDef) this.vehicle.VehicleDef.buildDef).graphic, color, (AltitudeLayer) 26, (Thing) null, true, (ThingDef) null);
    }
    if ((double) LandingTargeter.framesOpen % 60.0 == 0.0)
      LandingTargeter.ResetRestrictionCache();
    this.CheckStillValid();
  }

  public static Color GhostDrawerColor(LandingTargeter.PositionState state)
  {
    Color color;
    switch (state)
    {
      case LandingTargeter.PositionState.Invalid:
        color = Designator_Place.CannotPlaceColor;
        break;
      case LandingTargeter.PositionState.Obstructed:
        color = LandingTargeter.GhostOccupiedColor;
        break;
      default:
        color = Designator_Place.CanPlaceColor;
        break;
    }
    return color;
  }

  public void RecacheLandingPad(LocalTargetInfo target)
  {
    this.cachedTarget = target;
    IntVec2 size = ((BuildableDef) this.vehicle.VehicleDef).Size;
  }

  public void DoExtraGuiControls()
  {
    if (!this.allowRotating)
      return;
    Rect winRect = new Rect(0.0f, (float) (UI.screenHeight - 35) - 90f, 200f, 90f);
    Find.WindowStack.ImmediateWindow(73095, winRect, (WindowLayer) 0, (Action) (() =>
    {
      RotationDirection rotationDirection = (RotationDirection) 0;
      Text.Anchor = (TextAnchor) 4;
      Text.Font = (GameFont) 2;
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector((float) ((double) ((Rect) ref winRect).width / 2.0 - 64.0 - 5.0), 15f, 64f, 64f);
      if (Widgets.ButtonImage(rect1, TexUI.RotLeftTex, true, (string) null))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
        rotationDirection = (RotationDirection) 3;
        Event.current.Use();
      }
      Widgets.Label(rect1, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector((float) ((double) ((Rect) ref winRect).width / 2.0 + 5.0), 15f, 64f, 64f);
      if (Widgets.ButtonImage(rect2, TexUI.RotRightTex, true, (string) null))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
        rotationDirection = (RotationDirection) 1;
        Event.current.Use();
      }
      Widgets.Label(rect2, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel);
      if (rotationDirection != null)
        ((Rot4) ref this.landingRotation).Rotate(rotationDirection);
      Text.Anchor = (TextAnchor) 0;
      Text.Font = (GameFont) 1;
    }), true, false, 1f, (Action) null, false);
  }

  private void HandleRotationShortcuts()
  {
    if (!this.allowRotating)
      return;
    RotationDirection rotationDirection = (RotationDirection) 0;
    if (Event.current.button == 2)
    {
      if (Event.current.type == null)
      {
        Event.current.Use();
        LandingTargeter.middleMouseDownTime = Time.realtimeSinceStartup;
      }
      if (Event.current.type == 1 && (double) Time.realtimeSinceStartup - (double) LandingTargeter.middleMouseDownTime < 0.15000000596046448)
        rotationDirection = (RotationDirection) 1;
    }
    if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
      rotationDirection = (RotationDirection) 1;
    if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
      rotationDirection = (RotationDirection) 3;
    if (rotationDirection == 1)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
      ((Rot4) ref this.landingRotation).Rotate((RotationDirection) 1);
    }
    if (rotationDirection != 3)
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
    ((Rot4) ref this.landingRotation).Rotate((RotationDirection) 3);
  }

  protected override LocalTargetInfo CurrentTargetUnderMouse()
  {
    LocalTargetInfo target = base.CurrentTargetUnderMouse();
    if (!((LocalTargetInfo) ref this.cachedTarget).IsValid || LocalTargetInfo.op_Inequality(this.cachedTarget, target))
      this.RecacheLandingPad(target);
    return target;
  }

  public override void PostInit() => LandingTargeter.Instance = this;

  public enum PositionState
  {
    Invalid,
    Obstructed,
    Valid,
  }
}
