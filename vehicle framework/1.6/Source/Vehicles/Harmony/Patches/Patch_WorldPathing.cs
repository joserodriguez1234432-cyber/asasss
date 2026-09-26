// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_WorldPathing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools.Patching;
using System.Collections.Generic;
using System.Reflection;
using Vehicles.World;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal class Patch_WorldPathing : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldSelector), "AutoOrderToTileNow", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_WorldPathing), "AutoOrderVehicleCaravanPathing", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan_PathFollower), "StartPath", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_WorldPathing), "StartVehicleCaravanPath", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldRoutePlanner), "DoRoutePlannerButton", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldPathing), "VehicleRoutePlannerButton", (System.Type[]) null));
  }

  private static bool AutoOrderVehicleCaravanPathing(Caravan c, PlanetTile tile)
  {
    if (!(c is VehicleCaravan caravan))
      return true;
    if (PlanetTile.op_Implicit(tile) < 0 || PlanetTile.op_Equality(tile, ((WorldObject) caravan).Tile) && !caravan.vehiclePather.Moving || GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) caravan.VehiclesListForReading))
      return false;
    foreach (VehiclePawn vehiclePawn in caravan.VehiclesListForReading)
    {
      if (!WorldVehiclePathGrid.Instance.Passable(tile, vehiclePawn.VehicleDef) || vehiclePawn.VehicleDef.type == VehicleType.Air)
        return false;
    }
    int num = PlanetTile.op_Implicit(WorldHelper.BestGotoDestForVehicle(caravan, tile));
    if (num >= 0)
    {
      caravan.vehiclePather.StartPath(PlanetTile.op_Implicit(num), (CaravanArrivalAction) null, true);
      caravan.gotoMote.OrderedToTile(PlanetTile.op_Implicit(num));
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ColonistOrdered, (Map) null);
    }
    return false;
  }

  private static bool StartVehicleCaravanPath(
    PlanetTile destTile,
    CaravanArrivalAction arrivalAction,
    Caravan ___caravan,
    bool repathImmediately = false,
    bool resetPauseStatus = true)
  {
    if (!(___caravan is VehicleCaravan vehicleCaravan))
      return true;
    vehicleCaravan.vehiclePather.StartPath(destTile, arrivalAction, repathImmediately, resetPauseStatus);
    return false;
  }

  private static void VehicleRoutePlannerButton(ref float curBaseY)
  {
    Find.World.GetComponent<VehicleRoutePlanner>()?.DoRoutePlannerButton(ref curBaseY);
  }
}
