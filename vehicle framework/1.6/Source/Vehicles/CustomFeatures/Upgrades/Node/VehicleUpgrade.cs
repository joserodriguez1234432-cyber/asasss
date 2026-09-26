// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleUpgrade : Upgrade
{
  public List<VehicleUpgrade.ArmorUpgrade> armor;
  public List<VehicleUpgrade.HealthUpgrade> health;
  public List<VehicleUpgrade.RoleUpgrade> roles;
  public RetextureDef retextureDef;

  public override bool UnlockOnLoad => true;

  public override IEnumerable<UpgradeTextEntry> UpgradeDescription(VehiclePawn vehicle)
  {
    VehicleUpgrade vehicleUpgrade = this;
    if (!GenList.NullOrEmpty<VehicleUpgrade.ArmorUpgrade>((IList<VehicleUpgrade.ArmorUpgrade>) vehicleUpgrade.armor))
    {
      foreach (VehicleUpgrade.ArmorUpgrade armorUpgrade in vehicleUpgrade.armor)
      {
        if (!GenText.NullOrEmpty(armorUpgrade.key) && !GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) armorUpgrade.statModifiers))
        {
          VehicleComponent component = vehicle.statHandler.GetComponent(armorUpgrade.key);
          switch (armorUpgrade.type)
          {
            case UpgradeType.Add:
              IEnumerator<UpgradeTextEntry> enumerator = armorUpgrade.UpgradeEntries(vehicle, component).GetEnumerator();
              while (enumerator.MoveNext())
                yield return enumerator.Current;
              enumerator = (IEnumerator<UpgradeTextEntry>) null;
              continue;
            case UpgradeType.Set:
              component.SetArmorModifiers[vehicleUpgrade.node.key] = armorUpgrade.statModifiers;
              continue;
            default:
              continue;
          }
        }
      }
    }
  }

  public override void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.roles))
    {
      foreach (VehicleUpgrade.RoleUpgrade role in this.roles)
        this.UpgradeRole(vehicle, role, false, unlockingPostLoad);
    }
    if (this.retextureDef != null && !unlockingPostLoad)
      vehicle.SetRetexture(this.retextureDef);
    if (!GenList.NullOrEmpty<VehicleUpgrade.ArmorUpgrade>((IList<VehicleUpgrade.ArmorUpgrade>) this.armor))
    {
      foreach (VehicleUpgrade.ArmorUpgrade armorUpgrade in this.armor)
      {
        if (!GenText.NullOrEmpty(armorUpgrade.key) && !GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) armorUpgrade.statModifiers))
        {
          VehicleComponent component = vehicle.statHandler.GetComponent(armorUpgrade.key);
          switch (armorUpgrade.type)
          {
            case UpgradeType.Add:
              component.AddArmorModifiers[this.node.key] = armorUpgrade.statModifiers;
              continue;
            case UpgradeType.Set:
              component.SetArmorModifiers[this.node.key] = armorUpgrade.statModifiers;
              continue;
            default:
              continue;
          }
        }
      }
    }
    if (GenList.NullOrEmpty<VehicleUpgrade.HealthUpgrade>((IList<VehicleUpgrade.HealthUpgrade>) this.health))
      return;
    foreach (VehicleUpgrade.HealthUpgrade healthUpgrade in this.health)
    {
      if (!GenText.NullOrEmpty(healthUpgrade.key))
      {
        VehicleComponent component = vehicle.statHandler.GetComponent(healthUpgrade.key);
        if (healthUpgrade.value.HasValue)
        {
          switch (healthUpgrade.type)
          {
            case UpgradeType.Add:
              component.AddHealthModifiers[this.node.key] = (float) healthUpgrade.value.Value;
              break;
            case UpgradeType.Set:
              component.SetHealthModifier = (float) healthUpgrade.value.Value;
              break;
            default:
              throw new NotImplementedException("UpgradeType");
          }
          component.SetHealth(component.MaxHealth);
        }
        if (healthUpgrade.depth.HasValue)
          component.depthOverride = healthUpgrade.depth;
      }
    }
  }

  public override void Refund(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.roles))
    {
      foreach (VehicleUpgrade.RoleUpgrade role in this.roles)
        this.UpgradeRole(vehicle, role, true, false);
    }
    if (this.retextureDef != null)
      vehicle.SetRetexture((RetextureDef) null);
    if (!GenList.NullOrEmpty<VehicleUpgrade.ArmorUpgrade>((IList<VehicleUpgrade.ArmorUpgrade>) this.armor))
    {
      foreach (VehicleUpgrade.ArmorUpgrade armorUpgrade in this.armor)
      {
        if (!GenText.NullOrEmpty(armorUpgrade.key) && !GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) armorUpgrade.statModifiers))
        {
          VehicleComponent component = vehicle.statHandler.GetComponent(armorUpgrade.key);
          switch (armorUpgrade.type)
          {
            case UpgradeType.Add:
              component.AddArmorModifiers.Remove(this.node.key);
              continue;
            case UpgradeType.Set:
              component.SetArmorModifiers.Remove(this.node.key);
              continue;
            default:
              continue;
          }
        }
      }
    }
    if (GenList.NullOrEmpty<VehicleUpgrade.HealthUpgrade>((IList<VehicleUpgrade.HealthUpgrade>) this.health))
      return;
    foreach (VehicleUpgrade.HealthUpgrade healthUpgrade in this.health)
    {
      if (!GenText.NullOrEmpty(healthUpgrade.key))
      {
        VehicleComponent component = vehicle.statHandler.GetComponent(healthUpgrade.key);
        if (healthUpgrade.value.HasValue)
        {
          switch (healthUpgrade.type)
          {
            case UpgradeType.Add:
              component.AddHealthModifiers.Remove(this.node.key);
              break;
            case UpgradeType.Set:
              component.SetHealthModifier = -1f;
              break;
            default:
              throw new NotImplementedException("UpgradeType");
          }
          component.SetHealth(component.MaxHealth);
        }
        if (healthUpgrade.depth.HasValue)
          component.depthOverride = new VehicleComponent.VehiclePartDepth?();
      }
    }
  }

  public void UpgradeRole(
    VehiclePawn vehicle,
    VehicleUpgrade.RoleUpgrade roleUpgrade,
    bool isRefund,
    bool unlockingAfterLoad)
  {
    if (roleUpgrade.remove ^ isRefund)
    {
      VehicleRoleHandler handler = vehicle.GetHandler(roleUpgrade.key);
      if (!GenText.NullOrEmpty(roleUpgrade.editKey))
      {
        if (handler == null)
          Log.Error($"Unable to edit {roleUpgrade.editKey}. Matching VehicleRole not found.");
        else
          handler.role.RemoveUpgrade(roleUpgrade);
      }
      else
      {
        if (unlockingAfterLoad)
          return;
        if (handler == null)
          Log.Error($"Unable to remove {roleUpgrade.key} from {vehicle.Name}. Role not found.");
        else
          vehicle.RemoveRole(roleUpgrade.key);
      }
    }
    else
    {
      VehicleRoleHandler handler = vehicle.GetHandler(roleUpgrade.key);
      if (!GenText.NullOrEmpty(roleUpgrade.editKey))
      {
        if (handler == null)
          Log.Error($"Unable to edit {roleUpgrade.editKey}. Matching VehicleRole not found.");
        else
          handler.role.AddUpgrade(roleUpgrade);
      }
      else
      {
        if (unlockingAfterLoad)
          return;
        if (handler != null)
        {
          Log.Error("Attempting to create new role with existing key. If the upgrade is for modifying an existing role, an editKey must be specified.");
        }
        else
        {
          VehicleRole role = vehicle.VehicleDef.GetRole(roleUpgrade.key);
          if (role != null)
            vehicle.AddRole(new VehicleRole(role));
          else
            vehicle.AddRole(VehicleUpgrade.RoleUpgrade.RoleFromUpgrade(roleUpgrade));
        }
      }
    }
  }

  public struct ArmorUpgrade
  {
    public string key;
    public List<StatModifier> statModifiers;
    public UpgradeType type;

    public IEnumerable<UpgradeTextEntry> UpgradeEntries(
      VehiclePawn vehicle,
      VehicleComponent component)
    {
      if (!GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) this.statModifiers))
      {
        foreach (StatModifier statModifier in this.statModifiers)
        {
          string description = UpgradeTextEntry.FormatValue(statModifier.value, this.type, statModifier.stat.toStringStyle, statModifier.stat.toStringNumberSense, statModifier.stat.formatString);
          yield return new UpgradeTextEntry($"{((Def) statModifier.stat).LabelCap} ({component.props.label})", description, UpgradeEffectType.Positive);
        }
      }
    }
  }

  public struct HealthUpgrade
  {
    public string key;
    public int? value;
    public VehicleComponent.VehiclePartDepth? depth;
    public UpgradeType type;

    public IEnumerable<UpgradeTextEntry> UpgradeEntries(
      VehiclePawn vehicle,
      VehicleComponent component)
    {
      if (this.value.HasValue)
      {
        string description = UpgradeTextEntry.FormatValue((float) this.value.Value, this.type, (ToStringStyle) 0, (ToStringNumberSense) 1);
        yield return new UpgradeTextEntry(component.props.label ?? "", description, UpgradeEffectType.Positive);
      }
      if (this.depth.HasValue)
        yield return new UpgradeTextEntry(component.props.label ?? "", $"{Translator.Translate("Depth")} = {this.depth.Value.Translate()}");
    }
  }

  [PublicAPI]
  public class RoleUpgrade
  {
    public string key;
    public string label = "[MissingLabel]";
    public string editKey;
    public bool remove;
    public HandlingType? handlingTypes;
    public int? slots;
    public int? slotsToOperate;
    public float? comfort;
    public List<string> turretIds;
    public ComponentHitbox hitbox;
    public bool? exposed;
    public float? chanceToHit;
    public PawnOverlayRenderer pawnRenderer;

    public static VehicleRole RoleFromUpgrade(VehicleUpgrade.RoleUpgrade upgrade)
    {
      VehicleRole vehicleRole = new VehicleRole()
      {
        key = upgrade.key,
        label = upgrade.label
      };
      vehicleRole.CopyFrom(upgrade);
      return vehicleRole;
    }
  }
}
