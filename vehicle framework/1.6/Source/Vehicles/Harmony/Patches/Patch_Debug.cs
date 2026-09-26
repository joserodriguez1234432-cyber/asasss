// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Debug
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_Debug : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (DebugToolsSpawning), "SpawnPawn", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Debug), "DebugHideVehiclesFromPawnSpawner", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (HealthUtility), "DamageUntilDowned", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Debug), "DebugDamagePawnsInVehicleUntilDowned", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (HealthUtility), "DamageUntilDead", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Debug), "DebugDamagePawnsInVehicleUntilDead", (System.Type[]) null));
    if (!DebugProperties.Debug)
      return;
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldRoutePlanner), "WorldRoutePlannerUpdate", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Debug), "DebugSettlementPaths", (System.Type[]) null));
  }

  private static void TestPrefix(PlanetTile tile, WorldPath path, bool assumeCaravanMoving)
  {
    try
    {
      Log.Message($"CurrentTile: {tile} Path: {path == null} Moving: {assumeCaravanMoving}");
    }
    catch (Exception ex)
    {
      Log.Error($"[Test Prefix] Exception Thrown.\nException={ex}\nInnerException={ex.InnerException}\n");
    }
  }

  private static void TestPostfix(Dialog_Trade __instance, List<Tradeable> ___cachedTradeables)
  {
    try
    {
      TradeDeal deal = TradeSession.deal;
    }
    catch (Exception ex)
    {
      Log.Error($"[Test Postfix] Exception Thrown.\nException={ex}\nInnerException={ex.InnerException}\n");
    }
  }

  private static Exception ExceptionCatcher(Thing __instance, Exception __exception)
  {
    if (__exception != null)
      Log.Message($"Exception caught! Ex={__exception} Instance: {__instance}");
    return __exception;
  }

  private static void DebugHideVehiclesFromPawnSpawner(List<DebugActionNode> __result)
  {
    for (int index = __result.Count - 1; index >= 0; --index)
    {
      if (DefDatabase<PawnKindDef>.GetNamed(__result[index].label, true)?.race is VehicleDef)
        __result.RemoveAt(index);
    }
  }

  private static bool DebugDamagePawnsInVehicleUntilDowned(
    Pawn p,
    bool allowBleedingWounds,
    DamageDef damage,
    ThingDef sourceDef,
    BodyPartGroupDef bodyGroupDef)
  {
    if (!(p is VehiclePawn vehiclePawn))
      return true;
    Pawn pawn1 = GenCollection.RandomElementWithFallback<Pawn>(vehiclePawn.AllPawnsAboard.Where<Pawn>((Func<Pawn, bool>) (pawn => !pawn.Downed)), (Pawn) null);
    if (pawn1 != null)
      HealthUtility.DamageUntilDowned(pawn1, allowBleedingWounds, damage, sourceDef, bodyGroupDef);
    return false;
  }

  private static bool DebugDamagePawnsInVehicleUntilDead(
    Pawn p,
    DamageDef damage,
    ThingDef sourceDef,
    BodyPartGroupDef bodyGroupDef)
  {
    if (!(p is VehiclePawn vehiclePawn))
      return true;
    Pawn pawn1 = GenCollection.RandomElementWithFallback<Pawn>(vehiclePawn.AllPawnsAboard.Where<Pawn>((Func<Pawn, bool>) (pawn => !pawn.Dead)), (Pawn) null);
    if (pawn1 != null)
      HealthUtility.DamageUntilDead(pawn1, damage, sourceDef, bodyGroupDef);
    return false;
  }

  private static void DebugSettlementPaths()
  {
    if (!DebugProperties.DrawPaths || GenList.NullOrEmpty<WorldPath>((IList<WorldPath>) DebugHelper.debugLines))
      return;
    foreach (WorldPath debugLine in DebugHelper.debugLines)
      debugLine.DrawPath((Caravan) null);
  }

  [DebugAction("Vehicle Framework", "Draw Hitbox Size", false, false, false, false, false, 0, false)]
  private static void DebugDrawHitbox()
  {
    DebugTool tool = (DebugTool) null;
    IntVec3 first;
    tool = new DebugTool("first corner...", (Action) (() =>
    {
      first = UI.MouseCell();
      DebugTools.curTool = new DebugTool("second corner...", new Action(SecondCorner), first);
    }), (Action) null);
    DebugTools.curTool = tool;

    void SecondCorner()
    {
      CellRect cellRect1 = CellRect.FromLimits(first, UI.MouseCell());
      CellRect cellRect2 = ((CellRect) ref cellRect1).ClipInsideMap(Find.CurrentMap);
      IntVec3 intVec3_1 = cellRect2.ThingPositionFromRect();
      foreach (IntVec3 intVec3_2 in cellRect2)
      {
        IntVec3 intVec3_3 = IntVec3.op_Subtraction(intVec3_2, intVec3_1);
        Current.Game.CurrentMap.debugDrawer.FlashCell(intVec3_2, 0.75f, ((IntVec3) ref intVec3_3).ToIntVec2.ToString(), 3600);
      }
      DebugTools.curTool = tool;
    }
  }

  [DebugAction("Vehicle Framework", "Ground All Aerial Vehicles", false, false, false, false, false, 0, false)]
  private static void DebugGroundAllAerialVehicles()
  {
    foreach (AerialVehicleInFlight aerialVehicle in Find.World.GetComponent<VehicleWorldObjectsHolder>().AerialVehicles)
      Patch_Debug.DebugLandAerialVehicle(aerialVehicle);
    foreach (Map map in Find.Maps)
    {
      foreach (Thing thing in ((IEnumerable<Thing>) map.spawnedThings).ToList<Thing>())
      {
        if (thing is VehicleSkyfaller vehicleSkyfaller)
        {
          vehicleSkyfaller.vehicle.CompVehicleLauncher.launchProtocol.Release();
          vehicleSkyfaller.vehicle.CompVehicleLauncher.inFlight = false;
          GenSpawn.Spawn((Thing) vehicleSkyfaller.vehicle, vehicleSkyfaller.Position, vehicleSkyfaller.Map, vehicleSkyfaller.Rotation, (WipeMode) 0, false, false);
          if (VehicleMod.settings.main.deployOnLanding)
            vehicleSkyfaller.vehicle.CompVehicleLauncher.SetTimedDeployment();
          vehicleSkyfaller.Destroy((DestroyMode) 0);
        }
      }
    }
  }

  public static void DebugLandAerialVehicle(AerialVehicleInFlight aerialVehicleInFlight)
  {
    Settlement nearestSettlement = GenCollection.MinBy<Settlement, float>((IEnumerable<Settlement>) Find.WorldObjects.Settlements.Where<Settlement>((Func<Settlement, bool>) (s => ((WorldObject) s).Faction == Faction.OfPlayer)).ToList<Settlement>(), (Func<Settlement, float>) (s => Ext_Math.SphericalDistance(((WorldObject) s).DrawPos, ((WorldObject) aerialVehicleInFlight).DrawPos)));
    if (nearestSettlement == null)
    {
      Log.Error("Attempting to force land aerial vehicle without a valid settlement.");
    }
    else
    {
      Rot4 vehicleRotation = (Rot4?) aerialVehicleInFlight.Vehicle.CompVehicleLauncher.launchProtocol.LandingProperties?.forcedRotation ?? Rot4.Random;
      IntVec3 result;
      if (!CellFinderExtended.TryFindRandomCenterCell(((MapParent) nearestSettlement).Map, (Predicate<IntVec3>) (cell => !MapHelper.ImpassableOrVehicleBlocked(aerialVehicleInFlight.Vehicle, ((MapParent) nearestSettlement).Map, cell, vehicleRotation)), out result) && !CellFinderExtended.TryRadialSearchForCell(((MapParent) nearestSettlement).Map.Center, ((MapParent) nearestSettlement).Map, 50f, (Predicate<IntVec3>) (cell => !MapHelper.ImpassableOrVehicleBlocked(aerialVehicleInFlight.Vehicle, ((MapParent) nearestSettlement).Map, cell, vehicleRotation)), out result))
      {
        Log.Warning("Could not find cell to spawn aerial vehicle.  Picking random cell.");
        result = CellFinder.RandomCell(((MapParent) nearestSettlement).Map);
      }
      GenSpawn.Spawn((Thing) VehicleSkyfallerMaker.MakeSkyfaller(aerialVehicleInFlight.Vehicle.CompVehicleLauncher.Props.skyfallerIncoming, aerialVehicleInFlight.Vehicle), result, ((MapParent) nearestSettlement).Map, vehicleRotation, (WipeMode) 0, false, false);
      aerialVehicleInFlight.ClearAndDestroy();
    }
  }
}
