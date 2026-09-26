// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleComponent
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleComponent : IExposable, ITweakFields
{
  [Unsaved(false)]
  public readonly VehiclePawn vehicle;
  [TweakField]
  public VehicleComponentProperties props;
  private float health;
  public IndicatorDef indicator;
  public Color highlightColor = Color.white;
  public VehicleComponent.VehiclePartDepth? depthOverride;

  public VehicleComponent(VehiclePawn vehicle) => this.vehicle = vehicle;

  public float Health => this.health;

  public float HealthPercent => (this.health / this.MaxHealth).RoundTo(0.01f);

  public float Efficiency => this.props.efficiency.Evaluate(this.HealthPercent);

  public Dictionary<string, List<StatModifier>> SetArmorModifiers { get; private set; } = new Dictionary<string, List<StatModifier>>();

  public Dictionary<string, List<StatModifier>> AddArmorModifiers { get; private set; } = new Dictionary<string, List<StatModifier>>();

  public float SetHealthModifier { get; set; } = -1f;

  public Dictionary<string, float> AddHealthModifiers { get; private set; } = new Dictionary<string, float>();

  public VehicleComponent.VehiclePartDepth Depth => this.depthOverride ?? this.props.depth;

  string ITweakFields.Category => this.props.key;

  string ITweakFields.Label => this.props.key;

  public float MaxHealth
  {
    get
    {
      if ((double) this.SetHealthModifier > 0.0)
        return this.SetHealthModifier;
      float health = (float) this.props.health;
      if (!GenDictionary.NullOrEmpty<string, float>(this.AddHealthModifiers))
      {
        foreach (float num in this.AddHealthModifiers.Values)
          health += num;
      }
      return health;
    }
  }

  public virtual void DrawIcon(Rect rect)
  {
    if (this.indicator == null)
      return;
    Widgets.DrawTextureFitted(rect, (Texture) this.indicator.Icon, 1f, 1f);
    TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(this.indicator.label));
  }

  public void SetHealth(float health)
  {
    this.health = Mathf.Clamp(health, 0.0f, this.MaxHealth);
    this.vehicle?.EventRegistry[VehicleEventDefOf.HealthChanged].ExecuteEvents();
  }

  public void TakeDamage(VehiclePawn _, DamageInfo dinfo, bool ignoreArmor = false)
  {
    int damage = (int) this.TakeDamage((VehiclePawn) null, ref dinfo, ignoreArmor);
  }

  public virtual VehicleComponent.Penetration TakeDamage(
    VehiclePawn _,
    ref DamageInfo dinfo,
    bool ignoreArmor = false)
  {
    VehicleComponent.Penetration result = VehicleComponent.Penetration.NonPenetrated;
    if (!ignoreArmor)
      this.ReduceDamageFromArmor(ref dinfo, out result);
    if (((DamageInfo) ref dinfo).Def == DamageDefOf.EMP)
      result = VehicleComponent.Penetration.Electrified;
    this.health -= ((DamageInfo) ref dinfo).Amount;
    float num = Mathf.Clamp(-this.health, 0.0f, float.MaxValue);
    this.health = Mathf.Clamp(this.health, 0.0f, this.MaxHealth);
    if ((double) ((DamageInfo) ref dinfo).Amount > 0.0)
    {
      if (!GenList.NullOrEmpty<Reactor>((IList<Reactor>) this.props.reactors))
      {
        foreach (Reactor reactor in this.props.reactors)
          reactor.Hit(this.vehicle, this, ref dinfo, result);
      }
      this.vehicle.EventRegistry[VehicleEventDefOf.DamageTaken].ExecuteEvents();
      this.vehicle.EventRegistry[VehicleEventDefOf.HealthChanged].ExecuteEvents();
      if ((double) this.vehicle.GetStatValue(VehicleStatDefOf.MoveSpeed) <= 0.10000000149011612)
        this.vehicle.ignition.Drafted = false;
      if (((Thing) this.vehicle).Spawned && (double) this.vehicle.GetStatValue(VehicleStatDefOf.BodyIntegrity) < 0.0099999997764825821)
        this.vehicle.Kill(new DamageInfo?(dinfo), (DestroyMode) 2);
    }
    if (result == VehicleComponent.Penetration.Electrified)
      ((DamageInfo) ref dinfo).SetAmount(0.0f);
    else if (result < VehicleComponent.Penetration.Penetrated || (double) this.health > (double) this.MaxHealth / 2.0)
      ((DamageInfo) ref dinfo).SetAmount(0.0f);
    else if (result == VehicleComponent.Penetration.Penetrated || (double) num > 0.0 && (double) num >= (double) ((DamageInfo) ref dinfo).Amount / 2.0)
      ((DamageInfo) ref dinfo).SetAmount(num);
    return result;
  }

  public int ApplyEMPDamage(ref DamageInfo dinfo, ref bool adapted)
  {
    adapted = false;
    if (!this.vehicle.VehicleDef.properties.empStuns)
      return 0;
    VehicleEMPSeverity empSeverity = this.props.empSeverity;
    if (empSeverity == VehicleEMPSeverity.None)
      return 0;
    int num1 = 0;
    if (Rand.Chance(DamageHelper.EMPChanceToStun(empSeverity)))
    {
      num1 = DamageHelper.EMPStunLength(empSeverity, ((DamageInfo) ref dinfo).Amount);
      float num2 = DamageHelper.EMPStunDamage(empSeverity);
      ((DamageInfo) ref dinfo).SetAmount(num2 * this.MaxHealth);
      this.TakeDamage(this.vehicle, dinfo, true);
    }
    return num1;
  }

  public virtual void HealComponent(float amount)
  {
    float health = this.health;
    this.health = Mathf.Clamp(this.health + amount, 0.0f, this.MaxHealth);
    if (Mathf.Approximately(health, this.health))
      return;
    this.vehicle.EventRegistry[VehicleEventDefOf.Repaired].ExecuteEvents();
    this.vehicle.EventRegistry[VehicleEventDefOf.HealthChanged].ExecuteEvents();
  }

  public virtual void ReduceDamageFromArmor(
    ref DamageInfo dinfo,
    out VehicleComponent.Penetration result)
  {
    result = VehicleComponent.Penetration.NonPenetrated;
    if (((DamageInfo) ref dinfo).Def.armorCategory == null)
      return;
    float num1 = this.ArmorRating(((DamageInfo) ref dinfo).Def.armorCategory, out float _) - ((DamageInfo) ref dinfo).ArmorPenetrationInt;
    result = this.props.hitbox.fallthrough ? VehicleComponent.Penetration.Penetrated : VehicleComponent.Penetration.NonPenetrated;
    float num2 = Rand.Value;
    if ((double) num2 < (double) num1 / 2.0)
    {
      ((DamageInfo) ref dinfo).SetAmount(0.0f);
      result = VehicleComponent.Penetration.Deflected;
    }
    else
    {
      if ((double) num2 >= (double) num1)
        return;
      float num3 = (float) GenMath.RoundRandom(((DamageInfo) ref dinfo).Amount / 2f);
      ((DamageInfo) ref dinfo).SetAmount(num3);
      result = VehicleComponent.Penetration.Diminished;
      if (((DamageInfo) ref dinfo).Def.armorCategory != DamageArmorCategoryDefOf.Sharp)
        return;
      ((DamageInfo) ref dinfo).Def = DamageDefOf.Blunt;
    }
  }

  public float ArmorRating(DamageArmorCategoryDef armorCategoryDef, out float upgraded)
  {
    float statValue = StatExtension.GetStatValue((Thing) this.vehicle, armorCategoryDef.armorRatingStat, true, -1);
    upgraded = 0.0f;
    if (!GenDictionary.NullOrEmpty<string, List<StatModifier>>(this.SetArmorModifiers))
    {
      foreach (List<StatModifier> statModifiers in this.SetArmorModifiers.Values)
      {
        float statModifierValue;
        if (TryGetModifier(statModifiers, out statModifierValue))
        {
          upgraded = statModifierValue - statValue;
          return statModifierValue;
        }
      }
    }
    float num = statValue + this.vehicle.statHandler.GetUpgradeableStatValue(armorCategoryDef.armorRatingStat);
    List<StatModifier> armor = this.props.armor;
    StatModifier statModifier = armor != null ? GenCollection.FirstOrDefault<StatModifier>(armor, (Predicate<StatModifier>) (rating => rating.stat == armorCategoryDef.armorRatingStat)) : (StatModifier) null;
    if (statModifier != null)
    {
      statValue = statModifier.value;
      num = statValue;
    }
    if (!GenDictionary.NullOrEmpty<string, List<StatModifier>>(this.AddArmorModifiers))
    {
      foreach (List<StatModifier> statModifiers in this.AddArmorModifiers.Values)
      {
        float statModifierValue;
        if (TryGetModifier(statModifiers, out statModifierValue))
          num += statModifierValue;
      }
    }
    upgraded = num - statValue;
    return num;

    bool TryGetModifier(List<StatModifier> statModifiers, out float statModifierValue)
    {
      statModifierValue = 0.0f;
      if (GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) statModifiers))
        return false;
      foreach (StatModifier statModifier in statModifiers)
      {
        if (statModifier.stat == armorCategoryDef.armorRatingStat)
        {
          statModifierValue = statModifier.value;
          return true;
        }
      }
      return false;
    }
  }

  public virtual void PostCreate() => this.health = this.MaxHealth;

  public virtual void Initialize(VehicleComponentProperties props)
  {
    this.props = props;
    List<Reactor> reactors1 = props.reactors;
    this.indicator = reactors1 != null ? GenCollection.FirstOrDefault<Reactor>(reactors1, (Predicate<Reactor>) (reactor => reactor.indicator != null))?.indicator : (IndicatorDef) null;
    List<Reactor> reactors2 = props.reactors;
    this.highlightColor = (reactors2 != null ? reactors2.FirstOrDefault<Reactor>()?.highlightColor : new Color?()) ?? Color.white;
  }

  public virtual void ExposeData()
  {
    Scribe_Values.Look<float>(ref this.health, "health", 0.0f, false);
  }

  void ITweakFields.OnFieldChanged()
  {
  }

  public enum VehiclePartDepth
  {
    Undefined,
    External,
    Internal,
  }

  public enum Penetration
  {
    NonPenetrated,
    Deflected,
    Diminished,
    Penetrated,
    Electrified,
  }

  public struct DamageResult
  {
    public VehicleComponent.Penetration penetration;
    public DamageInfo damageInfo;
    public IntVec2 cell;
  }
}
