// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_MapPawns
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using SmashTools.Patching;
using System.Collections.Generic;
using System.Reflection;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_MapPawns : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (PawnsFinder), "AllCaravansAndTravellingTransporters_AliveOrDead"), postfix: new HarmonyMethod(typeof (Patch_MapPawns), "AllAerialVehicles_AliveOrDead", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (PawnsFinder), "AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction"), postfix: new HarmonyMethod(typeof (Patch_MapPawns), "AllMapsVehiclePassengers_Alive_OfPlayerFaction", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MapPawns), "PlayerEjectablePodHolder", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_MapPawns), "PlayerEjectableVehicles", (System.Type[]) null));
  }

  private static void AllAerialVehicles_AliveOrDead(ref List<Pawn> __result)
  {
    VehicleWorldObjectsHolder component = Find.World.GetComponent<VehicleWorldObjectsHolder>();
    if (component == null)
      return;
    foreach (AerialVehicleInFlight aerialVehicle in component.AerialVehicles)
      __result.AddRange((IEnumerable<Pawn>) aerialVehicle.Vehicle.AllPawnsAboard);
  }

  private static void AllMapsVehiclePassengers_Alive_OfPlayerFaction(ref List<Pawn> __result)
  {
    if (Current.ProgramState == null)
      return;
    foreach (Map map in Find.Maps)
    {
      foreach (VehiclePawn allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
      {
        if (((Thing) allClaimant).Faction == Faction.OfPlayer && allClaimant.AllPawnsAboard.Count != 0)
        {
          foreach (Pawn pawn in allClaimant.AllPawnsAboard)
          {
            if (((Thing) pawn).Faction == Faction.OfPlayer)
              __result.Add(pawn);
          }
        }
      }
    }
  }

  private static bool PlayerEjectableVehicles(Thing thing, ref IThingHolder __result)
  {
    if (!(thing is VehiclePawn vehiclePawn))
      return true;
    __result = (IThingHolder) vehiclePawn;
    return false;
  }
}
