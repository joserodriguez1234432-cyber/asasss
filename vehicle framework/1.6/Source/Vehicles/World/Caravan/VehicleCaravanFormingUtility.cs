// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravanFormingUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public static class VehicleCaravanFormingUtility
{
  public static void StartFormingCaravan(
    [NotNull] List<TransferableOneWay> transferables,
    in IntVec3 meetingPoint,
    in IntVec3 exitSpot,
    in PlanetTile startingTile,
    in PlanetTile destinationTile)
  {
    List<VehiclePawn> vehicles = new List<VehiclePawn>();
    List<Pawn> pawns = new List<Pawn>();
    List<TransferableOneWay> transferables1 = new List<TransferableOneWay>();
    for (int index = transferables.Count - 1; index >= 0; --index)
    {
      TransferableOneWay transferable = transferables[index];
      if (((Transferable) transferable).CountToTransfer != 0)
      {
        switch (((Transferable) transferable).AnyThing)
        {
          case VehiclePawn vehiclePawn:
            vehicles.Add(vehiclePawn);
            continue;
          case Pawn pawn:
            pawns.Add(pawn);
            continue;
          default:
            transferables1.Add(transferable);
            continue;
        }
      }
    }
    VehicleCaravanFormingUtility.StartFormingCaravan(vehicles, pawns, transferables1, in meetingPoint, in exitSpot, in startingTile, in destinationTile);
  }

  public static void StartFormingCaravan(
    [NotNull] List<VehiclePawn> vehicles,
    [NotNull] List<Pawn> pawns,
    [NotNull] List<TransferableOneWay> transferables,
    in IntVec3 meetingPoint,
    in IntVec3 exitSpot,
    in PlanetTile startingTile,
    in PlanetTile destinationTile)
  {
    if (!((PlanetTile) ref startingTile).Valid)
      Trace.Fail($"Can't start forming caravan because startingTile ({startingTile}) is invalid.");
    else if (vehicles.Count == 0)
    {
      Trace.Fail("Can't start forming caravan with 0 vehicles.");
    }
    else
    {
      foreach (VehiclePawn vehicle in vehicles)
        LordUtility.GetLord((Pawn) vehicle)?.Notify_PawnLost((Pawn) vehicle, (PawnLostCondition) 9, new DamageInfo?());
      foreach (Pawn pawn in pawns)
        LordUtility.GetLord(pawn)?.Notify_PawnLost(pawn, (PawnLostCondition) 9, new DamageInfo?());
      LordMaker.MakeNewLord(Faction.OfPlayer, (LordJob) new LordJob_FormAndSendVehicles(vehicles, pawns, transferables, meetingPoint, exitSpot, startingTile, destinationTile), ((Thing) pawns[0]).MapHeld, ((IEnumerable<Pawn>) vehicles).Concat<Pawn>((IEnumerable<Pawn>) pawns));
      foreach (VehiclePawn vehicle in vehicles)
        vehicle.DisembarkAll();
      foreach (Pawn pawn in pawns)
        pawn.jobs.EndCurrentJob((JobCondition) 16 /*0x10*/, true, true);
      LookTargets lookTargets = LookTargets.op_Implicit((Thing) (pawns.FirstOrDefault<Pawn>() ?? (Pawn) vehicles.FirstOrDefault<VehiclePawn>()));
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("CaravanFormationProcessStarted")), lookTargets, MessageTypeDefOf.PositiveEvent, false);
      if (!ModsConfig.BiotechActive || !pawns.Exists((Predicate<Pawn>) (pawn => pawn.RaceProps.IsMechanoid)))
        return;
      LessonAutoActivator.TeachOpportunity(ConceptDefOf.MechsInCaravans, (OpportunityType) 0);
    }
  }

  public static void RemovePawnFromVehicleCaravan(
    Pawn pawn,
    Lord lord,
    PawnLostCondition condition,
    bool removeFromDowned = true)
  {
    bool flag1 = false;
    bool flag2 = condition == 9;
    string str1 = "";
    string str2 = "";
    foreach (Pawn ownedPawn in lord.ownedPawns)
    {
      if (ownedPawn is VehiclePawn vehiclePawn && vehiclePawn.AllPawnsAboard.Contains(pawn))
      {
        TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("VF_PawnBoardedFormingCaravan", NamedArgument.op_Implicit((Thing) pawn), NamedArgument.op_Implicit(((Entity) vehiclePawn).LabelShort));
        str2 = TaggedString.op_Implicit(((TaggedString) ref taggedString).CapitalizeFirst());
        flag2 = true;
        break;
      }
    }
    if (!flag2)
    {
      foreach (Pawn ownedPawn in lord.ownedPawns)
      {
        if (ownedPawn != pawn && CaravanUtility.IsOwner(ownedPawn, Faction.OfPlayer))
        {
          flag1 = true;
          break;
        }
      }
    }
    string str3;
    if (flag1)
    {
      string str4 = str1;
      TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("MessagePawnLostWhileFormingCaravan", NamedArgument.op_Implicit((Thing) pawn));
      string str5 = ((TaggedString) ref taggedString).CapitalizeFirst().ToString();
      str3 = str4 + str5;
    }
    else
    {
      string str6 = str1;
      string str7;
      if (!flag2)
      {
        TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("MessagePawnLostWhileFormingCaravan", NamedArgument.op_Implicit((Thing) pawn));
        taggedString = ((TaggedString) ref taggedString).CapitalizeFirst();
        string str8 = taggedString.ToString();
        taggedString = Translator.Translate("MessagePawnLostWhileFormingCaravan_AllLost");
        string str9 = taggedString.ToString();
        str7 = str8 + str9;
      }
      else
        str7 = str2;
      str3 = str6 + str7;
    }
    bool flag3 = true;
    if (!flag2 && !flag1)
      CaravanFormingUtility.StopFormingCaravan(lord);
    if (flag1)
    {
      pawn.inventory.UnloadEverything = true;
      if (lord.ownedPawns.Contains(pawn))
      {
        lord.Notify_PawnLost(pawn, (PawnLostCondition) 10, new DamageInfo?());
        flag3 = false;
      }
      if (lord.LordJob is LordJob_FormAndSendVehicles lordJob && lordJob.downedPawns.Contains(pawn))
      {
        if (!removeFromDowned)
          flag3 = false;
        else
          lordJob.downedPawns.Remove(pawn);
      }
    }
    if (!flag3)
      return;
    MessageTypeDef messageTypeDef = flag2 ? MessageTypeDefOf.SilentInput : MessageTypeDefOf.NegativeEvent;
    Messages.Message(str3, LookTargets.op_Implicit((Thing) pawn), messageTypeDef, true);
  }
}
