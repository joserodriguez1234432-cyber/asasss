// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_GiveItemToVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class JobDriver_GiveItemToVehicle : JobDriverGetItemForVehicleBase
{
  protected override string ListerTag => "LoadVehicleForTurret";

  protected override IEnumerable<ThingDefCountClass> ThingsToLoad
  {
    get
    {
      return (IEnumerable<ThingDefCountClass>) JobDriver_GiveItemToVehicle.FindThingDefsToPack(this.Vehicle, this.pawn);
    }
  }

  protected override bool ShouldFailJob()
  {
    return MassUtility.IsOverEncumbered((Pawn) this.Vehicle) || base.ShouldFailJob();
  }

  [Profile]
  public static List<ThingDefCountClass> FindThingDefsToPack(VehiclePawn vehicle, Pawn pawn)
  {
    int num1 = Mathf.RoundToInt(vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity) - MassUtility.InventoryMass((Pawn) vehicle));
    ThingDefCountList countList = new ThingDefCountList();
    foreach (VehicleTurret turret in (IEnumerable<VehicleTurret>) vehicle.CompVehicleTurrets.Turrets)
    {
      ThingDef thingDef = turret.loadedAmmo;
      if (thingDef == null)
      {
        ThingFilter ammunition = turret.def.ammunition;
        thingDef = ammunition != null ? ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>() : (ThingDef) null;
      }
      if (thingDef != null)
      {
        int quotaLevel = vehicle.CompVehicleTurrets.GetQuotaLevel(turret);
        int num2 = Mathf.RoundToInt((float) num1 / Mathf.Max(StatExtension.GetStatValueAbstract((BuildableDef) thingDef, StatDefOf.Mass, (ThingDef) null), 0.1f));
        int num3 = vehicle.inventory.Count(thingDef);
        int num4 = quotaLevel - num3;
        if (num4 > 0)
        {
          int quota = Mathf.Min(num2, num4);
          JobDriver_GiveItemToVehicle.AddThingDefsToPackForTurret(vehicle, pawn, turret, quota, countList);
        }
      }
    }
    return countList.InnerListForReading;
  }

  [Profile]
  private static void AddThingDefsToPackForTurret(
    VehiclePawn vehicle,
    Pawn pawn,
    VehicleTurret turret,
    int quota,
    ThingDefCountList countList)
  {
    List<Thing> thingsToPack = JobDriver_GiveItemToVehicle.FindThingsToPack(vehicle, pawn, turret, quota);
    if (GenList.NullOrEmpty<Thing>((IList<Thing>) thingsToPack))
      return;
    foreach (Thing thing in thingsToPack)
    {
      if (quota <= 0)
        break;
      int num = Mathf.Min(quota, thing.stackCount);
      quota -= num;
      ThingDefCountClass countClass = countList.Find(thing.def);
      if (countClass == null)
      {
        countClass = new ThingDefCountClass(thing.def, 0);
        countList.Add(countClass);
      }
      countClass.count += num;
    }
  }

  [Profile]
  private static List<Thing> FindThingsToPack(
    VehiclePawn vehicle,
    Pawn pawn,
    VehicleTurret turret,
    int count)
  {
    return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, ((Thing) vehicle).Position, new IntRange(1, count), new Predicate<Thing>(IsValidThing));

    bool IsValidThing(Thing thing)
    {
      if (turret.def.ammunition == null)
        return false;
      return turret.loadedAmmo == null ? turret.def.ammunition.Allows(thing) : turret.loadedAmmo == thing.def;
    }
  }
}
