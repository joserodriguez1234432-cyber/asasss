// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleComp
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class VehicleComp : ThingComp
{
  public VehiclePawn Vehicle => this.parent as VehiclePawn;

  public virtual bool TickByRequest => false;

  public virtual IEnumerable<AnimationDriver> Animations { get; }

  public virtual IEnumerable<Gizmo> CompCaravanGizmos()
  {
    yield break;
  }

  public virtual IEnumerable<FloatMenuOption> CompFloatMenuOptions()
  {
    yield break;
  }

  public virtual void CompCaravanInspectString(StringBuilder stringBuilder)
  {
  }

  public virtual float CompStatCard(Rect rect) => 0.0f;

  public virtual void PostLoad()
  {
  }

  public virtual void OnDeSpawn()
  {
  }

  public virtual void OnDestroy()
  {
  }

  public virtual void PostGeneration()
  {
  }

  public virtual void EventRegistration()
  {
  }

  public virtual void SpawnedInGodMode()
  {
  }

  public virtual void Notify_ColorChanged()
  {
  }

  public virtual AcceptanceReport CanMove(FloatMenuContext context)
  {
    return AcceptanceReport.op_Implicit(true);
  }

  public virtual AcceptanceReport CanDraft() => AcceptanceReport.op_Implicit(true);

  public virtual bool IsThreat(IAttackTargetSearcher searcher) => false;

  public virtual void StartTicking()
  {
    if (!this.TickByRequest)
      return;
    this.Vehicle.RequestTickStart<VehicleComp>(this);
  }

  public virtual void StopTicking()
  {
    if (!this.TickByRequest)
      return;
    this.Vehicle.RequestTickStop<VehicleComp>(this);
  }
}
