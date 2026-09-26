// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRole
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleRole : ITweakFields
{
  public string key;
  public string label = "[MissingLabel]";
  public HandlingType handlingTypes;
  public int slots;
  public int slotsToOperate;
  public float comfort = 0.5f;
  public List<string> turretIds;
  public ComponentHitbox hitbox = new ComponentHitbox();
  public bool exposed;
  public float chanceToHit = 0.3f;
  [TweakField]
  public PawnOverlayRenderer pawnRenderer;
  private readonly List<VehicleUpgrade.RoleUpgrade> upgrades = new List<VehicleUpgrade.RoleUpgrade>();

  public VehicleRole()
  {
  }

  public VehicleRole(VehicleRoleHandler group)
    : this(group.role)
  {
  }

  public VehicleRole(VehicleRole reference)
  {
    if (string.IsNullOrEmpty(reference.key))
      Log.Error("Missing Key on VehicleRole " + reference.label);
    this.CopyFrom(reference);
  }

  public HandlingType HandlingTypes
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.handlingTypes.HasValue)
            return upgrade.handlingTypes.Value;
        }
      }
      return this.handlingTypes;
    }
  }

  public int Slots
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.slots.HasValue)
            return upgrade.slots.Value;
        }
      }
      return this.slots;
    }
  }

  public int SlotsToOperate
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        foreach (VehicleUpgrade.RoleUpgrade upgrade in this.upgrades)
        {
          if (upgrade.slotsToOperate.HasValue)
            return upgrade.slotsToOperate.Value;
        }
      }
      return this.slotsToOperate;
    }
  }

  public float Comfort
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        foreach (VehicleUpgrade.RoleUpgrade upgrade in this.upgrades)
        {
          if (upgrade.comfort.HasValue)
            return upgrade.comfort.Value;
        }
      }
      return this.comfort;
    }
  }

  public List<string> TurretIds
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.turretIds != null)
            return upgrade.turretIds;
        }
      }
      return this.turretIds;
    }
  }

  public ComponentHitbox Hitbox
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.hitbox != null)
            return upgrade.hitbox;
        }
      }
      return this.hitbox;
    }
  }

  public bool Exposed
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.exposed.HasValue)
            return upgrade.exposed.Value;
        }
      }
      return this.exposed;
    }
  }

  public float ChanceToHit
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.chanceToHit.HasValue)
            return upgrade.chanceToHit.Value;
        }
      }
      return this.chanceToHit;
    }
  }

  public PawnOverlayRenderer PawnRenderer
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.upgrades))
      {
        for (int index = this.upgrades.Count - 1; index >= 0; --index)
        {
          VehicleUpgrade.RoleUpgrade upgrade = this.upgrades[index];
          if (upgrade.pawnRenderer != null)
            return upgrade.pawnRenderer;
        }
      }
      return this.pawnRenderer;
    }
  }

  public bool RequiredForCaravan
  {
    get => this.slotsToOperate > 0 && (this.handlingTypes & HandlingType.Movement) != 0;
  }

  string ITweakFields.Category => "PawnOverlayRenderer";

  string ITweakFields.Label => this.label;

  public bool Resolved { get; private set; }

  public void AddUpgrade(VehicleUpgrade.RoleUpgrade roleUpgrade) => this.upgrades.Add(roleUpgrade);

  public void RemoveUpgrade(VehicleUpgrade.RoleUpgrade roleUpgrade)
  {
    this.upgrades.Remove(roleUpgrade);
  }

  public void CopyFrom(VehicleRole reference)
  {
    this.key = reference.key;
    this.label = reference.label;
    this.handlingTypes = reference.handlingTypes;
    this.slots = reference.slots;
    this.slotsToOperate = reference.slotsToOperate;
    this.turretIds = (List<string>) null;
    if (!GenList.NullOrEmpty<string>((IList<string>) reference.turretIds))
      this.turretIds = new List<string>((IEnumerable<string>) reference.turretIds);
    this.hitbox = reference.hitbox;
    this.exposed = reference.exposed;
    this.chanceToHit = reference.chanceToHit;
    this.pawnRenderer = reference.pawnRenderer;
  }

  public void CopyFrom(VehicleUpgrade.RoleUpgrade upgrade)
  {
    if (upgrade.handlingTypes.HasValue)
      this.handlingTypes = upgrade.handlingTypes.Value;
    if (upgrade.slots.HasValue)
      this.slots = upgrade.slots.Value;
    if (upgrade.slotsToOperate.HasValue)
      this.slotsToOperate = upgrade.slotsToOperate.Value;
    if (!GenList.NullOrEmpty<string>((IList<string>) upgrade.turretIds))
      this.turretIds = upgrade.turretIds.ToList<string>();
    if (upgrade.hitbox != null)
      this.hitbox = upgrade.hitbox;
    if (upgrade.exposed.HasValue)
      this.exposed = upgrade.exposed.Value;
    if (upgrade.chanceToHit.HasValue)
      this.chanceToHit = upgrade.chanceToHit.Value;
    this.pawnRenderer = upgrade.pawnRenderer;
  }

  public void ResolveReferences(VehicleDef vehicleDef)
  {
    if (this.Resolved)
      return;
    this.hitbox.Initialize(vehicleDef);
    this.Resolved = true;
  }

  void ITweakFields.OnFieldChanged()
  {
  }
}
