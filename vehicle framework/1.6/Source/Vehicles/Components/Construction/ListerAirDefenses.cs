// Decompiled with JetBrains decompiler
// Type: Vehicles.ListerAirDefenses
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public class ListerAirDefenses
{
  private readonly Dictionary<Faction, HashSet<Building_Artillery>> airDefensesToScan = new Dictionary<Faction, HashSet<Building_Artillery>>();

  public List<Building_Artillery> AllAirDefenses()
  {
    return this.airDefensesToScan.SelectMany<KeyValuePair<Faction, HashSet<Building_Artillery>>, Building_Artillery>((Func<KeyValuePair<Faction, HashSet<Building_Artillery>>, IEnumerable<Building_Artillery>>) (dict => (IEnumerable<Building_Artillery>) dict.Value)).ToList<Building_Artillery>();
  }

  public HashSet<Building_Artillery> AirDefensesForFaction(Faction faction)
  {
    HashSet<Building_Artillery> buildingArtillerySet;
    return this.airDefensesToScan.TryGetValue(faction, out buildingArtillerySet) ? buildingArtillerySet : new HashSet<Building_Artillery>();
  }

  public void Notify_AirDefenseSpawned(Building_Artillery airDefense)
  {
    HashSet<Building_Artillery> buildingArtillerySet;
    if (this.airDefensesToScan.TryGetValue(((Thing) airDefense).Faction, out buildingArtillerySet))
      buildingArtillerySet.Add(airDefense);
    else
      this.airDefensesToScan.Add(((Thing) airDefense).Faction, new HashSet<Building_Artillery>()
      {
        airDefense
      });
  }

  public void Notify_AirDefenseDespawned(Building_Artillery airDefense)
  {
    HashSet<Building_Artillery> buildingArtillerySet;
    if (!this.airDefensesToScan.TryGetValue(((Thing) airDefense).Faction, out buildingArtillerySet))
      return;
    buildingArtillerySet.Remove(airDefense);
  }
}
