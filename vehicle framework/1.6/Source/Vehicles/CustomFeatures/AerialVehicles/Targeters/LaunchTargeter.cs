// Decompiled with JetBrains decompiler
// Type: Vehicles.LaunchTargeter
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
using Vehicles.Rendering;
using Vehicles.World;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class LaunchTargeter : BaseVehicleWorldTargeter
{
  private const float BaseFeedbackTexSize = 0.8f;
  private Func<GlobalTargetInfo, float, bool> action;
  private Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter;

  public static LaunchTargeter Instance { get; private set; }

  public static float TotalDistance { get; private set; }

  public static float TotalFuelCost { get; private set; }

  public static List<FlightNode> FlightPath { get; } = new List<FlightNode>();

  public override bool IsTargeting => this.action != null;

  public static void BeginTargeting(
    VehiclePawn vehicle,
    Func<GlobalTargetInfo, float, bool> action,
    PlanetTile origin,
    bool canTargetTiles,
    Texture2D mouseAttachment = null,
    bool closeWorldTabWhenFinished = false,
    Action onUpdate = null,
    Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter = null)
  {
    LaunchTargeter.Instance.vehicle = vehicle;
    LaunchTargeter.Instance.action = action;
    LaunchTargeter.Instance.origin = origin;
    LaunchTargeter.Instance.originOnMap = WorldHelper.GetTilePos(origin);
    LaunchTargeter.Instance.canTargetTiles = canTargetTiles;
    LaunchTargeter.Instance.mouseAttachment = mouseAttachment;
    LaunchTargeter.Instance.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
    LaunchTargeter.Instance.onUpdate = onUpdate;
    LaunchTargeter.Instance.extraLabelGetter = extraLabelGetter;
    LaunchTargeter.FlightPath.Clear();
    LaunchTargeter.TotalDistance = 0.0f;
    LaunchTargeter.TotalFuelCost = 0.0f;
    LaunchTargeter.Instance.OnStart();
  }

  public static void BeginTargeting(
    VehiclePawn vehicle,
    Func<GlobalTargetInfo, float, bool> action,
    AerialVehicleInFlight aerialVehicle,
    bool canTargetTiles,
    Texture2D mouseAttachment = null,
    bool closeWorldTabWhenFinished = false,
    Action onUpdate = null,
    Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter = null)
  {
    LaunchTargeter.Instance.vehicle = vehicle;
    LaunchTargeter.Instance.action = action;
    LaunchTargeter.Instance.aerialVehicle = aerialVehicle;
    LaunchTargeter.Instance.canTargetTiles = canTargetTiles;
    LaunchTargeter.Instance.mouseAttachment = mouseAttachment;
    LaunchTargeter.Instance.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
    LaunchTargeter.Instance.onUpdate = onUpdate;
    LaunchTargeter.Instance.extraLabelGetter = extraLabelGetter;
    LaunchTargeter.FlightPath.Clear();
    LaunchTargeter.TotalDistance = 0.0f;
    LaunchTargeter.TotalFuelCost = 0.0f;
    LaunchTargeter.Instance.OnStart();
  }

  public static void ContinueTargeting(
    VehiclePawn vehicle,
    Func<GlobalTargetInfo, float, bool> action,
    PlanetTile origin,
    bool canTargetTiles,
    Texture2D mouseAttachment = null,
    bool closeWorldTabWhenFinished = false,
    Action onUpdate = null,
    Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter = null)
  {
    LaunchTargeter.Instance.vehicle = vehicle;
    LaunchTargeter.Instance.action = action;
    LaunchTargeter.Instance.origin = origin;
    LaunchTargeter.Instance.originOnMap = WorldHelper.GetTilePos(origin);
    LaunchTargeter.Instance.canTargetTiles = canTargetTiles;
    LaunchTargeter.Instance.mouseAttachment = mouseAttachment;
    LaunchTargeter.Instance.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
    LaunchTargeter.Instance.onUpdate = onUpdate;
    LaunchTargeter.Instance.extraLabelGetter = extraLabelGetter;
    LaunchTargeter.Instance.OnStart();
  }

  public void ContinueTargeting(
    VehiclePawn vehicle,
    Func<GlobalTargetInfo, float, bool> action,
    AerialVehicleInFlight aerialVehicle,
    bool canTargetTiles,
    Texture2D mouseAttachment = null,
    bool closeWorldTabWhenFinished = false,
    Action onUpdate = null,
    Func<GlobalTargetInfo, List<FlightNode>, float, string> extraLabelGetter = null)
  {
    LaunchTargeter.Instance.vehicle = vehicle;
    LaunchTargeter.Instance.action = action;
    LaunchTargeter.Instance.aerialVehicle = aerialVehicle;
    LaunchTargeter.Instance.canTargetTiles = canTargetTiles;
    LaunchTargeter.Instance.mouseAttachment = mouseAttachment;
    LaunchTargeter.Instance.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
    LaunchTargeter.Instance.onUpdate = onUpdate;
    LaunchTargeter.Instance.extraLabelGetter = extraLabelGetter;
    LaunchTargeter.Instance.OnStart();
  }

  public override void RegisterActionOnTile(PlanetTile tile, IArrivalAction arrivalAction)
  {
    GenCollection.Pop<FlightNode>(LaunchTargeter.FlightPath);
    LaunchTargeter.FlightPath.Add(new FlightNode(tile));
  }

  public override void StopTargeting()
  {
    if (this.closeWorldTabWhenFinished)
      CameraJumper.TryHideWorld();
    this.action = (Func<GlobalTargetInfo, float, bool>) null;
    this.canTargetTiles = false;
    this.mouseAttachment = (Texture2D) null;
    this.closeWorldTabWhenFinished = false;
    this.onUpdate = (Action) null;
    this.extraLabelGetter = (Func<GlobalTargetInfo, List<FlightNode>, float, string>) null;
    this.aerialVehicle = (AerialVehicleInFlight) null;
  }

  public override void ProcessInputEvents()
  {
    if (Event.current.type == null)
    {
      if (Event.current.button == 0 && this.IsTargeting)
      {
        GlobalTargetInfo globalTargetInfo = this.CurrentTargetUnderMouse();
        bool flag1 = LaunchTargeter.FlightPath.Count == this.vehicle.CompVehicleLauncher.launchProtocol.MaxFlightNodes;
        AerialVehicleInFlight aerialVehicle = this.aerialVehicle;
        PlanetTile planetTile1;
        if (aerialVehicle == null)
        {
          Map map = ((Thing) this.vehicle).Map;
          planetTile1 = map != null ? map.Tile : this.origin;
        }
        else
          planetTile1 = aerialVehicle.Tile;
        PlanetTile planetTile2 = planetTile1;
        bool flag2 = !this.vehicle.CompVehicleLauncher.inFlight && PlanetTile.op_Equality(planetTile2, ((GlobalTargetInfo) ref globalTargetInfo).Tile) && GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) LaunchTargeter.FlightPath);
        if (WorldHelper.WorldObjectAt(((GlobalTargetInfo) ref globalTargetInfo).Tile) != null && this.vehicle.CompVehicleLauncher.SpaceFlight && !flag1 && !flag2)
          LaunchTargeter.FlightPath.Add(new FlightNode(((GlobalTargetInfo) ref globalTargetInfo).Tile));
        if (PlanetTile.op_Equality(LaunchTargeter.FlightPath.LastOrDefault<FlightNode>().Tile, ((GlobalTargetInfo) ref globalTargetInfo).Tile))
        {
          if (this.action(globalTargetInfo, LaunchTargeter.TotalFuelCost))
          {
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
            this.StopTargeting();
          }
        }
        else if (flag1)
          Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_FlightPathMaxNodes", NamedArgument.op_Implicit(((Entity) this.vehicle).LabelShortCap))), MessageTypeDefOf.RejectInput, true);
        else if (flag2)
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
        else if (((GlobalTargetInfo) ref globalTargetInfo).IsValid)
          LaunchTargeter.FlightPath.Add(new FlightNode(((GlobalTargetInfo) ref globalTargetInfo).Tile));
        Event.current.Use();
      }
      if (Event.current.button == 1 && this.IsTargeting)
      {
        if (LaunchTargeter.FlightPath.Count > 0)
        {
          GenCollection.Pop<FlightNode>(LaunchTargeter.FlightPath);
        }
        else
        {
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
          this.StopTargeting();
        }
        Event.current.Use();
      }
    }
    if ((Event.current.type != null || Event.current.button != 1) && (!KeyBindingDefOf.Cancel.KeyDownEvent || !this.IsTargeting))
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
    this.StopTargeting();
    Event.current.Use();
  }

  public override void TargeterOnGUI()
  {
    if (!this.IsTargeting || Mouse.IsInputBlockedNow)
      return;
    GlobalTargetInfo globalTargetInfo = this.CurrentTargetUnderMouse();
    Vector2 mousePosition = Event.current.mousePosition;
    Texture2D texture2D = this.mouseAttachment ?? TexCommand.Attack;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
    GUI.DrawTexture(rect1, (Texture) texture2D);
    float fuelCost1;
    float distance;
    this.CostAndDistanceCalculator(out fuelCost1, out distance);
    Vector3 from = this.originOnMap;
    if (!GenList.NullOrEmpty<FlightNode>((IList<FlightNode>) LaunchTargeter.FlightPath))
      from = WorldHelper.GetTilePos(LaunchTargeter.FlightPath.LastOrDefault<FlightNode>().Tile);
    float num1 = fuelCost1;
    float num2 = distance;
    if (((GlobalTargetInfo) ref globalTargetInfo).IsValid && LaunchTargeter.FlightPath.Count < this.vehicle.CompVehicleLauncher.launchProtocol.MaxFlightNodes)
    {
      float fuelCost2;
      (fuelCost2, distance) = this.CostAndDistanceCalculator(from, WorldHelper.GetTilePos(((GlobalTargetInfo) ref globalTargetInfo).Tile));
      num1 += fuelCost2;
      num2 += distance;
    }
    LaunchTargeter.TotalFuelCost = (float) Mathf.RoundToInt(num1);
    LaunchTargeter.TotalDistance = num2;
    string str1 = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleFuelCost", NamedArgument.op_Implicit(LaunchTargeter.TotalFuelCost)));
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Vector2 vector2_1 = Text.CalcSize(str1);
      Rect rect2 = new Rect(mousePosition.x, (float) ((double) mousePosition.y + (double) vector2_1.y + 20.0), vector2_1.x, vector2_1.y);
      float num3 = vector2_1.x * 1.2f;
      if (this.extraLabelGetter != null)
      {
        string str2 = this.extraLabelGetter(globalTargetInfo, LaunchTargeter.FlightPath, LaunchTargeter.TotalFuelCost);
        Vector2 vector2_2 = Text.CalcSize(str2);
        Rect rect3;
        // ISSUE: explicit constructor call
        ((Rect) ref rect3).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect1).y, 9999f, 100f);
        Rect rect4;
        // ISSUE: explicit constructor call
        ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).x - vector2_2.x * 0.1f, ((Rect) ref rect3).y, vector2_2.x * 1.2f, vector2_2.y);
        GUI.DrawTexture(rect4, (Texture) TexUI.GrayTextBG);
        Widgets.Label(rect3, str2);
      }
      if ((double) LaunchTargeter.TotalFuelCost <= 0.0)
        return;
      GUI.DrawTexture(new Rect(((Rect) ref rect2).x - vector2_1.x * 0.1f, ((Rect) ref rect2).y, num3, vector2_1.y), (Texture) TexUI.GrayTextBG);
      Widgets.Label(rect2, str1);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public virtual void DrawAirDefenseGrid()
  {
    foreach (AirDefense airDefense in AirDefensePositionTracker.airDefenseCache.Values)
    {
      if (FactionUtility.HostileTo(airDefense.parent.Faction, Faction.OfPlayer))
        RenderHelper.DrawWorldRadiusRing(airDefense.parent.Tile, Mathf.CeilToInt(airDefense.MaxDistance), TexData.OneSidedWorldLineMatRed);
    }
  }

  public override void TargeterUpdate()
  {
    this.DrawAirDefenseGrid();
    if (this.aerialVehicle != null)
      this.originOnMap = ((WorldObject) this.aerialVehicle).DrawPos;
    Vector3 vector3 = Vector3.zero;
    GlobalTargetInfo globalTargetInfo = this.CurrentTargetUnderMouse();
    if (((GlobalTargetInfo) ref globalTargetInfo).HasWorldObject)
      vector3 = ((GlobalTargetInfo) ref globalTargetInfo).WorldObject.DrawPos;
    else if (PlanetTile.op_Implicit(((GlobalTargetInfo) ref globalTargetInfo).Tile) >= 0)
      vector3 = WorldHelper.GetTilePos(((GlobalTargetInfo) ref globalTargetInfo).Tile);
    if (((GlobalTargetInfo) ref globalTargetInfo).IsValid && !Mouse.IsInputBlockedNow)
      WorldRendererUtility.DrawQuadTangentialToPlanet(vector3, 0.8f * Find.WorldGrid.AverageTileSize, 0.018f, WorldMaterials.CurTargetingMat, 0.0f, false, false, (MaterialPropertyBlock) null);
    Vector3 start = this.originOnMap;
    List<PlanetTile> list = new List<PlanetTile>(LaunchTargeter.FlightPath.Select<FlightNode, PlanetTile>((Func<FlightNode, PlanetTile>) (n => n.Tile)));
    Material material = (Material) null;
    for (int index = 0; index < LaunchTargeter.FlightPath.Count; ++index)
    {
      Vector3 tilePos = WorldHelper.GetTilePos(list.PopAt<PlanetTile>(0));
      LaunchTargeter.DrawTravelPoint(start, tilePos, material);
      start = tilePos;
    }
    if (LaunchTargeter.FlightPath.Count > 0)
    {
      Vector2 vector2 = Text.CalcSize(TaggedString.op_Implicit(Translator.Translate("VF_DoubleClickShuttleTarget")));
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(start.x, start.y, 32f, 32f);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect1).y, 9999f, 100f);
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).x - vector2.x * 0.1f, ((Rect) ref rect2).y, vector2.x * 1.2f, vector2.y);
      Graphics.DrawTexture(rect3, (Texture) TexUI.GrayTextBG);
      WorldRendererUtility.DrawQuadTangentialToPlanet(start, 0.8f * Find.WorldGrid.AverageTileSize, 0.018f, WorldMaterials.CurTargetingMat, 0.0f, false, false, (MaterialPropertyBlock) null);
    }
    if (LaunchTargeter.FlightPath.Count < this.vehicle.CompVehicleLauncher.launchProtocol.MaxFlightNodes && ((GlobalTargetInfo) ref globalTargetInfo).IsValid)
      LaunchTargeter.DrawTravelPoint(start, WorldHelper.GetTilePos(((GlobalTargetInfo) ref globalTargetInfo).Tile), material);
    Action onUpdate = this.onUpdate;
    if (onUpdate == null)
      return;
    onUpdate();
  }

  public bool IsTargetedNow(WorldObject worldObject, List<WorldObject> worldObjectsUnderMouse = null)
  {
    if (!this.IsTargeting)
      return false;
    if (worldObjectsUnderMouse == null)
      worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
    return GenCollection.Any<WorldObject>(worldObjectsUnderMouse) && worldObject == worldObjectsUnderMouse[0];
  }

  private void CostAndDistanceCalculator(out float fuelCost, out float distance)
  {
    fuelCost = 0.0f;
    distance = 0.0f;
    Vector3 from = this.originOnMap;
    foreach (FlightNode flightNode in LaunchTargeter.FlightPath)
    {
      Vector3 tilePos = WorldHelper.GetTilePos(flightNode.Tile);
      (float fuelCost1, float distance1) = this.CostAndDistanceCalculator(from, tilePos);
      fuelCost += fuelCost1;
      distance += distance1;
      from = tilePos;
    }
  }

  private (float fuelCost, float distance) CostAndDistanceCalculator(Vector3 from, Vector3 to)
  {
    if (Vector3.op_Equality(from, to))
      return (0.0f, 0.0f);
    float tileDistance = Ext_Math.SphericalDistance(from, to);
    float num = 0.0f;
    if (this.vehicle.CompFueledTravel != null)
      num = this.vehicle.CompVehicleLauncher.FuelNeededToLaunchAtDist(tileDistance);
    return (num, tileDistance);
  }

  public static void DrawTravelPoint(Vector3 start, Vector3 end, Material material = null)
  {
    if (material == null)
      material = TexData.WorldLineMatWhite;
    int num1 = Mathf.CeilToInt(Ext_Math.SphericalDistance(start, end) * 100f / 5f);
    start = Vector3.op_Addition(start, Vector3.op_Multiply(((Vector3) ref start).normalized, 0.05f));
    end = Vector3.op_Addition(end, Vector3.op_Multiply(((Vector3) ref end).normalized, 0.05f));
    Vector3 vector3_1 = start;
    for (int index = 1; index <= num1; ++index)
    {
      float num2 = (float) index / (float) num1;
      Vector3 vector3_2 = Vector3.Slerp(start, end, num2);
      GenDraw.DrawWorldLineBetween(vector3_1, vector3_2, material, 0.5f);
      vector3_1 = vector3_2;
    }
  }

  public override void PostInit() => LaunchTargeter.Instance = this;
}
