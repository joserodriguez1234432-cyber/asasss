// Decompiled with JetBrains decompiler
// Type: Vehicles.FireMode
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;

#nullable enable
namespace Vehicles;

[PublicAPI]
public record FireMode()
{
  public const float DistanceTouch = 3f;
  public const float DistanceShort = 12f;
  public const float DistanceMedium = 25f;
  public const float DistanceLong = 40f;
  public 
  #nullable disable
  string label;
  public string texPath;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public IntRange shotsPerBurst;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int ticksBetweenShots;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public IntRange ticksBetweenBursts;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int burstsTillWarmup;
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool canMiss;
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool applyShooterAccuracy;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [LoadAlias("spreadRadius")]
  public float forcedMissRadius;
  [TweakField(SettingsType = UISettingsType.SliderPercent)]
  public float accuracyTouch;
  [TweakField(SettingsType = UISettingsType.SliderPercent)]
  public float accuracyShort;
  [TweakField(SettingsType = UISettingsType.SliderPercent)]
  public float accuracyMedium;
  [TweakField(SettingsType = UISettingsType.SliderPercent)]
  public float accuracyLong;
  [Unsaved(false)]
  private Texture2D icon;

  public Texture2D Icon
  {
    get
    {
      if (this.icon == null && !string.IsNullOrEmpty(this.texPath))
      {
        this.icon = ContentFinder<Texture2D>.Get(this.texPath, true);
        if (this.icon == null)
          this.icon = BaseContent.BadTex;
      }
      return this.icon;
    }
  }

  public int RoundsPerMinute
  {
    get
    {
      return ((IntRange) ref this.ticksBetweenBursts).TrueMin > this.ticksBetweenShots ? Mathf.RoundToInt(60f / (((IntRange) ref this.shotsPerBurst).Average / (60f / (float) this.ticksBetweenShots) + GenTicks.TicksToSeconds(((IntRange) ref this.ticksBetweenBursts).TrueMin)) * ((IntRange) ref this.shotsPerBurst).Average) : Mathf.RoundToInt(3600f / (float) this.ticksBetweenShots);
    }
  }

  public bool IsValid => ((IntRange) ref this.shotsPerBurst).TrueMin > 0;

  public float GetHitChanceFactor(float distance)
  {
    if ((double) distance >= 0.0)
      return (double) distance <= 3.0 ? this.accuracyTouch : ((double) distance <= 12.0 ? Mathf.Lerp(this.accuracyTouch, this.accuracyShort, (float) (((double) distance - 3.0) / 9.0)) : ((double) distance <= 25.0 ? Mathf.Lerp(this.accuracyShort, this.accuracyMedium, (float) (((double) distance - 12.0) / 13.0)) : ((double) distance <= 40.0 ? Mathf.Lerp(this.accuracyMedium, this.accuracyLong, (float) (((double) distance - 25.0) / 15.0)) : this.accuracyLong)));
    throw new ArgumentOutOfRangeException(nameof (distance));
  }

  [CompilerGenerated]
  protected virtual bool PrintMembers(
  #nullable enable
  StringBuilder builder)
  {
    RuntimeHelpers.EnsureSufficientExecutionStack();
    builder.Append("label = ");
    builder.Append((object) this.label);
    builder.Append(", texPath = ");
    builder.Append((object) this.texPath);
    builder.Append(", shotsPerBurst = ");
    builder.Append(this.shotsPerBurst.ToString());
    builder.Append(", ticksBetweenShots = ");
    builder.Append(this.ticksBetweenShots.ToString());
    builder.Append(", ticksBetweenBursts = ");
    builder.Append(this.ticksBetweenBursts.ToString());
    builder.Append(", burstsTillWarmup = ");
    builder.Append(this.burstsTillWarmup.ToString());
    builder.Append(", canMiss = ");
    builder.Append(this.canMiss.ToString());
    builder.Append(", applyShooterAccuracy = ");
    builder.Append(this.applyShooterAccuracy.ToString());
    builder.Append(", forcedMissRadius = ");
    builder.Append(this.forcedMissRadius.ToString());
    builder.Append(", accuracyTouch = ");
    builder.Append(this.accuracyTouch.ToString());
    builder.Append(", accuracyShort = ");
    builder.Append(this.accuracyShort.ToString());
    builder.Append(", accuracyMedium = ");
    builder.Append(this.accuracyMedium.ToString());
    builder.Append(", accuracyLong = ");
    builder.Append(this.accuracyLong.ToString());
    builder.Append(", Icon = ");
    builder.Append((object) this.Icon);
    builder.Append(", RoundsPerMinute = ");
    builder.Append(this.RoundsPerMinute.ToString());
    builder.Append(", IsValid = ");
    builder.Append(this.IsValid.ToString());
    return true;
  }

  [CompilerGenerated]
  public override int GetHashCode()
  {
    return (((((((((((((EqualityComparer<System.Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.label)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.texPath)) * -1521134295 + EqualityComparer<IntRange>.Default.GetHashCode(this.shotsPerBurst)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.ticksBetweenShots)) * -1521134295 + EqualityComparer<IntRange>.Default.GetHashCode(this.ticksBetweenBursts)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.burstsTillWarmup)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.canMiss)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.applyShooterAccuracy)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.forcedMissRadius)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.accuracyTouch)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.accuracyShort)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.accuracyMedium)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.accuracyLong)) * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(this.icon);
  }

  [CompilerGenerated]
  public virtual bool Equals(FireMode? other)
  {
    if ((object) this == (object) other)
      return true;
    return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.label, other.label) && EqualityComparer<string>.Default.Equals(this.texPath, other.texPath) && EqualityComparer<IntRange>.Default.Equals(this.shotsPerBurst, other.shotsPerBurst) && EqualityComparer<int>.Default.Equals(this.ticksBetweenShots, other.ticksBetweenShots) && EqualityComparer<IntRange>.Default.Equals(this.ticksBetweenBursts, other.ticksBetweenBursts) && EqualityComparer<int>.Default.Equals(this.burstsTillWarmup, other.burstsTillWarmup) && EqualityComparer<bool>.Default.Equals(this.canMiss, other.canMiss) && EqualityComparer<bool>.Default.Equals(this.applyShooterAccuracy, other.applyShooterAccuracy) && EqualityComparer<float>.Default.Equals(this.forcedMissRadius, other.forcedMissRadius) && EqualityComparer<float>.Default.Equals(this.accuracyTouch, other.accuracyTouch) && EqualityComparer<float>.Default.Equals(this.accuracyShort, other.accuracyShort) && EqualityComparer<float>.Default.Equals(this.accuracyMedium, other.accuracyMedium) && EqualityComparer<float>.Default.Equals(this.accuracyLong, other.accuracyLong) && EqualityComparer<Texture2D>.Default.Equals(this.icon, other.icon);
  }

  [CompilerGenerated]
  protected FireMode(FireMode original)
  {
    this.label = original.label;
    this.texPath = original.texPath;
    this.shotsPerBurst = original.shotsPerBurst;
    this.ticksBetweenShots = original.ticksBetweenShots;
    this.ticksBetweenBursts = original.ticksBetweenBursts;
    this.burstsTillWarmup = original.burstsTillWarmup;
    this.canMiss = original.canMiss;
    this.applyShooterAccuracy = original.applyShooterAccuracy;
    this.forcedMissRadius = original.forcedMissRadius;
    this.accuracyTouch = original.accuracyTouch;
    this.accuracyShort = original.accuracyShort;
    this.accuracyMedium = original.accuracyMedium;
    this.accuracyLong = original.accuracyLong;
    this.icon = original.icon;
  }
}
