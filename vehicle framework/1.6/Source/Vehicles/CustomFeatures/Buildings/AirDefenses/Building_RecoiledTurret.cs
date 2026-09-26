// Decompiled with JetBrains decompiler
// Type: Vehicles.Building_RecoiledTurret
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public class Building_RecoiledTurret : Building_Artillery
{
  public Building_RecoiledTurret()
  {
    this.top = (TurretTop) Activator.CreateInstance(typeof (TurretTop_Recoiled), (object) this);
  }

  protected TurretTop_Recoiled TopRecoiled => this.top as TurretTop_Recoiled;

  public virtual void Notify_Recoiled() => this.TopRecoiled.Notify_TurretRecoil();

  public override void NotifyTargetInRange(GlobalTargetInfo target)
  {
    base.NotifyTargetInRange(target);
    if (!(((Building_Turret) this).AttackVerb is Verb_ShootWorldRecoiled))
      return;
    if (this.CanExtractShell && this.MannedByColonist)
    {
      CompChangeableProjectile comp = ThingCompUtility.TryGetComp<CompChangeableProjectile>(this.gun);
      if (!comp.allowedShellsSettings.AllowedToAccept(comp.LoadedShell))
        this.ExtractShell();
    }
    if (((GlobalTargetInfo) ref this.forcedWorldTarget).IsValid && !this.CanSetForcedTarget)
      this.ResetForcedTarget();
    if (!this.CanToggleHoldFire)
      AccessTools.Field(typeof (Building_TurretGun), "holdFire")?.SetValue((object) this, (object) false);
    if (((GlobalTargetInfo) ref this.forcedWorldTarget).ThingDestroyed)
      this.ResetForcedTarget();
    if (this.Active && (this.mannableComp == null || this.mannableComp.MannedNow) && !((Building_Turret) this).IsStunned && ((Thing) this).Spawned)
    {
      this.GunCompEq.verbTracker.VerbsTick();
      if (((Building_Turret) this).AttackVerb.state != 1)
      {
        if (this.WarmingUp)
        {
          --this.burstWarmupTicksLeft;
          if (this.burstWarmupTicksLeft == 0)
            this.BeginBurst();
        }
        else
        {
          if (this.burstCooldownTicksLeft > 0)
          {
            --this.burstCooldownTicksLeft;
            if (this.IsMortar)
            {
              if (this.progressBarEffecter == null)
                this.progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
              this.progressBarEffecter.EffectTick(TargetInfo.op_Implicit((Thing) this), TargetInfo.Invalid);
              MoteProgressBar mote = ((SubEffecter_ProgressBar) this.progressBarEffecter.children[0]).mote;
              mote.progress = (float) (1.0 - (double) Math.Max(this.burstCooldownTicksLeft, 0) / (double) GenTicks.SecondsToTicks(this.BurstCooldownTime()));
              mote.offsetZ = -0.8f;
            }
          }
          if (this.burstCooldownTicksLeft <= 0 && Gen.IsHashIntervalTick((Thing) this, 10))
            this.TryStartShootSomething(true);
        }
        this.top.TurretTopTick();
        return;
      }
    }
    else
      this.ResetCurrentTarget();
    if (!this.WarmingUp)
      return;
    --this.burstWarmupTicksLeft;
    if (this.burstWarmupTicksLeft != 0)
      return;
    this.BeginBurst();
  }
}
