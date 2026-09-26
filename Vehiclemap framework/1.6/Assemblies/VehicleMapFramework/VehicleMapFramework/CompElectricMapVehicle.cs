// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompElectricMapVehicle
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

public class CompElectricMapVehicle : VehicleComp
{
  public CompFueledTravel CompFueledTravel
  {
    get
    {
      return this.\u003CCompFueledTravel\u003Ek__BackingField ?? (this.\u003CCompFueledTravel\u003Ek__BackingField = ((ThingWithComps) this.Vehicle).GetComp<CompFueledTravel>());
    }
  }

  public virtual bool TickByRequest => true;

  public virtual void CompTickInterval(int delta)
  {
    ((ThingComp) this).CompTickInterval(delta);
    if (ModCompat.VehicleFramework.connectedPower.Invoke(this.CompFueledTravel) != null || !(this.Vehicle is VehiclePawnWithMap vehicle) || ConnectPower(vehicle.VehicleMap, this.CompFueledTravel) || !ModCompat.CompatBase<ModCompat.MultiFloors>.Active)
      return;
    using (IEnumerator<Map> enumerator = ModCompat.MultiFloors.GetOtherLevels(vehicle.VehicleMap).GetEnumerator())
    {
      do
        ;
      while (enumerator.MoveNext() && !ConnectPower(enumerator.Current, this.CompFueledTravel));
    }

    static bool ConnectPower(Map map, CompFueledTravel comp)
    {
      foreach (PowerNet powerNet in map.powerNetManager.AllNetsListForReading)
      {
        float chargeRate = comp.Props.chargeRate;
        if ((double) powerNet.CurrentStoredEnergy() > (double) chargeRate || (double) powerNet.CurrentEnergyGainRate() > (double) chargeRate)
        {
          CompPower compPower = GenCollection.FirstOrDefault<CompPower>(powerNet.transmitters, (Predicate<CompPower>) (t => t.TransmitsPowerNow));
          if (compPower != null)
          {
            ModCompat.VehicleFramework.connectedPower.Invoke(comp) = compPower;
            return true;
          }
        }
      }
      return false;
    }
  }
}
