// Decompiled with JetBrains decompiler
// Type: Vehicles.HaulTargeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class HaulTargeter : BaseTargeter
{
  private Action<LocalTargetInfo> action;
  private TargetingParameters targetParams;
  private Map map;

  public static HaulTargeter Instance { get; private set; }

  public override bool IsTargeting => this.action != null;

  public static void BeginTargeting(
    TargetingParameters targetParams,
    Action<LocalTargetInfo> action,
    VehiclePawn vehicle,
    Action actionWhenFinished = null,
    Texture2D mouseAttachment = null)
  {
    HaulTargeter.Instance.action = action;
    HaulTargeter.Instance.targetParams = targetParams;
    HaulTargeter.Instance.vehicle = vehicle;
    HaulTargeter.Instance.actionWhenFinished = actionWhenFinished;
    HaulTargeter.Instance.mouseAttachment = mouseAttachment;
    HaulTargeter.Instance.map = ((Thing) vehicle).Map;
    HaulTargeter.Instance.OnStart();
  }

  public override void StopTargeting()
  {
    if (this.actionWhenFinished != null)
    {
      Action actionWhenFinished = this.actionWhenFinished;
      this.actionWhenFinished = (Action) null;
      actionWhenFinished();
    }
    this.action = (Action<LocalTargetInfo>) null;
  }

  public override void ProcessInputEvents()
  {
    this.ConfirmStillValid();
    if (!this.IsTargeting)
      return;
    if (Event.current.type == null && Event.current.button == 0)
    {
      Event.current.Use();
      if (this.action != null)
      {
        LocalTargetInfo target = this.CurrentTargetUnderMouse();
        if (GenGrid.InBounds(((LocalTargetInfo) ref target).Cell, this.map) && this.TargetMeetsRequirements(target))
        {
          this.action(target);
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
        }
        else
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
      }
    }
    if ((Event.current.type != null || Event.current.button != 1) && !KeyBindingDefOf.Cancel.KeyDownEvent)
      return;
    this.StopTargeting();
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
    Event.current.Use();
  }

  public override void TargeterOnGUI()
  {
    if (this.action == null)
      return;
    Texture2D texture2D = this.mouseAttachment ?? TexCommand.Attack;
    if (!Object.op_Implicit((Object) texture2D))
      return;
    GenUI.DrawMouseAttachment(texture2D);
  }

  public override void TargeterUpdate()
  {
    if (!this.IsTargeting)
      return;
    LocalTargetInfo target = this.CurrentTargetUnderMouse();
    SimpleColor simpleColor = (SimpleColor) 1;
    if (this.TargetMeetsRequirements(target))
      simpleColor = (SimpleColor) 0;
    Vector3 vector3 = UI.MouseMapPosition();
    if (((LocalTargetInfo) ref target).IsValid)
    {
      GenDraw.DrawTargetHighlight(target);
      vector3 = ((LocalTargetInfo) ref target).CenterVector3;
    }
    GenDraw.DrawLineBetween(((Thing) this.vehicle).DrawPos, vector3, simpleColor, 0.2f);
  }

  private void ConfirmStillValid()
  {
    if (this.vehicle != null && ((Thing) this.vehicle).Map == Find.CurrentMap && !((Thing) this.vehicle).Destroyed && Find.Selector.IsSelected((object) this.vehicle))
      return;
    this.StopTargeting();
  }

  protected override LocalTargetInfo CurrentTargetUnderMouse()
  {
    return !this.IsTargeting ? LocalTargetInfo.Invalid : GenCollection.FirstOrFallback<LocalTargetInfo>(GenUI.TargetsAtMouse(this.targetParams, false, (ITargetingSource) null), LocalTargetInfo.Invalid);
  }

  public bool TargetMeetsRequirements(LocalTargetInfo target)
  {
    return ((LocalTargetInfo) ref target).HasThing && (((LocalTargetInfo) ref target).Thing is Pawn || ((LocalTargetInfo) ref target).Thing.def.EverHaulable) && ((LocalTargetInfo) ref target).Thing.Spawned && !((LocalTargetInfo) ref target).Thing.Destroyed;
  }

  public override void PostInit() => HaulTargeter.Instance = this;
}
