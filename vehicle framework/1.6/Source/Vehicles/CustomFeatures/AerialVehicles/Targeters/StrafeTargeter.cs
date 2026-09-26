// Decompiled with JetBrains decompiler
// Type: Vehicles.StrafeTargeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class StrafeTargeter : BaseTargeter
{
  private LaunchProtocol launchProtocol;
  private Action<IntVec3, IntVec3> action;
  private IntVec3 start;
  private IntVec3 end;
  private Func<LocalTargetInfo, bool> targetValidator;

  public static StrafeTargeter Instance { get; private set; }

  public bool ForcedTargeting { get; set; }

  public override bool IsTargeting => this.action != null;

  public void BeginTargeting(
    VehiclePawn vehicle,
    LaunchProtocol launchProtocol,
    Map map,
    Action<IntVec3, IntVec3> action,
    Func<LocalTargetInfo, bool> targetValidator = null,
    Action actionWhenFinished = null,
    Texture2D mouseAttachment = null,
    bool forcedTargeting = false)
  {
    Current.Game.CurrentMap = map;
    this.BeginTargeting(vehicle, launchProtocol, action, targetValidator, actionWhenFinished, mouseAttachment);
    this.ForcedTargeting = forcedTargeting;
  }

  public void BeginTargeting(
    VehiclePawn vehicle,
    LaunchProtocol launchProtocol,
    Action<IntVec3, IntVec3> action,
    Func<LocalTargetInfo, bool> targetValidator = null,
    Action actionWhenFinished = null,
    Texture2D mouseAttachment = null,
    bool forcedTargeting = false)
  {
    this.vehicle = vehicle;
    this.launchProtocol = launchProtocol;
    this.action = action;
    this.actionWhenFinished = actionWhenFinished;
    this.mouseAttachment = mouseAttachment;
    this.targetValidator = targetValidator;
    this.ForcedTargeting = forcedTargeting;
    this.start = IntVec3.Invalid;
    this.end = IntVec3.Invalid;
    this.OnStart();
  }

  public override void StopTargeting()
  {
    if (this.actionWhenFinished != null)
    {
      this.actionWhenFinished();
      this.actionWhenFinished = (Action) null;
    }
    this.action = (Action<IntVec3, IntVec3>) null;
    this.targetValidator = (Func<LocalTargetInfo, bool>) null;
    this.ForcedTargeting = false;
  }

  public override void ProcessInputEvents()
  {
    if (Event.current.type == null && Event.current.button == 0)
    {
      LocalTargetInfo localTargetInfo = this.CurrentTargetUnderMouse();
      if (this.action != null && ((LocalTargetInfo) ref localTargetInfo).IsValid && IntVec3.op_Inequality(((LocalTargetInfo) ref localTargetInfo).Cell, this.start) && (this.targetValidator == null || this.targetValidator(localTargetInfo)))
      {
        if (((IntVec3) ref this.start).IsValid)
        {
          this.end = ((LocalTargetInfo) ref localTargetInfo).Cell;
          this.action(this.start, this.end);
          this.StopTargeting();
        }
        else
          this.start = ((LocalTargetInfo) ref localTargetInfo).Cell;
      }
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
      Event.current.Use();
    }
    if ((Event.current.type != null || Event.current.button != 1) && !KeyBindingDefOf.Cancel.KeyDownEvent)
      return;
    if (((IntVec3) ref this.start).IsValid)
    {
      this.start = IntVec3.Invalid;
      Event.current.Use();
    }
    else if (this.ForcedTargeting)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("MustTargetStrafe")), MessageTypeDefOf.RejectInput, true);
      Event.current.Use();
    }
    else
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
      this.StopTargeting();
      Event.current.Use();
    }
  }

  public override void TargeterOnGUI()
  {
    GenUI.DrawMouseAttachment(this.mouseAttachment ?? CompLaunchable.TargeterMouseAttachment);
  }

  public override void TargeterUpdate()
  {
    LocalTargetInfo localTargetInfo1 = this.CurrentTargetUnderMouse();
    IntVec3 cell = ((LocalTargetInfo) ref localTargetInfo1).Cell;
    if (((IntVec3) ref cell).IsValid && (this.targetValidator == null || this.targetValidator(LocalTargetInfo.op_Implicit(cell))))
      GenDraw.DrawTargetHighlight(LocalTargetInfo.op_Implicit(cell));
    if (((IntVec3) ref this.start).IsValid && IntVec3.op_Inequality(cell, this.start))
    {
      Vector3 vector3Shifted1 = ((IntVec3) ref this.start).ToVector3Shifted();
      Vector3 vector3Shifted2 = ((IntVec3) ref cell).ToVector3Shifted();
      GenDraw.DrawTargetHighlight(LocalTargetInfo.op_Implicit(this.start));
      GenDraw.DrawLineBetween(vector3Shifted1, vector3Shifted2, (SimpleColor) 1, 0.2f);
      GenDraw.DrawLineBetween(vector3Shifted1.PointToEdge(Current.Game.CurrentMap, vector3Shifted2.AngleToPoint(vector3Shifted1)), vector3Shifted1);
      GenDraw.DrawLineBetween(vector3Shifted2.PointToEdge(Current.Game.CurrentMap, vector3Shifted1.AngleToPoint(vector3Shifted2)), vector3Shifted2);
    }
    LocalTargetInfo localTargetInfo2 = this.CurrentTargetUnderMouse();
    int num = ((LocalTargetInfo) ref localTargetInfo2).IsValid ? 1 : 0;
  }

  public override void PostInit() => StrafeTargeter.Instance = this;
}
