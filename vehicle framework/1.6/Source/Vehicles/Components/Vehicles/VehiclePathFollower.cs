// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePathFollower
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class VehiclePathFollower : IExposable
{
  public const int MaxMoveTicks = 450;
  public const float SnowReductionFromWalking = 0.001f;
  public const int ClamorCellsInterval = 12;
  public const int MinCostWalk = 50;
  public const int MinCostAmble = 60;
  public const int MinCheckAheadNodes = 1;
  public const int MaxCheckAheadNodes = 5;
  public const int TicksWhileWaiting = 10;
  public const int CheckAheadNodesForCollisions = 3;
  public const int MaxCheckAheadNodesForCollisions = 8;
  private static readonly HashSet<IntVec3> CollisionCells = new HashSet<IntVec3>();
  protected VehiclePawn vehicle;
  private List<IntVec3> bumperCells;
  private bool moving;
  public IntVec3 nextCell;
  private IntVec3 lastCell;
  public IntVec3 lastPathedTargetPosition;
  private LocalTargetInfo destination;
  public float nextCellCostLeft;
  public float nextCellCostTotal = 1f;
  private int cellsUntilClamor;
  private int lastMovedTick = -999999;
  private int waitTicks;
  public VehiclePath curPath;
  private PathEndMode peMode;
  private Rot8 endRot = Rot8.Invalid;
  private CancellationTokenSource pathCancellationTokenSource = new CancellationTokenSource();
  private Task curPathTask;
  private bool shouldStopClipping;

  public VehiclePathFollower(VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    this.bumperCells = new List<IntVec3>();
    this.shouldStopClipping = vehicle.VehicleDef.size.x != vehicle.VehicleDef.size.z;
    this.LookAheadStartingIndex = Mathf.CeilToInt((float) ((BuildableDef) vehicle.VehicleDef).Size.z / 2f);
    this.LookAheadDistance = 1 + this.LookAheadStartingIndex;
    this.CollisionsLookAheadStartingIndex = Mathf.CeilToInt((float) ((BuildableDef) vehicle.VehicleDef).Size.z / 2f);
    this.CollisionsLookAheadDistance = 3 + this.CollisionsLookAheadStartingIndex;
  }

  public int LookAheadDistance { get; private set; }

  public int LookAheadStartingIndex { get; private set; }

  public int CollisionsLookAheadDistance { get; private set; }

  public int CollisionsLookAheadStartingIndex { get; private set; }

  public VehiclePathFollower.PathRequestStatus RequestStatus { get; internal set; }

  public LocalTargetInfo Destination => this.destination;

  public bool Moving => this.moving;

  public bool Waiting => this.waitTicks > 0;

  public IntVec3 LastPassableCellInPath
  {
    get
    {
      if (!this.Moving || this.curPath == null)
        return IntVec3.Invalid;
      LocalTargetInfo destination1 = this.Destination;
      if (!GenGrid.Impassable(((LocalTargetInfo) ref destination1).Cell, ((Thing) this.vehicle).Map))
      {
        LocalTargetInfo destination2 = this.Destination;
        return ((LocalTargetInfo) ref destination2).Cell;
      }
      foreach (IntVec3 node in (IEnumerable<IntVec3>) this.curPath.Nodes)
      {
        if (!GenGrid.Impassable(node, ((Thing) this.vehicle).Map))
          return node;
      }
      return GenGrid.Impassable(((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map) ? IntVec3.Invalid : ((Thing) this.vehicle).Position;
    }
  }

  public void RecalculatePermissions()
  {
    if (!this.Moving || this.vehicle.CanMoveFinal && this.vehicle.Drafted)
      return;
    this.PatherFailed();
  }

  public void SetEndRotation(Rot8 rot) => this.endRot = rot;

  public void ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.moving, "moving", false, false);
    Scribe_Values.Look<IntVec3>(ref this.nextCell, "nextCell", new IntVec3(), false);
    Scribe_Values.Look<float>(ref this.nextCellCostLeft, "nextCellCostLeft", 0.0f, false);
    Scribe_Values.Look<float>(ref this.nextCellCostTotal, "nextCellCostTotal", 0.0f, false);
    Scribe_Values.Look<PathEndMode>(ref this.peMode, "peMode", (PathEndMode) 0, false);
    Scribe_Values.Look<int>(ref this.cellsUntilClamor, "cellsUntilClamor", 0, false);
    Scribe_Values.Look<int>(ref this.lastMovedTick, "lastMovedTick", -999999, false);
    if (this.moving)
      Scribe_TargetInfo.Look(ref this.destination, "destination");
    if (Scribe.mode != 4)
      return;
    this.vehicle.animator?.SetBool(PropertyIds.Moving, this.moving);
  }

  public void StartPath(LocalTargetInfo dest, PathEndMode peMode, bool ignoreReachability = false)
  {
    if (!this.vehicle.Drafted)
    {
      this.PatherFailed();
    }
    else
    {
      dest = TargetInfo.op_Explicit(GenPathVehicles.ResolvePathMode(this.vehicle.VehicleDef, ((Thing) this.vehicle).Map, ((LocalTargetInfo) ref dest).ToTargetInfo(((Thing) this.vehicle).Map), ref peMode));
      if (((LocalTargetInfo) ref dest).HasThing && ((LocalTargetInfo) ref dest).ThingDestroyed)
      {
        Log.Error($"{((object) this.vehicle)?.ToString()} pathing to destroyed thing {((LocalTargetInfo) ref dest).Thing?.ToString()}");
        this.PatherFailed();
      }
      else if (!((Thing) this.vehicle).Position.Walkable(this.vehicle.VehicleDef, ((Thing) this.vehicle).Map) && !this.TryRecoverFromUnwalkablePosition())
        this.PatherFailed();
      else if (this.Moving && this.curPath != null && LocalTargetInfo.op_Equality(this.destination, dest) && this.peMode == peMode)
        this.PatherFailed();
      else if (!ignoreReachability && !((Thing) this.vehicle).Map.GetCachedMapComponent<VehiclePathingSystem>()[this.vehicle.VehicleDef].VehicleReachability.CanReachVehicle(((Thing) this.vehicle).Position, dest, peMode, TraverseParms.For((TraverseMode) 0, (Danger) 3, false, false, false, true, false)))
      {
        this.PatherFailed();
      }
      else
      {
        this.peMode = peMode;
        this.destination = dest;
        PawnDestinationReservationManager.PawnDestinationReservation destinationReservation = ((Thing) this.vehicle).Map.pawnDestinationReservationManager.MostRecentReservationFor((Pawn) this.vehicle);
        if (destinationReservation != null)
        {
          LocalTargetInfo destination = this.Destination;
          if (((LocalTargetInfo) ref destination).HasThing)
          {
            IntVec3 target = destinationReservation.target;
            destination = this.Destination;
            IntVec3 cell = ((LocalTargetInfo) ref destination).Cell;
            if (IntVec3.op_Inequality(target, cell))
              goto label_15;
          }
          if (destinationReservation.job != this.vehicle.CurJob)
          {
            IntVec3 target = destinationReservation.target;
            destination = this.Destination;
            IntVec3 cell = ((LocalTargetInfo) ref destination).Cell;
            if (!IntVec3.op_Inequality(target, cell))
              goto label_16;
          }
          else
            goto label_16;
label_15:
          ((Thing) this.vehicle).Map.pawnDestinationReservationManager.ObsoleteAllClaimedBy((Pawn) this.vehicle);
        }
label_16:
        if (this.AtDestinationPosition())
        {
          this.PatherArrived();
        }
        else
        {
          this.curPath?.Dispose();
          this.curPath = (VehiclePath) null;
          this.moving = true;
          this.vehicle.animator?.SetBool(PropertyIds.Moving, this.moving);
          this.vehicle.EventRegistry[VehicleEventDefOf.MoveStart].ExecuteEvents();
        }
      }
    }
  }

  public void StopDead()
  {
    if (!((Thing) this.vehicle).Spawned)
      return;
    if (this.curPath != null)
    {
      this.vehicle.EventRegistry[VehicleEventDefOf.MoveStop].ExecuteEvents();
      this.curPath.Dispose();
    }
    this.curPath = (VehiclePath) null;
    this.moving = false;
    this.vehicle.animator?.SetBool(PropertyIds.Moving, this.moving);
    this.nextCell = ((Thing) this.vehicle).Position;
  }

  [Profile]
  public void PatherTick()
  {
    if ((!this.vehicle.Drafted || !this.vehicle.CanMoveFinal) && this.curPath != null)
    {
      this.PatherFailed();
    }
    else
    {
      if (this.vehicle.stances.stunner.Stunned)
        return;
      if (VehicleMod.settings.debug.debugDrawBumpers)
        GenDraw.DrawFieldEdges(this.bumperCells, 2900);
      this.lastMovedTick = Find.TickManager.TicksGame;
      if ((double) this.nextCellCostLeft > 0.0)
      {
        this.nextCellCostLeft -= this.CostToPayThisTick();
      }
      else
      {
        if (!this.moving)
          return;
        this.TryEnterNextPathCell();
      }
    }
  }

  public void TryResumePathingAfterLoading()
  {
    if (!this.moving)
      return;
    this.StartPath(this.destination, this.peMode, true);
  }

  public void Notify_Teleported()
  {
    this.StopDead();
    this.ResetToCurrentPosition();
  }

  public void ResetToCurrentPosition()
  {
    this.nextCell = ((Thing) this.vehicle).Position;
    this.nextCellCostLeft = 0.0f;
    this.nextCellCostTotal = 1f;
  }

  public Building BuildingBlockingNextPathCell()
  {
    Building edifice = GridsUtility.GetEdifice(this.nextCell, ((Thing) this.vehicle).Map);
    return edifice != null && ((Thing) edifice).BlocksPawn((Pawn) this.vehicle) ? edifice : (Building) null;
  }

  private bool AtDestinationPosition()
  {
    return this.vehicle.CanReachImmediateVehicle(this.destination, this.peMode);
  }

  public void PatherDraw()
  {
    if (this.curPath == null || ((Thing) this.vehicle).Faction != Faction.OfPlayer && !DebugViewSettings.drawPaths || !Find.Selector.IsSelected((object) this.vehicle))
      return;
    this.curPath.DrawPath(this.vehicle);
  }

  public bool TryRecoverFromUnwalkablePosition(bool error = true)
  {
    bool flag = false;
    foreach (IntVec3 intVec3 in GenRadial.RadialPattern)
    {
      IntVec3 cell = IntVec3.op_Addition(((Thing) this.vehicle).Position, intVec3);
      if (!this.vehicle.Drivable(cell))
      {
        if (IntVec3.op_Equality(cell, ((Thing) this.vehicle).Position))
          return true;
        if (error)
          Log.Warning($"{this.vehicle} on impassable cell {((Thing) this.vehicle).Position}. Teleporting to {cell}");
        ((Thing) this.vehicle).Position = cell;
        this.vehicle.Notify_Teleported();
        flag = true;
        break;
      }
    }
    if (!flag)
      Log.Error($"{this.vehicle} on impassable cell {((Thing) this.vehicle).Position}. Cound not find nearby position to teleport to.");
    return flag;
  }

  private void PatherArrived()
  {
    if (this.endRot.IsValid)
      this.vehicle.FullRotation = this.endRot;
    this.StopDead();
    if (this.vehicle.jobs.curJob == null)
      return;
    this.vehicle.jobs.curDriver.Notify_PatherArrived();
  }

  public void PatherFailed()
  {
    if (this.RequestStatus == VehiclePathFollower.PathRequestStatus.Calculating)
      this.pathCancellationTokenSource.Cancel();
    this.StopDead();
    this.SetEndRotation(Rot8.Invalid);
    this.vehicle.jobs?.curDriver?.Notify_PatherFailed();
    this.RequestStatus = VehiclePathFollower.PathRequestStatus.None;
  }

  public void EngageBrakes()
  {
    this.vehicle.EventRegistry[VehicleEventDefOf.Braking].ExecuteEvents();
    this.PatherFailed();
  }

  private void SetBumperCells()
  {
    Rot8 rot8 = Ext_Map.DirectionToCell(((Thing) this.vehicle).Position, this.nextCell);
    if (!rot8.IsValid)
      rot8 = this.vehicle.FullRotation;
    CellRect cellRect = rot8.IsDiagonal ? this.vehicle.MinRectShifted(new IntVec2(0, 2), new Rot4?((Rot4) rot8)) : this.vehicle.OccupiedRectShifted(new IntVec2(0, 2), new Rot4?((Rot4) rot8));
    List<IntVec3> intVec3List = new List<IntVec3>();
    foreach (IntVec3 intVec3 in cellRect)
      intVec3List.Add(intVec3);
    this.bumperCells = intVec3List;
  }

  private void TryEnterNextPathCell()
  {
    if (this.waitTicks > 0)
    {
      --this.waitTicks;
    }
    else
    {
      if (this.RequestStatus == VehiclePathFollower.PathRequestStatus.Calculating)
        return;
      if (this.vehicle.beached)
      {
        this.vehicle.BeachShip();
        ((Thing) this.vehicle).Position = this.nextCell;
        double angle = (double) this.vehicle.CalculateAngle();
        this.PatherFailed();
      }
      else
      {
        switch (this.NeedNewPath())
        {
          case VehiclePathFollower.PathRequest.None:
            if (this.curPath == null)
              break;
            if (VehicleMod.settings.main.runOverPawns)
            {
              float moveSpeed = (float) (1.0 / ((double) this.nextCellCostTotal / 60.0 / (double) this.CostToPayThisTick()));
              if (this.vehicle.FullRotation.IsDiagonal)
                moveSpeed *= Ext_Math.Sqrt2;
              this.WarnPawnsImpendingCollision();
              this.vehicle.CheckForCollisions(moveSpeed);
            }
            this.UpdateVehiclePosition();
            if (this.AtDestinationPosition())
            {
              this.PatherArrived();
              break;
            }
            this.SetupMoveIntoNextCell();
            break;
          case VehiclePathFollower.PathRequest.Wait:
            this.waitTicks = 10;
            break;
          case VehiclePathFollower.PathRequest.Fail:
            this.PatherFailed();
            break;
          case VehiclePathFollower.PathRequest.NeedNew:
            this.RequestNewPath();
            goto case VehiclePathFollower.PathRequest.None;
          default:
            throw new NotImplementedException("TryEnterNextPathCell.PathRequest");
        }
      }
    }
  }

  private void UpdateVehiclePosition()
  {
    if (IntVec3.op_Equality(((Thing) this.vehicle).Position, this.nextCell))
      return;
    CellRect cellRect = GenAdj.OccupiedRect((Thing) this.vehicle);
    this.lastCell = ((Thing) this.vehicle).Position;
    ((Thing) this.vehicle).Position = this.nextCell;
    double angle = (double) this.vehicle.CalculateAngle();
    foreach (IntVec3 intVec3 in cellRect.AllCellsNoRepeat(GenAdj.OccupiedRect((Thing) this.vehicle)))
      ((Thing) this.vehicle).Map.pathing.RecalculatePerceivedPathCostAt(intVec3);
  }

  private void SetupMoveIntoNextCell()
  {
    if (this.curPath.NodesLeft <= 1)
    {
      Log.Error($"{this.vehicle} at {((Thing) this.vehicle).Position} ran out of path nodes while pathing to {this.destination}.");
      this.PatherFailed();
    }
    else
    {
      this.nextCell = this.curPath.ConsumeNextNode();
      if (!this.vehicle.DrivableFast(this.nextCell))
      {
        Log.Error($"{this.vehicle} entering {this.nextCell} which is impassable.");
        this.PatherFailed();
      }
      else
      {
        Rot4 cell1 = (Rot4) Ext_Map.DirectionToCell(((Thing) this.vehicle).Position, this.nextCell);
        if (((Rot4) ref cell1).IsValid && ((IEnumerable<IntVec3>) (object) this.vehicle.PawnOccupiedCells(this.nextCell, cell1)).Any<IntVec3>((Func<IntVec3, bool>) (cell => !GenGrid.InBounds(cell, ((Thing) this.vehicle).Map))))
          this.PatherFailed();
        else if (this.shouldStopClipping && this.curPath.NodesLeft < this.LookAheadStartingIndex && this.vehicle.LocationRestrictedBySize(((Thing) this.vehicle).Map, this.nextCell, this.vehicle.FullRotation))
        {
          this.PatherFailed();
        }
        else
        {
          float moveIntoCell = VehiclePathFollower.CostToMoveIntoCell(this.vehicle, ((Thing) this.vehicle).Position, this.nextCell);
          this.nextCellCostTotal = moveIntoCell;
          this.nextCellCostLeft = moveIntoCell;
          this.SetBumperCells();
        }
      }
    }
  }

  public static float MoveTicksAt(VehiclePawn vehicle, IntVec3 from, IntVec3 to)
  {
    return to.x == from.x || to.z == from.z ? vehicle.TicksPerMoveCardinal : vehicle.TicksPerMoveDiagonal;
  }

  private static void LocomotionTicks(
    VehiclePawn vehicle,
    IntVec3 from,
    IntVec3 to,
    ref float tickCost)
  {
    Pawn locomotionUrgencySameAs = vehicle.jobs.curDriver.locomotionUrgencySameAs;
    if (locomotionUrgencySameAs is VehiclePawn vehicle1 && locomotionUrgencySameAs != vehicle && ((Thing) locomotionUrgencySameAs).Spawned)
    {
      float moveIntoCell = VehiclePathFollower.CostToMoveIntoCell(vehicle1, from, to);
      tickCost = Mathf.Max(tickCost, moveIntoCell);
    }
    else
    {
      switch (vehicle.jobs.curJob.locomotionUrgency - 1)
      {
        case 0:
          tickCost *= 3f;
          if ((double) tickCost >= 60.0)
            break;
          tickCost = 60f;
          break;
        case 1:
          tickCost *= 2f;
          if ((double) tickCost >= 50.0)
            break;
          tickCost = 50f;
          break;
        case 3:
          tickCost = (float) Mathf.RoundToInt(tickCost * 0.75f);
          break;
      }
    }
  }

  public static float CostToMoveIntoCell(VehiclePawn vehicle, IntVec3 from, IntVec3 to)
  {
    float tickCost = Mathf.Min(VehiclePathFollower.MoveTicksAt(vehicle, from, to) + (float) ((Thing) vehicle).Map.GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef].VehiclePathGrid.PerceivedPathCostAt(to), 450f);
    if (vehicle.CurJob != null)
      VehiclePathFollower.LocomotionTicks(vehicle, from, to, ref tickCost);
    return Mathf.Max(tickCost, 1f);
  }

  private float CostToPayThisTick() => Mathf.Max(1f, this.nextCellCostTotal / 450f);

  private VehiclePath FindPath(CancellationToken token)
  {
    this.lastPathedTargetPosition = ((LocalTargetInfo) ref this.destination).Cell;
    return ((Thing) this.vehicle).Map.GetCachedMapComponent<VehiclePathingSystem>()[this.vehicle.VehicleDef].VehiclePathFinder.FindPath(((Thing) this.vehicle).Position, this.destination, this.vehicle, token, this.peMode);
  }

  internal void GeneratePath(CancellationToken token)
  {
    VehiclePath path = this.FindPath(token);
    if (path == null || !path.Found)
    {
      this.PatherFailed();
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_NoPathForVehicle")), MessageTypeDefOf.RejectInput, false);
    }
    else
    {
      if (this.curPath != null)
      {
        VehiclePath curPath = this.curPath;
        if (UnityData.IsInMainThread)
          curPath.Dispose();
        else
          UnityThread.ExecuteOnMainThread(new Action(curPath.Dispose));
      }
      this.curPath = path;
      this.RequestStatus = VehiclePathFollower.PathRequestStatus.None;
    }
  }

  private void RequestNewPath()
  {
    Task curPathTask = this.curPathTask;
    if (curPathTask != null && curPathTask.Status == TaskStatus.Running)
    {
      Trace.Fail("Restarting task while it is ongoing. Cancelling before continuing.");
      this.pathCancellationTokenSource.Cancel();
      Task.WaitAll(new Task[1]{ this.curPathTask }, 1000);
    }
    CancellationTokenSource cancellationTokenSource = this.pathCancellationTokenSource;
    if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
      this.pathCancellationTokenSource = new CancellationTokenSource();
    this.RequestStatus = VehiclePathFollower.PathRequestStatus.Calculating;
    AsyncPathFindAction asyncPathFindAction = AsyncPool<AsyncPathFindAction>.Get();
    asyncPathFindAction.Set(this.vehicle, this.pathCancellationTokenSource.Token);
    this.curPathTask = TaskManager.Run(new Action(((AsyncAction) asyncPathFindAction).Invoke), this.pathCancellationTokenSource.Token);
  }

  private VehiclePathFollower.PathRequest NeedNewPath()
  {
    if (this.RequestStatus == VehiclePathFollower.PathRequestStatus.Calculating)
      return VehiclePathFollower.PathRequest.None;
    if (!((LocalTargetInfo) ref this.destination).IsValid || this.curPath == null || !this.curPath.Found || this.curPath.NodesLeft == 0 || ((LocalTargetInfo) ref this.destination).HasThing && ((LocalTargetInfo) ref this.destination).Thing.Map != ((Thing) this.vehicle).Map)
      return VehiclePathFollower.PathRequest.NeedNew;
    CellRect cellRect1 = this.vehicle.VehicleRect(((LocalTargetInfo) ref this.destination).Cell, Rot4.North, true);
    foreach (IntVec3 cell in cellRect1)
    {
      VehiclePawn vehiclePawn = PathingHelper.AnyVehicleBlockingPathAt(cell, this.vehicle);
      if (vehiclePawn != null && !vehiclePawn.vehiclePather.Moving && !vehiclePawn.vehiclePather.Waiting)
      {
        IntVec3 result;
        if (!PathingHelper.TryFindNearestStandableCell(this.vehicle, ((LocalTargetInfo) ref this.destination).Cell, out result))
          return VehiclePathFollower.PathRequest.None;
        this.destination = LocalTargetInfo.op_Implicit(result);
        return VehiclePathFollower.PathRequest.NeedNew;
      }
    }
    IntVec3 position1 = ((Thing) this.vehicle).Position;
    if (!((IntVec3) ref position1).InHorDistOf(this.curPath.LastNode, 15f))
    {
      IntVec3 position2 = ((Thing) this.vehicle).Position;
      if (!((IntVec3) ref position2).InHorDistOf(((LocalTargetInfo) ref this.destination).Cell, 15f))
        goto label_16;
    }
    if (!VehicleReachabilityImmediate.CanReachImmediateVehicle(this.curPath.LastNode, this.destination, ((Thing) this.vehicle).Map, this.vehicle.VehicleDef, this.peMode))
      return VehiclePathFollower.PathRequest.NeedNew;
label_16:
    if (this.curPath.UsedHeuristics && this.curPath.NodesConsumedCount >= 75)
      return VehiclePathFollower.PathRequest.NeedNew;
    if (IntVec3.op_Inequality(this.lastPathedTargetPosition, ((LocalTargetInfo) ref this.destination).Cell))
    {
      IntVec3 intVec3_1 = IntVec3.op_Subtraction(((Thing) this.vehicle).Position, ((LocalTargetInfo) ref this.destination).Cell);
      float horizontalSquared = (float) ((IntVec3) ref intVec3_1).LengthHorizontalSquared;
      float num = (double) horizontalSquared > 900.0 ? 10f : ((double) horizontalSquared > 289.0 ? 5f : ((double) horizontalSquared > 100.0 ? 3f : ((double) horizontalSquared > 49.0 ? 2f : 0.5f)));
      IntVec3 intVec3_2 = IntVec3.op_Subtraction(this.lastPathedTargetPosition, ((LocalTargetInfo) ref this.destination).Cell);
      if ((double) ((IntVec3) ref intVec3_2).LengthHorizontalSquared > (double) num * (double) num)
        return VehiclePathFollower.PathRequest.NeedNew;
    }
    IntVec3 c1 = IntVec3.Invalid;
    for (int aheadStartingIndex = this.LookAheadStartingIndex; aheadStartingIndex < this.LookAheadStartingIndex + 5 && aheadStartingIndex < this.curPath.NodesLeft; ++aheadStartingIndex)
    {
      IntVec3 intVec3 = this.curPath.Peek(aheadStartingIndex);
      Rot8 cell1 = Ext_Map.DirectionToCell(c1, intVec3);
      if (!intVec3.Walkable(this.vehicle.VehicleDef, ((Thing) this.vehicle).Map))
        return VehiclePathFollower.PathRequest.NeedNew;
      CellRect cellRect2 = this.vehicle.VehicleRect(intVec3, (Rot4) cell1);
      foreach (IntVec3 cell2 in cellRect2)
      {
        VehiclePawn vehiclePawn = PathingHelper.AnyVehicleBlockingPathAt(cell2, this.vehicle);
        if (vehiclePawn != null)
          return vehiclePawn.vehiclePather.Moving && !vehiclePawn.vehiclePather.Waiting ? VehiclePathFollower.PathRequest.Wait : VehiclePathFollower.PathRequest.NeedNew;
      }
      c1 = intVec3;
    }
    return VehiclePathFollower.PathRequest.None;
  }

  private void WarnPawnsImpendingCollision()
  {
    if (this.curPath == null)
      return;
    using (new ClearOnDispose<IntVec3>((ICollection<IntVec3>) VehiclePathFollower.CollisionCells))
    {
      IntVec3 c1 = IntVec3.Invalid;
      for (int aheadStartingIndex = this.CollisionsLookAheadStartingIndex; aheadStartingIndex < this.CollisionsLookAheadStartingIndex + 8 && aheadStartingIndex < this.curPath.NodesLeft; ++aheadStartingIndex)
      {
        IntVec3 intVec3_1 = this.curPath.Peek(aheadStartingIndex);
        Rot8 cell = Ext_Map.DirectionToCell(c1, intVec3_1);
        CellRect cellRect1 = this.vehicle.VehicleRect(intVec3_1, (Rot4) cell);
        CellRect cellRect2 = ((CellRect) ref cellRect1).ExpandedBy(1);
        foreach (IntVec3 intVec3_2 in cellRect2)
        {
          if (GenGrid.InBounds(intVec3_2, ((Thing) this.vehicle).Map) && VehiclePathFollower.CollisionCells.Add(intVec3_2))
          {
            List<Thing> thingList = GridsUtility.GetThingList(intVec3_2, ((Thing) this.vehicle).Map);
            for (int index = thingList.Count - 1; index >= 0; --index)
            {
              if (thingList[index] is Pawn pawn)
              {
                Room room1 = RegionAndRoomQuery.RoomAt(intVec3_2, ((Thing) this.vehicle).Map, (RegionType) 14);
                Room room2 = RegionAndRoomQuery.GetRoom((Thing) pawn, (RegionType) 14);
                if (room2 == null || room2.CellCount == 1 || room1 == room2 && GenSight.LineOfSight(((Thing) this.vehicle).Position, ((Thing) pawn).Position, ((Thing) this.vehicle).Map))
                  pawn.Notify_DangerousVehiclePath(this.vehicle);
              }
            }
          }
        }
        c1 = intVec3_1;
      }
    }
  }

  public enum PathRequest
  {
    None,
    Wait,
    Fail,
    NeedNew,
  }

  public enum PathRequestStatus
  {
    None,
    Calculating,
    Failed,
  }
}
