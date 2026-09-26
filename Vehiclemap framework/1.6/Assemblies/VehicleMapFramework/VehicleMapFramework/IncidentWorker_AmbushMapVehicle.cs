// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.IncidentWorker_AmbushMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Vehicles;
using Vehicles.World;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public abstract class IncidentWorker_AmbushMapVehicle : IncidentWorker
{
  protected abstract WorldObjectDef MapParentDef { get; }

  protected abstract List<VehiclePawnWithMap> GenerateVehicles(IncidentParms parms);

  protected abstract List<Pawn> GeneratePawns(IncidentParms parms);

  protected virtual void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
  {
  }

  protected virtual void PostProcessGeneratedVehiclesAfterSpawning(
    List<VehiclePawnWithMap> generatedVehicles)
  {
  }

  protected virtual LordJob CreateLordJob(IncidentParms parms) => (LordJob) null;

  protected virtual bool CanFireNowSub(IncidentParms parms)
  {
    Map map = parms.target as Map;
    if (map != null)
    {
      IntVec3 intVec3;
      return CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>) (x => GenGrid.Standable(x, map) && map.reachability.CanReachColony(x)), map, CellFinder.EdgeRoadChance_Hostile, ref intVec3);
    }
    return parms.target is VehicleCaravan && CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile);
  }

  private static void CleanUpGeneratedVehicles(List<VehiclePawnWithMap> generatedVehicles)
  {
    foreach (VehiclePawnWithMap generatedVehicle in generatedVehicles)
    {
      if (!((Thing) generatedVehicle).Destroyed)
        ((Thing) generatedVehicle).Destroy((DestroyMode) 0);
    }
  }

  protected virtual bool TryExecuteWorker(IncidentParms parms)
  {
    if (!PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, ref parms.faction, (Predicate<Faction>) null, false, false, false, true))
    {
      Log.Error($"Could not find any valid faction for {this.def} incident.");
      return false;
    }
    Map map = parms.target as Map;
    IntVec3 existingMapEdgeCell = IntVec3.Invalid;
    List<VehiclePawnWithMap> generatedVehicles = this.GenerateVehicles(parms);
    if (GenList.NullOrEmpty<VehiclePawnWithMap>((IList<VehiclePawnWithMap>) generatedVehicles))
      return false;
    VehiclePawnWithMap largestVehicle = GenCollection.MaxBy<VehiclePawnWithMap, int>((IEnumerable<VehiclePawnWithMap>) generatedVehicles, (Func<VehiclePawnWithMap, int>) (v => ((IntVec2) ref ((Thing) v).def.size).Area));
    if (map != null && !TryFindCellEdgeCell(Rot4.North) && !TryFindCellEdgeCell(Rot4.South) && !TryFindCellEdgeCell(Rot4.East) && !TryFindCellEdgeCell(Rot4.West))
    {
      IncidentWorker_AmbushMapVehicle.CleanUpGeneratedVehicles(generatedVehicles);
      return false;
    }
    List<Pawn> generatedEnemies = this.GeneratePawns(parms);
    if (GenList.NullOrEmpty<Pawn>((IList<Pawn>) generatedEnemies))
      return false;
    if (map != null)
    {
      if (this.DoExecute(parms, generatedVehicles, generatedEnemies, existingMapEdgeCell))
        return true;
      IncidentWorker_AmbushMapVehicle.CleanUpGeneratedVehicles(generatedVehicles);
      return false;
    }
    LongEventHandler.QueueLongEvent((Action) (() =>
    {
      if (this.DoExecute(parms, generatedVehicles, generatedEnemies, existingMapEdgeCell))
        return;
      IncidentWorker_AmbushMapVehicle.CleanUpGeneratedVehicles(generatedVehicles);
    }), "GeneratingMapForNewEncounter", false, (Action<Exception>) null, true, false, (Action) null);
    return true;

    bool TryFindCellEdgeCell(Rot4 rot)
    {
      return CellFinderExtended.TryFindRandomEdgeCellWith((Predicate<IntVec3>) (c => Ext_Vehicles.CellRectStandable((VehiclePawn) largestVehicle, map, new IntVec3?(c), new Rot4?())), map, rot, largestVehicle.VehicleDef, CellFinder.EdgeRoadChance_Hostile, ref existingMapEdgeCell);
    }
  }

  private bool DoExecute(
    IncidentParms parms,
    List<VehiclePawnWithMap> generatedVehicles,
    List<Pawn> generatedEnemies,
    IntVec3 existingMapEdgeCell)
  {
    bool flag = false;
    if (parms.target is Map map)
    {
      CellRect cellRect = CellRect.WholeMap(map);
      Rot4 closestEdge = ((CellRect) ref cellRect).GetClosestEdge(existingMapEdgeCell);
      VehicleCaravanIncidentUtility.SpawnEnemies(map, generatedVehicles, generatedEnemies, new Rot4?(closestEdge));
    }
    else
    {
      if (!(parms.target is VehicleCaravan target))
        return false;
      map = VehicleCaravanIncidentUtility.SetupCaravanAttackMap(target, generatedVehicles, generatedEnemies, false, this.MapParentDef);
      flag = true;
    }
    if (map == null)
      return false;
    this.PostProcessGeneratedPawnsAfterSpawning(generatedEnemies);
    this.PostProcessGeneratedVehiclesAfterSpawning(generatedVehicles);
    LordJob lordJob = this.CreateLordJob(parms);
    if (lordJob != null)
      LordMaker.MakeNewLord(parms.faction, lordJob, map, (IEnumerable<Pawn>) generatedEnemies).AddPawns((IEnumerable<Pawn>) generatedVehicles, true);
    TaggedString taggedString1 = TaggedString.op_Implicit(this.GetLetterLabel(generatedEnemies[0], parms));
    TaggedString taggedString2 = TaggedString.op_Implicit(this.GetLetterText(generatedEnemies[0], parms));
    PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter((IEnumerable<Pawn>) generatedEnemies, ref taggedString1, ref taggedString2, this.GetRelatedPawnsInfoLetterText(parms), true, true);
    this.SendStandardLetter(taggedString1, taggedString2, this.GetLetterDef(generatedEnemies[0], parms), parms, LookTargets.op_Implicit((Thing) generatedEnemies[0]), Array.Empty<NamedArgument>());
    if (flag)
      Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
    return true;
  }

  protected virtual string GetLetterLabel(Pawn anyPawn, IncidentParms parms)
  {
    return this.def.letterLabel;
  }

  protected virtual string GetLetterText(Pawn anyPawn, IncidentParms parms) => this.def.letterText;

  protected virtual LetterDef GetLetterDef(Pawn anyPawn, IncidentParms parms) => this.def.letterDef;

  protected virtual string GetRelatedPawnsInfoLetterText(IncidentParms parms)
  {
    return TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsGroupGeneric", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural)));
  }
}
