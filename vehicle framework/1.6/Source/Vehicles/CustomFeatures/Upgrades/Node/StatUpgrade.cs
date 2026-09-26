// Decompiled with JetBrains decompiler
// Type: Vehicles.StatUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class StatUpgrade : Upgrade
{
  public List<StatUpgrade.StatDefUpgrade> stats;
  public List<StatUpgrade.VehicleStatDefUpgrade> vehicleStats;
  public List<StatUpgrade.StatCategoryUpgrade> statCategories;

  public override bool UnlockOnLoad => true;

  public override IEnumerable<UpgradeTextEntry> UpgradeDescription(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<StatUpgrade.StatDefUpgrade>((IList<StatUpgrade.StatDefUpgrade>) this.stats))
    {
      foreach (StatUpgrade.StatDefUpgrade stat in this.stats)
      {
        switch (stat.type)
        {
          case UpgradeType.Add:
            yield return new UpgradeTextEntry(TaggedString.op_Implicit(((Def) stat.def).LabelCap), stat.ValueFormatted, stat.value, UpgradeEffectType.Positive);
            continue;
          case UpgradeType.Set:
            yield return new UpgradeTextEntry(TaggedString.op_Implicit(((Def) stat.def).LabelCap), stat.value.ToString());
            continue;
          default:
            continue;
        }
      }
    }
    if (!GenList.NullOrEmpty<StatUpgrade.VehicleStatDefUpgrade>((IList<StatUpgrade.VehicleStatDefUpgrade>) this.vehicleStats))
    {
      foreach (StatUpgrade.VehicleStatDefUpgrade vehicleStat in this.vehicleStats)
      {
        switch (vehicleStat.type)
        {
          case UpgradeType.Add:
            yield return new UpgradeTextEntry(TaggedString.op_Implicit(vehicleStat.def.LabelCap), vehicleStat.ValueFormatted, vehicleStat.value, vehicleStat.def.upgradeEffectType);
            continue;
          case UpgradeType.Set:
            yield return new UpgradeTextEntry(TaggedString.op_Implicit(vehicleStat.def.LabelCap), vehicleStat.value.ToString());
            continue;
          default:
            continue;
        }
      }
    }
    if (!GenList.NullOrEmpty<StatUpgrade.StatCategoryUpgrade>((IList<StatUpgrade.StatCategoryUpgrade>) this.statCategories))
    {
      foreach (StatUpgrade.StatCategoryUpgrade statCategory in this.statCategories)
      {
        switch (statCategory.type)
        {
          case UpgradeType.Add:
            yield return new UpgradeTextEntry(TaggedString.op_Implicit(statCategory.def.LabelCap), statCategory.ValueFormatted, statCategory.value, statCategory.def.upgradeEffectType);
            continue;
          case UpgradeType.Set:
            yield return new UpgradeTextEntry(TaggedString.op_Implicit(statCategory.def.LabelCap), statCategory.value.ToString());
            continue;
          default:
            continue;
        }
      }
    }
  }

  public override void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    if (!GenList.NullOrEmpty<StatUpgrade.StatDefUpgrade>((IList<StatUpgrade.StatDefUpgrade>) this.stats))
    {
      foreach (StatUpgrade.StatDefUpgrade stat in this.stats)
      {
        switch (stat.type)
        {
          case UpgradeType.Add:
            vehicle.statHandler.AddUpgradeableStatValue(stat.def, stat.value);
            continue;
          case UpgradeType.Set:
            vehicle.statHandler.SetUpgradeableStatValue(this.node.key, stat.def, stat.value);
            continue;
          default:
            continue;
        }
      }
    }
    if (!GenList.NullOrEmpty<StatUpgrade.VehicleStatDefUpgrade>((IList<StatUpgrade.VehicleStatDefUpgrade>) this.vehicleStats))
    {
      foreach (StatUpgrade.VehicleStatDefUpgrade vehicleStat in this.vehicleStats)
      {
        switch (vehicleStat.type)
        {
          case UpgradeType.Add:
            vehicle.statHandler.AddStatOffset(vehicleStat.def, vehicleStat.value);
            continue;
          case UpgradeType.Set:
            vehicle.statHandler.SetStatOffset(this.node.key, vehicleStat.def, vehicleStat.value);
            continue;
          default:
            continue;
        }
      }
    }
    if (GenList.NullOrEmpty<StatUpgrade.StatCategoryUpgrade>((IList<StatUpgrade.StatCategoryUpgrade>) this.statCategories))
      return;
    foreach (StatUpgrade.StatCategoryUpgrade statCategory in this.statCategories)
    {
      switch (statCategory.type)
      {
        case UpgradeType.Add:
          vehicle.statHandler.AddStatOffset(statCategory.def, statCategory.value);
          continue;
        case UpgradeType.Set:
          vehicle.statHandler.SetStatOffset(this.node.key, statCategory.def, statCategory.value);
          continue;
        default:
          continue;
      }
    }
  }

  public override void Refund(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<StatUpgrade.StatDefUpgrade>((IList<StatUpgrade.StatDefUpgrade>) this.stats))
    {
      foreach (StatUpgrade.StatDefUpgrade stat in this.stats)
      {
        switch (stat.type)
        {
          case UpgradeType.Add:
            vehicle.statHandler.SubtractUpgradeableStatValue(stat.def, stat.value);
            continue;
          case UpgradeType.Set:
            vehicle.statHandler.RemoveUpgradeableStatValue(this.node.key, stat.def);
            continue;
          default:
            continue;
        }
      }
    }
    if (!GenList.NullOrEmpty<StatUpgrade.VehicleStatDefUpgrade>((IList<StatUpgrade.VehicleStatDefUpgrade>) this.vehicleStats))
    {
      foreach (StatUpgrade.VehicleStatDefUpgrade vehicleStat in this.vehicleStats)
      {
        switch (vehicleStat.type)
        {
          case UpgradeType.Add:
            vehicle.statHandler.SubtractStatOffset(vehicleStat.def, vehicleStat.value);
            continue;
          case UpgradeType.Set:
            vehicle.statHandler.RemoveStatOffset(this.node.key, vehicleStat.def);
            continue;
          default:
            continue;
        }
      }
    }
    if (GenList.NullOrEmpty<StatUpgrade.StatCategoryUpgrade>((IList<StatUpgrade.StatCategoryUpgrade>) this.statCategories))
      return;
    foreach (StatUpgrade.StatCategoryUpgrade statCategory in this.statCategories)
    {
      switch (statCategory.type)
      {
        case UpgradeType.Add:
          vehicle.statHandler.SubtractStatOffset(statCategory.def, statCategory.value);
          continue;
        case UpgradeType.Set:
          vehicle.statHandler.RemoveStatOffset(this.node.key, statCategory.def);
          continue;
        default:
          continue;
      }
    }
  }

  public class StatDefUpgrade
  {
    public StatDef def;
    public float value;
    public UpgradeType type;

    public string ValueFormatted
    {
      get
      {
        string valueFormatted = GenText.ToStringByStyle(this.value, this.def.toStringStyle, this.def.toStringNumberSense);
        if (this.def.toStringNumberSense != 2 && !GenText.NullOrEmpty(this.def.formatString))
          valueFormatted = string.Format(this.def.formatString, (object) valueFormatted);
        if (this.type == UpgradeType.Add && (double) this.value > 0.0)
          valueFormatted = "+" + valueFormatted;
        return valueFormatted;
      }
    }
  }

  public class VehicleStatDefUpgrade
  {
    public VehicleStatDef def;
    public float value;
    public UpgradeType type;

    public string ValueFormatted
    {
      get
      {
        return UpgradeTextEntry.FormatValue(this.value, this.type, this.def.toStringStyle, this.def.toStringNumberSense, this.def.formatString);
      }
    }
  }

  public class StatCategoryUpgrade
  {
    public StatUpgradeCategoryDef def;
    public float value;
    public UpgradeType type;

    public string ValueFormatted
    {
      get
      {
        return UpgradeTextEntry.FormatValue(this.value, this.type, this.def.toStringStyle, this.def.toStringNumberSense, this.def.formatString);
      }
    }
  }
}
