// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.RoleUpgradeBuildable
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class RoleUpgradeBuildable : VehicleUpgrade.RoleUpgrade
{
  public static VehicleRoleBuildable RoleFromUpgrade(
    RoleUpgradeBuildable upgrade,
    CompBuildableUpgrades compBuildableUpgrades,
    out RoleUpgradeBuildable upgrade2,
    List<string> turretIds = null)
  {
    ref RoleUpgradeBuildable local = ref upgrade2;
    RoleUpgradeBuildable upgradeBuildable1 = new RoleUpgradeBuildable();
    upgradeBuildable1.key = upgrade.key;
    upgradeBuildable1.label = upgrade.label;
    upgradeBuildable1.editKey = upgrade.editKey;
    upgradeBuildable1.remove = upgrade.remove;
    upgradeBuildable1.slots = upgrade.slots;
    upgradeBuildable1.slotsToOperate = upgrade.slotsToOperate;
    upgradeBuildable1.comfort = upgrade.comfort;
    upgradeBuildable1.turretIds = !GenList.NullOrEmpty<string>((IList<string>) turretIds) ? turretIds : upgrade.turretIds;
    upgradeBuildable1.hitbox = upgrade.hitbox;
    upgradeBuildable1.exposed = upgrade.exposed;
    upgradeBuildable1.chanceToHit = upgrade.chanceToHit;
    upgradeBuildable1.pawnRenderer = upgrade.pawnRenderer;
    local = upgradeBuildable1;
    upgrade2.handlingTypes = upgrade.handlingTypes.GetValueOrDefault() != 2 || !GenList.NullOrEmpty<string>((IList<string>) upgrade2.turretIds) ? upgrade.handlingTypes : new HandlingType?((HandlingType) 0);
    if (!GenList.NullOrEmpty<string>((IList<string>) upgrade2.turretIds))
    {
      RoleUpgradeBuildable upgradeBuildable2 = upgrade2;
      upgradeBuildable2.label = $"{upgradeBuildable2.label}: {GenText.ToCommaList(upgrade2.turretIds.Select<string, string>((Func<string, string>) (i => GenText.CapitalizeFirst(i))), false, false)}";
    }
    VehicleRoleBuildable vehicleRoleBuildable = new VehicleRoleBuildable();
    vehicleRoleBuildable.key = upgrade2.key;
    vehicleRoleBuildable.label = upgrade2.label;
    vehicleRoleBuildable.upgradeComp = compBuildableUpgrades;
    vehicleRoleBuildable.sourceRenderer = upgrade.pawnRenderer;
    VehicleRoleBuildable role = vehicleRoleBuildable;
    VehiclePawnWithMap vehicle;
    if (((Thing) compBuildableUpgrades.parent).IsOnVehicleMapOf(out vehicle))
    {
      PawnOverlayRenderer pawnRenderer = upgrade.pawnRenderer;
      if (pawnRenderer != null)
      {
        Rot4 rotation = ((Thing) compBuildableUpgrades.parent).Rotation;
        RoleUpgradeBuildable upgradeBuildable3 = upgrade2;
        PawnOverlayRenderer pawnOverlayRenderer = new PawnOverlayRenderer();
        pawnOverlayRenderer.showBody = pawnRenderer.showBody;
        pawnOverlayRenderer.north = new Rot4(((Rot4) ref pawnRenderer.north).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.east = new Rot4(((Rot4) ref pawnRenderer.east).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.south = new Rot4(((Rot4) ref pawnRenderer.south).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.west = new Rot4(((Rot4) ref pawnRenderer.west).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.northEast = new Rot4(((Rot4) ref pawnRenderer.northEast).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.southEast = new Rot4(((Rot4) ref pawnRenderer.southEast).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.southWest = new Rot4(((Rot4) ref pawnRenderer.southWest).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.northWest = new Rot4(((Rot4) ref pawnRenderer.northWest).AsInt + ((Rot4) ref rotation).AsInt);
        pawnOverlayRenderer.layer = pawnRenderer.layer;
        pawnOverlayRenderer.layerNorth = pawnRenderer.layerNorth;
        pawnOverlayRenderer.layerEast = pawnRenderer.layerEast;
        pawnOverlayRenderer.layerSouth = pawnRenderer.layerSouth;
        pawnOverlayRenderer.layerWest = pawnRenderer.layerWest;
        pawnOverlayRenderer.layerNorthEast = pawnRenderer.layerNorthEast;
        pawnOverlayRenderer.layerSouthEast = pawnRenderer.layerSouthEast;
        pawnOverlayRenderer.layerSouthWest = pawnRenderer.layerSouthWest;
        pawnOverlayRenderer.layerNorthWest = pawnRenderer.layerNorthWest;
        pawnOverlayRenderer.angle = pawnRenderer.angle;
        float? nullable1 = pawnRenderer.angleNorth;
        float? nullable2;
        float? nullable3;
        double num1;
        if (!nullable1.HasValue)
        {
          nullable2 = pawnRenderer.angleSouth;
          float num2 = 180f;
          nullable3 = nullable2.HasValue ? new float?(nullable2.GetValueOrDefault() + num2) : new float?();
          num1 = (double) nullable3 ?? (double) pawnRenderer.angle;
        }
        else
          num1 = (double) nullable1.GetValueOrDefault();
        pawnOverlayRenderer.angleNorth = new float?((float) num1);
        nullable1 = pawnRenderer.angleEast;
        double num3;
        if (!nullable1.HasValue)
        {
          nullable2 = pawnRenderer.angleWest;
          nullable3 = nullable2.HasValue ? new float?(-nullable2.GetValueOrDefault()) : new float?();
          num3 = (double) nullable3 ?? (double) pawnRenderer.angle;
        }
        else
          num3 = (double) nullable1.GetValueOrDefault();
        pawnOverlayRenderer.angleEast = new float?((float) num3);
        nullable1 = pawnRenderer.angleSouth;
        double num4;
        if (!nullable1.HasValue)
        {
          nullable2 = pawnRenderer.angleNorth;
          float num5 = 180f;
          nullable3 = nullable2.HasValue ? new float?(nullable2.GetValueOrDefault() + num5) : new float?();
          num4 = (double) nullable3 ?? (double) pawnRenderer.angle;
        }
        else
          num4 = (double) nullable1.GetValueOrDefault();
        pawnOverlayRenderer.angleSouth = new float?((float) num4);
        nullable1 = pawnRenderer.angleWest;
        double num6;
        if (!nullable1.HasValue)
        {
          nullable2 = pawnRenderer.angleEast;
          nullable3 = nullable2.HasValue ? new float?(-nullable2.GetValueOrDefault()) : new float?();
          num6 = (double) nullable3 ?? (double) pawnRenderer.angle;
        }
        else
          num6 = (double) nullable1.GetValueOrDefault();
        pawnOverlayRenderer.angleWest = new float?((float) num6);
        double num7;
        if (!((Rot4) ref rotation).IsHorizontal)
        {
          nullable1 = pawnRenderer.angleNorthEast;
          if (!nullable1.HasValue)
          {
            nullable3 = pawnRenderer.angleNorthWest;
            num7 = (double) nullable3 ?? (double) pawnRenderer.angle + 45.0;
          }
          else
            num7 = (double) nullable1.GetValueOrDefault();
        }
        else
        {
          nullable1 = Rot4.op_Equality(rotation, Rot4.West) ? pawnRenderer.angleWest : pawnRenderer.angleEast;
          num7 = (double) Ext_Math.RotateAngle(nullable1.GetValueOrDefault(), -45f);
        }
        pawnOverlayRenderer.angleNorthEast = new float?((float) num7);
        double num8;
        if (!((Rot4) ref rotation).IsHorizontal)
        {
          nullable1 = pawnRenderer.angleSouthEast;
          if (!nullable1.HasValue)
          {
            nullable3 = pawnRenderer.angleSouthWest;
            num8 = (double) nullable3 ?? (double) pawnRenderer.angle - 45.0;
          }
          else
            num8 = (double) nullable1.GetValueOrDefault();
        }
        else
        {
          nullable1 = Rot4.op_Equality(rotation, Rot4.East) ? pawnRenderer.angleWest : pawnRenderer.angleEast;
          num8 = (double) Ext_Math.RotateAngle(nullable1.GetValueOrDefault(), 45f);
        }
        pawnOverlayRenderer.angleSouthEast = new float?((float) num8);
        double num9;
        if (!((Rot4) ref rotation).IsHorizontal)
        {
          nullable1 = pawnRenderer.angleSouthWest;
          if (!nullable1.HasValue)
          {
            nullable3 = pawnRenderer.angleSouthEast;
            num9 = (double) nullable3 ?? (double) pawnRenderer.angle + 45.0;
          }
          else
            num9 = (double) nullable1.GetValueOrDefault();
        }
        else
        {
          nullable1 = Rot4.op_Equality(rotation, Rot4.East) ? pawnRenderer.angleWest : pawnRenderer.angleEast;
          num9 = (double) Ext_Math.RotateAngle(nullable1.GetValueOrDefault(), -45f);
        }
        pawnOverlayRenderer.angleSouthWest = new float?((float) num9);
        double num10;
        if (!((Rot4) ref rotation).IsHorizontal)
        {
          nullable1 = pawnRenderer.angleNorthWest;
          if (!nullable1.HasValue)
          {
            nullable3 = pawnRenderer.angleNorthEast;
            num10 = (double) nullable3 ?? (double) pawnRenderer.angle - 45.0;
          }
          else
            num10 = (double) nullable1.GetValueOrDefault();
        }
        else
        {
          nullable1 = Rot4.op_Equality(rotation, Rot4.West) ? pawnRenderer.angleWest : pawnRenderer.angleEast;
          num10 = (double) Ext_Math.RotateAngle(nullable1.GetValueOrDefault(), 45f);
        }
        pawnOverlayRenderer.angleNorthWest = new float?((float) num10);
        upgradeBuildable3.pawnRenderer = pawnOverlayRenderer;
        upgrade2.pawnRenderer.SetDrawOffsets(vehicle, role);
      }
      RoleUpgradeBuildable upgradeBuildable4 = upgrade2;
      if (upgradeBuildable4.hitbox == null)
      {
        RoleUpgradeBuildable upgradeBuildable5 = upgradeBuildable4;
        ComponentHitbox componentHitbox = new ComponentHitbox();
        CellRect cellRect1 = GenAdj.OccupiedRect((Thing) compBuildableUpgrades.parent);
        CellRect cellRect2 = ((CellRect) ref cellRect1).MovedBy(VehicleMapUtility.MapCellToHitbox(vehicle));
        componentHitbox.Hitbox = ((CellRect) ref cellRect2).Cells2D.ToList<IntVec2>();
        upgradeBuildable5.hitbox = componentHitbox;
      }
    }
    role.CopyFrom((VehicleUpgrade.RoleUpgrade) upgrade2);
    return role;
  }
}
