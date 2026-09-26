// Decompiled with JetBrains decompiler
// Type: Vehicles.World.CrashSite
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class CrashSite : MapParent
{
  public const int TicksTillRemovalAfterCrash = 600;
  private Settlement reinforcementsFrom;
  private int ticksSinceCrash;
  private int ticksTillReinforcements;
  private FloatRange scaleFactor = new FloatRange(1.5f, 2.5f);
  private WorldPath pathToSite;

  public virtual Settlement Settlement => this.reinforcementsFrom;

  public int InitiateReinforcementsRequest([NotNull] Settlement reinforcementsFrom)
  {
    this.reinforcementsFrom = reinforcementsFrom;
    this.ticksSinceCrash = 0;
    PlanetTile tile = ((WorldObject) reinforcementsFrom).Tile;
    this.pathToSite = ((PlanetTile) ref tile).Layer.Pather.FindPath(((WorldObject) reinforcementsFrom).Tile, ((WorldObject) this).Tile, (Caravan) null, (Func<float, bool>) null);
    if (this.pathToSite.Found)
      return this.ticksTillReinforcements = Mathf.RoundToInt(this.pathToSite.TotalCost * 1.5f);
    this.ticksTillReinforcements = int.MaxValue;
    return -1;
  }

  protected virtual void Tick()
  {
    ((WorldObject) this).Tick();
    ++this.ticksSinceCrash;
    --this.ticksTillReinforcements;
    if (this.ticksTillReinforcements >= 0 || this.reinforcementsFrom == null)
      return;
    this.ReinforcementsArrived();
  }

  protected virtual LordJob CreateLordJob(IncidentParms parms)
  {
    return (LordJob) new LordJob_AssaultColony(parms.faction, true, false, false, false, true, false, false);
  }

  protected virtual void ReinforcementsArrived()
  {
    IntVec3 intVec3;
    if (!CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>) (cell => GenGrid.Standable(cell, this.Map) && this.Map.reachability.CanReachColony(cell)), this.Map, CellFinder.EdgeRoadChance_Hostile, ref intVec3))
      return;
    IncidentParms parms = new IncidentParms()
    {
      target = (IIncidentTarget) this.Map,
      points = StorytellerUtility.DefaultThreatPointsNow((IIncidentTarget) Find.CurrentMap),
      faction = ((WorldObject) this.reinforcementsFrom).Faction
    };
    PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms, false);
    pawnGroupMakerParms.generateFightersOnly = true;
    pawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
    List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true).ToList<Pawn>();
    foreach (Thing thing in list)
      GenSpawn.Spawn(thing, CellFinder.RandomSpawnCellForPawnNear(intVec3, this.Map, 4), this.Map, Rot4.Random, (WipeMode) 0, false, false);
    LordJob lordJob = this.CreateLordJob(parms);
    LordMaker.MakeNewLord(parms.faction, lordJob, this.Map, (IEnumerable<Pawn>) list);
    Find.LetterStack.ReceiveLetter((Letter) LetterMaker.MakeLetter(Translator.Translate("VF_ReinforcementsArrivedLabel"), TranslatorFormattedStringExtensions.Translate("VF_ReinforcementsArrived", NamedArgument.op_Implicit(((WorldObject) this.reinforcementsFrom).Label)), LetterDefOf.ThreatBig, ((WorldObject) this.reinforcementsFrom).Faction, (Quest) null), (string) null, 0, true);
    this.ticksTillReinforcements = Mathf.RoundToInt(this.pathToSite.TotalCost * ((FloatRange) ref this.scaleFactor).RandomInRange);
  }

  public virtual bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
  {
    alsoRemoveWorldObject = false;
    if (this.ticksSinceCrash < 600 || this.Map.mapPawns.AnyPawnBlockingMapRemoval)
      return false;
    foreach (PocketMapParent pocketMap in Find.World.pocketMaps)
    {
      if (pocketMap.sourceMap == this.Map && ((MapParent) pocketMap).Map.mapPawns.AnyPawnBlockingMapRemoval)
        return false;
    }
    if (ModsConfig.OdysseyActive && this.Map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor) || TransporterUtility.IncomingTransporterPreventingMapRemoval(this.Map))
      return false;
    alsoRemoveWorldObject = true;
    return true;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<Settlement>(ref this.reinforcementsFrom, "reinforcementsFrom", false);
    Scribe_Values.Look<int>(ref this.ticksTillReinforcements, "ticksTillReinforcements", 0, false);
    Scribe_Values.Look<int>(ref this.ticksSinceCrash, "ticksSinceCrash", 0, false);
    if (Scribe.mode != 4)
      return;
    PlanetTile tile = ((WorldObject) this.reinforcementsFrom).Tile;
    this.pathToSite = ((PlanetTile) ref tile).Layer.Pather.FindPath(((WorldObject) this.reinforcementsFrom).Tile, ((WorldObject) this).Tile, (Caravan) null, (Func<float, bool>) null);
  }
}
