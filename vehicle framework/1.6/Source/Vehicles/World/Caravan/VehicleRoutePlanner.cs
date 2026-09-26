// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleRoutePlanner
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
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
[StaticConstructorOnStartup]
public class VehicleRoutePlanner : WorldComponent
{
  private const float BottomWindowBotMargin = 45f;
  private const float BottomWindowEntryExtraBotMargin = 22f;
  private const float RouteButtonDimension = 57f;
  private const int MaxCount = 25;
  private static readonly Texture2D MouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/WaypointMouseAttachment", true);
  private static readonly Vector2 BottomWindowSize = new Vector2(500f, 95f);
  private static readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
  private WorldVehiclePathfinder pathfinder;
  private Action onFinishCallback;
  private Action<PlanetTile> onChooseRouteCallback;
  private List<VehicleDef> vehicleDefs = new List<VehicleDef>();
  private VehicleCaravanInfo caravanInfo;
  private readonly List<WorldPath> paths = new List<WorldPath>();
  private readonly List<int> cachedTicksToWaypoint = new List<int>();
  private readonly List<RoutePlannerWaypoint> waypoints = new List<RoutePlannerWaypoint>();
  private bool cantRemoveFirstWaypoint;

  public VehicleRoutePlanner(RimWorld.Planet.World world)
    : base(world)
  {
    this.world = world;
  }

  public bool IsActive { get; private set; }

  private bool IsCaravaning { get; set; }

  private bool ShouldStop
  {
    get
    {
      if (!this.IsActive || !WorldRendererUtility.WorldSelected)
        return true;
      return Current.ProgramState == 2 && Find.TickManager.CurTimeSpeed > 0;
    }
  }

  private int CaravanTicksPerMove
  {
    get
    {
      return this.caravanInfo == null ? VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(this.vehicleDefs) : VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(this.caravanInfo);
    }
  }

  private Caravan CaravanAtTheFirstWaypoint
  {
    get
    {
      return this.waypoints.Count <= 0 ? (Caravan) null : Find.WorldObjects.PlayerControlledCaravanAt(((WorldObject) this.waypoints[0]).Tile);
    }
  }

  public virtual void FinalizeInit(bool fromLoad)
  {
    base.FinalizeInit(fromLoad);
    this.pathfinder = Find.World.GetComponent<WorldVehiclePathfinder>();
  }

  private void Start(Action onFinishCallback, Action<PlanetTile> onChooseRouteCallback)
  {
    if (this.IsActive)
      this.Stop();
    if (onFinishCallback != null)
      this.onFinishCallback += onFinishCallback;
    if (onChooseRouteCallback != null)
      this.onChooseRouteCallback += onChooseRouteCallback;
    this.IsActive = true;
    if (Current.ProgramState != 2)
      return;
    AmbientSoundManager.EnsureWorldAmbientSoundCreated();
    Find.World.renderer.wantedMode = (WorldRenderMode) 1;
    Find.TickManager.Pause();
  }

  public void Start(
    [NotNull] List<VehicleDef> vehicleDefs,
    Action onFinishCallback = null,
    Action<PlanetTile> onChooseRouteCallback = null)
  {
    this.Start(onFinishCallback, onChooseRouteCallback);
    this.vehicleDefs = vehicleDefs;
  }

  public void Start(
    [NotNull] VehicleCaravanInfo caravanInfo,
    Action onFinishCallback = null,
    Action<PlanetTile> onChooseRouteCallback = null)
  {
    this.Start(onFinishCallback, onChooseRouteCallback);
    this.caravanInfo = caravanInfo;
    this.vehicleDefs = caravanInfo.pawns.UniqueVehicleDefsInList();
    this.IsCaravaning = caravanInfo.caravaning;
    if (!((PlanetTile) ref caravanInfo.tile).Valid)
      return;
    this.TryAddWaypoint(caravanInfo.tile);
    this.cantRemoveFirstWaypoint = true;
  }

  public void Stop()
  {
    this.IsActive = false;
    this.IsCaravaning = false;
    foreach (WorldObject waypoint in this.waypoints)
      waypoint.Destroy();
    this.waypoints.Clear();
    this.cachedTicksToWaypoint.Clear();
    this.vehicleDefs.Clear();
    this.cantRemoveFirstWaypoint = false;
    Action onFinishCallback = this.onFinishCallback;
    if (onFinishCallback != null)
      onFinishCallback();
    this.onFinishCallback = (Action) null;
    this.onChooseRouteCallback = (Action<PlanetTile>) null;
    this.ReleasePaths();
  }

  public virtual void WorldComponentUpdate()
  {
    if (this.IsActive && this.ShouldStop)
      this.Stop();
    if (!this.IsActive)
      return;
    foreach (WorldPath path in this.paths)
      path.DrawPath((Caravan) null);
  }

  public virtual void WorldComponentOnGUI()
  {
    if (!this.IsActive)
      return;
    if (KeyBindingDefOf.Cancel.KeyDownEvent)
    {
      if (!this.IsCaravaning)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
      this.Stop();
      Event.current.Use();
    }
    else
    {
      GenUI.DrawMouseAttachment(VehicleRoutePlanner.MouseAttachment);
      if (Event.current.type == null && Event.current.button == 1)
      {
        int tile = PlanetTile.op_Implicit(Find.WorldSelector.SelectableObjectsUnderMouse().FirstOrDefault<WorldObject>() is VehicleCaravan vehicleCaravan ? ((WorldObject) vehicleCaravan).Tile : GenWorld.MouseTile(true));
        if (tile >= 0)
        {
          RoutePlannerWaypoint waypoint = this.MostRecentWaypointAt(tile);
          if (waypoint != null)
          {
            RoutePlannerWaypoint routePlannerWaypoint1 = waypoint;
            List<RoutePlannerWaypoint> waypoints = this.waypoints;
            RoutePlannerWaypoint routePlannerWaypoint2 = waypoints[waypoints.Count - 1];
            if (routePlannerWaypoint1 == routePlannerWaypoint2)
              this.TryRemoveWaypoint(waypoint);
            else
              Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>(2)
              {
                new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("AddWaypoint")), (Action) (() => this.TryAddWaypoint(PlanetTile.op_Implicit(tile))), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
                new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("RemoveWaypoint")), (Action) (() => this.TryRemoveWaypoint(waypoint)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
              }));
          }
          else
            this.TryAddWaypoint(PlanetTile.op_Implicit(tile));
          Event.current.Use();
        }
      }
      this.DoRouteDetailsBox();
      if (this.IsCaravaning && this.DoChooseRouteButton())
        return;
      this.DoTileTooltips();
    }
  }

  private void DoRouteDetailsBox()
  {
    Rect rect = new Rect((float) (((double) UI.screenWidth - (double) VehicleRoutePlanner.BottomWindowSize.x) / 2.0), (float) ((double) UI.screenHeight - (double) VehicleRoutePlanner.BottomWindowSize.y - 45.0), VehicleRoutePlanner.BottomWindowSize.x, VehicleRoutePlanner.BottomWindowSize.y);
    if (Current.ProgramState == null)
    {
      ref Rect local = ref rect;
      ((Rect) ref local).y = ((Rect) ref local).y - 22f;
    }
    Find.WindowStack.ImmediateWindow(1373514241, rect, (WindowLayer) 1, (Action) (() =>
    {
      if (!this.IsActive)
        return;
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 1), new TextAnchor?((TextAnchor) 1), new Color?(Color.white));
      try
      {
        float lineHeight = Verse.Text.LineHeight;
        float num1 = 6f;
        if (this.waypoints.Count >= 2)
        {
          Rect rect1 = new Rect(0.0f, num1, ((Rect) ref rect).width, 25f);
          List<int> cachedTicksToWaypoint = this.cachedTicksToWaypoint;
          TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("RoutePlannerEstTimeToFinalDest", NamedArgument.op_Implicit(GenDate.ToStringTicksToDays(cachedTicksToWaypoint[cachedTicksToWaypoint.Count - 1], "0.#")));
          Widgets.Label(rect1, taggedString);
        }
        else if (this.cantRemoveFirstWaypoint)
          Widgets.Label(new Rect(0.0f, num1, ((Rect) ref rect).width, 25f), Translator.Translate("RoutePlannerAddOneOrMoreWaypoints"));
        else
          Widgets.Label(new Rect(0.0f, num1, ((Rect) ref rect).width, 25f), Translator.Translate("RoutePlannerAddTwoOrMoreWaypoints"));
        float num2 = num1 + lineHeight;
        if (!this.IsCaravaning && this.CaravanAtTheFirstWaypoint != null)
        {
          GUI.color = Color.gray;
          Widgets.Label(new Rect(0.0f, num2, ((Rect) ref rect).width, 25f), TranslatorFormattedStringExtensions.Translate("RoutePlannerUsingTicksPerMoveOfCaravan", NamedArgument.op_Implicit(((WorldObject) this.CaravanAtTheFirstWaypoint).LabelCap)));
        }
        float num3 = num2 + lineHeight;
        GUI.color = Color.gray;
        Widgets.Label(new Rect(0.0f, num3, ((Rect) ref rect).width, 25f), Translator.Translate("RoutePlannerPressRMBToAddAndRemoveWaypoints"));
        Widgets.Label(new Rect(0.0f, num3 + lineHeight, ((Rect) ref rect).width, 25f), this.IsCaravaning ? Translator.Translate("RoutePlannerPressEscapeToReturnToCaravanFormationDialog") : Translator.Translate("RoutePlannerPressEscapeToExit"));
      }
      finally
      {
        textBlock.Dispose();
      }
    }), true, false, 1f, (Action) null, false);
  }

  private bool DoChooseRouteButton()
  {
    if (!this.IsCaravaning || this.waypoints.Count < 2 || !Widgets.ButtonText(new Rect((float) (((double) UI.screenWidth - (double) VehicleRoutePlanner.BottomButtonSize.x) / 2.0), (float) ((double) UI.screenHeight - (double) VehicleRoutePlanner.BottomWindowSize.y - 45.0 - 10.0) - VehicleRoutePlanner.BottomButtonSize.y, VehicleRoutePlanner.BottomButtonSize.x, VehicleRoutePlanner.BottomButtonSize.y), TaggedString.op_Implicit(Translator.Translate("ChooseRouteButton")), true, true, true, new TextAnchor?()))
      return false;
    Action<PlanetTile> chooseRouteCallback = this.onChooseRouteCallback;
    if (chooseRouteCallback != null)
      chooseRouteCallback(this.waypoints.Count >= 2 ? ((WorldObject) this.waypoints[1]).Tile : PlanetTile.Invalid);
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    this.Stop();
    return true;
  }

  private void DoTileTooltips()
  {
    if (Mouse.IsInputBlockedNow)
      return;
    int tile = PlanetTile.op_Implicit(GenWorld.MouseTile(true));
    if (tile == -1)
      return;
    for (int index = 0; index < this.paths.Count; ++index)
    {
      if (this.paths[index].NodesReversed.Contains(PlanetTile.op_Implicit(tile)))
      {
        string str = this.GetTileTip(tile, index);
        Verse.Text.Font = (GameFont) 1;
        Vector2 vector2 = Verse.Text.CalcSize(str);
        vector2.x += 20f;
        vector2.y += 20f;
        Rect rect = new Rect(GenUI.GetMouseAttachedWindowPos(vector2.x, vector2.y), vector2);
        Find.WindowStack.ImmediateWindow(1859615246, rect, (WindowLayer) 3, (Action) (() =>
        {
          Verse.Text.Font = (GameFont) 1;
          Widgets.Label(GenUI.ContractedBy(GenUI.AtZero(rect), 10f), str);
        }), true, false, 1f, (Action) null, false);
        break;
      }
    }
  }

  private string GetTileTip(int tile, int pathIndex)
  {
    int num1 = this.paths[pathIndex].NodesReversed.IndexOf(PlanetTile.op_Implicit(tile));
    int num2;
    if (num1 > 0)
      num2 = PlanetTile.op_Implicit(this.paths[pathIndex].NodesReversed[num1 - 1]);
    else if (pathIndex < this.paths.Count - 1 && this.paths[pathIndex + 1].NodesReversed.Count >= 2)
    {
      List<PlanetTile> nodesReversed = this.paths[pathIndex + 1].NodesReversed;
      num2 = PlanetTile.op_Implicit(nodesReversed[nodesReversed.Count - 2]);
    }
    else
      num2 = -1;
    int arrive = VehicleCaravanPathingHelper.EstimatedTicksToArrive(this.vehicleDefs, this.paths[pathIndex].FirstNode, PlanetTile.op_Implicit(tile), this.paths[pathIndex], 0.0f, this.CaravanTicksPerMove, GenTicks.TicksAbs + this.cachedTicksToWaypoint[pathIndex]);
    int num3 = this.cachedTicksToWaypoint[pathIndex] + arrive;
    int num4 = GenTicks.TicksAbs + num3;
    StringBuilder stringBuilder1 = new StringBuilder();
    if (num3 != 0)
      stringBuilder1.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("EstimatedTimeToTile", NamedArgument.op_Implicit(GenDate.ToStringTicksToDays(num3, "0.##")))));
    stringBuilder1.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(TaggedString.op_Addition(Translator.Translate("ForagedFoodAmount"), ": "), GenText.ToStringPercent(((Tile) Find.WorldGrid[tile]).PrimaryBiome.forageability))));
    stringBuilder1.Append(VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(PlanetTile.op_Implicit(tile), num4));
    if (num2 != -1)
    {
      stringBuilder1.AppendLine();
      stringBuilder1.AppendLine();
      StringBuilder stringBuilder2 = new StringBuilder();
      float num5 = WorldPathGrid.CalculatedMovementDifficultyAt(PlanetTile.op_Implicit(num2), false, new int?(num4), stringBuilder2);
      float difficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(PlanetTile.op_Implicit(tile), PlanetTile.op_Implicit(num2), stringBuilder2);
      stringBuilder1.Append(TaggedString.op_Implicit(TaggedString.op_Addition(TaggedString.op_Addition(Translator.Translate("TileMovementDifficulty"), ":\n"), GenText.Indented(stringBuilder2.ToString(), "  "))));
      stringBuilder1.AppendLine();
      stringBuilder1.Append("  = ");
      stringBuilder1.Append((num5 * difficultyMultiplier).ToString("0.#"));
    }
    return stringBuilder1.ToString();
  }

  public void DoRoutePlannerButton(ref float curBaseY)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector((float) ((double) UI.screenWidth - 10.0 - 57.0), (float) ((double) curBaseY - 10.0 - 57.0), 57f, 57f);
    if (Widgets.ButtonImage(rect, VehicleTex.RoutePlanner, Color.white, new Color(0.8f, 0.8f, 0.8f), true, (string) null))
    {
      if (this.IsActive)
      {
        this.Stop();
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
      }
      else
      {
        Find.WindowStack.Add((Window) new Dialog_VehicleSelector());
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
      }
    }
    TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(Translator.Translate("VF_RoutePlannerButtonTip")));
    curBaseY -= 77f;
  }

  private void TryAddWaypoint(PlanetTile tile, bool playSound = true)
  {
    if (this.vehicleDefs.NotNullAndAny<VehicleDef>((Predicate<VehicleDef>) (v => !WorldVehiclePathGrid.Instance.Passable(tile, v))))
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageCantAddWaypointBecauseImpassable")), MessageTypeDefOf.RejectInput, false);
    else if (this.waypoints.NotNullAndAny<RoutePlannerWaypoint>() && !this.vehicleDefs.All<VehicleDef>((Func<VehicleDef, bool>) (vehicle =>
    {
      WorldVehicleReachability reachability = WorldVehiclePathGrid.Instance.reachability;
      VehicleDef vehicleDef = vehicle;
      List<RoutePlannerWaypoint> waypoints = this.waypoints;
      PlanetTile tile1 = ((WorldObject) waypoints[waypoints.Count - 1]).Tile;
      PlanetTile destTile = tile;
      return reachability.CanReach(vehicleDef, tile1, destTile);
    })))
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageCantAddWaypointBecauseUnreachable")), MessageTypeDefOf.RejectInput, false);
    else if (this.waypoints.Count >= 25)
    {
      Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageCantAddWaypointBecauseLimit", NamedArgument.op_Implicit(25))), MessageTypeDefOf.RejectInput, false);
    }
    else
    {
      RoutePlannerWaypoint routePlannerWaypoint = (RoutePlannerWaypoint) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.RoutePlannerWaypoint);
      ((WorldObject) routePlannerWaypoint).Tile = tile;
      Find.WorldObjects.Add((WorldObject) routePlannerWaypoint);
      this.waypoints.Add(routePlannerWaypoint);
      this.RecreatePaths();
      if (!playSound)
        return;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
    }
  }

  private void TryRemoveWaypoint(RoutePlannerWaypoint point, bool playSound = true)
  {
    if (this.cantRemoveFirstWaypoint && this.waypoints.NotNullAndAny<RoutePlannerWaypoint>() && point == this.waypoints[0])
    {
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageCantRemoveWaypointBecauseFirst")), MessageTypeDefOf.RejectInput, false);
    }
    else
    {
      ((WorldObject) point).Destroy();
      this.waypoints.Remove(point);
      for (int index = this.waypoints.Count - 1; index >= 1; --index)
      {
        if (PlanetTile.op_Equality(((WorldObject) this.waypoints[index]).Tile, ((WorldObject) this.waypoints[index - 1]).Tile))
        {
          ((WorldObject) this.waypoints[index]).Destroy();
          this.waypoints.RemoveAt(index);
        }
      }
      this.RecreatePaths();
      if (!playSound)
        return;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
    }
  }

  private void ReleasePaths()
  {
    foreach (WorldPath path in this.paths)
      path.ReleaseToPool();
    this.paths.Clear();
  }

  private void RecreatePaths()
  {
    this.ReleasePaths();
    for (int index = 1; index < this.waypoints.Count; ++index)
      this.paths.Add(this.pathfinder.FindPath(((WorldObject) this.waypoints[index - 1]).Tile, ((WorldObject) this.waypoints[index]).Tile, this.vehicleDefs));
    this.cachedTicksToWaypoint.Clear();
    int num = 0;
    int caravanTicksPerMove = this.CaravanTicksPerMove;
    if (this.waypoints.Count == 0)
      return;
    this.cachedTicksToWaypoint.Add(0);
    for (int index = 1; index < this.waypoints.Count; ++index)
    {
      num += VehicleCaravanPathingHelper.EstimatedTicksToArrive(this.vehicleDefs, ((WorldObject) this.waypoints[index - 1]).Tile, ((WorldObject) this.waypoints[index]).Tile, this.paths[index - 1], 0.0f, caravanTicksPerMove, GenTicks.TicksAbs + num);
      this.cachedTicksToWaypoint.Add(num);
    }
  }

  private RoutePlannerWaypoint MostRecentWaypointAt(int tile)
  {
    for (int index = this.waypoints.Count - 1; index >= 0; --index)
    {
      if (PlanetTile.op_Equality(((WorldObject) this.waypoints[index]).Tile, PlanetTile.op_Implicit(tile)))
        return this.waypoints[index];
    }
    return (RoutePlannerWaypoint) null;
  }
}
