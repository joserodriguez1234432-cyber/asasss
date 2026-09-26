// Decompiled with JetBrains decompiler
// Type: Vehicles.StockGenerator_Vehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class StockGenerator_Vehicles : StockGenerator
{
  public VehicleCategory category;
  public HashSet<VehicleDef> excludedDefs;
  public float chance = 1f;

  public virtual IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
  {
    StockGenerator_Vehicles generatorVehicles = this;
    if ((double) generatorVehicles.chance >= 1.0 || Rand.Chance(generatorVehicles.chance))
    {
      List<VehicleDef> vehicleDefs = DefDatabase<VehicleDef>.AllDefsListForReading.Where<VehicleDef>(new Func<VehicleDef, bool>(((StockGenerator) generatorVehicles).HandlesThingDef)).ToList<VehicleDef>();
      if (vehicleDefs.Count != 0)
      {
        int count = ((IntRange) ref generatorVehicles.countRange).RandomInRange;
        for (int i = 0; i < count; ++i)
        {
          float priceRange = ((FloatRange) ref generatorVehicles.totalPriceRange).RandomInRange;
          yield return (Thing) VehicleSpawner.GenerateVehicle(GenCollection.RandomElementWithFallback<VehicleDef>(vehicleDefs.Where<VehicleDef>((Func<VehicleDef, bool>) (def => (double) StatExtension.GetStatValueAbstract((BuildableDef) def, StatDefOf.MarketValue, (ThingDef) null) <= (double) priceRange)), (VehicleDef) null) ?? GenCollection.RandomElement<VehicleDef>((IEnumerable<VehicleDef>) vehicleDefs), faction);
        }
      }
    }
  }

  public virtual bool HandlesThingDef(ThingDef thingDef)
  {
    return thingDef is VehicleDef vehicleDef && (GenCollection.NullOrEmpty<VehicleDef>(this.excludedDefs) || !this.excludedDefs.Contains(vehicleDef)) && thingDef.tradeability != null && (vehicleDef.vehicleCategory & this.category) == this.category;
  }
}
