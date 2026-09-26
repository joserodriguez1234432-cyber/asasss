// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSpawner
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleSpawner
{
  private const int BiologicalAgeTicksMultiplier = 3600000;
  private static readonly SimpleCurve DefaultAgeGenerationCurve;

  public static VehiclePawn GenerateVehicle(VehicleDef vehicleDef, Faction faction)
  {
    return VehicleSpawner.GenerateVehicle(new VehicleGenerationRequest(vehicleDef, faction));
  }

  public static VehiclePawn GenerateVehicle(VehicleGenerationRequest request)
  {
    if (request.vehicleDef == null)
      throw new ArgumentNullException("vehicleDef", "Cannot generate vehicle with null def.");
    VehiclePawn vehicle = (VehiclePawn) null;
    try
    {
      vehicle = (VehiclePawn) ThingMaker.MakeThing((ThingDef) request.vehicleDef, (ThingDef) null);
      vehicle.kindDef = request.vehicleDef.kindDef;
      PawnComponentsUtility.CreateInitialComponents((Pawn) vehicle);
      vehicle.sustainers = new VehicleSustainers(vehicle);
      vehicle.kindDef = request.vehicleDef.kindDef;
      ((Thing) vehicle).SetFactionDirect(request.faction);
      PatternData colors = GetColors(request.vehicleDef, request.randomizeColors);
      ((Thing) vehicle).DrawColor = colors.color;
      vehicle.DrawColorTwo = colors.colorTwo;
      vehicle.DrawColorThree = colors.colorThree;
      vehicle.Displacement = colors.displacement;
      vehicle.Tiles = colors.tiles;
      vehicle.PostGenerationSetup();
      foreach (ThingComp allComp in ((ThingWithComps) vehicle).AllComps)
      {
        if (allComp is VehicleComp vehicleComp)
          vehicleComp.PostGeneration();
      }
      if (!request.cleanSlate)
        VehicleSpawner.DistributeAmmunition(vehicle);
      float num = Rand.ByCurve(VehicleSpawner.DefaultAgeGenerationCurve);
      vehicle.ageTracker.AgeBiologicalTicks = (long) ((double) num * 3600000.0) + (long) Rand.Range(0, 3600000);
      vehicle.needs.SetInitialLevels();
    }
    catch (Exception ex)
    {
      Log.Error($"{"[VehicleFramework]"} Exception thrown while generating {request.vehicleDef}. Exception: {ex}");
    }
    return vehicle;

    static PatternData GetColors(VehicleDef vehicleDef, bool randomize)
    {
      using (new RandStateBlock())
      {
        PatternData colors = new PatternData();
        if (!randomize)
        {
          PatternData patternData = (PatternData) vehicleDef.graphicData ?? new PatternData(Color.white, Color.white, Color.white, PatternDefOf.Default, Vector2.zero, 0.0f);
          GraphicDataRGB reference = (GraphicDataRGB) GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, patternData);
          colors.Copy((PatternData) reference);
          return colors;
        }
        colors.patternDef = GenCollection.RandomElementWithFallback<PatternDef>((IEnumerable<PatternDef>) DefDatabase<PatternDef>.AllDefsListForReading, PatternDefOf.Default);
        if (Rand.Chance(0.1f))
        {
          PatternData patternData1 = colors;
          PatternData patternData2 = colors;
          PatternData patternData3 = colors;
          (Color colorOne, Color colorTwo, Color colorThree) completelyRandomColors = VehicleGenerationRequest.GetCompletelyRandomColors();
          Color colorOne = completelyRandomColors.colorOne;
          patternData1.color = colorOne;
          patternData2.colorTwo = completelyRandomColors.colorTwo;
          patternData3.colorThree = completelyRandomColors.colorThree;
        }
        else
        {
          PatternData patternData4 = colors;
          PatternData patternData5 = colors;
          PatternData patternData6 = colors;
          (Color colorOne, Color colorTwo, Color colorThree) randomPalette = VehicleMod.settings.colorStorage.GetRandomPalette();
          Color colorOne = randomPalette.colorOne;
          patternData4.color = colorOne;
          patternData5.colorTwo = randomPalette.colorTwo;
          patternData6.colorThree = randomPalette.colorThree;
        }
        Vector2 vector2 = new Vector2(Rand.Range(-1.5f, 1.5f), Rand.Range(-1.5f, 1.5f));
        colors.displacement = vector2;
        float num = Rand.Range(0.25f, 1.5f);
        colors.tiles = num;
        return colors;
      }
    }
  }

  public static VehiclePawn SpawnVehicleRandomized(
    VehicleDef vehicleDef,
    IntVec3 cell,
    Map map,
    Faction faction,
    Rot4? rot = null,
    bool autoFill = false)
  {
    rot.GetValueOrDefault();
    if (!rot.HasValue)
      rot = new Rot4?(Rot4.Random);
    VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(new VehicleGenerationRequest(vehicleDef, faction, true, true));
    vehicle.CompFueledTravel?.Refuel(vehicle.CompFueledTravel.FuelCapacity);
    GenSpawn.Spawn((Thing) vehicle, cell, map, rot.Value, (WipeMode) 1, false, false);
    if (autoFill)
    {
      foreach (VehicleRoleHandler handler in vehicle.handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (h => h.role.HandlingTypes > HandlingType.None)))
      {
        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, faction, (PawnGenerationContext) 2, new PlanetTile?(), false, false, false, true, false, 1f, false, true, false, true, true, false, false, false, false, 0.0f, 0.0f, (Pawn) null, 1f, (Predicate<Pawn>) null, (Predicate<Pawn>) null, (IEnumerable<TraitDef>) null, (IEnumerable<TraitDef>) null, new float?(), new float?(), new float?(), new Gender?(), (string) null, (string) null, (RoyalTitleDef) null, (Ideo) null, false, false, false, false, (List<GeneDef>) null, (List<GeneDef>) null, (XenotypeDef) null, (CustomXenotype) null, (List<XenotypeDef>) null, 0.0f, (DevelopmentalStage) 8, (Func<XenotypeDef, PawnKindDef>) null, new FloatRange?(), new FloatRange?(), false, false, false, -1, 0, false));
        ((Thing) pawn).SetFactionDirect(faction);
        vehicle.TryAddPawn(pawn, handler);
      }
    }
    return vehicle;
  }

  private static void UpgradeAtRandom(VehiclePawn vehicle, int upgradeCount)
  {
    if (vehicle.CompUpgradeTree == null)
      return;
    Rand.PushState();
    for (int index = 0; index < upgradeCount; ++index)
    {
      UpgradeNode node1;
      if (GenCollection.TryRandomElement<UpgradeNode>(vehicle.CompUpgradeTree.Props.def.nodes.Where<UpgradeNode>((Func<UpgradeNode, bool>) (node => !vehicle.CompUpgradeTree.NodeUnlocked(node) && vehicle.CompUpgradeTree.PrerequisitesMet(node))), ref node1))
        vehicle.CompUpgradeTree.FinishUnlock(node1);
    }
    Rand.PopState();
  }

  private static void DistributeAmmunition(VehiclePawn vehicle)
  {
    if (vehicle.CompVehicleTurrets == null)
      return;
    using (new RandStateBlock())
    {
      foreach (VehicleTurret turret in (IEnumerable<VehicleTurret>) vehicle.CompVehicleTurrets.Turrets)
      {
        if (turret.def.ammunition != null)
        {
          int num1 = Rand.RangeInclusive(1, turret.def.ammunition.AllowedDefCount);
          for (int index = 0; index < num1; ++index)
          {
            ThingDef thingDef = turret.def.ammunition.AllowedThingDefs.ElementAt<ThingDef>(index);
            int num2 = Rand.RangeInclusive(10, 25);
            int num3 = Rand.RangeInclusive(10, 50);
            int num4 = Rand.RangeInclusive(2, 5);
            float num5 = (float) num2 * Mathf.Exp((float) -turret.def.magazineCapacity / (float) num3) + (float) num4;
            Thing thing = ThingMaker.MakeThing(thingDef, (ThingDef) null);
            thing.stackCount = Mathf.RoundToInt((float) turret.def.magazineCapacity * num5);
            vehicle.AddOrTransfer(thing);
          }
          turret.AutoReload();
        }
      }
    }
  }

  static VehicleSpawner()
  {
    SimpleCurve simpleCurve = new SimpleCurve();
    simpleCurve.Add(new CurvePoint(0.05f, 0.0f), true);
    simpleCurve.Add(new CurvePoint(0.1f, 100f), true);
    simpleCurve.Add(new CurvePoint(0.675f, 100f), true);
    simpleCurve.Add(new CurvePoint(0.75f, 30f), true);
    simpleCurve.Add(new CurvePoint(0.875f, 18f), true);
    simpleCurve.Add(new CurvePoint(1f, 10f), true);
    simpleCurve.Add(new CurvePoint(1.125f, 3f), true);
    simpleCurve.Add(new CurvePoint(1.25f, 0.0f), true);
    VehicleSpawner.DefaultAgeGenerationCurve = simpleCurve;
  }
}
