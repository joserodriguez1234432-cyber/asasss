// Decompiled with JetBrains decompiler
// Type: Vehicles.World.CaravanFormation
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

public static class CaravanFormation
{
  public static SplitInfo splitter;
  public static FormationInfo formation;

  public static ICaravanInfo Current
  {
    get
    {
      return CaravanFormation.formation == null ? (ICaravanInfo) CaravanFormation.splitter : (ICaravanInfo) CaravanFormation.formation;
    }
  }

  public static bool TryShowConfirmLeaveVehiclesDialog(Dialog_FormCaravan formCaravan)
  {
    CaravanFormation.formation.RecacheTransferables();
    if (CaravanFormation.formation.unselectedVehicles.Count <= 0 || formCaravan.transferables.Exists(new Predicate<TransferableOneWay>(PawnLeftBehind)))
      return false;
    string str = "";
    foreach (VehiclePawn unselectedVehicle in CaravanFormation.formation.unselectedVehicles)
      str += ((Entity) unselectedVehicle).LabelShort;
    Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("VF_LeaveVehicleBehindCaravan", NamedArgument.op_Implicit(str)), (Action) (() =>
    {
      if (!CaravanFormation.CheckForErrors())
        return;
      CaravanFormation.formation.AddItemsFromTransferablesToRandomInventories(CaravanFormation.formation.AllPawnsAndVehicles);
      VehicleCaravan vehicleCaravan = CaravanHelper.ExitMapAndCreateVehicleCaravan((IEnumerable<Pawn>) CaravanFormation.formation.AllPawnsAndVehicles, Faction.OfPlayer, formCaravan.CurrentTile, formCaravan.CurrentTile, CaravanFormation.formation.DestinationTile, false);
      CaravanFormation.formation.Map.Parent.CheckRemoveMapNow();
      TaggedString taggedString = Translator.Translate("MessageReformedCaravan");
      if (vehicleCaravan.vehiclePather.Moving && vehicleCaravan.vehiclePather.ArrivalAction != null)
        taggedString = TaggedString.op_Addition(taggedString, TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition(" ", Translator.Translate("MessageFormedCaravan_Orders")), ": "), vehicleCaravan.vehiclePather.ArrivalAction.Label), "."));
      Messages.Message(TaggedString.op_Implicit(taggedString), LookTargets.op_Implicit((WorldObject) vehicleCaravan), MessageTypeDefOf.TaskCompletion, false);
    }), false, (string) null, (WindowLayer) 1));
    return true;

    static bool PawnLeftBehind(TransferableOneWay transferable)
    {
      return ((Transferable) transferable).AnyThing is Pawn anyThing && !(anyThing is VehiclePawn) && ((Transferable) transferable).CountToTransfer == 0;
    }
  }

  public static void TrySendVehicleCaravan(Dialog_FormCaravan formCaravan)
  {
    CaravanFormation.formation.RecacheTransferables();
    if (CaravanFormation.formation.Reform)
    {
      CaravanFormation.ReformInstantly();
    }
    else
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      (float days, float tillRot) daysWorthOfFood = CaravanFormation.formation.DaysWorthOfFood;
      if ((double) daysWorthOfFood.days < 5.0)
      {
        StringBuilder stringBuilder2 = stringBuilder1;
        string str;
        if ((double) daysWorthOfFood.days >= 0.10000000149011612)
        {
          TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("DaysWorthOfFoodWarningDialog", NamedArgument.op_Implicit(daysWorthOfFood.days.ToString("0.#")));
          str = ((TaggedString) ref taggedString).Resolve();
        }
        else
          str = Translator.Translate("DaysWorthOfFoodWarningDialog_NoFood").ToString();
        stringBuilder2.AppendLine(str);
      }
      else if (CaravanFormation.formation.MostFoodWillRotSoon)
        stringBuilder1.AppendLine(TaggedString.op_Implicit(Translator.Translate("CaravanFoodWillRotSoonWarningDialog")));
      if (!GenCollection.Any<Pawn>(CaravanFormation.formation.pawns, (Predicate<Pawn>) (pawn =>
      {
        if (!CaravanUtility.IsOwner(pawn, Faction.OfPlayer))
          return false;
        SkillRecord skill = pawn.skills?.GetSkill(SkillDefOf.Social);
        return (skill == null ? 0 : (!skill.TotallyDisabled ? 1 : 0)) == 0;
      })))
        stringBuilder1.AppendLine(TaggedString.op_Implicit(Translator.Translate("CaravanIncapableOfSocial")));
      if (CaravanFormation.formation.ShouldShowWarningForUndesirableFood())
        stringBuilder1.AppendLine(TaggedString.op_Implicit(Translator.Translate("DaysWorthOfFoodDietWarningDialog")));
      if (CaravanFormation.formation.ShouldShowWarningForMechWithoutMechanitor())
        stringBuilder1.AppendLine(TaggedString.op_Implicit(Translator.Translate("CaravanLacksMechMechanitorWarning")));
      if (ModsConfig.BiotechActive)
      {
        bool flag = false;
        foreach (Pawn pawn in CaravanFormation.formation.pawns)
        {
          if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond, false) is Hediff_PsychicBond firstHediffOfDef && ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(pawn, firstHediffOfDef) && !((IEnumerable<Thing>) CaravanFormation.formation.pawns).Contains<Thing>(((HediffWithTarget) firstHediffOfDef).target))
          {
            if (!flag)
            {
              flag = true;
              stringBuilder1.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(Translator.Translate("PsychicBondDistanceWillBeActive_Caravan"), ":")));
            }
            StringBuilder stringBuilder3 = stringBuilder1;
            string[] strArray = new string[5]
            {
              "  - ",
              null,
              null,
              null,
              null
            };
            TaggedString nameFullColored = pawn.NameFullColored;
            strArray[1] = ((TaggedString) ref nameFullColored).Resolve();
            strArray[2] = " (";
            TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("Partner", NamedArgument.op_Implicit(((HediffWithTarget) firstHediffOfDef).target));
            strArray[3] = GenText.CapitalizeFirst(((TaggedString) ref taggedString).Resolve());
            strArray[4] = ")";
            string str = string.Concat(strArray);
            stringBuilder3.AppendLine(str);
          }
        }
      }
      if (stringBuilder1.Length > 0 && CaravanFormation.CheckForErrors())
      {
        stringBuilder1.AppendLine(TaggedString.op_Implicit(Translator.Translate("CaravanAreYouSure")));
        Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TaggedString.op_Implicit(stringBuilder1.ToString()), (Action) (() =>
        {
          if (!TryFormAndSendCaravan())
            return;
          ((Window) formCaravan).Close(false);
        }), false, (string) null, (WindowLayer) 1));
      }
      else
      {
        if (!TryFormAndSendCaravan())
          return;
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
        ((Window) formCaravan).Close(false);
      }
    }

    static bool TryFormAndSendCaravan()
    {
      foreach (Pawn pawn in CaravanFormation.formation.pawns)
      {
        if (pawn is VehiclePawn vehiclePawn)
          vehiclePawn.DisembarkAll();
      }
      if (!CaravanFormation.CheckForErrors())
        return false;
      Direction8Way direction8WayFromTo = Find.WorldGrid.GetDirection8WayFromTo(CaravanFormation.formation.Dialog.CurrentTile, CaravanFormation.formation.StartingTile);
      IntVec3 exitSpot;
      if (!CaravanFormation.TryFindExitSpot(CaravanFormation.formation.pawns, true, out exitSpot))
      {
        if (!CaravanFormation.TryFindExitSpot(CaravanFormation.formation.pawns, false, out exitSpot))
        {
          Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanCouldNotFindExitSpot", NamedArgument.op_Implicit(Direction8WayUtility.LabelShort(direction8WayFromTo)))), MessageTypeDefOf.RejectInput, false);
          return false;
        }
        Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanCouldNotFindReachableExitSpot", NamedArgument.op_Implicit(Direction8WayUtility.LabelShort(direction8WayFromTo)))), LookTargets.op_Implicit(new GlobalTargetInfo(exitSpot, CaravanFormation.formation.Map, false)), MessageTypeDefOf.CautionInput, false);
      }
      IntVec3 meetingPoint;
      if (!CaravanFormation.TryFindRandomPackingSpot(exitSpot, out meetingPoint))
      {
        Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanCouldNotFindPackingSpot", NamedArgument.op_Implicit(Direction8WayUtility.LabelShort(direction8WayFromTo)))), LookTargets.op_Implicit(new GlobalTargetInfo(exitSpot, CaravanFormation.formation.Map, false)), MessageTypeDefOf.RejectInput, false);
        return false;
      }
      CaravanFormation.formation.RecacheTransferables();
      VehicleCaravanFormingUtility.StartFormingCaravan(CaravanFormation.formation.Dialog.transferables, in meetingPoint, in exitSpot, CaravanFormation.formation.StartingTile, CaravanFormation.formation.DestinationTile);
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("CaravanFormationProcessStarted")), LookTargets.op_Implicit((Thing) CaravanFormation.formation.pawns[0]), MessageTypeDefOf.PositiveEvent, false);
      return true;
    }
  }

  private static void ReformInstantly()
  {
    if (!CaravanFormation.CheckForErrors())
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      CaravanHelper.BoardAllAssignedPawns();
      CaravanFormation.formation.AddItemsFromTransferablesToRandomInventories(CaravanFormation.formation.AllPawnsAndVehicles);
      foreach (CaravanGrouper.Group incompatibleCaravanGroup in CaravanGrouper.ExtractIncompatibleCaravanGroups(CaravanFormation.formation.vehicles, CaravanFormation.formation.pawns))
      {
        VehicleCaravan vehicleCaravan = CaravanHelper.ExitMapAndCreateVehicleCaravan(incompatibleCaravanGroup.AllPawns, Faction.OfPlayer, CaravanFormation.formation.Dialog.CurrentTile, CaravanFormation.formation.Dialog.CurrentTile, CaravanFormation.formation.DestinationTile, false);
        TaggedString taggedString = Translator.Translate("MessageReformedCaravan");
        if (vehicleCaravan.vehiclePather.Moving && vehicleCaravan.vehiclePather.ArrivalAction != null)
          taggedString = TaggedString.op_Addition(taggedString, $" {Translator.Translate("MessageFormedCaravan_Orders")}: {vehicleCaravan.vehiclePather.ArrivalAction.Label}.");
        Messages.Message(TaggedString.op_Implicit(taggedString), LookTargets.op_Implicit((WorldObject) vehicleCaravan), MessageTypeDefOf.TaskCompletion, false);
      }
      CaravanFormation.formation.Map.Parent.CheckRemoveMapNow();
      ((Window) CaravanFormation.formation.Dialog).Close(false);
    }
  }

  private static bool CheckForErrors()
  {
    if (CaravanFormation.formation.MustChooseRoute)
    {
      PlanetTile destinationTile = CaravanFormation.formation.DestinationTile;
      if (!((PlanetTile) ref destinationTile).Valid)
      {
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageMustChooseRouteFirst")), MessageTypeDefOf.RejectInput, false);
        return false;
      }
    }
    if (!CaravanFormation.formation.Reform)
    {
      PlanetTile startingTile = CaravanFormation.formation.StartingTile;
      if (!((PlanetTile) ref startingTile).Valid)
      {
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageNoValidExitTile")), MessageTypeDefOf.RejectInput, false);
        return false;
      }
    }
    if (!GenCollection.Any<Pawn>(CaravanFormation.formation.pawns, (Predicate<Pawn>) (pawn => CaravanUtility.IsOwner(pawn, Faction.OfPlayer) && !pawn.Downed)))
    {
      Messages.Message(TaggedString.op_Implicit(ModsConfig.IdeologyActive ? Translator.Translate("CaravanMustHaveAtLeastOneNonSlaveColonist") : Translator.Translate("CaravanMustHaveAtLeastOneColonist")), MessageTypeDefOf.RejectInput, false);
      return false;
    }
    if (!CaravanFormation.formation.Reform && (double) CaravanFormation.formation.Dialog.MassUsage > (double) CaravanFormation.formation.Dialog.MassCapacity)
    {
      CaravanFormation.formation.FlashMass();
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("TooBigCaravanMassUsage")), MessageTypeDefOf.RejectInput, false);
      return false;
    }
    if (!CaravanHelper.CanStartCaravan(CaravanFormation.formation.pawns))
      return false;
    if (CaravanFormation.formation.pawns.Count > 0)
    {
      foreach (VehiclePawn vehicle in CaravanFormation.formation.vehicles)
      {
        foreach (Pawn pawn in CaravanFormation.formation.pawns)
        {
          if (((Thing) pawn).Spawned && pawn.IsColonist && !ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit((Thing) vehicle), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0))
          {
            Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanPawnIsUnreachable", NamedArgument.op_Implicit(((Entity) pawn).LabelShort), NamedArgument.op_Implicit((Thing) pawn))), LookTargets.op_Implicit((Thing) pawn), MessageTypeDefOf.RejectInput, false);
            return false;
          }
        }
      }
    }
    GenCollection.Any<VehiclePawn>(CaravanFormation.formation.vehicles, (Predicate<VehiclePawn>) (v => v.CountAssignedToVehicle() < v.PawnCountToOperate));
    foreach (TransferableOneWay transferable in CaravanFormation.formation.Dialog.transferables)
    {
      if (((Transferable) transferable).ThingDef.category == 2)
      {
        int countToTransfer = ((Transferable) transferable).CountToTransfer;
        int num = 0;
        if (countToTransfer > 0)
        {
          foreach (Thing thing1 in transferable.things)
          {
            Thing thing = thing1;
            if (!thing.Spawned || CaravanFormation.formation.pawns.NotNullAndAny<Pawn>((Predicate<Pawn>) (pawn =>
            {
              if (!pawn.IsColonist)
                return false;
              return ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(thing), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0) || CanReachUnspawned(pawn, thing, (PathEndMode) 2, (TraverseMode) 1, (Danger) 3);
            })))
            {
              num += thing.stackCount;
              if (num >= countToTransfer)
                break;
            }
          }
          if (num < countToTransfer)
          {
            Messages.Message(TaggedString.op_Implicit(countToTransfer == 1 ? TranslatorFormattedStringExtensions.Translate("CaravanItemIsUnreachableSingle", NamedArgument.op_Implicit(((Def) ((Transferable) transferable).ThingDef).label)) : TranslatorFormattedStringExtensions.Translate("CaravanItemIsUnreachableMulti", NamedArgument.op_Implicit(countToTransfer), NamedArgument.op_Implicit(((Def) ((Transferable) transferable).ThingDef).label))), MessageTypeDefOf.RejectInput, false);
            return false;
          }
        }
      }
    }
    return true;

    static bool CanReachUnspawned(
      Pawn pawn,
      Thing thing,
      PathEndMode peMode = 2,
      TraverseMode mode = 1,
      Danger maxDanger = 3)
    {
      VehiclePawn vehicle = pawn.GetVehicle();
      if (vehicle == null)
        return false;
      return CaravanFormation.formation.Map.reachability.CanReach(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(thing.Position), peMode, new TraverseParms()
      {
        maxDanger = maxDanger,
        mode = mode,
        canBashDoors = false,
        canBashFences = false,
        alwaysUseAvoidGrid = false,
        fenceBlocked = false
      });
    }
  }

  private static bool TryFindExitSpot(
    List<Pawn> pawns,
    bool reachableForEveryColonist,
    out IntVec3 spot)
  {
    Rot4 exitDirection1;
    Rot4 exitDirection2;
    CaravanExitMapUtility.GetExitMapEdges(Find.WorldGrid, CaravanFormation.formation.Dialog.CurrentTile, CaravanFormation.formation.StartingTile, ref exitDirection1, ref exitDirection2);
    bool exitSpot = Rot4.op_Inequality(exitDirection1, Rot4.Invalid) && CaravanFormation.TryFindExitSpot(pawns, reachableForEveryColonist, exitDirection1, out spot) || Rot4.op_Inequality(exitDirection2, Rot4.Invalid) && CaravanFormation.TryFindExitSpot(pawns, reachableForEveryColonist, exitDirection2, out spot) || CaravanFormation.TryFindExitSpot(pawns, reachableForEveryColonist, ((Rot4) ref exitDirection1).Rotated((RotationDirection) 1), out spot) || CaravanFormation.TryFindExitSpot(pawns, reachableForEveryColonist, ((Rot4) ref exitDirection1).Rotated((RotationDirection) 3), out spot);
    CaravanFormation.formation.LeadVehicle.ClampToMap(ref spot, CaravanFormation.formation.Map);
    return exitSpot;
  }

  private static bool TryFindExitSpot(
    List<Pawn> pawns,
    bool reachableForEveryColonist,
    Rot4 exitDirection,
    out IntVec3 spot)
  {
    spot = IntVec3.Invalid;
    PlanetTile startingTile = CaravanFormation.formation.StartingTile;
    if (((PlanetTile) ref startingTile).Valid)
      return CaravanFormation.TryFindExitSpot(CaravanFormation.formation.Map, pawns, reachableForEveryColonist, exitDirection, out spot, false);
    Log.Error("Can't find exit spot because startingTile is not set.");
    return ((IntVec3) ref spot).IsValid;
  }

  private static bool TryFindExitSpot(
    Map map,
    List<Pawn> pawns,
    bool reachableForEveryColonist,
    Rot4 exitDirection,
    out IntVec3 spot,
    bool _)
  {
    return CaravanFormation.TryFindExitSpot(map, pawns, reachableForEveryColonist, exitDirection, out spot);
  }

  private static bool TryFindExitSpot(
    Map map,
    List<Pawn> pawns,
    bool reachableForEveryColonist,
    Rot4 exitDirection,
    out IntVec3 spot)
  {
    if (reachableForEveryColonist)
      return CellFinderExtended.TryFindRandomEdgeCellWith(new Predicate<IntVec3>(CellValidator), map, exitDirection, CaravanFormation.formation.LeadVehicle.VehicleDef, CellFinder.EdgeRoadChance_Always, out spot);
    IntVec3 intVec3 = IntVec3.Invalid;
    int num1 = -1;
    List<IntVec3> list;
    using (GlobalObjectPool.Get<IntVec3>(out list))
    {
      CellRect cellRect = CellRect.WholeMap(map);
      foreach (IntVec3 cell1 in GenCollection.InRandomOrder<IntVec3>(((CellRect) ref cellRect).GetEdgeCells(exitDirection), (IList<IntVec3>) list))
      {
        IntVec3 cell2 = cell1.PadForHitbox(map, CaravanFormation.formation.LeadVehicle);
        if (ValidForAllVehicles(map, cell2))
        {
          int num2 = 0;
          foreach (Pawn pawn in pawns)
          {
            if (pawn.IsColonist && !pawn.Downed && !CaravanHelper.assignedSeats.IsAssigned(pawn) && ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(cell2), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0))
              ++num2;
          }
          if (num2 > num1)
          {
            num1 = num2;
            intVec3 = cell2;
          }
        }
      }
      spot = intVec3;
      return ((IntVec3) ref intVec3).IsValid;
    }

    static bool ValidForAllVehicles(Map map, IntVec3 cell)
    {
      foreach (VehiclePawn vehicle in CaravanFormation.formation.vehicles)
      {
        if (!ValidVehicleExitSpot(cell, vehicle, map))
          return false;
      }
      return true;
    }

    static bool ValidVehicleExitSpot(IntVec3 cell, VehiclePawn vehicle, Map map)
    {
      return !GridsUtility.Fogged(cell, map) && vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(cell), (PathEndMode) 1, (Danger) 3, (TraverseMode) 0) && vehicle.DrivableRectOnCell(cell, Ext_Vehicles.DestinationHitboxReq.AnyRotation);
    }

    bool CellValidator(IntVec3 exitSpot)
    {
      foreach (VehiclePawn vehicle in CaravanFormation.formation.vehicles)
      {
        if (!ValidVehicleExitSpot(exitSpot, vehicle, map))
          return false;
      }
      foreach (Pawn pawn in pawns)
      {
        if (pawn.IsColonist && !CaravanHelper.assignedSeats.IsAssigned(pawn) && !ReachabilityUtility.CanReach(pawn, LocalTargetInfo.op_Implicit(exitSpot), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0))
          return false;
      }
      return true;
    }
  }

  private static bool TryFindRandomPackingSpot(IntVec3 exitSpot, out IntVec3 packingSpot)
  {
    List<Thing> list;
    using (GlobalObjectPool.Get<Thing>(out list))
    {
      List<Thing> thingList = CaravanFormation.formation.Map.listerThings.ThingsOfDef(ThingDefOf.CaravanPackingSpot);
      if (CaravanFormation.formation.Dialog.transferables.NotNullAndAny<TransferableOneWay>((Predicate<TransferableOneWay>) (x => ((Transferable) x).ThingDef.category == 1 && ((Transferable) x).AnyThing.IsBoat())))
      {
        TraverseParms traverseParms = TraverseParms.For((TraverseMode) 1, (Danger) 3, false, false, false, true, false);
        foreach (Thing thing in thingList)
        {
          foreach (VehiclePawn vehicle in CaravanFormation.formation.vehicles)
          {
            if (CaravanFormation.formation.Map.reachability.CanReach(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(thing), (PathEndMode) 1, traverseParms))
              list.Add(thing);
          }
        }
        if (list.Count > 0)
        {
          Thing thing = GenCollection.RandomElement<Thing>((IEnumerable<Thing>) list);
          packingSpot = thing.Position;
          return true;
        }
        bool randomPackingSpot = CellFinder.TryFindRandomCellNear(((Thing) CaravanFormation.formation.LeadVehicle).Position, CaravanFormation.formation.Map, 15, new Predicate<IntVec3>(Validator), ref packingSpot, -1);
        if (!randomPackingSpot)
          randomPackingSpot = CellFinder.TryFindRandomCellNear(((Thing) CaravanFormation.formation.LeadVehicle).Position, CaravanFormation.formation.Map, 25, new Predicate<IntVec3>(ValidatorRelaxed), ref packingSpot, -1);
        if (!randomPackingSpot)
        {
          Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_PackingSpotNotFound")), MessageTypeDefOf.CautionInput, false);
          randomPackingSpot = RCellFinder.TryFindRandomSpotJustOutsideColony(((Thing) CaravanFormation.formation.LeadVehicle).Position, CaravanFormation.formation.Map, ref packingSpot);
        }
        return randomPackingSpot;
      }
      TraverseParms traverseParms1 = TraverseParms.For((TraverseMode) 1, (Danger) 3, false, false, false, true, false);
      foreach (Thing thing in thingList)
      {
        if (CaravanFormation.formation.Map.reachability.CanReach(exitSpot, LocalTargetInfo.op_Implicit(thing), (PathEndMode) 1, traverseParms1))
          list.Add(thing);
      }
      if (list.Count <= 0)
        return RCellFinder.TryFindRandomSpotJustOutsideColony(exitSpot, CaravanFormation.formation.Map, ref packingSpot);
      Thing thing1 = GenCollection.RandomElement<Thing>((IEnumerable<Thing>) list);
      packingSpot = thing1.Position;
      return true;
    }

    static bool Validator(IntVec3 cell)
    {
      return GenGrid.InBounds(cell, CaravanFormation.formation.Map) && GenGrid.Standable(cell, CaravanFormation.formation.Map) && NotUnderVehicle(cell) && !CaravanFormation.formation.Map.terrainGrid.TerrainAt(cell).IsWater;
    }

    static bool ValidatorRelaxed(IntVec3 cell)
    {
      return GenGrid.InBounds(cell, CaravanFormation.formation.Map) && GenGrid.Standable(cell, CaravanFormation.formation.Map);
    }

    static bool NotUnderVehicle(IntVec3 cell)
    {
      List<Thing> thingList = GridsUtility.GetThingList(cell, CaravanFormation.formation.Map);
      return thingList != null && thingList.Exists(new Predicate<Thing>(ThingIsVehicle));
    }

    static bool ThingIsVehicle(Thing thing) => thing is VehiclePawn;
  }
}
