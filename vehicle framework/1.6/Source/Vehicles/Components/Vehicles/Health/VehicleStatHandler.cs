// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatHandler
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Vehicles.Compatibility;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleStatHandler : IExposable, ITweakFields
{
  private const int TicksHighlighted = 100;
  private const float ChanceDirectHit = 1.25f;
  private const float ChanceFallthroughHit = 1f;
  private const float ChanceMinorDeflectHit = 0.75f;
  private const float ChanceMajorDeflectHit = 0.75f;
  private static readonly FieldInfo StunFromEmp = AccessTools.Field(typeof (StunHandler), "stunFromEMP");
  private static readonly FieldInfo AdaptationTicksLeft = AccessTools.Field(typeof (StunHandler), "adaptationTicksLeft");
  private static readonly List<IntVec3> HitboxHighlightCells = new List<IntVec3>();
  private int adaptTicks;
  private readonly List<Pair<IntVec2, int>> debugCellHighlight = new List<Pair<IntVec2, int>>();
  private readonly Dictionary<string, VehicleComponent> componentsByKeys = new Dictionary<string, VehicleComponent>();
  private readonly Dictionary<IntVec2, List<VehicleComponent>> componentLocations = new Dictionary<IntVec2, List<VehicleComponent>>();
  private readonly Dictionary<VehicleStatDef, List<VehicleComponent>> statComponents = new Dictionary<VehicleStatDef, List<VehicleComponent>>();
  private readonly Dictionary<StatUpgradeCategoryDef, StatOffset> categoryOffsets = new Dictionary<StatUpgradeCategoryDef, StatOffset>();
  private readonly Dictionary<StatDef, StatOffset> baseStatOffsets = new Dictionary<StatDef, StatOffset>();
  private readonly Dictionary<VehicleStatDef, StatOffset> statOffsets = new Dictionary<VehicleStatDef, StatOffset>();
  public readonly StatCache statCache;
  [TweakField]
  public List<VehicleComponent> components = new List<VehicleComponent>();
  [Unsaved(false)]
  private readonly VehiclePawn vehicle;
  private readonly Dictionary<Thing, IntVec3> impacter = new Dictionary<Thing, IntVec3>();

  public VehicleStatHandler(VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    this.statCache = new StatCache(vehicle);
  }

  public List<VehicleComponent> ComponentsPrioritized
  {
    get
    {
      return this.components.OrderBy<VehicleComponent, int>((Func<VehicleComponent, int>) (c => GenList.NullOrEmpty<VehicleStatDef>((IList<VehicleStatDef>) c.props.categories) ? 9999 : c.props.categories.Min<VehicleStatDef>((Func<VehicleStatDef, int>) (ctg => ctg.displayPriorityInCategory)))).ThenBy<VehicleComponent, float>((Func<VehicleComponent, float>) (c => c.HealthPercent)).ToList<VehicleComponent>();
    }
  }

  public bool CanDirty { get; internal set; } = true;

  public bool NeedsRepairs
  {
    get
    {
      return GenCollection.Any<VehicleComponent>(this.components, (Predicate<VehicleComponent>) (c => (double) c.HealthPercent < 1.0));
    }
  }

  public float HealthPercent { get; private set; }

  public bool OverrideStunPatch { get; private set; }

  string ITweakFields.Category => nameof (VehicleStatHandler);

  string ITweakFields.Label => "Stat Handler";

  public void InitializeComponents()
  {
    this.components.Clear();
    this.statComponents.Clear();
    this.componentsByKeys.Clear();
    if (GenList.NullOrEmpty<VehicleComponentProperties>((IList<VehicleComponentProperties>) this.vehicle.VehicleDef.components))
      return;
    foreach (VehicleComponentProperties component in this.vehicle.VehicleDef.components)
    {
      VehicleComponent instance = (VehicleComponent) Activator.CreateInstance(component.compClass, (object) this.vehicle);
      this.components.Add(instance);
      instance.Initialize(component);
      instance.PostCreate();
      this.componentsByKeys[instance.props.key] = instance;
      this.RecacheStatCategories(instance);
    }
  }

  public void AddStatOffset(VehicleStatDef vehicleStatDef, float value)
  {
    StatOffset statOffset;
    if (!this.statOffsets.TryGetValue(vehicleStatDef, out statOffset))
    {
      this.statOffsets[vehicleStatDef] = new StatOffset(this.vehicle, vehicleStatDef);
      statOffset = this.statOffsets[vehicleStatDef];
    }
    statOffset.Offset += value;
  }

  public void AddStatOffset(StatUpgradeCategoryDef upgradeCategoryDef, float value)
  {
    StatOffset categoryOffset;
    if (!this.categoryOffsets.TryGetValue(upgradeCategoryDef, out categoryOffset))
    {
      this.categoryOffsets[upgradeCategoryDef] = new StatOffset(this.vehicle, upgradeCategoryDef);
      categoryOffset = this.categoryOffsets[upgradeCategoryDef];
    }
    categoryOffset.Offset += value;
  }

  public void SetStatOffset(string key, VehicleStatDef vehicleStatDef, float value)
  {
    StatOffset statOffset;
    if (!this.statOffsets.TryGetValue(vehicleStatDef, out statOffset))
    {
      this.statOffsets[vehicleStatDef] = new StatOffset(this.vehicle, vehicleStatDef);
      statOffset = this.statOffsets[vehicleStatDef];
    }
    statOffset.AddOverride(key, value);
  }

  public void SetStatOffset(string key, StatUpgradeCategoryDef upgradeCategoryDef, float value)
  {
    StatOffset categoryOffset;
    if (!this.categoryOffsets.TryGetValue(upgradeCategoryDef, out categoryOffset))
    {
      this.categoryOffsets[upgradeCategoryDef] = new StatOffset(this.vehicle, upgradeCategoryDef);
      categoryOffset = this.categoryOffsets[upgradeCategoryDef];
    }
    categoryOffset.AddOverride(key, value);
  }

  public void SubtractStatOffset(VehicleStatDef vehicleStatDef, float value)
  {
    StatOffset statOffset;
    if (!this.statOffsets.TryGetValue(vehicleStatDef, out statOffset))
    {
      this.statOffsets[vehicleStatDef] = new StatOffset(this.vehicle, vehicleStatDef);
      statOffset = this.statOffsets[vehicleStatDef];
    }
    statOffset.Offset -= value;
  }

  public void SubtractStatOffset(StatUpgradeCategoryDef upgradeCategoryDef, float value)
  {
    StatOffset categoryOffset;
    if (!this.categoryOffsets.TryGetValue(upgradeCategoryDef, out categoryOffset))
    {
      this.categoryOffsets[upgradeCategoryDef] = new StatOffset(this.vehicle, upgradeCategoryDef);
      categoryOffset = this.categoryOffsets[upgradeCategoryDef];
    }
    categoryOffset.Offset -= value;
  }

  public void RemoveStatOffset(string key, VehicleStatDef vehicleStatDef)
  {
    StatOffset statOffset;
    if (!this.statOffsets.TryGetValue(vehicleStatDef, out statOffset))
    {
      this.statOffsets[vehicleStatDef] = new StatOffset(this.vehicle, vehicleStatDef);
      statOffset = this.statOffsets[vehicleStatDef];
    }
    statOffset.RemoveOverride(key);
  }

  public void RemoveStatOffset(string key, StatUpgradeCategoryDef upgradeCategoryDef)
  {
    StatOffset categoryOffset;
    if (!this.categoryOffsets.TryGetValue(upgradeCategoryDef, out categoryOffset))
    {
      this.categoryOffsets[upgradeCategoryDef] = new StatOffset(this.vehicle, upgradeCategoryDef);
      categoryOffset = this.categoryOffsets[upgradeCategoryDef];
    }
    categoryOffset.RemoveOverride(key);
  }

  public float GetStatOffset(VehicleStatDef vehicleStatDef)
  {
    StatOffset statOffset;
    return this.statOffsets.TryGetValue(vehicleStatDef, out statOffset) ? statOffset.Offset : 0.0f;
  }

  public float GetStatOffset(StatUpgradeCategoryDef upgradeCategoryDef, float value)
  {
    StatOffset statOffset;
    if (!this.categoryOffsets.TryGetValue(upgradeCategoryDef, out statOffset))
      return value;
    float num;
    return statOffset.TryGetOverride(out num) ? num : value + statOffset.Offset;
  }

  public float GetStatValue(VehicleStatDef vehicleStatDef) => this.statCache[vehicleStatDef];

  public void AddUpgradeableStatValue(StatDef statDef, float value)
  {
    StatOffset baseStatOffset;
    if (!this.baseStatOffsets.TryGetValue(statDef, out baseStatOffset))
    {
      this.baseStatOffsets[statDef] = new StatOffset(this.vehicle, statDef);
      baseStatOffset = this.baseStatOffsets[statDef];
    }
    baseStatOffset.Offset += value;
  }

  public void SubtractUpgradeableStatValue(StatDef statDef, float value)
  {
    StatOffset baseStatOffset;
    if (!this.baseStatOffsets.TryGetValue(statDef, out baseStatOffset))
    {
      this.baseStatOffsets[statDef] = new StatOffset(this.vehicle, statDef);
      baseStatOffset = this.baseStatOffsets[statDef];
    }
    baseStatOffset.Offset -= value;
  }

  public void SetUpgradeableStatValue(string key, StatDef statDef, float value)
  {
    StatOffset baseStatOffset;
    if (!this.baseStatOffsets.TryGetValue(statDef, out baseStatOffset))
    {
      this.baseStatOffsets[statDef] = new StatOffset(this.vehicle, statDef);
      baseStatOffset = this.baseStatOffsets[statDef];
    }
    baseStatOffset.AddOverride(key, value);
  }

  public void RemoveUpgradeableStatValue(string key, StatDef statDef)
  {
    StatOffset baseStatOffset;
    if (!this.baseStatOffsets.TryGetValue(statDef, out baseStatOffset))
    {
      this.baseStatOffsets[statDef] = new StatOffset(this.vehicle, statDef);
      baseStatOffset = this.baseStatOffsets[statDef];
    }
    baseStatOffset.RemoveOverride(key);
  }

  public float GetUpgradeableStatValue(StatDef statDef)
  {
    StatOffset statOffset;
    return this.baseStatOffsets.TryGetValue(statDef, out statOffset) ? statOffset.Offset : 0.0f;
  }

  public void MarkStatDirty(VehicleStatDef statDef)
  {
    if (!this.CanDirty)
      return;
    this.statCache.MarkDirty(statDef);
  }

  public void MarkAllDirty()
  {
    if (!this.CanDirty)
      return;
    this.statCache.Reset();
    this.RecalculateHealthPercent();
  }

  private void RecacheStatCategories(VehicleComponent component)
  {
    if (GenList.NullOrEmpty<VehicleStatDef>((IList<VehicleStatDef>) component.props.categories))
      return;
    foreach (VehicleStatDef category in component.props.categories)
    {
      List<VehicleComponent> vehicleComponentList;
      if (this.statComponents.TryGetValue(category, out vehicleComponentList))
        vehicleComponentList.Add(component);
      else
        this.statComponents[category] = new List<VehicleComponent>()
        {
          component
        };
    }
  }

  public float GetComponentHealth(string key)
  {
    VehicleComponent vehicleComponent;
    if (this.componentsByKeys.TryGetValue(key, out vehicleComponent))
      return vehicleComponent.Health;
    Log.Error($"Unable to locate component {key} in stat handler.");
    return 0.0f;
  }

  public void SetComponentHealth(string key, float value)
  {
    VehicleComponent vehicleComponent;
    if (!this.componentsByKeys.TryGetValue(key, out vehicleComponent))
    {
      Log.Error($"Unable to locate component {key} in stat handler.");
    }
    else
    {
      vehicleComponent.SetHealth(value);
      this.MarkAllDirty();
      this.vehicle.EventRegistry[VehicleEventDefOf.HealthChanged].ExecuteEvents();
    }
  }

  public VehicleComponent GetComponent(string key)
  {
    return GenCollection.TryGetValue<string, VehicleComponent>((IReadOnlyDictionary<string, VehicleComponent>) this.componentsByKeys, key, (VehicleComponent) null);
  }

  public float GetComponentHealthPercent(string key)
  {
    VehicleComponent vehicleComponent;
    if (this.componentsByKeys.TryGetValue(key, out vehicleComponent))
      return vehicleComponent.HealthPercent;
    Log.Error($"Unable to locate component {key} in stat handler.");
    return 0.0f;
  }

  public void SetComponentHealthPercent(string key, float value)
  {
    VehicleComponent vehicleComponent;
    if (!this.componentsByKeys.TryGetValue(key, out vehicleComponent))
    {
      Log.Error($"Unable to locate component {key} in stat handler.");
    }
    else
    {
      vehicleComponent.SetHealth(vehicleComponent.MaxHealth * value);
      this.MarkAllDirty();
      this.vehicle.EventRegistry[VehicleEventDefOf.HealthChanged].ExecuteEvents();
    }
  }

  public float StatEfficiency(VehicleStatDef statDef)
  {
    List<VehicleComponent> vehicleComponentList;
    if (!this.statComponents.TryGetValue(statDef, out vehicleComponentList))
      return 1f;
    float num1 = this.EfficiencyFor(statDef.operationType, (IEnumerable<VehicleComponent>) vehicleComponentList);
    float num2 = this.EfficiencyFor(statDef.operationType, vehicleComponentList.Where<VehicleComponent>((Func<VehicleComponent, bool>) (component => component.props.priorityStatEfficiency)));
    return (double) num2 < (double) num1 ? num2 : num1;
  }

  private float EfficiencyFor(
    EfficiencyOperationType operationType,
    IEnumerable<VehicleComponent> components)
  {
    if (GenCollection.EnumerableNullOrEmpty<VehicleComponent>(components))
      return 1f;
    float num;
    switch (operationType)
    {
      case EfficiencyOperationType.None:
        num = 1f;
        break;
      case EfficiencyOperationType.Sum:
        num = components.Sum<VehicleComponent>((Func<VehicleComponent, float>) (c => c.Efficiency)).Clamp(0.0f, 1f);
        break;
      case EfficiencyOperationType.MinValue:
        num = components.Min<VehicleComponent>((Func<VehicleComponent, float>) (c => c.Efficiency));
        break;
      case EfficiencyOperationType.MaxValue:
        num = components.Max<VehicleComponent>((Func<VehicleComponent, float>) (c => c.Efficiency));
        break;
      default:
        num = GenCollection.AverageWeighted<VehicleComponent>(components, (Func<VehicleComponent, float>) (c => c.props.efficiencyWeight), (Func<VehicleComponent, float>) (c => c.Efficiency));
        break;
    }
    return num;
  }

  public void InitializeHitboxCells()
  {
    this.componentLocations.Clear();
    CellRect hitbox = this.vehicle.Hitbox;
    foreach (IntVec2 key in ((CellRect) ref hitbox).Cells2D)
      this.componentLocations.Add(key, new List<VehicleComponent>());
    foreach (VehicleComponent component in this.components)
    {
      foreach (IntVec2 key in component.props.hitbox.Hitbox)
      {
        List<VehicleComponent> vehicleComponentList;
        if (!this.componentLocations.TryGetValue(key, out vehicleComponentList))
          SmashLog.Error($"Unable to add to internal component list for <field>{key}</field>. Component = {component.props.key}");
        else
          vehicleComponentList.Add(component);
      }
    }
  }

  public void RegisterImpacter(DamageInfo dinfo, IntVec3 cell)
  {
    if (((DamageInfo) ref dinfo).Instigator == null)
      return;
    this.RegisterImpacter(((DamageInfo) ref dinfo).Instigator, cell);
  }

  public IntVec3 RegisterImpacter(Thing thing, IntVec3 cell)
  {
    CellRect cellRect = GenAdj.OccupiedRect((Thing) this.vehicle);
    if (!((CellRect) ref cellRect).Contains(cell))
      cell = GenCollection.MinBy<IntVec3, float>((IEnumerable<IntVec3>) (object) cellRect, (Func<IntVec3, float>) (c => Ext_Map.Distance(c, cell)));
    this.impacter[thing] = cell;
    return cell;
  }

  public void DeregisterImpacter(Thing thing)
  {
    if (thing == null)
      return;
    this.impacter.Remove(thing);
  }

  public static IntVec2 AdjustFromVehiclePosition(VehiclePawn vehicle, IntVec2 cell)
  {
    if (!((Thing) vehicle).Spawned)
      return cell;
    int num1 = cell.x - ((Thing) vehicle).Position.x;
    int num2 = cell.z - ((Thing) vehicle).Position.z;
    IntVec2 intVec2;
    // ISSUE: explicit constructor call
    ((IntVec2) ref intVec2).\u002Ector(num1, num2);
    return intVec2;
  }

  private void RecalculateHealthPercent()
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    foreach (VehicleComponent component in this.components)
    {
      num1 += component.Health;
      num2 += component.MaxHealth;
    }
    this.HealthPercent = num1 / num2;
  }

  public void TakeDamage(DamageInfo dinfo)
  {
    IntVec3 intVec3;
    if (((DamageInfo) ref dinfo).Instigator == null || !this.impacter.TryGetValue(((DamageInfo) ref dinfo).Instigator, out intVec3))
    {
      if (((DamageInfo) ref dinfo).Instigator != null)
      {
        intVec3 = GenCollection.MinBy<IntVec3, float>((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect((Thing) this.vehicle), (Func<IntVec3, float>) (cell => Ext_Map.Distance(((DamageInfo) ref dinfo).Instigator.Position, cell)));
      }
      else
      {
        CellRect cellRect = GenAdj.OccupiedRect((Thing) this.vehicle);
        intVec3 = ((CellRect) ref cellRect).RandomCell;
      }
    }
    IntVec2 hitCell = VehicleStatHandler.AdjustFromVehiclePosition(this.vehicle, ((IntVec3) ref intVec3).ToIntVec2).RotatedBy(((Thing) this.vehicle).Rotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
    this.ApplyDamage(dinfo, hitCell);
  }

  public void TakeDamage(DamageInfo dinfo, IntVec2 hitCellPreRotate)
  {
    IntVec2 hitCell = hitCellPreRotate.RotatedBy(((Thing) this.vehicle).Rotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
    this.ApplyDamage(dinfo, hitCell);
  }

  public void ApplyDamage(DamageInfo dinfo, IntVec2 hitCell)
  {
    StringBuilder report = VehicleMod.settings.debug.debugLogging ? new StringBuilder() : (StringBuilder) null;
    this.ApplyDamageToComponent(dinfo, hitCell, report);
    this.DeregisterImpacter(((DamageInfo) ref dinfo).Instigator);
    Debug.Message(Gen.ToStringSafe<StringBuilder>(report));
  }

  private void ApplyDamageToComponent(DamageInfo dinfo, IntVec2 hitCell, StringBuilder report = null)
  {
    DamageDef def = ((DamageInfo) ref dinfo).Def;
    float num1 = ((DamageInfo) ref dinfo).Amount;
    if (def.workerClass == typeof (DamageWorker_Extinguish))
      this.TryExtinguishFire(dinfo, hitCell);
    if (!def.harmsHealth)
      num1 = 0.0f;
    try
    {
      report?.AppendLine("-- DAMAGE REPORT --");
      report?.AppendLine($"Base Damage: {num1}");
      report?.AppendLine($"DamageDef: {((DamageInfo) ref dinfo).Def}");
      report?.AppendLine($"HitCell: {hitCell}");
      VehicleDamageMultiplierDefModExtension modExtension1 = ((Def) ((DamageInfo) ref dinfo).Weapon)?.GetModExtension<VehicleDamageMultiplierDefModExtension>();
      if (modExtension1 != null)
      {
        num1 *= modExtension1.multiplier;
        report?.AppendLine($"ModExtension Multiplier: {modExtension1.multiplier} Result: {num1}");
      }
      VehicleDamageMultiplierDefModExtension modExtension2 = ((Def) ((DamageInfo) ref dinfo).Instigator?.def).GetModExtension<VehicleDamageMultiplierDefModExtension>();
      if (modExtension2 != null)
      {
        num1 *= modExtension2.multiplier;
        report?.AppendLine($"ModExtension Multiplier: {modExtension2.multiplier} Result: {num1}");
      }
      float num2;
      if (!GenDictionary.NullOrEmpty<DamageDef, float>((Dictionary<DamageDef, float>) this.vehicle.VehicleDef.properties.damageDefMultipliers) && this.vehicle.VehicleDef.properties.damageDefMultipliers.TryGetValue(((DamageInfo) ref dinfo).Def, out num2))
      {
        num1 *= num2;
        report?.AppendLine($"DamageDef Multiplier: {num2} Result: {num1}");
      }
      float num3;
      if (((DamageInfo) ref dinfo).Def.isRanged)
      {
        num3 = num1 * VehicleMod.settings.main.rangedDamageMultiplier;
        report?.AppendLine($"Settings Multiplier: {VehicleMod.settings.main.rangedDamageMultiplier} Result: {num3}");
      }
      else if (((DamageInfo) ref dinfo).Def.isExplosive)
      {
        num3 = num1 * VehicleMod.settings.main.explosiveDamageMultiplier;
        report?.AppendLine($"Settings Multiplier: {VehicleMod.settings.main.explosiveDamageMultiplier} Result: {num3}");
      }
      else
      {
        num3 = num1 * VehicleMod.settings.main.meleeDamageMultiplier;
        report?.AppendLine($"Settings Multiplier: {VehicleMod.settings.main.meleeDamageMultiplier} Result: {num3}");
      }
      if (def == DamageDefOf.EMP)
        this.ElectrifyAllComponents(ref dinfo);
      if ((double) num3 <= 0.0)
      {
        report?.AppendLine($"Final Damage = {num3}. Exiting.");
      }
      else
      {
        ((DamageInfo) ref dinfo).SetAmount(num3);
        Rot4 dir = this.DirectionFromAngle(((DamageInfo) ref dinfo).Angle);
        VehicleComponent.VehiclePartDepth hitDepth = VehicleComponent.VehiclePartDepth.External;
        for (int index = 0; index < Mathf.Max(((BuildableDef) this.vehicle.VehicleDef).Size.x, ((BuildableDef) this.vehicle.VehicleDef).Size.z) && !((Thing) this.vehicle).Destroyed && (double) ((DamageInfo) ref dinfo).Amount > 0.0; ++index)
        {
          VehicleComponent vehicleComponent = (VehicleComponent) null;
          report?.AppendLine($"Damaging = {hitCell}");
          List<VehicleComponent> source1;
          if (this.componentLocations.TryGetValue(hitCell, out source1))
          {
            report?.AppendLine($"components=({string.Join(",", source1.Select<VehicleComponent, string>((Func<VehicleComponent, string>) (c => c.props.label)))})");
            report?.AppendLine($"hitDepth = {hitDepth}");
            IEnumerable<VehicleComponent> source2 = source1.Where<VehicleComponent>((Func<VehicleComponent, bool>) (comp => comp.Depth == hitDepth && (double) comp.HealthPercent > 0.0));
            report?.AppendLine($"components at hitDepth {hitDepth}: ({string.Join(",", source2.Select<VehicleComponent, string>((Func<VehicleComponent, string>) (comp => comp.props.label)))})");
            if (!GenCollection.TryRandomElementByWeight<VehicleComponent>(source2, (Func<VehicleComponent, float>) (component => component.props.hitWeight), ref vehicleComponent))
            {
              report?.AppendLine("No components found. Hitting internal parts.");
              hitDepth = VehicleComponent.VehiclePartDepth.Internal;
              if (!GenCollection.TryRandomElementByWeight<VehicleComponent>(source1.Where<VehicleComponent>((Func<VehicleComponent, bool>) (comp => comp.Depth == hitDepth && (double) comp.HealthPercent > 0.0)), (Func<VehicleComponent, float>) (component => component.props.hitWeight), ref vehicleComponent))
              {
                vehicleComponent = GenCollection.RandomElementByWeightWithFallback<VehicleComponent>(this.components.Where<VehicleComponent>((Func<VehicleComponent, bool>) (comp => comp.Depth == hitDepth && (double) comp.HealthPercent > 0.0)), (Func<VehicleComponent, float>) (component => component.props.hitWeight), (VehicleComponent) null) ?? GenCollection.RandomElementByWeightWithFallback<VehicleComponent>(this.components.Where<VehicleComponent>((Func<VehicleComponent, bool>) (comp => (double) comp.HealthPercent > 0.0)), (Func<VehicleComponent, float>) (component => component.props.hitWeight), (VehicleComponent) null);
                if (vehicleComponent == null)
                  break;
              }
              else
                report?.AppendLine($"Found Internal Component {vehicleComponent.props.label} at {hitCell}");
            }
            else
              report?.AppendLine($"Found External Component {vehicleComponent.props.label} at {hitCell}");
          }
          else
          {
            report?.AppendLine("No components found. Hitting internal parts.");
            hitDepth = VehicleComponent.VehiclePartDepth.Internal;
            vehicleComponent = GenCollection.RandomElementByWeightWithFallback<VehicleComponent>(this.components.Where<VehicleComponent>((Func<VehicleComponent, bool>) (comp => comp.Depth == hitDepth && (double) comp.HealthPercent > 0.0)), (Func<VehicleComponent, float>) (component => component.props.hitWeight), (VehicleComponent) null) ?? GenCollection.RandomElementByWeightWithFallback<VehicleComponent>(this.components.Where<VehicleComponent>((Func<VehicleComponent, bool>) (comp => (double) comp.HealthPercent > 0.0)), (Func<VehicleComponent, float>) (component => component.props.hitWeight), (VehicleComponent) null);
            if (vehicleComponent == null)
              break;
          }
          if (!((IntVec2) ref hitCell).IsValid)
            break;
          if (VehicleMod.settings.debug.debugDrawHitbox)
          {
            IntVec2 cell = hitCell;
            if (Rot4.op_Inequality(((Thing) this.vehicle).Rotation, Rot4.North))
              cell = cell.MirrorRotatedBy(((Thing) this.vehicle).Rotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
            this.debugCellHighlight.Add(new Pair<IntVec2, int>(cell, 100));
          }
          report?.AppendLine($"Damaging {hitCell}");
          Pawn hitPawn;
          if (this.HitPawn(dinfo, hitDepth, hitCell, dir, out hitPawn))
          {
            if (report == null)
              break;
            report.AppendLine($"Hit {hitPawn} for {((DamageInfo) ref dinfo).Amount}. Impact site = {hitCell}");
            break;
          }
          report?.AppendLine($"Applying Damage = {((DamageInfo) ref dinfo).Amount} to {vehicleComponent.props.key} at {hitCell}");
          VehicleComponent.Penetration damage = vehicleComponent.TakeDamage(this.vehicle, ref dinfo);
          if (index == 0)
          {
            IntVec3 intVec3;
            // ISSUE: explicit constructor call
            ((IntVec3) ref intVec3).\u002Ector(((Thing) this.vehicle).Position.x + hitCell.x, 0, ((Thing) this.vehicle).Position.z + hitCell.z);
            if (((Thing) this.vehicle).Spawned && Compatibility_DamageIndicators.ModLoaded)
              VehicleStatHandler.ThrowDamageMote(((DamageInfo) ref dinfo).Amount, ((Thing) this.vehicle).Map, ((IntVec3) ref intVec3).ToVector3Shifted(), ((DamageInfo) ref dinfo).Amount.ToString("F0"));
            this.vehicle.Notify_DamageImpact(new VehicleComponent.DamageResult()
            {
              penetration = damage,
              damageInfo = dinfo,
              cell = hitCell
            });
          }
          report?.AppendLine($"Fallthrough Damage = {((DamageInfo) ref dinfo).Amount}");
        }
      }
    }
    finally
    {
      this.RecalculateHealthPercent();
    }
  }

  private static void ThrowDamageMote(float damage, Map map, Vector3 loc, string text)
  {
    if (Compatibility_DamageIndicators.ModLoaded && Compatibility_DamageIndicators.throwDamageMote != null)
    {
      Compatibility_DamageIndicators.throwDamageMote(damage, map, loc, text);
    }
    else
    {
      Color color = (double) damage >= 90.0 ? Color.cyan : ((double) damage >= 70.0 ? Color.magenta : ((double) damage >= 50.0 ? Color.red : ((double) damage >= 30.0 ? Color.Lerp(Color.red, Color.yellow, 0.5f) : ((double) damage >= 10.0 ? Color.yellow : Color.white))));
      MoteMaker.ThrowText(loc, map, text, color, 3.65f);
    }
  }

  private void ElectrifyAllComponents(ref DamageInfo dinfo)
  {
    bool flag = SettingsCache.TryGetValue<bool>(this.vehicle.VehicleDef, typeof (VehicleProperties), "canAdaptToEmp", this.vehicle.VehicleDef.properties.canAdaptToEmp);
    this.OverrideStunPatch = true;
    try
    {
      bool adapted = false;
      int num1 = 0;
      foreach (VehicleComponent component in this.components)
      {
        int num2 = component.ApplyEMPDamage(ref dinfo, ref adapted);
        if (num2 > num1)
          num1 = num2;
        if (flag && ((DamageInfo) ref dinfo).Def.stunAdaptationTicks > 0)
        {
          Dictionary<DamageDef, int> dictionary = (Dictionary<DamageDef, int>) VehicleStatHandler.AdaptationTicksLeft.GetValue((object) this.vehicle.stances.stunner);
          int num3;
          if (dictionary.TryGetValue(((DamageInfo) ref dinfo).Def, out num3) && num3 > 0)
          {
            adapted = true;
            break;
          }
          dictionary[((DamageInfo) ref dinfo).Def] = ((DamageInfo) ref dinfo).Def.stunAdaptationTicks;
        }
      }
      if (!adapted && num1 > 0)
      {
        VehicleStatHandler.StunFromEmp.SetValue((object) this.vehicle.stances.stunner, (object) true);
        this.vehicle.stances.stunner.StunFor(num1, ((DamageInfo) ref dinfo).Instigator, true, true, false);
      }
      if (!adapted || !((DamageInfo) ref dinfo).Def.displayAdaptedTextMote || Find.TickManager.TicksGame <= this.adaptTicks + 60)
        return;
      this.adaptTicks = Find.TickManager.TicksGame;
      MoteMaker.ThrowText(((Thing) this.vehicle).DrawPos, ((Thing) this.vehicle).Map, ((DamageInfo) ref dinfo).Def.adaptedText ?? TaggedString.op_Implicit(Translator.Translate("Adapted")), Color.white, -1f);
    }
    finally
    {
      this.OverrideStunPatch = false;
    }
  }

  private void TryExtinguishFire(DamageInfo damageInfo, IntVec2 hitCell)
  {
    HediffDef hediff = ((DamageInfo) ref damageInfo).Def.hediff;
    HediffDef coveredInFirefoam = HediffDefOf.CoveredInFirefoam;
    if (!(AttachmentUtility.GetAttachment((Thing) this.vehicle, ThingDefOf.Fire) is Fire attachment) || ((Thing) attachment).Destroyed)
      return;
    attachment.fireSize -= ((DamageInfo) ref damageInfo).Amount * 0.01f;
    if ((double) attachment.fireSize >= 0.10000000149011612)
      return;
    ((Thing) attachment).Destroy((DestroyMode) 0);
  }

  private bool HitPawn(
    DamageInfo dinfo,
    VehicleComponent.VehiclePartDepth hitDepth,
    IntVec2 cell,
    Rot4 dir,
    out Pawn hitPawn,
    StringBuilder report = null)
  {
    float num = 1f;
    hitPawn = (Pawn) null;
    VehicleRoleHandler handler;
    if (hitDepth == VehicleComponent.VehiclePartDepth.External)
    {
      num = 1.25f;
      this.TrySelectHandler(cell, out handler, true);
    }
    else if (this.TrySelectHandler(cell, out handler))
      num = 1.25f;
    else if (((Rot4) ref dir).IsValid)
    {
      if (this.TrySelectHandler(cell.Shifted(dir, 1), out handler))
        num = 1f;
      else if (this.TrySelectHandler(cell.Shifted(dir, 0, -1), out handler))
        num = 0.75f;
      else if (this.TrySelectHandler(cell.Shifted(dir, 0, 1), out handler))
        num = 0.75f;
      else if (this.TrySelectHandler(cell.Shifted(dir, 1, -1), out handler))
        num = 0.75f;
      else if (this.TrySelectHandler(cell.Shifted(dir, 1, 1), out handler))
        num = 0.75f;
    }
    if (handler == null || ((ThingOwner) handler.thingOwner).Count <= 0 || !Rand.Chance(handler.role.ChanceToHit * num))
      return false;
    hitPawn = GenCollection.RandomElement<Pawn>((IEnumerable<Pawn>) handler.thingOwner.InnerListForReading);
    report?.AppendLine($"Hitting {handler} with chance {(ValueType) (float) ((double) handler.role.ChanceToHit * (double) num)}");
    ((Thing) hitPawn).TakeDamage(dinfo);
    return true;
  }

  private bool TrySelectHandler(IntVec2 cell, out VehicleRoleHandler handler1, bool exposed = false)
  {
    handler1 = GenCollection.FirstOrDefault<VehicleRoleHandler>(this.vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler2 => handler2.role.Hitbox != null && handler2.role.Hitbox.Contains(cell) && ((ThingOwner) handler2.thingOwner).Count > 0 && handler2.role.Exposed == exposed));
    return handler1 != null;
  }

  private Rot4 DirectionFromAngle(float angle)
  {
    if ((double) angle < 0.0 || (double) angle > 360.0)
      return Rot4.Invalid;
    if ((double) angle > 45.0 && (double) angle <= 135.0)
      return Rot4.East;
    if ((double) angle > 135.0 && (double) angle <= 225.0)
      return Rot4.South;
    if ((double) angle > 225.0 && (double) angle <= 315.0)
      return Rot4.West;
    return (double) angle > 315.0 || (double) angle <= 45.0 ? Rot4.North : Rot4.Invalid;
  }

  public void DrawHitbox(VehicleComponent component)
  {
    if (component != null)
    {
      using (new ClearOnDispose<IntVec3>((ICollection<IntVec3>) VehicleStatHandler.HitboxHighlightCells))
      {
        if (!component.props.hitbox.Empty)
        {
          foreach (IntVec2 cell in component.props.hitbox.Hitbox)
          {
            IntVec2 intVec2 = cell.MirrorRotatedBy(((Thing) this.vehicle).Rotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
            VehicleStatHandler.HitboxHighlightCells.Add(new IntVec3(((Thing) this.vehicle).Position.x + intVec2.x, 0, ((Thing) this.vehicle).Position.z + intVec2.z));
          }
        }
        else if (component.Depth == VehicleComponent.VehiclePartDepth.External)
          VehicleStatHandler.HitboxHighlightCells.AddRange((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect((Thing) this.vehicle));
        if (VehicleStatHandler.HitboxHighlightCells.Count > 0)
          GenDraw.DrawFieldEdges(VehicleStatHandler.HitboxHighlightCells, component.highlightColor, new float?(), (HashSet<IntVec3>) null, 2900);
      }
    }
    if (!VehicleMod.settings.debug.debugDrawHitbox)
      return;
    for (int index = this.debugCellHighlight.Count - 1; index >= 0; --index)
    {
      GenDraw.DrawFieldEdges(new List<IntVec3>(1)
      {
        new IntVec3(((Thing) this.vehicle).Position.x + this.debugCellHighlight[index].First.x, 0, ((Thing) this.vehicle).Position.z + this.debugCellHighlight[index].First.z)
      }, Color.red, new float?(), (HashSet<IntVec3>) null, 2900);
      if (!Find.TickManager.Paused)
      {
        int num = this.debugCellHighlight[index].Second - 1;
        if (num <= 0)
          this.debugCellHighlight.RemoveAt(index);
        else
          this.debugCellHighlight[index] = new Pair<IntVec2, int>(this.debugCellHighlight[index].First, num);
      }
    }
  }

  public void ExposeData()
  {
    Scribe_Collections.Look<VehicleComponent>(ref this.components, "components", (LookMode) 2, new object[1]
    {
      (object) this.vehicle
    });
    if (Scribe.mode != 4 || GenList.NullOrEmpty<VehicleComponent>((IList<VehicleComponent>) this.components))
      return;
    for (int index = 0; index < this.components.Count; ++index)
    {
      VehicleComponent component1 = this.components[index];
      VehicleComponentProperties component2 = this.vehicle.VehicleDef.components[index];
      component1.Initialize(component2);
      this.componentsByKeys[component1.props.key] = component1;
      this.RecacheStatCategories(component1);
    }
  }

  void ITweakFields.OnFieldChanged() => this.MarkAllDirty();
}
