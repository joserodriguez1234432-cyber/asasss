// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Construction
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal class Patch_Construction : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnGenerator), "GeneratePawn", new System.Type[1]
    {
      typeof (PawnGenerationRequest)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Construction), "GenerateVehiclePawn", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Frame), "CompleteConstruction", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Construction), "CompleteConstructionVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ListerBuildingsRepairable), "Notify_BuildingRepaired", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Construction), "Notify_RepairedVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GenSpawn), "Spawn", new System.Type[7]
    {
      typeof (Thing),
      typeof (IntVec3),
      typeof (Map),
      typeof (Rot4),
      typeof (WipeMode),
      typeof (bool),
      typeof (bool)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Construction), "RegisterThingSpawned", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Designator_Deconstruct), "CanDesignateThing", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Construction), "AllowDeconstructVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GenLeaving), "DoLeavingsFor", new System.Type[4]
    {
      typeof (Thing),
      typeof (Map),
      typeof (DestroyMode),
      typeof (List<Thing>)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Construction), "DoUnsupportedVehicleRefunds", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn), "Destroy", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_Construction), "ValidDestroyModeForVehicles", (System.Type[]) null));
  }

  private static bool GenerateVehiclePawn(PawnGenerationRequest request, ref Pawn __result)
  {
    if (!(((PawnGenerationRequest) ref request).KindDef?.race is VehicleDef race))
      return true;
    __result = (Pawn) VehicleSpawner.GenerateVehicle(race, ((PawnGenerationRequest) ref request).Faction);
    return false;
  }

  private static bool CompleteConstructionVehicle(Pawn worker, Frame __instance)
  {
    if (!(((Thing) __instance).def.entityDefToBuild is VehicleBuildDef entityDefToBuild) || entityDefToBuild.thingToSpawn == null)
      return true;
    VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(entityDefToBuild.thingToSpawn, ((Thing) worker).Faction);
    __instance.resourceContainer.ClearAndDestroyContents((DestroyMode) 0);
    Map map = ((Thing) __instance).Map;
    ((Thing) __instance).Destroy((DestroyMode) 0);
    SoundDef soundBuilt = entityDefToBuild.soundBuilt;
    if (soundBuilt != null)
      SoundStarter.PlayOneShot(soundBuilt, SoundInfo.op_Implicit(new TargetInfo(((Thing) __instance).Position, map, false)));
    ((Thing) vehicle).SetFaction(((Thing) worker).Faction, (Pawn) null);
    GenSpawn.Spawn((Thing) vehicle, ((Thing) __instance).Position, map, ((Thing) __instance).Rotation, (WipeMode) 1, false, false);
    worker.records.Increment(RecordDefOf.ThingsConstructed);
    if (!DebugSettings.godMode)
    {
      vehicle.Rename();
    }
    else
    {
      foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
      {
        if (allComp is VehicleComp vehicleComp)
          vehicleComp.SpawnedInGodMode();
      }
    }
    return false;
  }

  private static bool Notify_RepairedVehicle(Building b, ListerBuildingsRepairable __instance)
  {
    if (!(b is VehicleBuilding vehicleBuilding) || !(((Thing) b).def is VehicleBuildDef def) || def.thingToSpawn == null || ((Thing) b).HitPoints < ((Thing) b).MaxHitPoints)
      return true;
    Pawn pawn;
    if (vehicleBuilding.vehicle != null)
    {
      pawn = (Pawn) vehicleBuilding.vehicle;
      pawn.health.Reset();
    }
    else
      pawn = PawnGenerator.GeneratePawn(def.thingToSpawn.kindDef, (Faction) null, new PlanetTile?());
    Map map = ((Thing) b).Map;
    IntVec3 position = ((Thing) b).Position;
    Rot4 rotation = ((Thing) b).Rotation;
    AccessTools.Method(typeof (ListerBuildingsRepairable), "UpdateBuilding", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, new object[1]
    {
      (object) b
    });
    SoundDef soundBuilt = def.soundBuilt;
    if (soundBuilt != null)
      SoundStarter.PlayOneShot(soundBuilt, SoundInfo.op_Implicit(new TargetInfo(position, map, false)));
    if (((Thing) pawn).Faction != Faction.OfPlayer)
      ((Thing) pawn).SetFaction(Faction.OfPlayer, (Pawn) null);
    ((Thing) b).Destroy((DestroyMode) 0);
    ((Thing) pawn).ForceSetStateToUnspawned();
    GenSpawn.Spawn((Thing) pawn, position, map, rotation, (WipeMode) 1, false, false);
    return false;
  }

  private static bool RegisterThingSpawned(
    Thing newThing,
    ref IntVec3 loc,
    Map map,
    ref Rot4 rot,
    ref Thing __result,
    bool respawningAfterLoad)
  {
    if (newThing.def is VehicleBuildDef def && !VehicleMod.settings.debug.debugSpawnVehicleBuildingGodMode && newThing.HitPoints == newThing.MaxHitPoints && !respawningAfterLoad)
      return BuildVehicle(newThing, def, map, ref rot, ref loc, out __result);
    switch (newThing)
    {
      case VehiclePawn vehicle:
        __result = (Thing) vehicle;
        return PlaceVehicle(vehicle, map, ref rot, ref loc, respawningAfterLoad);
      case Pawn pawn:
        if (!pawn.Dead)
        {
          TryAdjustPawn(pawn, map, ref loc);
          break;
        }
        break;
    }
    return true;

    static bool BuildVehicle(
      Thing newThing,
      VehicleBuildDef buildDef,
      Map map,
      ref Rot4 rot,
      ref IntVec3 loc,
      out Thing __result)
    {
      VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(buildDef.thingToSpawn, newThing.Faction);
      SoundDef soundBuilt = buildDef.soundBuilt;
      if (soundBuilt != null)
        SoundStarter.PlayOneShot(soundBuilt, SoundInfo.op_Implicit(new TargetInfo(loc, map, false)));
      GenSpawn.Spawn((Thing) vehicle, loc, map, rot, (WipeMode) 1, false, false);
      if (!DebugSettings.godMode)
      {
        vehicle.Rename();
      }
      else
      {
        foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
        {
          if (allComp is VehicleComp vehicleComp)
            vehicleComp.SpawnedInGodMode();
        }
      }
      __result = (Thing) vehicle;
      return false;
    }

    static bool PlaceVehicle(
      VehiclePawn vehicle,
      Map map,
      ref Rot4 rot,
      ref IntVec3 loc,
      bool respawningAfterLoad)
    {
      if (!vehicle.VehicleDef.rotatable)
        rot = ((BuildableDef) vehicle.VehicleDef).defaultPlacingRot;
      VehiclePositionManager positionManager = map.GetDetachedMapComponent<VehiclePositionManager>();
      bool flag = true;
      CellRect cellRect1 = vehicle.PawnOccupiedCells(loc, rot);
      foreach (IntVec3 cell in cellRect1)
      {
        if (VehicleCanSpawnAt(vehicle, positionManager, map, in cell))
        {
          flag = false;
          break;
        }
      }
      if (flag)
      {
        if (!respawningAfterLoad)
          FinalizePosition(vehicle, rot, ref loc);
        return true;
      }
      Rot4 lambdaRot = rot;
      IntVec3 result;
      if (!CellFinderExtended.TryRadialSearchForCell(loc, map, 30f, (Predicate<IntVec3>) (cell =>
      {
        CellRect cellRect = vehicle.PawnOccupiedCells(cell, lambdaRot);
        foreach (IntVec3 cell1 in cellRect)
        {
          if (VehicleCanSpawnAt(vehicle, positionManager, map, in cell1))
            return false;
        }
        return true;
      }), out result))
      {
        Log.Error($"Unable to find location to spawn {((Entity) vehicle).LabelShort}. Performing wider search.");
        if (!CellFinderExtended.TryRadialSearchForCell(loc, map, 100f, (Predicate<IntVec3>) (cell =>
        {
          CellRect cellRect = vehicle.PawnOccupiedCells(cell, lambdaRot);
          foreach (IntVec3 intVec3 in cellRect)
          {
            if (!GenGrid.InBounds(intVec3, map))
              return false;
          }
          return true;
        }), out result))
        {
          Log.Error($"Unable to find location to spawn {((Entity) vehicle).LabelShort}. Aborting spawn.");
          return false;
        }
      }
      loc = result;
      if (!respawningAfterLoad)
        FinalizePosition(vehicle, rot, ref loc);
      return true;
    }

    static bool VehicleCanSpawnAt(
      VehiclePawn vehicle,
      VehiclePositionManager positionManager,
      Map map,
      in IntVec3 cell)
    {
      if (!GenGrid.InBounds(cell, map) || !cell.Walkable(vehicle.VehicleDef, map))
        return true;
      VehiclePawn vehiclePawn = positionManager.ClaimedBy(cell);
      return vehiclePawn != null && vehiclePawn != vehicle;
    }

    static void TryAdjustPawn(Pawn pawn, Map map, ref IntVec3 loc)
    {
      try
      {
        VehiclePositionManager detachedMapComponent = map.GetDetachedMapComponent<VehiclePositionManager>();
        if (!detachedMapComponent.PositionClaimed(loc))
          return;
        VehiclePawn vehiclePawn = detachedMapComponent.ClaimedBy(loc);
        CellRect cellRect1 = GenAdj.OccupiedRect((Thing) vehiclePawn);
        CellRect cellRect2 = ((CellRect) ref cellRect1).ExpandedBy(1);
        Rand.PushState();
        for (int index = 0; index < 3; ++index)
        {
          IntVec3 intVec3 = GenCollection.RandomElementWithFallback<IntVec3>(((CellRect) ref cellRect2).EdgeCells.Where<IntVec3>((Func<IntVec3, bool>) (c => GenGrid.InBounds(c, map) && GenGrid.Standable(c, map))), ((Thing) vehiclePawn).Position);
          if (((CellRect) ref cellRect2).EdgeCells.Contains<IntVec3>(intVec3))
          {
            loc = intVec3;
            break;
          }
          cellRect2 = ((CellRect) ref cellRect2).ExpandedBy(1);
        }
        Rand.PopState();
      }
      catch (Exception ex)
      {
        Log.Error($"Pawn {((Entity) pawn).Label} could not be readjusted for spawn location.\nException={ex}");
      }
    }

    static void FinalizePosition(VehiclePawn vehicle, Rot4 rot, ref IntVec3 cell)
    {
      switch (((Rot4) ref rot).AsInt)
      {
        case 2:
          if (((BuildableDef) vehicle.VehicleDef).Size.x % 2 == 0)
            --cell.x;
          if (((BuildableDef) vehicle.VehicleDef).Size.z % 2 != 0)
            break;
          --cell.z;
          break;
        case 3:
          if (((BuildableDef) vehicle.VehicleDef).Size.x % 2 == 0)
            ++cell.z;
          if (((BuildableDef) vehicle.VehicleDef).Size.z % 2 != 0)
            break;
          --cell.x;
          break;
      }
    }
  }

  private static void AllowDeconstructVehicle(
    Designator_Deconstruct __instance,
    Thing t,
    ref AcceptanceReport __result)
  {
    if (!(t is VehiclePawn vehiclePawn) || !vehiclePawn.DeconstructibleBy(Faction.OfPlayer))
      return;
    if (((Designator) __instance).Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
      __result = AcceptanceReport.op_Implicit(false);
    else if (((Designator) __instance).Map.designationManager.DesignationOn(t, DesignationDefOf.Uninstall) != null)
      __result = AcceptanceReport.op_Implicit(false);
    else
      __result = AcceptanceReport.op_Implicit(true);
  }

  private static bool DoUnsupportedVehicleRefunds(Thing diedThing, Map map, DestroyMode mode)
  {
    if (!(diedThing is VehiclePawn vehicle))
      return true;
    vehicle.RefundMaterials(map, mode);
    return false;
  }

  private static IEnumerable<CodeInstruction> ValidDestroyModeForVehicles(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if ((instruction.opcode == OpCodes.Brfalse || instruction.opcode == OpCodes.Brfalse_S) && !instructionList.OutOfBounds<CodeInstruction>(i - 1) && instructionList[i - 1].opcode == OpCodes.Ldarg_1)
      {
        List<Label> labels = instruction.labels;
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldarg_1, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_Construction), "VehicleValidDestroyMode", (System.Type[]) null, (System.Type[]) null));
        yield return new CodeInstruction(OpCodes.Brtrue, (object) labels.FirstOrDefault<Label>());
        labels = (List<Label>) null;
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static bool VehicleValidDestroyMode(Pawn pawn, DestroyMode destroyMode)
  {
    return pawn is VehiclePawn && destroyMode != 8 && destroyMode != 5 && destroyMode != 1;
  }
}
