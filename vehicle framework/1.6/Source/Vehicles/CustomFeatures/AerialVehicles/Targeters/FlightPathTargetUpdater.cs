// Decompiled with JetBrains decompiler
// Type: Vehicles.World.FlightPathTargetUpdater
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Targeting;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class FlightPathTargetUpdater : ITargeterUpdate<GlobalTargetInfo>
{
  protected readonly ILauncher launcher;
  protected readonly VehiclePawn vehicle;

  public FlightPathTargetUpdater(VehiclePawn vehicle, ILauncher launcher)
  {
    this.launcher = launcher;
    this.vehicle = vehicle;
  }

  protected float TotalDistance { get; private set; }

  private Material LineMaterial
  {
    get
    {
      switch (this.LaunchStatus())
      {
        case ShuttleLaunchStatus.Invalid:
          return TexData.WorldLineMatRed;
        case ShuttleLaunchStatus.NoReturnTrip:
          return TexData.WorldLineMatYellow;
        case ShuttleLaunchStatus.Valid:
          return TexData.WorldLineMatWhite;
        default:
          throw new NotImplementedException("ShuttleLaunchStatus");
      }
    }
  }

  public virtual void TargeterOnGUI()
  {
  }

  public virtual void TargeterUpdate([RequiresLocation, In] ref TargetData<GlobalTargetInfo> targetData)
  {
    this.TotalDistance = 0.0f;
    GlobalTargetInfo globalTargetInfo = FlightPathTargetUpdater.CurrentTargetUnderMouse();
    Vector3 tilePos1 = WorldHelper.GetTilePos(((GlobalTargetInfo) ref globalTargetInfo).Tile);
    Vector3 vector3 = this.launcher.Origin;
    Material lineMaterial = this.LineMaterial;
    foreach (GlobalTargetInfo target in targetData.targets)
    {
      Vector3 tilePos2 = WorldHelper.GetTilePos(((GlobalTargetInfo) ref target).Tile);
      this.TotalDistance += Ext_Math.SphericalDistance(vector3, tilePos2);
      FlightPath.DrawPath(vector3, tilePos2, lineMaterial);
      vector3 = tilePos2;
    }
    LaunchProtocol launchProtocol = this.vehicle.CompVehicleLauncher.launchProtocol;
    if (((GlobalTargetInfo) ref globalTargetInfo).IsValid && targetData.targets.Count < launchProtocol.MaxFlightNodes)
    {
      this.TotalDistance += Ext_Math.SphericalDistance(vector3, tilePos1);
      FlightPath.DrawPath(vector3, tilePos1, lineMaterial);
      WorldRendererUtility.DrawQuadTangentialToPlanet(tilePos1, 0.8f * Find.WorldGrid.AverageTileSize, 0.018f, WorldMaterials.CurTargetingMat, 0.0f, false, false, (MaterialPropertyBlock) null);
    }
    Vector2 vector2 = Text.CalcSize(TaggedString.op_Implicit(Translator.Translate("VF_DoubleClickShuttleTarget")));
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(tilePos1.x, tilePos1.y, 32f, 32f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect1).y, 9999f, 100f);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).x - vector2.x * 0.1f, ((Rect) ref rect2).y, vector2.x * 1.2f, vector2.y);
    Graphics.DrawTexture(rect3, (Texture) TexUI.GrayTextBG);
  }

  protected virtual ShuttleLaunchStatus LaunchStatus()
  {
    return this.vehicle.CompVehicleLauncher.FixedMaxDistance > 0 && (double) this.TotalDistance > (double) this.vehicle.CompVehicleLauncher.FixedMaxDistance ? ShuttleLaunchStatus.Invalid : ShuttleLaunchStatus.Valid;
  }

  protected static GlobalTargetInfo CurrentTargetUnderMouse()
  {
    List<WorldObject> worldObjectList = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
    if (!GenList.NullOrEmpty<WorldObject>((IList<WorldObject>) worldObjectList))
      return GlobalTargetInfo.op_Implicit(worldObjectList[0]);
    PlanetTile planetTile = GenWorld.MouseTile(false);
    return !((PlanetTile) ref planetTile).Valid ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(planetTile);
  }
}
