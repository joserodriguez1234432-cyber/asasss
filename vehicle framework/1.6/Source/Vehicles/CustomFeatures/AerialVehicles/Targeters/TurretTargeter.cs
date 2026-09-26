// Decompiled with JetBrains decompiler
// Type: Vehicles.TurretTargeter
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

public class TurretTargeter : BaseTargeter
{
  private Action<LocalTargetInfo> action;
  private TargetingParameters targetParams;
  private Map map;

  public static TurretTargeter Instance { get; private set; }

  public static VehicleTurret Turret { get; private set; }

  public override bool IsTargeting => this.action != null;

  private bool TargeterValid
  {
    get
    {
      return this.vehicle != null && ((Thing) this.vehicle).Map == Find.CurrentMap && !((Thing) this.vehicle).Destroyed && TurretTargeter.Turret != null && !TurretTargeter.Turret.ComponentDisabled && Find.Selector.IsSelected((object) this.vehicle);
    }
  }

  public static void BeginTargeting(
    TargetingParameters targetParams,
    Action<LocalTargetInfo> action,
    VehicleTurret turret,
    Action actionWhenFinished = null,
    Texture2D mouseAttachment = null)
  {
    TurretTargeter.Instance.action = action;
    TurretTargeter.Instance.targetParams = targetParams;
    TurretTargeter.Instance.vehicle = turret.vehicle;
    TurretTargeter.Turret = turret;
    TurretTargeter.Turret.SetTarget(LocalTargetInfo.Invalid);
    TurretTargeter.Instance.actionWhenFinished = actionWhenFinished;
    TurretTargeter.Instance.mouseAttachment = mouseAttachment;
    TurretTargeter.Instance.map = ((Thing) turret.vehicle).Map;
    TurretTargeter.Instance.OnStart();
    TurretTargeter.Turret.StartTicking();
  }

  public override void StopTargeting()
  {
    Action actionWhenFinished = this.actionWhenFinished;
    if (actionWhenFinished != null)
      actionWhenFinished();
    TurretTargeter.Turret = (VehicleTurret) null;
    this.action = (Action<LocalTargetInfo>) null;
    this.actionWhenFinished = (Action) null;
  }

  public void StopTargeting(bool canceled)
  {
    if (canceled && TurretTargeter.Turret != null)
    {
      TurretTargeter.Turret.AlignToAngleRestricted(TurretTargeter.Turret.TurretRotation);
      TurretTargeter.Turret.SetTarget(LocalTargetInfo.Invalid);
    }
    this.StopTargeting();
  }

  public override void ProcessInputEvents()
  {
    if (!this.TargeterValid)
    {
      this.StopTargeting(true);
    }
    else
    {
      if (!this.IsTargeting)
        return;
      if (Event.current.type == null && Event.current.button == 0)
      {
        Event.current.Use();
        if (this.action != null)
        {
          LocalTargetInfo target = this.CurrentTargetUnderMouse();
          if (GenGrid.InBounds(((LocalTargetInfo) ref target).Cell, this.map) && TargetingHelper.TargetMeetsRequirements(TurretTargeter.Turret, target, out IntVec3 _))
          {
            this.action(target);
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
            this.StopTargeting(false);
          }
          else
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
        }
      }
      if ((Event.current.type != null || Event.current.button != 1) && !KeyBindingDefOf.Cancel.KeyDownEvent)
        return;
      this.StopTargeting(true);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
      Event.current.Use();
    }
  }

  public override void TargeterOnGUI()
  {
    if (this.action == null || !TargetingHelper.TargetMeetsRequirements(TurretTargeter.Turret, this.CurrentTargetUnderMouse(), out IntVec3 _))
      return;
    GenUI.DrawMouseAttachment(this.mouseAttachment ?? TexCommand.Attack);
  }

  public override void TargeterUpdate()
  {
    if (!this.IsTargeting)
      return;
    LocalTargetInfo target = this.CurrentTargetUnderMouse();
    IntVec3 goodDest;
    if (!TargetingHelper.TargetMeetsRequirements(TurretTargeter.Turret, target, out goodDest))
      return;
    GenDraw.DrawTargetHighlight(target);
    if ((double) TurretTargeter.Turret.CurrentFireMode.forcedMissRadius > 1.0)
      GenDraw.DrawRadiusRing(((LocalTargetInfo) ref target).Cell, TurretTargeter.Turret.CurrentFireMode.forcedMissRadius);
    if (!LocalTargetInfo.op_Inequality(target, LocalTargetInfo.op_Implicit((Thing) TurretTargeter.Turret.vehicle)))
      return;
    Vector3 turretLocation = TurretTargeter.Turret.TurretLocation;
    Thing thing = ((LocalTargetInfo) ref target).Thing;
    Vector3 point;
    if (thing == null)
    {
      goodDest = ((LocalTargetInfo) ref target).Cell;
      point = ((IntVec3) ref goodDest).ToVector3Shifted();
    }
    else
      point = thing.DrawPos;
    TurretTargeter.Turret.AlignToAngleRestricted(turretLocation.AngleToPoint(point));
  }

  protected override LocalTargetInfo CurrentTargetUnderMouse()
  {
    return !this.IsTargeting ? LocalTargetInfo.Invalid : GenCollection.FirstOrFallback<LocalTargetInfo>(GenUI.TargetsAtMouse(this.targetParams, false, (ITargetingSource) null), LocalTargetInfo.Invalid);
  }

  public override void PostInit() => TurretTargeter.Instance = this;
}
