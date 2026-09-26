// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AntiAirTargeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

public class AntiAirTargeter : BaseWorldTargeter
{
  protected const float BaseFeedbackTexSize = 0.8f;
  protected Thing caster;
  protected Func<GlobalTargetInfo, float, bool> action;

  public static AntiAirTargeter Instance { get; private set; }

  public override bool IsTargeting => this.action != null;

  public void BeginTargeting(
    Thing caster,
    Func<GlobalTargetInfo, float, bool> action,
    int origin,
    bool canTargetTiles,
    Texture2D mouseAttachment = null,
    bool closeWorldTabWhenFinished = false,
    Action onUpdate = null,
    Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter = null)
  {
    this.caster = caster;
    this.action = action;
    this.originOnMap = WorldHelper.GetTilePos(PlanetTile.op_Implicit(origin));
    this.canTargetTiles = canTargetTiles;
    this.mouseAttachment = mouseAttachment;
    this.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
    this.onUpdate = onUpdate;
    this.OnStart();
  }

  public void BeginTargeting(
    VehiclePawn vehicle,
    Func<GlobalTargetInfo, float, bool> action,
    AerialVehicleInFlight aerialVehicle,
    bool canTargetTiles,
    Texture2D mouseAttachment = null,
    bool closeWorldTabWhenFinished = false,
    Action onUpdate = null,
    Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter = null)
  {
    this.action = action;
    this.canTargetTiles = canTargetTiles;
    this.mouseAttachment = mouseAttachment;
    this.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
    this.onUpdate = onUpdate;
    this.OnStart();
  }

  public override void StopTargeting()
  {
    if (this.closeWorldTabWhenFinished)
      CameraJumper.TryHideWorld();
    this.action = (Func<GlobalTargetInfo, float, bool>) null;
    this.canTargetTiles = false;
    this.mouseAttachment = (Texture2D) null;
    this.closeWorldTabWhenFinished = false;
    this.onUpdate = (Action) null;
  }

  public override void ProcessInputEvents()
  {
    if (Event.current.type == null && this.IsTargeting)
    {
      if (Event.current.button == 0)
      {
        this.CurrentTargetUnderMouse();
        Event.current.Use();
      }
      if (Event.current.button == 1)
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
        this.StopTargeting();
        Event.current.Use();
      }
    }
    if (!KeyBindingDefOf.Cancel.KeyDownEvent || !this.IsTargeting)
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
    this.StopTargeting();
    Event.current.Use();
  }

  public override void TargeterOnGUI()
  {
    if (!this.IsTargeting || Mouse.IsInputBlockedNow)
      return;
    this.CurrentTargetUnderMouse();
    Vector2 mousePosition = Event.current.mousePosition;
    Texture2D texture2D = this.mouseAttachment ?? TexCommand.Attack;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
    GUI.DrawTexture(rect, (Texture) texture2D);
  }

  public override void TargeterUpdate()
  {
    if (!this.IsTargeting)
      return;
    Action onUpdate = this.onUpdate;
    if (onUpdate == null)
      return;
    onUpdate();
  }

  public override void PostInit() => AntiAirTargeter.Instance = this;
}
