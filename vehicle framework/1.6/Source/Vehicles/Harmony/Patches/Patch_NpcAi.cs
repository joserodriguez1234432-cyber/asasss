// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_NpcAi
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_NpcAi : IPatchCategory
{
  private static readonly LinearCurve VehicleCountByPointsCurve = new LinearCurve()
  {
    new CurvePoint(0.0f, 0.0f),
    new CurvePoint(1000f, 0.0f),
    new CurvePoint(3000f, 1f),
    new CurvePoint(5000f, 2f),
    new CurvePoint(20000f, 5f)
  };
  private static readonly HashSet<PawnsArrivalModeDef> VehicleArrivalModes = new HashSet<PawnsArrivalModeDef>();

  PatchSequence IPatchCategory.PatchAt => PatchSequence.PostDefDatabase;

  void IPatchCategory.PatchMethods()
  {
  }

  private static void InjectVehiclesIntoPawnKindGroupPrepare(
    PawnGroupMakerParms parms,
    PawnGroupMaker groupMaker,
    [UsedImplicitly] ref List<VehicleDef> __state)
  {
    Faction faction = parms.faction;
    Debug.Message($"Attempting generation for raid. Faction={(faction != null ? ((Def) faction.def).LabelCap : TaggedString.op_Implicit("Null"))}");
    if (!FactionUtility.HostileTo(parms.faction, Faction.OfPlayer))
      return;
    VehicleRaiderDefModExtension modExtension = ((Def) parms.faction?.def).GetModExtension<VehicleRaiderDefModExtension>();
    if (modExtension == null)
      return;
    Debug.Message($"[PREFIX] Generating with points: {parms.points}");
    float vehicleBudget = (float) ((double) modExtension.pointMultiplier * ((double) parms.points - 250.0) / 2.0);
    if ((double) vehicleBudget <= 0.0)
      return;
    float num1 = 0.0f;
    int num2 = Mathf.FloorToInt(Patch_NpcAi.VehicleCountByPointsCurve.Evaluate(parms.points));
    if (num2 <= 0)
      return;
    VehicleCategory category = RaidInjectionHelper.GetResolvedCategory(parms);
    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading.Where<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => RaidInjectionHelper.ValidRaiderVehicle(vehicleDef, category, (PawnsArrivalModeDef) null, parms.faction, vehicleBudget))).ToList<VehicleDef>();
    Debug.Message($"[PREFIX] Vehicle Budget: {vehicleBudget} AvailableDefs: {list.Count}");
    if (list.Count <= 0)
      return;
    __state = new List<VehicleDef>();
    for (int index = 0; index < num2; ++index)
    {
      VehicleDef vehicleDef = GenCollection.RandomElement<VehicleDef>((IEnumerable<VehicleDef>) list);
      __state.Add(vehicleDef);
      vehicleBudget -= vehicleDef.combatPower;
      num1 += vehicleDef.combatPower;
      Debug.Message($"[PREFIX] Adding {vehicleDef}");
    }
    parms.points -= num1;
  }

  private static void InjectVehiclesIntoPawnKindGroupPassthrough(
    PawnGroupMakerParms parms,
    PawnGroupMaker groupMaker,
    List<Pawn> outPawns1,
    List<VehicleDef> __state)
  {
    if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) __state))
      return;
    Debug.Message($"[POSTFIX] Injecting vehicles with points: {parms.points}");
    List<Pawn> list = outPawns1.Where<Pawn>((Func<Pawn, bool>) (outPawns2 => outPawns2.RaceProps.Humanlike)).ToList<Pawn>();
    foreach (VehicleDef vehicleDef in __state)
    {
      VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(new VehicleGenerationRequest(vehicleDef, parms.faction, true, true));
      while (vehicle.SeatsAvailable > 0 && list.Count > 0)
      {
        Pawn pawn = GenCollection.Pop<Pawn>(list);
        outPawns1.Remove(pawn);
        if (!vehicle.TryAddPawn(pawn))
        {
          Log.Error($"Unable to add {pawn} to {vehicle} during raid generation.");
          outPawns1.Add(pawn);
        }
      }
      outPawns1.Add((Pawn) vehicle);
    }
  }

  private static void InjectVehiclesIntoRaidPrepare(IncidentParms parms, [UsedImplicitly] List<VehicleDef> __state)
  {
    if (parms.pawnKind == null || parms.faction == null || parms.faction.def == FactionDefOf.Mechanoid || (double) parms.points <= 1000.0 || parms.pawnCount <= 5)
      return;
    int num = Mathf.FloorToInt(Patch_NpcAi.VehicleCountByPointsCurve.Evaluate(parms.points));
    VehicleCategory category = RaidInjectionHelper.GetResolvedCategory(parms);
    List<VehicleDef> list = DefDatabase<VehicleDef>.AllDefsListForReading.Where<VehicleDef>((Func<VehicleDef, bool>) (vehicleDef => RaidInjectionHelper.ValidRaiderVehicle(vehicleDef, category, parms.raidArrivalMode, parms.faction, parms.points))).ToList<VehicleDef>();
    if (num <= 0 || GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) list))
      return;
    __state = new List<VehicleDef>();
    for (int index = 0; index < num; ++index)
    {
      VehicleDef vehicleDef = GenCollection.RandomElement<VehicleDef>((IEnumerable<VehicleDef>) list);
      __state.Add(vehicleDef);
    }
  }

  private static void InjectVehiclesIntoRaidPassthrough(
    List<Pawn> __result,
    IncidentParms parms,
    List<VehicleDef> __state)
  {
    if (GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) __state))
      return;
    List<Pawn> list = __result.Where<Pawn>((Func<Pawn, bool>) (outPawns => outPawns.RaceProps.Humanlike)).ToList<Pawn>();
    foreach (VehicleDef vehicleDef in __state)
    {
      VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(new VehicleGenerationRequest(vehicleDef, parms.faction, true, true));
      while (vehicle.SeatsAvailable > 0 && list.Count > 0)
      {
        Pawn pawn = GenCollection.Pop<Pawn>(list);
        __result.Remove(pawn);
        if (!vehicle.TryAddPawn(pawn))
        {
          Log.Error($"Unable to add {pawn} to {vehicle} during raid generation.");
          __result.Add(pawn);
        }
      }
      __result.Add((Pawn) vehicle);
    }
  }

  private static void VehicleHasBuildingDestroyerTurret(ref bool __result, Pawn p)
  {
    if (__result || !(p is VehiclePawn vehiclePawn) || vehiclePawn.CompVehicleTurrets == null)
      return;
    __result = true;
  }

  private static bool DisableVanillaJobForVehicle(Pawn pawn, ref Job __result)
  {
    if (!(pawn is VehiclePawn))
      return true;
    Trace.Fail(((Entity) pawn).LabelCap + " assigned a humanlike pawn job.");
    __result = (Job) null;
    return false;
  }
}
