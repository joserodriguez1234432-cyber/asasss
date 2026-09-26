// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.RenderHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles.Rendering;

public static class RenderHelper
{
  private static readonly List<PlanetTile> cachedEdgeTiles = new List<PlanetTile>();
  private static readonly Dictionary<(Vector2 size, Rot4 rot), Mesh> rotatedMeshes = new Dictionary<(Vector2, Rot4), Mesh>();
  private static int cachedEdgeTilesForCenter = -1;
  private static int cachedEdgeTilesForRadius = -1;
  private static int cachedEdgeTilesForWorldSeed = -1;

  public static void DrawLinesBetweenTargets(VehiclePawn vehicle, Job curJob, JobQueue jobQueue)
  {
    IntVec3 position = ((Thing) vehicle).Position;
    Vector3 vector3 = ((IntVec3) ref position).ToVector3Shifted();
    if (vehicle.vehiclePather.curPath != null)
    {
      LocalTargetInfo destination = vehicle.vehiclePather.Destination;
      vector3 = ((LocalTargetInfo) ref destination).CenterVector3;
    }
    else if (curJob != null && ((LocalTargetInfo) ref curJob.targetA).IsValid && (!((LocalTargetInfo) ref curJob.targetA).HasThing || ((LocalTargetInfo) ref curJob.targetA).Thing.Spawned && ((LocalTargetInfo) ref curJob.targetA).Thing != vehicle && ((LocalTargetInfo) ref curJob.targetA).Thing.Map == ((Thing) vehicle).Map))
    {
      GenDraw.DrawLineBetween(vector3, ((LocalTargetInfo) ref curJob.targetA).CenterVector3, Altitudes.AltitudeFor((AltitudeLayer) 18));
      vector3 = ((LocalTargetInfo) ref curJob.targetA).CenterVector3;
    }
    for (int index1 = 0; index1 < jobQueue.Count; ++index1)
    {
      if (((LocalTargetInfo) ref jobQueue[index1].job.targetA).IsValid)
      {
        if (!((LocalTargetInfo) ref jobQueue[index1].job.targetA).HasThing || ((LocalTargetInfo) ref jobQueue[index1].job.targetA).Thing.Spawned && ((LocalTargetInfo) ref jobQueue[index1].job.targetA).Thing.Map == ((Thing) vehicle).Map)
        {
          Vector3 centerVector3 = ((LocalTargetInfo) ref jobQueue[index1].job.targetA).CenterVector3;
          GenDraw.DrawLineBetween(vector3, centerVector3, Altitudes.AltitudeFor((AltitudeLayer) 18));
          vector3 = centerVector3;
        }
      }
      else
      {
        List<LocalTargetInfo> targetQueueA = jobQueue[index1].job.targetQueueA;
        if (targetQueueA != null)
        {
          for (int index2 = 0; index2 < targetQueueA.Count; ++index2)
          {
            LocalTargetInfo localTargetInfo1 = targetQueueA[index2];
            if (((LocalTargetInfo) ref localTargetInfo1).HasThing)
            {
              LocalTargetInfo localTargetInfo2 = targetQueueA[index2];
              if (((LocalTargetInfo) ref localTargetInfo2).Thing.Spawned)
              {
                LocalTargetInfo localTargetInfo3 = targetQueueA[index2];
                if (((LocalTargetInfo) ref localTargetInfo3).Thing.Map != ((Thing) vehicle).Map)
                  continue;
              }
              else
                continue;
            }
            LocalTargetInfo localTargetInfo4 = targetQueueA[index2];
            Vector3 centerVector3 = ((LocalTargetInfo) ref localTargetInfo4).CenterVector3;
            GenDraw.DrawLineBetween(vector3, centerVector3, Altitudes.AltitudeFor((AltitudeLayer) 18));
            vector3 = centerVector3;
          }
        }
      }
    }
  }

  [Obsolete("Use MoteGenerator instead.")]
  public static Mote ThrowMoteEnhanced(
    Vector3 loc,
    Map map,
    MoteThrown mote,
    bool overrideSaturation = false)
  {
    if (!GenView.ShouldSpawnMotesAt(loc, map, true) || overrideSaturation && map.moteCounter.Saturated)
      return (Mote) null;
    GenSpawn.Spawn((Thing) mote, IntVec3Utility.ToIntVec3(loc), map, (WipeMode) 0);
    return (Mote) mote;
  }

  public static Mesh NewConeMesh(float distance, int arc)
  {
    float angle = (float) arc / -2f;
    Vector3[] vector3Array = new Vector3[arc + 2];
    Vector2[] vector2Array = new Vector2[vector3Array.Length];
    int[] numArray = new int[arc * 3];
    vector3Array[0] = Vector3.zero;
    vector2Array[0] = Vector2.op_Implicit(Vector3.zero);
    int index1 = 0;
    for (int index2 = 1; index2 <= arc; ++index2)
    {
      vector3Array[index2] = vector3Array[0].PointFromAngle(distance, angle);
      vector2Array[index2] = Vector2.op_Implicit(vector3Array[index2]);
      ++angle;
      numArray[index1] = 0;
      numArray[index1 + 1] = index2;
      numArray[index1 + 2] = index2 + 1;
      index1 += 3;
    }
    Mesh mesh = new Mesh();
    ((Object) mesh).name = "ConeMesh";
    mesh.vertices = vector3Array;
    mesh.uv = vector2Array;
    mesh.SetTriangles(numArray, 0);
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    return mesh;
  }

  public static void DrawWorldRadiusRing(PlanetTile center, int radius, Material material)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    RenderHelper.\u003C\u003Ec__DisplayClass8_0 cDisplayClass80 = new RenderHelper.\u003C\u003Ec__DisplayClass8_0();
    // ISSUE: reference to a compiler-generated field
    cDisplayClass80.radius = radius;
    // ISSUE: reference to a compiler-generated field
    if (cDisplayClass80.radius < 0)
      return;
    // ISSUE: reference to a compiler-generated field
    if (PlanetTile.op_Inequality(PlanetTile.op_Implicit(RenderHelper.cachedEdgeTilesForCenter), center) || RenderHelper.cachedEdgeTilesForRadius != cDisplayClass80.radius || RenderHelper.cachedEdgeTilesForWorldSeed != Find.World.info.Seed)
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      RenderHelper.\u003C\u003Ec__DisplayClass8_1 cDisplayClass81 = new RenderHelper.\u003C\u003Ec__DisplayClass8_1();
      RenderHelper.cachedEdgeTilesForCenter = PlanetTile.op_Implicit(center);
      // ISSUE: reference to a compiler-generated field
      RenderHelper.cachedEdgeTilesForRadius = cDisplayClass80.radius;
      RenderHelper.cachedEdgeTilesForWorldSeed = Find.World.info.Seed;
      RenderHelper.cachedEdgeTiles.Clear();
      // ISSUE: method pointer
      ((PlanetTile) ref center).Layer.Filler.FloodFill(center, (Predicate<PlanetTile>) (_ => true), new Predicate<PlanetTile, int>((object) cDisplayClass80, __methodptr(\u003CDrawWorldRadiusRing\u003Eb__1)), int.MaxValue, (IEnumerable<PlanetTile>) null);
      // ISSUE: reference to a compiler-generated field
      cDisplayClass81.worldGrid = Find.WorldGrid;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      cDisplayClass81.c = cDisplayClass81.worldGrid.GetTileCenter(center);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      cDisplayClass81.n = ((Vector3) ref cDisplayClass81.c).normalized;
      // ISSUE: reference to a compiler-generated method
      RenderHelper.cachedEdgeTiles.Sort(new Comparison<PlanetTile>(cDisplayClass81.\u003CDrawWorldRadiusRing\u003Eb__2));
    }
    GenDraw.DrawWorldLineStrip(RenderHelper.cachedEdgeTiles, material, 5f);
  }

  public static bool ShouldShow(Map map, IntVec3 cell, RenderConditions shouldShow)
  {
    if ((shouldShow & RenderConditions.CurrentMap) == RenderConditions.CurrentMap && map != Find.CurrentMap)
      return false;
    if ((shouldShow & RenderConditions.OnScreen) == RenderConditions.OnScreen)
    {
      CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
      CellRect cellRect = ((CellRect) ref currentViewRect).ExpandedBy(5);
      if (!((CellRect) ref cellRect).Contains(cell))
        return false;
    }
    return GenGrid.InBounds(cell, map);
  }
}
