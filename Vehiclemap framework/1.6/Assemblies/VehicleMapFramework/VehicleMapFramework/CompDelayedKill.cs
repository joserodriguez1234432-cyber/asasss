// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompDelayedKill
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools.Rendering;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompDelayedKill : VehicleComp
{
  private DestroyMode destroyMode;
  private Effecter effecter;
  private bool killTimerStarted;
  private bool spawnWreckage;
  private int ticksUntilKilled;

  public CompProperties_DelayedKill Props => (CompProperties_DelayedKill) ((ThingComp) this).props;

  public virtual bool TickByRequest => true;

  public bool KillOnTick => this.ticksUntilKilled <= 0;

  public bool KillStarted => this.killTimerStarted;

  public void StartKillTimer(DestroyMode _destroyMode, bool _spawnWreckage)
  {
    if (this.killTimerStarted)
      return;
    this.destroyMode = _destroyMode;
    this.spawnWreckage = _spawnWreckage;
    this.killTimerStarted = true;
    this.StartTicking();
    if (this.Props.message == null)
      return;
    Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate(this.Props.message, NamedArgument.op_Implicit(((Entity) ((ThingComp) this).parent).LabelCap))), GenHostility.HostileTo((Thing) ((ThingComp) this).parent, Faction.OfPlayer) ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.PawnDeath, true);
  }

  public virtual void Initialize(CompProperties _props)
  {
    ((ThingComp) this).Initialize(_props);
    this.ticksUntilKilled = this.Props.delayTicks;
  }

  public virtual void CompTick()
  {
    ((ThingComp) this).CompTick();
    --this.ticksUntilKilled;
    Transform transform = this.Vehicle.Transform;
    transform.position = Vector3.op_Addition(transform.position, Vector3.op_Division(this.Props.moveVector, 60f));
    if (this.KillOnTick)
    {
      this.Vehicle.Kill(new DamageInfo?(), this.destroyMode, this.spawnWreckage);
      this.effecter?.Cleanup();
      this.effecter = (Effecter) null;
    }
    else
    {
      if (this.effecter == null)
        this.effecter = this.Props.effecterDef?.Spawn((Thing) ((ThingComp) this).parent, ((Thing) ((ThingComp) this).parent).Map, 1f);
      this.effecter?.EffectTick(TargetInfo.op_Implicit((Thing) ((ThingComp) this).parent), TargetInfo.op_Implicit((Thing) ((ThingComp) this).parent));
    }
  }

  public virtual void PostExposeData()
  {
    ((ThingComp) this).PostExposeData();
    Scribe_Values.Look<bool>(ref this.killTimerStarted, "killTimerStarted", false, false);
    Scribe_Values.Look<int>(ref this.ticksUntilKilled, "ticksUntilKilled", 0, false);
    Scribe_Values.Look<DestroyMode>(ref this.destroyMode, "destroyMode", (DestroyMode) 0, false);
    Scribe_Values.Look<bool>(ref this.spawnWreckage, "spawnWreckage", false, false);
  }
}
