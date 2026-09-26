// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompBuildableUpgrades
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompBuildableUpgrades : ThingComp
{
  public List<UpgradeID> handlerUniqueIDs = new List<UpgradeID>();

  protected CompProperties_BuildableUpgrades Props => (CompProperties_BuildableUpgrades) this.props;

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    if (this.Props.syncWithPowerCondition)
    {
      if (!respawningAfterLoad)
        return;
      CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
      if (comp == null || !comp.PowerOn)
        return;
    }
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
        return;
      foreach (Upgrade upgrade in this.Props.upgrades)
      {
        if (upgrade is VehicleUpgradeBuildable upgradeBuildable2)
        {
          upgradeBuildable2.parent = this;
          ((Upgrade) upgradeBuildable2).Unlock((VehiclePawn) vehicle, respawningAfterLoad);
        }
        else
          upgrade.Unlock((VehiclePawn) vehicle, respawningAfterLoad);
      }
      vehicle.EventRegistry[VehicleEventDefOf.UpgradeCompleted].ExecuteEvents();
    }));
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    if (this.Props.syncWithPowerCondition)
    {
      CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
      if (comp != null && !comp.PowerOn)
        return;
    }
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return;
    foreach (Upgrade upgrade in this.Props.upgrades)
    {
      if (upgrade is VehicleUpgradeBuildable upgradeBuildable)
      {
        upgradeBuildable.parent = this;
        ((Upgrade) upgradeBuildable).Refund((VehiclePawn) vehicle);
      }
      else
        upgrade.Refund((VehiclePawn) vehicle);
    }
    vehicle.EventRegistry[VehicleEventDefOf.UpgradeRefundCompleted].ExecuteEvents();
    CompFueledTravel compFueledTravel;
    if ((compFueledTravel = vehicle.CompFueledTravel) == null)
      return;
    float num = compFueledTravel.Fuel - compFueledTravel.FuelCapacity;
    if ((double) num <= 0.0)
      return;
    compFueledTravel.ConsumeFuel(num);
  }

  public virtual void ReceiveCompSignal(string signal)
  {
    VehiclePawnWithMap vehicle;
    if (!this.Props.syncWithPowerCondition || !((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
      return;
    switch (signal)
    {
      case "PowerTurnedOn":
        foreach (Upgrade upgrade in this.Props.upgrades)
        {
          if (upgrade is VehicleUpgradeBuildable upgradeBuildable)
          {
            upgradeBuildable.parent = this;
            ((Upgrade) upgradeBuildable).Unlock((VehiclePawn) vehicle, false);
          }
          else
            upgrade.Unlock((VehiclePawn) vehicle, false);
        }
        vehicle.EventRegistry[VehicleEventDefOf.UpgradeCompleted].ExecuteEvents();
        break;
      case "PowerTurnedOff":
        foreach (Upgrade upgrade in this.Props.upgrades)
        {
          if (upgrade is VehicleUpgradeBuildable upgradeBuildable)
          {
            upgradeBuildable.parent = this;
            ((Upgrade) upgradeBuildable).Refund((VehiclePawn) vehicle);
          }
          else
            upgrade.Refund((VehiclePawn) vehicle);
        }
        vehicle.EventRegistry[VehicleEventDefOf.UpgradeRefundCompleted].ExecuteEvents();
        break;
    }
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompBuildableUpgrades buildableUpgrades = this;
    if (((Thing) buildableUpgrades.parent).IsOnVehicleMapOf(out VehiclePawnWithMap _))
    {
      List<Upgrade> turretRoleUpgrades = buildableUpgrades.Props.upgrades.Where<Upgrade>((Func<Upgrade, bool>) (u =>
      {
        if (!(u is VehicleUpgrade vehicleUpgrade2))
          return false;
        List<VehicleUpgrade.RoleUpgrade> roles = vehicleUpgrade2.roles;
        return roles != null && GenCollection.Any<VehicleUpgrade.RoleUpgrade>(roles, (Predicate<VehicleUpgrade.RoleUpgrade>) (r => r.handlingTypes.GetValueOrDefault() == 2));
      })).ToList<Upgrade>();
      if (turretRoleUpgrades.Count != 0)
      {
        CompVehicleTurrets compVehicleTurrets = vehicle.CompVehicleTurrets;
        VehicleTurret vehicleTurret = compVehicleTurrets != null ? compVehicleTurrets.Turrets.FirstOrDefault<VehicleTurret>((Func<VehicleTurret, bool>) (t => GenCollection.Any<UpgradeID>(this.handlerUniqueIDs, (Predicate<UpgradeID>) (h =>
        {
          List<string> turretIds1 = h.turretIds;
          // ISSUE: explicit non-virtual call
          if ((turretIds1 != null ? (__nonvirtual (turretIds1.Contains(t.key)) ? 1 : 0) : 0) != 0)
            return true;
          List<string> turretIds2 = h.turretIds;
          // ISSUE: explicit non-virtual call
          return turretIds2 != null && __nonvirtual (turretIds2.Contains(t.groupKey));
        })))) : (VehicleTurret) null;
        Command_Action commandAction = new Command_Action();
        commandAction.action = (Action) (() =>
        {
          foreach (Upgrade upgrade in turretRoleUpgrades)
          {
            if (upgrade is VehicleUpgradeBuildable upgradeBuildable3)
            {
              upgradeBuildable3.parent = this;
              ((Upgrade) upgradeBuildable3).Refund((VehiclePawn) vehicle);
            }
            else
              upgrade.Refund((VehiclePawn) vehicle);
          }
          foreach (Upgrade upgrade in turretRoleUpgrades)
          {
            if (upgrade is VehicleUpgradeBuildable upgradeBuildable4)
            {
              upgradeBuildable4.parent = this;
              ((Upgrade) upgradeBuildable4).Unlock((VehiclePawn) vehicle, false);
            }
            else
              upgrade.Unlock((VehiclePawn) vehicle, false);
          }
        });
        ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_Reassign"));
        ((Command) commandAction).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_ReassignDesc"));
        ((Command) commandAction).icon = (Texture) (vehicleTurret?.GizmoIcon ?? BaseContent.ClearTex);
        yield return (Gizmo) commandAction;
      }
    }
  }

  public virtual void PostExposeData()
  {
    Scribe_Collections.Look<UpgradeID>(ref this.handlerUniqueIDs, "handlerUniqueIDs", (LookMode) 2, Array.Empty<object>());
    if (Scribe.mode != 4 || this.handlerUniqueIDs != null)
      return;
    this.handlerUniqueIDs = new List<UpgradeID>();
  }
}
