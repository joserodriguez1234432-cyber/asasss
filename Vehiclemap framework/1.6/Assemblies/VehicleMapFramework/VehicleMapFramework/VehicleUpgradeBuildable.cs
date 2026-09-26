// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleUpgradeBuildable
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleUpgradeBuildable : VehicleUpgrade
{
  public CompBuildableUpgrades parent;

  public virtual void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.roles))
    {
      foreach (VehicleUpgrade.RoleUpgrade role in this.roles)
      {
        if (role is RoleUpgradeBuildable roleUpgrade)
        {
          if (!unlockingPostLoad && roleUpgrade.handlingTypes.HasValue && ((Enum) (object) roleUpgrade.handlingTypes.Value).HasFlag((Enum) (object) (HandlingType) 2))
            Find.WindowStack.Add((Window) new Dialog_ChooseVehicleRoles(vehicle, roleUpgrade, this));
          else
            this.UpgradeRole(vehicle, roleUpgrade, false, unlockingPostLoad);
        }
        else
          this.UpgradeRole(vehicle, role, false, unlockingPostLoad);
      }
    }
    if (this.retextureDef != null && !unlockingPostLoad)
      vehicle.SetRetexture(this.retextureDef);
    if (!GenList.NullOrEmpty<VehicleUpgrade.ArmorUpgrade>((IList<VehicleUpgrade.ArmorUpgrade>) this.armor))
    {
      foreach (VehicleUpgrade.ArmorUpgrade armorUpgrade in this.armor)
      {
        if (!GenText.NullOrEmpty(armorUpgrade.key) && !GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) armorUpgrade.statModifiers) && this.parent?.parent != null)
        {
          VehicleComponent component = vehicle.statHandler.GetComponent(armorUpgrade.key);
          UpgradeType type = armorUpgrade.type;
          if (type != null)
          {
            if (type == 1)
              component.SetArmorModifiers[((Thing) this.parent.parent).ThingID] = armorUpgrade.statModifiers;
          }
          else
            component.AddArmorModifiers[((Thing) this.parent.parent).ThingID] = armorUpgrade.statModifiers;
        }
      }
    }
    if (GenList.NullOrEmpty<VehicleUpgrade.HealthUpgrade>((IList<VehicleUpgrade.HealthUpgrade>) this.health))
      return;
    foreach (VehicleUpgrade.HealthUpgrade healthUpgrade in this.health)
    {
      if (!GenText.NullOrEmpty(healthUpgrade.key) && this.parent?.parent != null)
      {
        VehicleComponent component = vehicle.statHandler.GetComponent(healthUpgrade.key);
        if (healthUpgrade.value.HasValue)
        {
          UpgradeType type = healthUpgrade.type;
          if (type != null)
          {
            if (type == 1)
              component.SetHealthModifier = (float) healthUpgrade.value.Value;
          }
          else
            component.AddHealthModifiers[((Thing) this.parent.parent).ThingID] = (float) healthUpgrade.value.Value;
          component.SetHealth(component.MaxHealth);
        }
        if (healthUpgrade.depth.HasValue)
          component.depthOverride = healthUpgrade.depth;
      }
    }
  }

  public virtual void Refund(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) this.roles))
    {
      for (int index = this.roles.Count - 1; index >= 0; --index)
      {
        if (this.roles[index] is RoleUpgradeBuildable role)
          this.UpgradeRole(vehicle, role, true, false);
        else
          this.UpgradeRole(vehicle, this.roles[index], true, false);
      }
    }
    if (this.retextureDef != null)
      vehicle.SetRetexture((RetextureDef) null);
    if (!GenList.NullOrEmpty<VehicleUpgrade.ArmorUpgrade>((IList<VehicleUpgrade.ArmorUpgrade>) this.armor))
    {
      foreach (VehicleUpgrade.ArmorUpgrade armorUpgrade in this.armor)
      {
        if (!GenText.NullOrEmpty(armorUpgrade.key) && !GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) armorUpgrade.statModifiers) && this.parent?.parent != null)
        {
          VehicleComponent component = vehicle.statHandler.GetComponent(armorUpgrade.key);
          UpgradeType type = armorUpgrade.type;
          if (type != null)
          {
            if (type == 1)
              component.SetArmorModifiers.Remove(((Thing) this.parent.parent).ThingID);
          }
          else
            component.AddArmorModifiers.Remove(((Thing) this.parent.parent).ThingID);
        }
      }
    }
    if (GenList.NullOrEmpty<VehicleUpgrade.HealthUpgrade>((IList<VehicleUpgrade.HealthUpgrade>) this.health))
      return;
    foreach (VehicleUpgrade.HealthUpgrade healthUpgrade in this.health)
    {
      if (!GenText.NullOrEmpty(healthUpgrade.key) && this.parent?.parent != null)
      {
        VehicleComponent component = vehicle.statHandler.GetComponent(healthUpgrade.key);
        if (healthUpgrade.value.HasValue)
        {
          UpgradeType type = healthUpgrade.type;
          if (type != null)
          {
            if (type == 1)
              component.SetHealthModifier = -1f;
          }
          else
            component.AddHealthModifiers.Remove(((Thing) this.parent.parent).ThingID);
        }
        if (healthUpgrade.depth.HasValue)
          component.depthOverride = new VehicleComponent.VehiclePartDepth?();
      }
    }
  }

  public void UpgradeRole(
    VehiclePawn vehicle,
    RoleUpgradeBuildable roleUpgrade,
    bool isRefund,
    bool unlockingAfterLoad,
    List<string> turretIds = null)
  {
    if (roleUpgrade.remove ^ isRefund)
    {
      UpgradeID uniqueID = GenCollection.FirstOrDefault<UpgradeID>(this.parent.handlerUniqueIDs, (Predicate<UpgradeID>) (h => h.key == roleUpgrade.key && h.editKey == roleUpgrade.editKey));
      if (uniqueID == null)
      {
        VMF_Log.Error("No uniqueID corresponding to this role upgrade found.");
      }
      else
      {
        List<VehicleRoleHandler> handlers = vehicle.handlers;
        int index1 = handlers.FindIndex((Predicate<VehicleRoleHandler>) (h => h.uniqueID == uniqueID.id));
        if (index1 == -1)
        {
          VMF_Log.Error($"Unable to edit {roleUpgrade.editKey}. Matching VehicleRole not found.");
        }
        else
        {
          VehicleRoleHandler handler = handlers[index1];
          for (int index2 = ((ThingOwner) handler.thingOwner).Count - 1; index2 >= 0; --index2)
            vehicle.DisembarkPawn(handler.thingOwner[index2]);
          handler.role.RemoveUpgrade((VehicleUpgrade.RoleUpgrade) roleUpgrade);
          vehicle.handlers.RemoveAt(index1);
          this.parent.handlerUniqueIDs.RemoveAll((Predicate<UpgradeID>) (h => h.id == handler.uniqueID));
          vehicle.CompVehicleTurrets?.RecacheTurretPermissions();
        }
      }
    }
    else if (!unlockingAfterLoad)
    {
      RoleUpgradeBuildable upgrade2;
      VehicleRoleBuildable role = RoleUpgradeBuildable.RoleFromUpgrade(roleUpgrade, this.parent, out upgrade2, turretIds);
      role.ResolveReferences(vehicle.VehicleDef);
      role.AddUpgrade((VehicleUpgrade.RoleUpgrade) upgrade2);
      VehicleRoleHandlerBuildable handlerBuildable = new VehicleRoleHandlerBuildable(vehicle, role);
      vehicle.Handlers.Add((VehicleRoleHandler) handlerBuildable);
      if (role.PawnRenderer != null)
        vehicle.ResetRenderStatus();
      CompBuildableUpgrades parent = this.parent;
      (parent.handlerUniqueIDs ?? (parent.handlerUniqueIDs = new List<UpgradeID>())).Add(new UpgradeID(upgrade2.key, upgrade2.editKey, upgrade2.turretIds, handlerBuildable.uniqueID));
    }
    else
    {
      UpgradeID uniqueID = GenCollection.FirstOrDefault<UpgradeID>(this.parent.handlerUniqueIDs, (Predicate<UpgradeID>) (h => h.key == roleUpgrade.key && h.editKey == roleUpgrade.editKey));
      if (uniqueID == null)
      {
        VMF_Log.Error("No uniqueID corresponding to this role upgrade found.");
      }
      else
      {
        VehicleRoleHandler vehicleRoleHandler = GenCollection.FirstOrDefault<VehicleRoleHandler>(vehicle.handlers, (Predicate<VehicleRoleHandler>) (h => h.uniqueID == uniqueID.id));
        if (vehicleRoleHandler == null)
        {
          VMF_Log.Error($"Unable to edit {roleUpgrade.editKey}. Matching VehicleRole not found.");
        }
        else
        {
          RoleUpgradeBuildable upgrade2;
          VehicleRoleBuildable vehicleRoleBuildable = RoleUpgradeBuildable.RoleFromUpgrade(roleUpgrade, this.parent, out upgrade2, uniqueID.turretIds);
          vehicleRoleBuildable.ResolveReferences(vehicle.VehicleDef);
          vehicleRoleBuildable.AddUpgrade((VehicleUpgrade.RoleUpgrade) upgrade2);
          vehicleRoleHandler.role = (VehicleRole) vehicleRoleBuildable;
          if (vehicleRoleBuildable.PawnRenderer == null)
            return;
          vehicle.ResetRenderStatus();
        }
      }
    }
  }
}
