// Decompiled with JetBrains decompiler
// Type: Vehicles.RaidInjectionHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class RaidInjectionHelper
{
  public static VehicleCategory GetResolvedCategory(PawnGroupMakerParms parms)
  {
    return VehicleCategory.Combat;
  }

  public static VehicleCategory GetResolvedCategory(IncidentParms parms) => VehicleCategory.Combat;

  public static bool ValidRaiderVehicle(
    VehicleDef vehicleDef,
    VehicleCategory category,
    PawnsArrivalModeDef arrivalModeDef,
    Faction faction,
    float points)
  {
    return vehicleDef.type == VehicleType.Land && (vehicleDef.vehicleCategory & category) == category && (double) vehicleDef.combatPower <= (double) points && faction.def.techLevel >= vehicleDef.techLevel && (vehicleDef.enabled & VehicleEnabled.For.Raiders) != VehicleEnabled.For.None && vehicleDef.npcProperties != null && (vehicleDef.npcProperties.raidParams == null || vehicleDef.npcProperties.raidParams.Allows(faction, arrivalModeDef));
  }
}
