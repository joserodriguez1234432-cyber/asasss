// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravan_PathFollower
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public sealed class VehicleCaravan_PathFollower : IExposable
{
  private const int MaxMoveTicks = 30000;
  private const int MaxCheckAheadNodes = 20;
  public const float DefaultPathCostToPayPerTick = 1f;
  public const int FinalNoRestPushMaxDurationTicks = 10000;
  private readonly VehicleCaravan caravan;
  private bool moving;
  private bool paused;
  private PlanetTile nextTile = PlanetTile.Invalid;
  public int previousTileForDrawingIfInDoubt = -1;
  public float nextTileCostLeft;
  public float nextTileCostTotal = 1f;
  private PlanetTile destTile;
  private CaravanArrivalAction arrivalAction;
  public WorldPath curPath;

  public VehicleCaravan_PathFollower(VehicleCaravan caravan) => this.caravan = caravan;

  public PlanetTile Destination => this.destTile;

  public PlanetTile NextTile => this.nextTile;

  public bool Moving => this.moving && ((WorldObject) this.caravan).Spawned;

  public bool MovingNow
  {
    get
    {
      return this.Moving && !this.Paused && !this.caravan.CantMove && !this.caravan.OutOfFuel && !this.caravan.VehicleCantMove;
    }
  }

  public CaravanArrivalAction ArrivalAction
  {
    get => !this.Moving ? (CaravanArrivalAction) null : this.arrivalAction;
  }

  public bool Paused
  {
    get => this.Moving && this.paused;
    set
    {
      if (value == this.paused)
        return;
      if (!value)
        this.paused = false;
      else if (!this.Moving)
        Log.Error($"Tried to pause caravan movement of {Gen.ToStringSafe<Caravan>((Caravan) this.caravan)} but it's not moving.");
      else
        this.paused = true;
      this.caravan.Notify_DestinationOrPauseStatusChanged();
    }
  }

  public bool StartPath(
    PlanetTile destTile,
    CaravanArrivalAction arrivalAction,
    bool repathImmediately = false,
    bool resetPauseStatus = true)
  {
    this.caravan.EnsureWorldGridInitialized();
    this.caravan.autoJoinable = false;
    if (resetPauseStatus)
      this.paused = false;
    if (arrivalAction != null && !FloatMenuAcceptanceReport.op_Implicit(arrivalAction.StillValid((Caravan) this.caravan, destTile)) || !this.IsPassable(((WorldObject) this.caravan).Tile) && !this.TryRecoverFromUnwalkablePosition())
      return false;
    if (this.moving && this.curPath != null && PlanetTile.op_Equality(this.destTile, destTile))
    {
      this.arrivalAction = arrivalAction;
      return true;
    }
    if (!WorldVehiclePathGrid.Instance.reachability.CanReach(this.caravan, destTile))
    {
      this.PatherFailed();
      return false;
    }
    this.destTile = destTile;
    this.arrivalAction = arrivalAction;
    this.caravan.Notify_DestinationOrPauseStatusChanged();
    if (PlanetTile.op_Implicit(this.nextTile) < 0 || !this.IsNextTilePassable())
    {
      this.nextTile = ((WorldObject) this.caravan).Tile;
      this.nextTileCostLeft = 0.0f;
      this.previousTileForDrawingIfInDoubt = -1;
    }
    if (this.AtDestinationPosition())
    {
      this.PatherArrived();
      return true;
    }
    if (this.curPath != null)
      this.curPath.ReleaseToPool();
    this.curPath = (WorldPath) null;
    this.moving = true;
    if (repathImmediately && this.TrySetNewPath() && (double) this.nextTileCostLeft <= 0.0 && this.moving)
      this.TryEnterNextPathTile();
    return true;
  }

  public void StopDead()
  {
    if (this.curPath != null)
      this.curPath.ReleaseToPool();
    this.curPath = (WorldPath) null;
    this.moving = false;
    this.paused = false;
    this.nextTile = ((WorldObject) this.caravan).Tile;
    this.previousTileForDrawingIfInDoubt = -1;
    this.arrivalAction = (CaravanArrivalAction) null;
    this.nextTileCostLeft = 0.0f;
    this.caravan.Notify_DestinationOrPauseStatusChanged();
  }

  public void PatherTick()
  {
    if (this.moving && this.arrivalAction != null && !FloatMenuAcceptanceReport.op_Implicit(this.arrivalAction.StillValid((Caravan) this.caravan, this.Destination)))
    {
      FloatMenuAcceptanceReport acceptanceReport = this.arrivalAction.StillValid((Caravan) this.caravan, this.Destination);
      string failMessage = ((FloatMenuAcceptanceReport) ref acceptanceReport).FailMessage;
      TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("MessageCaravanArrivalActionNoLongerValid", NamedArgument.op_Implicit(this.caravan.Name));
      Messages.Message(TaggedString.op_Implicit(TaggedString.op_Addition(((TaggedString) ref taggedString).CapitalizeFirst(), failMessage != null ? " " + failMessage : "")), LookTargets.op_Implicit((WorldObject) this.caravan), MessageTypeDefOf.NegativeEvent, true);
      this.StopDead();
    }
    if (this.caravan.CantMove || this.caravan.VehicleCantMove || this.caravan.OutOfFuel || this.paused)
      return;
    if ((double) this.nextTileCostLeft > 0.0)
    {
      this.nextTileCostLeft -= this.CostToPayThisTick();
    }
    else
    {
      if (!this.moving)
        return;
      this.TryEnterNextPathTile();
    }
  }

  public void Notify_Teleported_Int() => this.StopDead();

  public bool IsPassable(PlanetTile tile)
  {
    return this.caravan.UniqueVehicleDefsInCaravan().All<VehicleDef>((Func<VehicleDef, bool>) (v => WorldVehiclePathGrid.Instance.Passable(tile, v)));
  }

  public bool IsNextTilePassable()
  {
    return this.caravan.UniqueVehicleDefsInCaravan().All<VehicleDef>((Func<VehicleDef, bool>) (v => WorldVehiclePathGrid.Instance.Passable(this.nextTile, v)));
  }

  private bool TryRecoverFromUnwalkablePosition()
  {
    if (this.caravan.VehiclesListForReading.All<VehiclePawn>((Func<VehiclePawn, bool>) (vehicle => vehicle.VehicleDef.type == VehicleType.Air)))
      return false;
    PlanetTile planetTile1;
    if (GenWorldClosest.TryFindClosestTile(((WorldObject) this.caravan).Tile, (Predicate<PlanetTile>) (planetTile => this.IsPassable(planetTile) && WorldVehiclePathGrid.Instance.reachability.CanReach(this.caravan, planetTile)), ref planetTile1, int.MaxValue, true))
    {
      Log.Warning($"{this.caravan} on impassable tile: {((WorldObject) this.caravan).Tile}. Teleporting to {planetTile1}");
      ((WorldObject) this.caravan).Tile = planetTile1;
      this.caravan.Notify_VehicleTeleported();
      return true;
    }
    Log.Error($"{this.caravan} on impassable tile: {((WorldObject) this.caravan).Tile}. Could not find moveable position nearby. Destroying caravan.");
    ((WorldObject) this.caravan).Destroy();
    return false;
  }

  private void PatherArrived()
  {
    CaravanArrivalAction arrivalAction = this.arrivalAction;
    this.StopDead();
    if (arrivalAction != null && FloatMenuAcceptanceReport.op_Implicit(arrivalAction.StillValid((Caravan) this.caravan, ((WorldObject) this.caravan).Tile)))
    {
      arrivalAction.Arrived((Caravan) this.caravan);
    }
    else
    {
      if (!this.caravan.IsPlayerControlled || WorldObjectSelectionUtility.VisibleToCameraNow((WorldObject) this.caravan))
        return;
      Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageCaravanArrivedAtDestination", NamedArgument.op_Implicit(((WorldObject) this.caravan).Label))), LookTargets.op_Implicit((WorldObject) this.caravan), MessageTypeDefOf.TaskCompletion, true);
    }
  }

  private void PatherFailed() => this.StopDead();

  private void TryEnterNextPathTile()
  {
    if (!this.IsNextTilePassable())
    {
      this.PatherFailed();
    }
    else
    {
      ((WorldObject) this.caravan).Tile = this.nextTile;
      if (this.NeedNewPath() && !this.TrySetNewPath())
        return;
      if (this.AtDestinationPosition())
        this.PatherArrived();
      else if (this.curPath.NodesLeftCount == 0)
      {
        Log.Error(((object) this.caravan)?.ToString() + " ran out of path nodes. Force-arriving.");
        this.PatherArrived();
      }
      else
        this.SetupMoveIntoNextTile();
    }
  }

  private void SetupMoveIntoNextTile()
  {
    if (this.curPath.NodesLeftCount < 2)
    {
      Log.Error($"{this.caravan} at {((WorldObject) this.caravan).Tile} ran out of path nodes while pathing to {this.destTile}.");
      this.PatherFailed();
    }
    else
    {
      this.nextTile = this.curPath.ConsumeNextNode();
      this.previousTileForDrawingIfInDoubt = -1;
      if (!this.IsPassable(this.nextTile))
        Log.Error($"{this.caravan} entering {this.nextTile} which is impassable");
      int move = this.CostToMove(((WorldObject) this.caravan).Tile, this.nextTile);
      this.nextTileCostTotal = (float) move;
      this.nextTileCostLeft = (float) move;
    }
  }

  private int CostToMove(PlanetTile start, PlanetTile end)
  {
    return VehicleCaravan_PathFollower.CostToMove(this.caravan, start, end);
  }

  public static int CostToMove(
    VehicleCaravan caravan,
    PlanetTile start,
    PlanetTile end,
    int? ticksAbs = null)
  {
    return VehicleCaravan_PathFollower.CostToMove(caravan.VehiclesListForReading, caravan.TicksPerMove, start, end, ticksAbs);
  }

  [Profile]
  public static int CostToMove(
    List<VehicleDef> vehicleDefs,
    int ticksPerMove,
    PlanetTile start,
    PlanetTile end,
    int? ticksAbs = null,
    StringBuilder explanation = null,
    string caravanTicksPerMoveExplanation = null)
  {
    if (PlanetTile.op_Equality(start, end))
      return 0;
    explanation?.AppendLine(caravanTicksPerMoveExplanation);
    StringBuilder explanation1 = explanation != null ? new StringBuilder() : (StringBuilder) null;
    float num1 = float.MaxValue;
    foreach (VehicleDef vehicleDef in vehicleDefs)
    {
      float num2 = WorldVehiclePathGrid.CalculatedMovementDifficultyAt(end, vehicleDef, explanation1);
      if ((double) num2 < (double) num1)
        num1 = num2;
    }
    float difficultyOffset = WinterPathingHelper.GetCurrentWinterMovementDifficultyOffset(vehicleDefs, PlanetTile.op_Implicit(end), explanation1);
    float num3 = num1 + difficultyOffset;
    float difficultyMultiplier = RoadCostHelper.GetRoadMovementDifficultyMultiplier(vehicleDefs, PlanetTile.op_Implicit(start), PlanetTile.op_Implicit(end), explanation1);
    if (explanation != null)
    {
      explanation.AppendLine();
      explanation.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(Translator.Translate("TileMovementDifficulty"), ":")));
      explanation.AppendLine(GenText.Indented(explanation1.ToString(), "  "));
      explanation.AppendLine($"  = {(ValueType) (float) ((double) num3 * (double) difficultyMultiplier):0.#}");
    }
    int move = Mathf.Clamp((int) ((double) ticksPerMove * (double) num3 * (double) difficultyMultiplier), 1, 30000);
    if (explanation != null)
    {
      explanation.AppendLine();
      explanation.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(Translator.Translate("FinalCaravanMovementSpeed"), ":")));
      int num4 = Mathf.CeilToInt((float) move / 1f);
      explanation.Append($"  {60000 / ticksPerMove:0.#} / {(ValueType) (float) ((double) num3 * (double) difficultyMultiplier):0.#} = {60000 / num4:0.#} {Translator.Translate("TilesPerDay")}");
    }
    return move;
  }

  [Profile]
  public static int CostToMove(
    List<VehiclePawn> vehicles,
    int ticksPerMove,
    PlanetTile start,
    PlanetTile end,
    int? ticksAbs = null,
    StringBuilder explanation = null,
    string caravanTicksPerMoveExplanation = null)
  {
    if (PlanetTile.op_Equality(start, end))
      return 0;
    explanation?.AppendLine(caravanTicksPerMoveExplanation);
    StringBuilder explanation1 = explanation != null ? new StringBuilder() : (StringBuilder) null;
    float num1 = float.MaxValue;
    foreach (VehiclePawn vehicle in vehicles)
    {
      float num2 = WorldVehiclePathGrid.CalculatedMovementDifficultyAt(end, vehicle.VehicleDef, explanation1);
      if ((double) num2 < (double) num1)
        num1 = num2;
    }
    float difficultyOffset = WinterPathingHelper.GetCurrentWinterMovementDifficultyOffset(vehicles, end, explanation1);
    float num3 = num1 + difficultyOffset;
    float difficultyMultiplier = RoadCostHelper.GetRoadMovementDifficultyMultiplier(vehicles, PlanetTile.op_Implicit(start), PlanetTile.op_Implicit(end), explanation1);
    if (explanation != null)
    {
      explanation.AppendLine();
      explanation.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(Translator.Translate("TileMovementDifficulty"), ":")));
      explanation.AppendLine(GenText.Indented(explanation1.ToString(), "  "));
      explanation.AppendLine($"  = {(ValueType) (float) ((double) num3 * (double) difficultyMultiplier):0.#}");
    }
    int move = Mathf.Clamp((int) ((double) ticksPerMove * (double) num3 * (double) difficultyMultiplier), 1, 30000);
    if (explanation != null)
    {
      explanation.AppendLine();
      explanation.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(Translator.Translate("FinalCaravanMovementSpeed"), ":")));
      int num4 = Mathf.CeilToInt((float) move / 1f);
      explanation.Append($"  {60000 / ticksPerMove:0.#} / {(ValueType) (float) ((double) num3 * (double) difficultyMultiplier):0.#} = {60000 / num4:0.#} {Translator.Translate("TilesPerDay")}");
    }
    return move;
  }

  public static bool IsValidFinalPushDestination(PlanetTile tile)
  {
    foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
    {
      if (PlanetTile.op_Equality(allWorldObject.Tile, tile) && !(allWorldObject is Caravan))
        return true;
    }
    return false;
  }

  private float CostToPayThisTick()
  {
    float payThisTick = 1f;
    if (DebugSettings.fastCaravans)
      payThisTick = 100f;
    if ((double) payThisTick < (double) this.nextTileCostTotal / 30000.0)
      payThisTick = this.nextTileCostTotal / 30000f;
    return payThisTick;
  }

  private bool TrySetNewPath()
  {
    WorldPath newPath = this.GenerateNewPath();
    if (!newPath.Found)
    {
      this.PatherFailed();
      return false;
    }
    if (this.curPath != null)
      this.curPath.ReleaseToPool();
    this.curPath = newPath;
    return true;
  }

  private WorldPath GenerateNewPath()
  {
    int num = PlanetTile.op_Implicit(!this.moving || !((PlanetTile) ref this.nextTile).Valid || !this.IsNextTilePassable() ? ((WorldObject) this.caravan).Tile : this.nextTile);
    WorldPath path = WorldVehiclePathfinder.Instance.FindPath(PlanetTile.op_Implicit(num), this.destTile, this.caravan);
    if (path.Found && PlanetTile.op_Inequality(PlanetTile.op_Implicit(num), ((WorldObject) this.caravan).Tile))
    {
      if (path.NodesLeftCount >= 2 && PlanetTile.op_Equality(path.Peek(1), ((WorldObject) this.caravan).Tile))
      {
        path.ConsumeNextNode();
        if (this.moving)
        {
          this.previousTileForDrawingIfInDoubt = PlanetTile.op_Implicit(this.nextTile);
          this.nextTile = ((WorldObject) this.caravan).Tile;
          this.nextTileCostLeft = this.nextTileCostTotal - this.nextTileCostLeft;
        }
      }
      else
        path.AddNodeAtStart(((WorldObject) this.caravan).Tile);
    }
    return path;
  }

  private bool AtDestinationPosition()
  {
    return PlanetTile.op_Equality(((WorldObject) this.caravan).Tile, this.destTile);
  }

  private bool NeedNewPath()
  {
    if (!this.moving)
      return false;
    if (this.curPath == null || !this.curPath.Found || this.curPath.NodesLeftCount == 0)
      return true;
    for (int index = 0; index < 20 && index < this.curPath.NodesLeftCount; ++index)
    {
      if (!this.IsPassable(PlanetTile.op_Implicit(PlanetTile.op_Implicit(this.curPath.Peek(index)))))
        return true;
    }
    return false;
  }

  public void ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.moving, "moving", true, false);
    Scribe_Values.Look<bool>(ref this.paused, "paused", false, false);
    Scribe_Values.Look<PlanetTile>(ref this.nextTile, "nextTile", new PlanetTile(), false);
    Scribe_Values.Look<int>(ref this.previousTileForDrawingIfInDoubt, "previousTileForDrawingIfInDoubt", 0, false);
    Scribe_Values.Look<float>(ref this.nextTileCostLeft, "nextTileCostLeft", 0.0f, false);
    Scribe_Values.Look<float>(ref this.nextTileCostTotal, "nextTileCostTotal", 0.0f, false);
    Scribe_Values.Look<PlanetTile>(ref this.destTile, "destTile", new PlanetTile(), false);
    Scribe_Deep.Look<CaravanArrivalAction>(ref this.arrivalAction, "arrivalAction", Array.Empty<object>());
    if (Scribe.mode != 4)
      return;
    this.caravan.RecacheVehiclesOrConvertCaravan();
    if (Current.ProgramState == null || !this.moving || this.StartPath(this.destTile, this.arrivalAction, true, false))
      return;
    this.StopDead();
  }
}
