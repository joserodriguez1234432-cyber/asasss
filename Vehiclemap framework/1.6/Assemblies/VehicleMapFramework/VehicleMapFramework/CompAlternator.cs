// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompAlternator
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

public class CompAlternator : CompPowerPlant
{
  private CompProperties_Alternator Props => (CompProperties_Alternator) ((ThingComp) this).props;

  public virtual void UpdateDesiredPowerOutput()
  {
    base.UpdateDesiredPowerOutput();
    VehiclePawnWithMap vehicle;
    CompFueledTravel compFueledTravel;
    ThingDef fuelType;
    if (((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle) && (compFueledTravel = vehicle.CompFueledTravel) != null && (fuelType = compFueledTravel.Props?.fuelType) != null && !compFueledTravel.Props.ElectricPowered)
    {
      List<CompProperties_Alternator.FuelProperties> consumptionRates = this.Props.fuelConsumptionRates;
      CompProperties_Alternator.FuelProperties fuelProperties;
      float num;
      if ((fuelProperties = consumptionRates != null ? GenCollection.FirstOrDefault<CompProperties_Alternator.FuelProperties>(consumptionRates, (Predicate<CompProperties_Alternator.FuelProperties>) (f => f.fuelDef == fuelType)) : (CompProperties_Alternator.FuelProperties) null) != null && (double) compFueledTravel.Fuel >= (double) (num = fuelProperties.fuelConsumptionRate / 60000f))
      {
        if ((double) ((CompPowerTrader) this).PowerOutput <= 0.0)
          return;
        compFueledTravel.ConsumeFuel(num);
        return;
      }
    }
    ((CompPowerTrader) this).PowerOutput = 0.0f;
  }
}
