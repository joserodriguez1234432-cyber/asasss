// Decompiled with JetBrains decompiler
// Type: Vehicles.LordJob_FormAndSendVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles.World;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public sealed class LordJob_FormAndSendVehicles : LordJob_FormAndSendCaravan
{
  private static readonly AccessTools.FieldRef<LordJob_FormAndSendCaravan, bool> CaravanSentFieldRef = (AccessTools.FieldRef<LordJob_FormAndSendCaravan, bool>) AccessTools.FieldRefAccess<bool>(typeof (LordJob_FormAndSendCaravan), "caravanSent");
  private static (LordToil toil, string memo) prevState;
  private VehiclePawn leadVehicle;
  public List<Pawn> prisoners = new List<Pawn>();
  public List<VehiclePawn> vehicles = new List<VehiclePawn>();
  public List<Pawn> pawns = new List<Pawn>();
  private Dictionary<Pawn, AssignedSeat> vehicleAssigned = new Dictionary<Pawn, AssignedSeat>();
  private IntVec3 meetingPoint;
  private IntVec3 exitPoint;
  private PlanetTile startingTile;
  private PlanetTile destinationTile;
  private LordToil gatherAnimals;
  private LordToil gatherAnimalsPause;
  private LordToil gatherItems;
  private LordToil gatherItemsPause;
  private LordToil gatherSlaves;
  private LordToil gatherSlavesPause;
  private LordToil gatherDownedPawns;
  private LordToil gatherDownedPawnsPause;
  private LordToil tieAnimals;
  private LordToil tieAnimalsPause;
  private LordToil boardVehicle;
  private LordToil boardVehiclePause;
  private LordToil leave;
  private LordToil leavePause;
  private List<Pawn> tmpPawnAssignments = new List<Pawn>();
  private List<AssignedSeat> tmpVehicleHandlerAssignments = new List<AssignedSeat>();

  public LordJob_FormAndSendVehicles()
  {
  }

  public LordJob_FormAndSendVehicles(
    List<VehiclePawn> vehicles,
    List<Pawn> pawns,
    List<TransferableOneWay> transferables,
    IntVec3 meetingPoint,
    IntVec3 exitPoint,
    PlanetTile startingTile,
    PlanetTile destinationTile)
  {
    this.vehicles = vehicles;
    this.transferables = transferables;
    this.downedPawns = new List<Pawn>();
    foreach (Pawn pawn in pawns)
    {
      if (pawn.Downed)
        this.downedPawns.Add(pawn);
      else if (!pawn.IsColonist && !pawn.RaceProps.Animal)
        this.prisoners.Add(pawn);
      else
        this.pawns.Add(pawn);
    }
    this.meetingPoint = meetingPoint;
    this.exitPoint = exitPoint;
    this.startingTile = startingTile;
    this.destinationTile = destinationTile;
    this.RequireAllSeated = this.vehicles.Exists((Predicate<VehiclePawn>) (vehicle => ((Thing) vehicle).IsBoat()));
    this.vehicleAssigned = new Dictionary<Pawn, AssignedSeat>((IDictionary<Pawn, AssignedSeat>) CaravanHelper.assignedSeats.AllAssignments);
  }

  public bool CaravanSent
  {
    get
    {
      return LordJob_FormAndSendVehicles.CaravanSentFieldRef.Invoke((LordJob_FormAndSendCaravan) this);
    }
    set
    {
      LordJob_FormAndSendVehicles.CaravanSentFieldRef.Invoke((LordJob_FormAndSendCaravan) this) = value;
    }
  }

  private (LordToil source, LordToil pause) GatherAnimals
  {
    get => (this.gatherAnimals, this.gatherAnimalsPause);
  }

  private (LordToil source, LordToil pause) GatherItems
  {
    get => (this.gatherItems, this.gatherItemsPause);
  }

  private (LordToil source, LordToil pause) GatherSlaves
  {
    get => (this.gatherSlaves, this.gatherSlavesPause);
  }

  private (LordToil source, LordToil pause) GatherDowned
  {
    get => (this.gatherDownedPawns, this.gatherDownedPawnsPause);
  }

  private (LordToil source, LordToil pause) TieAnimals => (this.tieAnimals, this.tieAnimalsPause);

  private (LordToil source, LordToil pause) Board => (this.boardVehicle, this.boardVehiclePause);

  private (LordToil source, LordToil pause) Leave => (this.leave, this.leavePause);

  public bool RequireAllSeated { get; private set; }

  public VehiclePawn LeadVehicle
  {
    get
    {
      if (this.leadVehicle == null)
        this.DetermineLeadVehicle();
      return this.leadVehicle;
    }
  }

  public bool GatherItemsNow => ((LordJob) this).lord.CurLordToil == this.gatherItems;

  public virtual bool NeverInRestraints => true;

  public virtual bool AddFleeToil => false;

  public void ForceCaravanLeave() => ((LordJob) this).lord.GotoToil(this.Board.source);

  public AssignedSeat GetVehicleAssigned(Pawn pawn)
  {
    return GenCollection.TryGetValue<Pawn, AssignedSeat>((IReadOnlyDictionary<Pawn, AssignedSeat>) this.vehicleAssigned, pawn, (AssignedSeat) null);
  }

  public bool SeatAssigned(VehiclePawn vehicle, VehicleRoleHandler handler)
  {
    foreach (AssignedSeat assignedSeat in this.vehicleAssigned.Values)
    {
      if (assignedSeat.Vehicle == vehicle && assignedSeat.handler == handler)
        return true;
    }
    return false;
  }

  public bool AssignSeat(Pawn pawn, VehiclePawn vehicle, VehicleRoleHandler handler)
  {
    return this.vehicleAssigned.TryAdd(pawn, new AssignedSeat(pawn, handler));
  }

  private void AssignRemainingPawns()
  {
    if (!this.RequireAllSeated)
      return;
    foreach (Pawn pawn in this.pawns)
    {
      if (!this.vehicleAssigned.ContainsKey(pawn))
      {
        foreach (VehiclePawn vehicle in this.vehicles)
        {
          if (vehicle.SeatsAvailable > 0)
            this.vehicleAssigned[pawn] = new AssignedSeat(pawn, vehicle.GetAnyAvailableHandler());
        }
      }
    }
  }

  private bool AssignSeats(VehiclePawn vehicle)
  {
    int num = vehicle.PawnCountToOperateLeft - this.vehicleAssigned.Values.CountWhere<AssignedSeat>((Predicate<AssignedSeat>) (seat => seat.Vehicle == vehicle));
    for (int index = 0; index < this.pawns.Count && index < num; ++num)
    {
      Pawn pawn = this.pawns[index];
      if (!this.vehicleAssigned.ContainsKey(pawn))
      {
        VehicleRoleHandler availableHandler = vehicle.GetNextAvailableHandler(pawn, HandlingType.Movement);
        this.vehicleAssigned.Add(pawn, new AssignedSeat(pawn, availableHandler));
      }
      ++index;
    }
    return true;
  }

  private void ResolveSeatingAssignments()
  {
    foreach (VehiclePawn vehicle1 in this.vehicles)
    {
      VehiclePawn vehicle = vehicle1;
      if (vehicle.PawnCountToOperateLeft - this.vehicleAssigned.Values.CountWhere<AssignedSeat>((Predicate<AssignedSeat>) (seat => seat.Vehicle == vehicle)) > 0 && !this.AssignSeats(vehicle))
      {
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("VehicleCaravanCanceled")), MessageTypeDefOf.NeutralEvent, true);
        CaravanFormingUtility.StopFormingCaravan(((LordJob) this).lord);
        return;
      }
    }
    this.AssignRemainingPawns();
  }

  private Transition PauseTransition(LordToil from, LordToil to)
  {
    Transition transition = new Transition(from, to, false, true);
    transition.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(Translator.Translate("MessageCaravanFormationPaused")), MessageTypeDefOf.NegativeEvent, (Func<TargetInfo>) (() => TargetInfo.op_Implicit((Thing) GenCollection.FirstOrDefault<Pawn>(((LordJob) this).lord.ownedPawns, (Predicate<Pawn>) (pawn => pawn.InMentalState)))), (string) null, 1f));
    transition.AddTrigger((Trigger) new Trigger_MentalState());
    transition.AddPostAction((TransitionAction) new TransitionAction_EndAllJobs());
    return transition;
  }

  private Transition UnpauseTransition(LordToil from, LordToil to)
  {
    Transition transition = new Transition(from, to, false, true);
    transition.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(Translator.Translate("MessageCaravanFormationUnpaused")), MessageTypeDefOf.SilentInput, (string) null, 1f, (Func<bool>) null));
    transition.AddTrigger((Trigger) new Trigger_NoMentalState());
    transition.AddPostAction((TransitionAction) new TransitionAction_EndAllJobs());
    return transition;
  }

  private void DetermineLeadVehicle()
  {
    this.leadVehicle = GenCollection.MaxBy<VehiclePawn, float>((IEnumerable<VehiclePawn>) this.vehicles, (Func<VehiclePawn, float>) (vehicle =>
    {
      IntVec2 size = ((BuildableDef) vehicle.VehicleDef).Size;
      return ((IntVec2) ref size).Magnitude;
    }));
    if (this.leadVehicle != null)
      return;
    Messages.Message(TaggedString.op_Implicit(Translator.Translate("VehicleCaravanCanceled")), MessageTypeDefOf.NeutralEvent, true);
    CaravanFormingUtility.StopFormingCaravan(((LordJob) this).lord);
  }

  public virtual void Notify_PawnAdded(Pawn pawn)
  {
    base.Notify_PawnAdded(pawn);
    if (pawn is VehiclePawn vehicle)
      VehicleReachabilityUtility.ClearCacheFor(vehicle);
    else
      ReachabilityUtility.ClearCacheFor(pawn);
    this.DetermineLeadVehicle();
  }

  public virtual void Notify_PawnLost(Pawn pawn, PawnLostCondition condition)
  {
    if (pawn is VehiclePawn vehicle)
      VehicleReachabilityUtility.ClearCacheFor(vehicle);
    else
      ReachabilityUtility.ClearCacheFor(pawn);
    if (!this.CaravanSent)
    {
      if (condition == 2 || condition == 3 && pawn.Downed)
        this.downedPawns.Add(pawn);
      VehicleCaravanFormingUtility.RemovePawnFromVehicleCaravan(pawn, ((LordJob) this).lord, condition, false);
      ((LordJob) this).lord.ReceiveMemo("RemovedPawn");
    }
    this.DetermineLeadVehicle();
  }

  public virtual bool CanOpenAnyDoor(Pawn p) => true;

  public virtual void LordJobTick()
  {
    if (VehicleMod.settings.debug.debugDrawLordMeetingPoint && Find.TickManager.TicksGame % 10 == 0 && ((LordJob) this).lord.CurLordToil is IDebugLordMeetingPoint curLordToil)
    {
      ((LordJob) this).lord.Map.debugDrawer.FlashCell(curLordToil.MeetingPoint, 0.95f, (string) null, 10);
      ((LordJob) this).lord.Map.debugDrawer.FlashLine(curLordToil.MeetingPoint, ((Thing) this.LeadVehicle).Position, 10, (SimpleColor) 4);
    }
    for (int index = this.downedPawns.Count - 1; index >= 0; --index)
    {
      if (((Thing) this.downedPawns[index]).Destroyed)
        this.downedPawns.RemoveAt(index);
      else if (!this.downedPawns[index].Downed)
      {
        ((LordJob) this).lord.AddPawn(this.downedPawns[index]);
        this.downedPawns.RemoveAt(index);
      }
    }
    if (((LordJob) this).lord.ownedPawns.NotNullAndAny<Pawn>((Predicate<Pawn>) (x => x is VehiclePawn)))
      return;
    ((LordJob) this).lord.lordManager.RemoveLord(((LordJob) this).lord);
    Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_CaravanTerminatedNoVehicles")), MessageTypeDefOf.NegativeEvent, true);
  }

  public virtual string GetReport(Pawn pawn)
  {
    return TaggedString.op_Implicit(Translator.Translate("LordReportFormingCaravan"));
  }

  private void SendCaravan()
  {
    this.CaravanSent = true;
    CaravanHelper.ExitMapAndCreateVehicleCaravan(((LordJob) this).lord.ownedPawns.Concat<Pawn>(this.downedPawns.Where<Pawn>((Func<Pawn, bool>) (pawn => JobGiver_PrepareCaravan_GatherDownedPawns.IsDownedPawnNearExitPoint(pawn, this.exitPoint)))), ((LordJob) this).lord.faction, ((LordJob) this).Map.Tile, this.startingTile, this.destinationTile);
  }

  public virtual StateGraph CreateGraph()
  {
    StateGraph stateGraph = new StateGraph();
    this.ResolveSeatingAssignments();
    this.gatherAnimals = (LordToil) new LordToil_PrepareCaravan_GatherAnimalsForVehicles(this.meetingPoint);
    this.gatherAnimalsPause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.gatherItems = (LordToil) new LordToil_PrepareCaravan_GatherCargo(this.meetingPoint);
    this.gatherItemsPause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.gatherSlaves = (LordToil) new LordToil_PrepareCaravan_GatherSlavesVehicle(this.meetingPoint);
    this.gatherSlavesPause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.gatherDownedPawns = (LordToil) new LordToil_PrepareCaravan_GatherDownedPawnsVehicle(this.meetingPoint);
    this.gatherDownedPawnsPause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.tieAnimals = (LordToil) new LordToil_PrepareCaravan_TieAnimalsToVehicle(this.meetingPoint);
    this.tieAnimalsPause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.boardVehicle = (LordToil) new LordToil_PrepareCaravan_BoardVehicles(this.exitPoint);
    this.boardVehiclePause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.leave = (LordToil) new LordToil_PrepareCaravan_LeaveWithVehicles(this.exitPoint);
    this.leavePause = (LordToil) new LordToil_PrepareCaravan_Pause();
    this.AddToStateGraph(stateGraph, this.GatherAnimals, "AllAnimalsGathered", postActions: new TransitionAction[1]
    {
      (TransitionAction) new TransitionAction_EndAllJobs()
    });
    this.AddToStateGraph(stateGraph, this.GatherItems, "AllItemsGathered", postActions: new TransitionAction[1]
    {
      (TransitionAction) new TransitionAction_EndAllJobs()
    });
    this.AddToStateGraph(stateGraph, this.GatherDowned, "AllDownedPawnsGathered");
    this.AddToStateGraph(stateGraph, this.Board, "AllPawnsOnboard", new TransitionAction[1]
    {
      (TransitionAction) new TransitionAction_EndAllJobs()
    }, new TransitionAction[1]
    {
      (TransitionAction) new TransitionAction_EndAllJobs()
    });
    this.AddToStateGraph(stateGraph, this.Leave);
    LordToil_End lordToilEnd = new LordToil_End();
    stateGraph.AddToil((LordToil) lordToilEnd);
    Transition transition = new Transition(this.Leave.source, (LordToil) lordToilEnd, false, true);
    transition.AddTrigger((Trigger) new Trigger_Memo("ReadyToExitMap"));
    transition.AddPreAction((TransitionAction) new TransitionAction_Custom(new Action(this.SendCaravan)));
    stateGraph.AddTransition(transition, false);
    return stateGraph;
  }

  public void AddToStateGraph(
    StateGraph stateGraph,
    (LordToil source, LordToil pause) toil,
    string memo = null,
    TransitionAction[] preActions = null,
    TransitionAction[] postActions = null)
  {
    stateGraph.AddToil(toil.source);
    stateGraph.AddToil(toil.pause);
    if (LordJob_FormAndSendVehicles.prevState.toil != null)
    {
      Transition transition1 = new Transition(LordJob_FormAndSendVehicles.prevState.toil, toil.source, false, true);
      if (!GenText.NullOrEmpty(LordJob_FormAndSendVehicles.prevState.memo))
        transition1.AddTrigger((Trigger) new Trigger_Memo(LordJob_FormAndSendVehicles.prevState.memo));
      if (!GenList.NullOrEmpty<TransitionAction>((IList<TransitionAction>) preActions))
      {
        foreach (TransitionAction preAction in preActions)
          transition1.AddPreAction(preAction);
      }
      if (!GenList.NullOrEmpty<TransitionAction>((IList<TransitionAction>) postActions))
      {
        foreach (TransitionAction postAction in postActions)
          transition1.AddPostAction(postAction);
      }
      stateGraph.AddTransition(transition1, false);
      Transition transition2 = this.PauseTransition(toil.source, toil.pause);
      Transition transition3 = this.UnpauseTransition(toil.pause, toil.source);
      stateGraph.AddTransition(transition2, false);
      stateGraph.AddTransition(transition3, false);
    }
    LordJob_FormAndSendVehicles.prevState = (toil.source, memo);
  }

  public virtual void ExposeData()
  {
    Scribe_Collections.Look<TransferableOneWay>(ref this.transferables, "transferables", (LookMode) 2, Array.Empty<object>());
    Scribe_Collections.Look<Pawn>(ref this.downedPawns, "downedPawns", (LookMode) 3, Array.Empty<object>());
    Scribe_Collections.Look<Pawn>(ref this.prisoners, "prisoners", (LookMode) 3, Array.Empty<object>());
    Scribe_Collections.Look<VehiclePawn>(ref this.vehicles, "vehicles", (LookMode) 3, Array.Empty<object>());
    Scribe_Collections.Look<Pawn>(ref this.pawns, "pawns", (LookMode) 3, Array.Empty<object>());
    Scribe_Values.Look<IntVec3>(ref this.meetingPoint, "meetingPoint", new IntVec3(), false);
    Scribe_Values.Look<IntVec3>(ref this.exitPoint, "exitPoint", new IntVec3(), false);
    Scribe_Values.Look<PlanetTile>(ref this.startingTile, "startingTile", new PlanetTile(), false);
    Scribe_Values.Look<PlanetTile>(ref this.destinationTile, "destinationTile", new PlanetTile(), false);
    Scribe_Collections.Look<Pawn, AssignedSeat>(ref this.vehicleAssigned, "vehicleAssigned", (LookMode) 3, (LookMode) 2, ref this.tmpPawnAssignments, ref this.tmpVehicleHandlerAssignments, true, false, false);
    if (Scribe.mode != 4)
      return;
    this.RequireAllSeated = this.vehicles.Exists((Predicate<VehiclePawn>) (vehicle => ((Thing) vehicle).IsBoat()));
  }
}
