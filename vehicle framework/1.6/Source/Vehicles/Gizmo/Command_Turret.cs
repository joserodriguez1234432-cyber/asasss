// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.Command_Turret
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.Rendering;

[UsedImplicitly]
public abstract class Command_Turret : Command
{
  protected const float AmmoWindowOffset = 5f;
  protected readonly Color DarkGrey = new Color(0.05f, 0.05f, 0.05f, 0.5f);
  public TargetingParameters targetingParams;
  public bool canReload;
  public VehiclePawn vehicle;
  public VehicleTurret turret;
  protected float cachedGizmoWidth = -1f;

  protected float GizmoWidth
  {
    get
    {
      if ((double) this.cachedGizmoWidth < 0.0)
        this.cachedGizmoWidth = this.RecalculateWidth();
      return this.cachedGizmoWidth;
    }
  }

  protected virtual float RecalculateWidth() => 75f;

  public virtual float GetWidth(float maxWidth) => this.GizmoWidth;

  public virtual void ProcessInput(Event @event)
  {
    base.ProcessInput(@event);
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Tiny, (Map) null);
    this.FireTurrets();
  }

  public abstract void FireTurret(VehicleTurret turret);

  public virtual void FireTurrets()
  {
    if (!GenText.NullOrEmpty(this.turret.groupKey))
    {
      foreach (VehicleTurret groupTurret in this.turret.GroupTurrets)
        this.FireTurret(groupTurret);
    }
    else
      this.FireTurret(this.turret);
  }

  public virtual bool GroupsWith(Gizmo other)
  {
    return other is Command_CooldownAction commandCooldownAction && commandCooldownAction.turret.GroupsWith(this.turret);
  }
}
