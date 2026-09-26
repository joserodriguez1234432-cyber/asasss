// Decompiled with JetBrains decompiler
// Type: Vehicles.World.CaravanGrouper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles.World;

internal static class CaravanGrouper
{
  public static List<CaravanGrouper.Group> ExtractIncompatibleCaravanGroups(
    List<VehiclePawn> vehicles,
    List<Pawn> pawns)
  {
    List<CaravanGrouper.Group> groups = new List<CaravanGrouper.Group>();
    foreach (VehiclePawn vehicle in vehicles)
    {
      if (!TryAddToGroup(groups, vehicle))
      {
        CaravanGrouper.Group group = new CaravanGrouper.Group()
        {
          Type = vehicle.VehicleDef.type
        };
        group.vehicles.Add(vehicle);
        groups.Add(group);
        if (group.Type == VehicleType.Land)
        {
          group.pawns.AddRange((IEnumerable<Pawn>) pawns);
          pawns.Clear();
        }
      }
    }
    return groups;

    static bool TryAddToGroup(List<CaravanGrouper.Group> groups, VehiclePawn vehicle)
    {
      if (vehicle.VehicleDef.type == VehicleType.Air)
        return false;
      foreach (CaravanGrouper.Group group in groups)
      {
        VehicleType? type1 = group.vehicles.FirstOrDefault<VehiclePawn>()?.VehicleDef.type;
        VehicleType type2 = vehicle.VehicleDef.type;
        if (type1.GetValueOrDefault() == type2 & type1.HasValue || vehicle.VehicleDef.type == VehicleType.Universal)
        {
          group.vehicles.Add(vehicle);
          return true;
        }
      }
      return false;
    }
  }

  public class Group
  {
    public readonly List<Pawn> pawns = new List<Pawn>();
    public readonly List<VehiclePawn> vehicles = new List<VehiclePawn>();

    public IEnumerable<Pawn> AllPawns => this.pawns.Concat<Pawn>((IEnumerable<Pawn>) this.vehicles);

    public VehicleType Type { get; init; }
  }
}
