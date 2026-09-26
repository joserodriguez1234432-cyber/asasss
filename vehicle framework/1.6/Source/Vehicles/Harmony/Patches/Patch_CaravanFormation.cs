// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_CaravanFormation
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vehicles.World;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

internal class Patch_CaravanFormation : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanFormingUtility), "IsFormingCaravan", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanFormation), "IsFormingCaravanVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TransferableUtility), "CanStack", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanFormation), "CanStackVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GiveToPackAnimalUtility), "UsablePackAnimalWithTheMostFreeSpace", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanFormation), "UsableVehicleWithMostFreeSpace", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanExitMapUtility), "CanExitMapAndJoinOrCreateCaravanNow", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanFormation), "CanVehicleExitMapAndJoinOrCreateCaravanNow", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanExitMapUtility), "ExitMapAndJoinOrCreateCaravan", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanFormation), "ExitMapAndJoinOrCreateVehicleCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertySetter(typeof (Pawn_InventoryTracker), "UnloadEverything"), new HarmonyMethod(typeof (Patch_CaravanFormation), "VehiclesShouldntUnloadEverything", (System.Type[]) null));
  }

  private static bool IsFormingCaravanVehicle(Pawn p, ref bool __result)
  {
    Lord lord = LordUtility.GetLord(p);
    if (lord == null || !(lord.LordJob is LordJob_FormAndSendVehicles))
      return true;
    __result = true;
    return false;
  }

  private static void CanStackVehicle(Thing thing, ref bool __result)
  {
    if (!(thing is VehiclePawn))
      return;
    __result = false;
  }

  private static bool UsableVehicleWithMostFreeSpace(Pawn pawn, ref Pawn __result)
  {
    if (!CaravanHelper.IsFormingCaravanShipHelper(pawn) && !((Thing) pawn).Map.mapPawns.SpawnedPawnsInFaction(((Thing) pawn).Faction).HasVehicle())
      return true;
    __result = (Pawn) CaravanHelper.UsableVehicleWithTheMostFreeSpace(pawn);
    return false;
  }

  private static void CanVehicleExitMapAndJoinOrCreateCaravanNow(Pawn pawn, ref bool __result)
  {
    if (!(pawn is VehiclePawn vehiclePawn))
      return;
    __result = ((Thing) vehiclePawn).Spawned && ((Thing) vehiclePawn).Map.exitMapGrid.MapUsesExitGrid && (vehiclePawn.AllPawnsAboard.NotNullAndAny<Pawn>((Predicate<Pawn>) (p => p.IsColonist)) || CaravanHelper.FindCaravanToJoinForAllowingVehicles((Pawn) vehiclePawn) != null);
  }

  private static bool ExitMapAndJoinOrCreateVehicleCaravan(Pawn pawn, Rot4 exitDir)
  {
    if (pawn is VehiclePawn vehicle && CaravanHelper.OpportunistcallyCreatedAerialVehicle(vehicle, PlanetTile.op_Implicit(((Thing) pawn).Map.Tile)))
      return false;
    Caravan allowingVehicles1 = CaravanHelper.FindCaravanToJoinForAllowingVehicles(pawn);
    if (allowingVehicles1 == null)
    {
      AerialVehicleInFlight allowingVehicles2 = CaravanHelper.FindAerialVehicleToJoinForAllowingVehicles(pawn);
      if (allowingVehicles2 != null)
      {
        VehicleRoleHandler handler1 = GenCollection.FirstOrDefault<VehicleRoleHandler>(allowingVehicles2.Vehicle.handlers, (Predicate<VehicleRoleHandler>) (handler => handler.AreSlotsAvailable));
        if (handler1 != null)
        {
          allowingVehicles2.Vehicle.TryAddPawn(pawn, handler1);
          return false;
        }
      }
    }
    if (allowingVehicles1 is VehicleCaravan vehicleCaravan1 && (vehicle == null || ((Thing) vehicle).IsBoat() == ((Thing) vehicleCaravan1.LeadVehicle).IsBoat()))
    {
      CaravanHelper.AddVehicleCaravanExitTaleIfShould(pawn);
      vehicleCaravan1.AddPawn(pawn, true);
      pawn.ExitMap(false, exitDir);
      return false;
    }
    if (vehicle == null)
      return true;
    Map map = ((Thing) pawn).Map;
    int num = PlanetTile.op_Implicit(CaravanHelper.FindRandomStartingTileBasedOnExitDir(vehicle, PlanetTile.op_Implicit(map.Tile), exitDir));
    VehicleCaravan vehicleCaravan2 = CaravanHelper.ExitMapAndCreateVehicleCaravan(Gen.YieldSingle<Pawn>(pawn), ((Thing) pawn).Faction, map.Tile, PlanetTile.op_Implicit(num), PlanetTile.op_Implicit(-1));
    vehicleCaravan2.autoJoinable = true;
    if (allowingVehicles1 != null)
    {
      ((ThingOwner) allowingVehicles1.pawns).TryTransferAllToContainer((ThingOwner) vehicleCaravan2.pawns, true);
      ((WorldObject) allowingVehicles1).Destroy();
      ((Caravan) vehicleCaravan2).Notify_Merged(new List<Caravan>(1)
      {
        allowingVehicles1
      });
    }
    bool flag = false;
    foreach (Pawn pawn1 in (IEnumerable<Pawn>) map.mapPawns.AllPawnsSpawned)
    {
      if (CaravanHelper.FindCaravanToJoinForAllowingVehicles(pawn1) != null && !pawn1.Downed && !pawn1.Drafted)
      {
        if (pawn1.RaceProps.Animal)
          flag = true;
        RestUtility.WakeUp(pawn1, true);
        pawn1.jobs.CheckForJobOverride(0.0f, true);
      }
    }
    TaggedString taggedString1 = TranslatorFormattedStringExtensions.Translate("MessagePawnLeftMapAndCreatedCaravan", NamedArgument.op_Implicit(((Entity) pawn).LabelShort), NamedArgument.op_Implicit((Thing) pawn));
    TaggedString taggedString2 = ((TaggedString) ref taggedString1).CapitalizeFirst();
    if (flag)
      taggedString2 = TaggedString.op_Addition(taggedString2, TaggedString.op_Addition(" ", Translator.Translate("MessagePawnLeftMapAndCreatedCaravan_AnimalsWantToJoin")));
    Messages.Message(TaggedString.op_Implicit(taggedString2), LookTargets.op_Implicit((WorldObject) allowingVehicles1), MessageTypeDefOf.TaskCompletion, true);
    return false;
  }

  private static void VehiclesShouldntUnloadEverything(ref bool value, Pawn ___pawn)
  {
    if (!(___pawn is VehiclePawn))
      return;
    value = false;
  }
}
