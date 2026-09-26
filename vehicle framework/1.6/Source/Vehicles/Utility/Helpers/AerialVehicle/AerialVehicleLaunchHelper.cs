// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AerialVehicleLaunchHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class AerialVehicleLaunchHelper
{
  public static AerialVehicleInFlight GetOrMakeAerialVehicle(this VehiclePawn vehicle)
  {
    if (vehicle.CompVehicleLauncher == null)
    {
      Trace.Fail($"Trying to launch {vehicle} which is not launchable.");
      return (AerialVehicleInFlight) null;
    }
    VehicleWorldObjectsHolder component = Find.World.GetComponent<VehicleWorldObjectsHolder>();
    AerialVehicleInFlight makeAerialVehicle = component.AerialVehicleObject(vehicle);
    if (makeAerialVehicle == null)
    {
      VehicleCaravan vehicleCaravan = component.VehicleCaravanObject(vehicle);
      if (vehicleCaravan == null)
      {
        Log.Error("Unable to launch aerial vehicle to empty tile. No existing aerial vehicle or caravan found to launch from.");
        return (AerialVehicleInFlight) null;
      }
      makeAerialVehicle = AerialVehicleInFlight.Create(vehicle, ((WorldObject) vehicleCaravan).Tile);
      bool flag = Find.WorldSelector.SelectedObjects.Contains((WorldObject) vehicleCaravan);
      for (int index = ((ThingOwner) vehicleCaravan.pawns).Count - 1; index >= 0; --index)
      {
        Pawn pawn = vehicleCaravan.pawns.InnerListForReading[index];
        if (!pawn.InVehicle() && vehicle.TryAddPawn(pawn))
          vehicleCaravan.RemovePawn(pawn);
      }
      vehicleCaravan.RemovePawn((Pawn) vehicle);
      if (flag)
        Find.WorldSelector.Select((WorldObject) makeAerialVehicle, false);
    }
    return makeAerialVehicle;
  }

  [DebugAction(null, null, false, false, false, false, false, 0, false)]
  private static void LockCameraToThing()
  {
    Map currentMap = Find.CurrentMap;
    if (currentMap == null)
    {
      Log.Error("Attempting to use LockCameraToThing with null map.");
    }
    else
    {
      IntVec3 intVec3 = UI.MouseCell();
      if (!GenGrid.InBounds(intVec3, currentMap))
        return;
      List<Thing> thingList = currentMap.thingGrid.ThingsListAtFast(intVec3);
      if (GenList.NullOrEmpty<Thing>((IList<Thing>) thingList))
        return;
      List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
      foreach (Thing thing1 in thingList)
      {
        Thing thing = thing1;
        floatMenuOptionList.Add(new FloatMenuOption(((Entity) thing).Label, (Action) (() => CameraAttacher.Create(thing)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
      }
      Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
    }
  }
}
