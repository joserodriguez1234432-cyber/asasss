// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.IncidentWorker_Ambush_EnemyMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

public class IncidentWorker_Ambush_EnemyMapVehicle : IncidentWorker_AmbushMapVehicle
{
  public static LinearCurve VehicleCountByPointsCurve { get; }

  protected override WorldObjectDef MapParentDef => WorldObjectDefOf.Ambush;

  protected override bool CanFireNowSub(IncidentParms parms)
  {
    Faction faction;
    return base.CanFireNowSub(parms) && PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, ref faction, (Predicate<Faction>) null, false, false, false, true);
  }

  protected override List<Pawn> GeneratePawns(IncidentParms parms)
  {
    PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms, false);
    pawnGroupMakerParms.generateFightersOnly = true;
    pawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
    return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true).ToList<Pawn>();
  }

  protected override List<VehiclePawnWithMap> GenerateVehicles(IncidentParms parms)
  {
    VehicleCategory category = RaidInjectionHelper.GetResolvedCategory(parms);
    List<VehicleDef> list1 = DefDatabase<VehicleDef>.AllDefs.Where<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => this.ValidRaiderVehicle(vehicleDef, category, (PawnsArrivalModeDef) null, parms.faction, parms.points))).ToList<VehicleDef>();
    List<VehiclePawnWithMap> list2 = MapVehicleGroupMakerUtility.GenerateVehicles(parms.faction, parms.points, IncidentWorker_Ambush_EnemyMapVehicle.VehicleCountByPointsCurve, list1).ToList<VehiclePawnWithMap>();
    parms.points = Mathf.Max(parms.points - list2.Sum<VehiclePawnWithMap>((Func<VehiclePawnWithMap, float>) (v => v.VehicleDef.combatPower)), parms.faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat, (PawnGroupMakerParms) null));
    return list2;
  }

  protected virtual bool ValidRaiderVehicle(
    VehicleDef vehicleDef,
    VehicleCategory category,
    PawnsArrivalModeDef arrivalModeDef,
    Faction faction,
    float points)
  {
    return VehicleCaravanIncidentUtility.ValidThreatVehicle(vehicleDef, category, arrivalModeDef, faction, points);
  }

  protected override LordJob CreateLordJob(IncidentParms parms)
  {
    return (LordJob) new LordJob_ArmoredAssault(parms.faction, LordJob_ArmoredAssault.RaiderPermissions.All);
  }

  protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
  {
    TaggedString taggedString = GrammarResolverSimpleStringExtensions.Formatted(this.def.letterText, NamedArgument.op_Implicit(parms.target is Caravan target ? target.Name : Translator.TranslateSimple("yourCaravan")), NamedArgument.op_Implicit(parms.faction.def.pawnsPlural), NamedArgument.op_Implicit(parms.faction.NameColored));
    return GenText.CapitalizeFirst(((TaggedString) ref taggedString).Resolve());
  }

  static IncidentWorker_Ambush_EnemyMapVehicle()
  {
    LinearCurve linearCurve = new LinearCurve();
    linearCurve.Add(new CurvePoint(0.0f, 1f));
    linearCurve.Add(new CurvePoint(1000f, 1f));
    linearCurve.Add(new CurvePoint(3000f, 1f));
    linearCurve.Add(new CurvePoint(5000f, 3f));
    linearCurve.Add(new CurvePoint(20000f, 5f));
    IncidentWorker_Ambush_EnemyMapVehicle.VehicleCountByPointsCurve = linearCurve;
  }
}
