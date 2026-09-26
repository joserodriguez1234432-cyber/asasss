// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSkyfallerMaker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleSkyfallerMaker
{
  public static VehicleSkyfaller MakeSkyfaller(
    ThingDef def,
    VehicleDef vehicleDef,
    Faction faction,
    bool randomizeColors = false,
    bool randomizeMask = false,
    bool cleanSlate = true)
  {
    VehicleSkyfaller vehicleSkyfaller = (VehicleSkyfaller) ThingMaker.MakeThing(def, (ThingDef) null);
    vehicleSkyfaller.vehicle = VehicleSpawner.GenerateVehicle(new VehicleGenerationRequest(vehicleDef, faction, randomizeColors, randomizeMask, cleanSlate));
    return vehicleSkyfaller;
  }

  public static VehicleSkyfaller MakeSkyfaller(ThingDef def, VehiclePawn vehicle)
  {
    VehicleSkyfaller vehicleSkyfaller = (VehicleSkyfaller) ThingMaker.MakeThing(def, (ThingDef) null);
    vehicleSkyfaller.vehicle = vehicle;
    return vehicleSkyfaller;
  }

  public static VehicleSkyfaller_FlyOver MakeSkyfallerFlyOver(
    ThingDef def,
    VehiclePawn vehicle,
    IntVec3 start,
    IntVec3 end)
  {
    try
    {
      VehicleSkyfaller_FlyOver skyfallerFlyOver = (VehicleSkyfaller_FlyOver) VehicleSkyfallerMaker.MakeSkyfaller(def, vehicle);
      skyfallerFlyOver.start = start;
      skyfallerFlyOver.end = end;
      skyfallerFlyOver.angle = start.AngleToPoint(end);
      return skyfallerFlyOver;
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to generate VehicleSkyfaller of type <type>{def.thingClass}</type>. Exception=\"{ex}\"");
    }
    return (VehicleSkyfaller_FlyOver) null;
  }
}
