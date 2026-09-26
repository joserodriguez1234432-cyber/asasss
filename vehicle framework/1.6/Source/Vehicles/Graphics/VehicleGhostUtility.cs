// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleGhostUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleGhostUtility
{
  public static readonly Color whiteGhostColor = new Color(1f, 1f, 1f, 0.5f);
  private static readonly Dictionary<int, Graphic> cachedGhostGraphics = new Dictionary<int, Graphic>();

  public static void DrawGhostVehicleDef(
    IntVec3 center,
    Rot8 rot,
    VehicleDef vehicleDef,
    Color ghostCol,
    AltitudeLayer drawAltitude,
    VehiclePawn vehicle = null)
  {
    Graphic graphic1 = ((BuildableDef) vehicleDef).graphic;
    Graphic graphic2 = GhostUtility.GhostGraphicFor(graphic1, (ThingDef) vehicleDef, ghostCol, (ThingDef) null);
    Vector3 vector3 = GenThing.TrueCenter(center, (Rot4) rot, ((BuildableDef) vehicleDef).Size, Altitudes.AltitudeFor(drawAltitude));
    Rot8 rot8 = rot;
    float num = rot.AsRotationAngle;
    if (rot8.IsDiagonal)
    {
      switch (rot8.AsInt)
      {
        case 4:
          rot8 = Rot8.North;
          num = 45f;
          break;
        case 5:
          rot8 = Rot8.South;
          num = -45f;
          break;
        case 6:
          rot8 = Rot8.South;
          num = 45f;
          break;
        case 7:
          rot8 = Rot8.North;
          num = -45f;
          break;
      }
    }
    graphic2.DrawFromDef(vector3, (Rot4) rot8, (ThingDef) vehicleDef, num);
    VehicleGhostUtility.DrawGhostOverlays(center, rot, vehicleDef, graphic1, ghostCol, drawAltitude, (Thing) vehicle);
  }

  public static void DrawGhostOverlays(
    IntVec3 center,
    Rot8 rot,
    VehicleDef vehicleDef,
    Graphic baseGraphic,
    Color ghostCol,
    AltitudeLayer drawAltitude,
    Thing thing = null)
  {
    Vector3 loc = GenThing.TrueCenter(center, (Rot4) rot, ((BuildableDef) vehicleDef).Size, Altitudes.AltitudeFor(drawAltitude));
    foreach ((Graphic graphic, float rotation) in vehicleDef.GhostGraphicOverlaysFor(ghostCol))
      graphic.DrawWorker(Vector3.op_Addition(loc, baseGraphic.DrawOffsetFull(rot)), (Rot4) rot, (ThingDef) vehicleDef, thing, rotation);
    if (vehicleDef.GetSortedCompProperties<CompProperties_VehicleTurrets>() == null)
      return;
    vehicleDef.DrawGhostTurretTextures(loc, rot, ghostCol);
  }

  private static Graphic_Turret GhostGraphicFor(
    this VehicleDef vehicleDef,
    VehicleTurret turret,
    Color ghostColor)
  {
    int key = Gen.HashCombineStruct<Color>(Gen.HashCombine<VehicleTurret>(Gen.HashCombine<VehicleDef>(0, vehicleDef), turret), ghostColor);
    Graphic graphic1;
    if (!VehicleGhostUtility.cachedGhostGraphics.TryGetValue(key, out graphic1))
    {
      turret.ResolveGraphics(vehicleDef, true);
      Graphic graphic2 = (Graphic) turret.Graphic;
      GraphicData graphicData = new GraphicData();
      graphicData.CopyFrom(graphic2.data);
      graphicData.drawOffsetWest = graphic2.data.drawOffsetWest;
      graphicData.shadowData = (ShadowData) null;
      graphicData.shaderType = ShaderTypeDefOf.EdgeDetect;
      Graphic graphic3 = graphicData.Graphic;
      graphic1 = GraphicDatabase.Get(graphic2.GetType(), graphic2.path, ShaderTypeDefOf.EdgeDetect.Shader, graphic2.drawSize, ghostColor, Color.white, graphicData, (List<ShaderParameter>) null, (string) null);
      VehicleGhostUtility.cachedGhostGraphics.Add(key, graphic1);
    }
    return (Graphic_Turret) graphic1;
  }

  private static IEnumerable<(Graphic graphic, float rotation)> GhostGraphicOverlaysFor(
    this VehicleDef vehicleDef,
    Color ghostColor)
  {
    int num = 0;
    num = Gen.HashCombine<VehicleDef>(num, vehicleDef);
    num = Gen.HashCombineStruct<Color>(num, ghostColor);
    foreach (GraphicOverlay overlay in vehicleDef.drawProperties.overlays)
    {
      int key = Gen.HashCombine<GraphicDataRGB>(num, overlay.data.graphicData);
      Graphic graphic1;
      if (!VehicleGhostUtility.cachedGhostGraphics.TryGetValue(key, out graphic1))
      {
        graphic1 = overlay.Graphic;
        GraphicData graphicData = new GraphicData();
        graphicData.CopyFrom(graphic1.data);
        graphicData.drawOffsetWest = graphic1.data.drawOffsetWest;
        graphicData.shadowData = (ShadowData) null;
        graphicData.shaderType = ShaderTypeDefOf.EdgeDetect;
        Graphic graphic2 = graphicData.Graphic;
        graphic1 = GraphicDatabase.Get(graphic1.GetType(), graphic1.path, ShaderTypeDefOf.EdgeDetect.Shader, graphic1.drawSize, ghostColor, Color.white, graphicData, (List<ShaderParameter>) null, (string) null);
        VehicleGhostUtility.cachedGhostGraphics.Add(key, graphic1);
      }
      yield return (graphic1, overlay.data.rotation);
    }
  }

  private static void DrawGhostTurretTextures(
    this VehicleDef vehicleDef,
    Vector3 loc,
    Rot8 rot,
    Color ghostColor)
  {
    CompProperties_VehicleTurrets sortedCompProperties = vehicleDef.GetSortedCompProperties<CompProperties_VehicleTurrets>();
    if (sortedCompProperties == null)
      return;
    foreach (VehicleTurret turret in sortedCompProperties.turrets)
    {
      if (!turret.NoGraphic && GenText.NullOrEmpty(turret.parentKey))
      {
        turret.ResolveGraphics(vehicleDef);
        try
        {
          float num = turret.defaultAngleRotated + rot.AsAngle;
          if (turret.attachedTo != null)
            num += turret.attachedTo.defaultAngleRotated;
          Vector3 vector3_1 = turret.DrawPosition(rot);
          Vector3 vector3_2 = Vector3.op_Addition(loc, vector3_1);
          if (!turret.NoGraphic)
          {
            Graphic graphic = (Graphic) vehicleDef.GhostGraphicFor(turret, ghostColor);
            Graphics.DrawMesh(graphic.MeshAt((Rot4) rot), vector3_2, GenMath.ToQuat(num), graphic.MatAt((Rot4) rot, (Thing) null), 0);
          }
        }
        catch (Exception ex)
        {
          Log.Error($"Failed to render Cannon=\"{turret.def.defName}\" for VehicleDef=\"{((Def) vehicleDef).defName}\", Exception: {ex}");
        }
      }
    }
  }

  private static void DrawTurretGhostOverlays(
    VehicleDef vehicleDef,
    VehicleTurret turret,
    Color ghostColor,
    Vector3 drawPos,
    Rot8 rot,
    float extraRotation)
  {
    if (GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) turret.TurretGraphics))
      return;
    for (int index = 0; index < turret.TurretGraphics.Count; ++index)
    {
      Graphic graphic = (Graphic) vehicleDef.GhostGraphicFor(turret, ghostColor);
      VehicleTurret.TurretDrawData turretGraphic = turret.TurretGraphics[index];
      Vector3 vector3 = Vector3.op_Addition(drawPos, turretGraphic.DrawOffset(rot, 0.0f, extraRotation));
      Graphics.DrawMesh(graphic.MeshAt((Rot4) rot), vector3, GenMath.ToQuat(extraRotation), graphic.MatAt((Rot4) rot, (Thing) null), 0);
    }
  }
}
