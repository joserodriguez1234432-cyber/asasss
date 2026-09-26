// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.AerialVehicleCompatibility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable disable
namespace Vehicles.Compatibility;

public static class AerialVehicleCompatibility
{
  private static readonly Dictionary<System.Type, AerialVehicleCompatibility.Settings> WorldObjectSettings = new Dictionary<System.Type, AerialVehicleCompatibility.Settings>();

  public static void RegisterWorldObjectType(
    System.Type type,
    AerialVehicleCompatibility.Settings settings)
  {
    AerialVehicleCompatibility.WorldObjectSettings.Add(type, settings);
  }

  public static bool CanLandIn(MapParent mapParent)
  {
    bool flag;
    switch (mapParent)
    {
      case Site _:
      case EscapeShip _:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    if (flag || mapParent is SpaceMapParent)
      return true;
    AerialVehicleCompatibility.Settings settings;
    if (!AerialVehicleCompatibility.WorldObjectSettings.TryGetValue(mapParent.GetType(), out settings))
      return false;
    return settings.canLandInValidator == null ? settings.canLandIn : settings.canLandInValidator(mapParent);
  }

  public static bool ShouldClaimOnArrival(MapParent mapParent)
  {
    if (mapParent is EscapeShip)
      return true;
    AerialVehicleCompatibility.Settings settings;
    if (!AerialVehicleCompatibility.WorldObjectSettings.TryGetValue(mapParent.GetType(), out settings))
      return false;
    return settings.claimOnArrivalValidator == null ? settings.claimOnArrival : settings.claimOnArrivalValidator(mapParent);
  }

  public class Settings
  {
    public required bool canLandIn;
    public required bool claimOnArrival;
    public AerialVehicleCompatibility.Settings.CanLandIn canLandInValidator;
    public AerialVehicleCompatibility.Settings.CanLandIn claimOnArrivalValidator;

    public Settings()
    {
    }

    [SetsRequiredMembers]
    public Settings(bool canLandIn, bool claimOnArrival)
    {
      this.canLandIn = canLandIn;
      this.claimOnArrival = claimOnArrival;
    }

    public delegate bool CanLandIn(MapParent mapParent);

    public delegate bool ShouldClaimOnArrival(MapParent mapParent);
  }
}
