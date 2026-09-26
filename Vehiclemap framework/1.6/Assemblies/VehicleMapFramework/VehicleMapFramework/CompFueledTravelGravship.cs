// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompFueledTravelGravship
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompFueledTravelGravship : CompFueledTravel
{
  internal const float EfficiencyIdleMultiplier = 0.5f;

  private Building_GravEngine Engine
  {
    get
    {
      return this.\u003CEngine\u003Ek__BackingField ?? (this.\u003CEngine\u003Ek__BackingField = GravshipUtility.GetPlayerGravEngine_NewTemp(((VehicleComp) this).Vehicle is VehiclePawnWithMap vehicle ? vehicle.VehicleMap : (Map) null));
    }
  }

  public virtual float FuelCapacity
  {
    get
    {
      Building_GravEngine engine = this.Engine;
      return engine == null ? 0.0f : engine.MaxFuel;
    }
  }

  public virtual bool TickByRequest => false;

  private bool ShouldConsumeNow
  {
    get
    {
      if (this.EmptyTank || !((Thing) ((VehicleComp) this).Vehicle).Spawned)
        return false;
      return this.ConsumeWhenDrafted || this.ConsumeWhenMoving || this.ConsumeAlways;
    }
  }

  private bool ConsumeAlways
  {
    get
    {
      return ((Enum) (object) this.FuelCondition).HasFlag((Enum) (object) (FuelConsumptionCondition) 8);
    }
  }

  private bool ConsumeWhenDrafted
  {
    get
    {
      return ((Thing) ((VehicleComp) this).Vehicle).Spawned && ((Enum) (object) this.FuelCondition).HasFlag((Enum) (object) (FuelConsumptionCondition) 1) && ((VehicleComp) this).Vehicle.Drafted;
    }
  }

  private bool ConsumeWhenMoving
  {
    get
    {
      if (!((Enum) (object) this.FuelCondition).HasFlag((Enum) (object) (FuelConsumptionCondition) 2))
        return false;
      if (((Thing) ((VehicleComp) this).Vehicle).Spawned && ((VehicleComp) this).Vehicle.vehiclePather.Moving)
        return true;
      VehicleCaravan vehicleCaravan = Ext_Vehicles.GetVehicleCaravan((Pawn) ((VehicleComp) this).Vehicle);
      return vehicleCaravan != null && vehicleCaravan.vehiclePather.MovingNow;
    }
  }

  public virtual void Refuel(float amount)
  {
    if (this.Engine == null || (double) this.Engine.TotalFuel >= (double) this.Engine.MaxFuel)
      return;
    List<CompRefuelable> list = this.Engine.GravshipComponents.Where<CompGravshipFacility>((Func<CompGravshipFacility, bool>) (c => ((CompFacility) c).CanBeActive && c.Props.providesFuel)).Select<CompGravshipFacility, CompRefuelable>((Func<CompGravshipFacility, CompRefuelable>) (c => ((ThingComp) c).parent.GetComp<CompRefuelable>())).Where<CompRefuelable>((Func<CompRefuelable, bool>) (c => c != null)).OrderByDescending<CompRefuelable, float>((Func<CompRefuelable, float>) (c => c.Props.fuelCapacity - c.Fuel)).ToList<CompRefuelable>();
    float num1 = amount;
    for (int index1 = 0; index1 < list.Count - 1; ++index1)
    {
      float num2 = Mathf.Min(list[index1 + 1].Fuel - list[index1].Fuel, num1);
      if ((double) num2 >= (double) Mathf.Epsilon)
      {
        num1 -= num2;
        float num3 = num2 / (float) (index1 + 1);
        for (int index2 = 0; index2 <= index1; ++index2)
        {
          float num4 = Mathf.Min(num3, list[index2].Props.fuelCapacity - list[index2].Fuel);
          list[index2].Refuel(num4);
          num1 += num3 - num4;
        }
        if ((double) num1 < (double) Mathf.Epsilon)
          break;
      }
    }
    while ((double) num1 > (double) Mathf.Epsilon)
    {
      list.RemoveAll((Predicate<CompRefuelable>) (c => c.IsFull));
      if (!GenCollection.Empty<CompRefuelable>(list))
      {
        float num5 = num1 / (float) list.Count;
        num1 = 0.0f;
        foreach (CompRefuelable compRefuelable in list)
        {
          float num6 = Mathf.Min(num5, compRefuelable.Props.fuelCapacity - compRefuelable.Fuel);
          compRefuelable.Refuel(num6);
          num1 += num5 - num6;
        }
      }
      else
        break;
    }
    base.Refuel(amount);
  }

  public virtual void ConsumeFuel(float amount)
  {
    float num = amount / this.Engine.TotalFuel;
    foreach (CompRefuelable compRefuelable in this.Engine.GravshipComponents.Where<CompGravshipFacility>((Func<CompGravshipFacility, bool>) (compGravshipFacility => ((CompFacility) compGravshipFacility).CanBeActive && compGravshipFacility.Props.providesFuel)).Select<CompGravshipFacility, CompRefuelable>((Func<CompGravshipFacility, CompRefuelable>) (compGravshipFacility => ((ThingComp) compGravshipFacility).parent.GetComp<CompRefuelable>())))
      compRefuelable?.ConsumeFuel(compRefuelable.Fuel * num);
    base.ConsumeFuel(amount);
  }

  public virtual void ConsumeFuelWorld()
  {
    if ((double) this.Fuel <= 0.0)
      return;
    float rateWorldPerTick = this.ConsumptionRateWorldPerTick;
    if (!Ext_Vehicles.GetVehicleCaravan((Pawn) ((VehicleComp) this).Vehicle).vehiclePather.Moving)
      rateWorldPerTick *= 0.5f;
    base.ConsumeFuel(rateWorldPerTick);
  }

  public virtual void CompTick()
  {
    if (this.ShouldConsumeNow)
      base.CompTick();
    float? totalFuel = this.Engine?.TotalFuel;
    float fuel = this.Fuel;
    float valueOrDefault = (totalFuel.HasValue ? new float?(totalFuel.GetValueOrDefault() - fuel) : new float?()).GetValueOrDefault();
    if ((double) valueOrDefault < (double) Mathf.Epsilon)
      return;
    if ((double) valueOrDefault > 0.0)
      base.Refuel(valueOrDefault);
    else
      base.ConsumeFuel(valueOrDefault);
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    if (respawningAfterLoad)
      return;
    FrameDelay.DelayOne<CompFueledTravelGravship>((Action<CompFueledTravelGravship>) (instance =>
    {
      float? totalFuel = instance.Engine?.TotalFuel;
      float fuel = instance.Fuel;
      float valueOrDefault = (totalFuel.HasValue ? new float?(totalFuel.GetValueOrDefault() - fuel) : new float?()).GetValueOrDefault();
      if ((double) valueOrDefault < (double) Mathf.Epsilon)
        return;
      if ((double) valueOrDefault > 0.0)
        ((CompFueledTravel) instance).Refuel(valueOrDefault);
      else
        ((CompFueledTravel) instance).ConsumeFuel(valueOrDefault);
    }), this);
  }
}
