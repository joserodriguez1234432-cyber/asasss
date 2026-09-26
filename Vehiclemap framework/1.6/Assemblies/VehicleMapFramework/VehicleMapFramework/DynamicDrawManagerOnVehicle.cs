// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.DynamicDrawManagerOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class DynamicDrawManagerOnVehicle
{
  private static readonly Dictionary<Map, (int frame, CellRect rect)> cachedRect = new Dictionary<Map, (int, CellRect)>();

  public static void DrawDynamicThings(Map map)
  {
    if (!DebugViewSettings.drawThingsDynamic || map.Disposed)
      return;
    bool flag1 = SilhouetteUtility.CanHighlightAny();
    IReadOnlyList<Thing> drawThings = map.dynamicDrawManager.DrawThings;
    if (!DebugViewSettings.singleThreadedDrawing)
    {
      ProfilerBlock profilerBlock;
      // ISSUE: explicit constructor call
      ((ProfilerBlock) ref profilerBlock).\u002Ector("Ensure Graphics Initialized");
      try
      {
        for (int index = 0; index < drawThings.Count; ++index)
          drawThings[index].DynamicDrawPhase((DrawPhase) 0);
      }
      finally
      {
        profilerBlock.Dispose();
      }
    }
    try
    {
      using (new ProfilerBlock("Draw Visible"))
      {
        MapComponent mapComponent = (MapComponent) null;
        bool flag2 = ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active && (mapComponent = ModCompat.AsAboveSoBelow.CompOf(map)) != null && ModCompat.AsAboveSoBelow.Banded(mapComponent);
        int num1 = flag2 ? ModCompat.AsAboveSoBelow.CurrentBand(map) : 0;
        for (int index = 0; index < drawThings.Count; ++index)
        {
          try
          {
            if (flag2)
            {
              IntVec3 position = drawThings[index].Position;
              int num2 = ModCompat.AsAboveSoBelow.BandOf(mapComponent, position);
              if (num2 <= num1)
              {
                if (num2 < num1)
                {
                  if (!(bool) ModCompat.AsAboveSoBelow.TryResolveVisibleBelow.Invoke((object) null, Params<(object, object, IntVec3, IntVec3, int)>.Get(((object) map, (object) mapComponent, ModCompat.AsAboveSoBelow.Translate(mapComponent, position, num1), IntVec3.Zero, 0))))
                    continue;
                }
              }
              else
                continue;
            }
            drawThings[index].DynamicDrawPhase((DrawPhase) 2);
            if (flag1)
            {
              if (drawThings[index] is Pawn pawn)
                SilhouetteUtility.DrawGraphicSilhouette((Thing) pawn, pawn.Drawer.renderer.SilhouettePos);
            }
          }
          catch (Exception ex)
          {
            Log.Error($"Exception drawing {drawThings[index]}: {ex}");
          }
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception drawing dynamic things: {ex}");
    }
  }

  public static CellRect GetSunShadowsViewRect(Map map, CellRect rect)
  {
    if (!DynamicDrawManagerOnVehicle.cachedRect.ContainsKey(map))
      DynamicDrawManagerOnVehicle.cachedRect[map] = (RealTime.frameCount, CellRect.Empty);
    else if (DynamicDrawManagerOnVehicle.cachedRect[map].frame == RealTime.frameCount)
      return DynamicDrawManagerOnVehicle.cachedRect[map].rect;
    GenCelestial.LightInfo lightSourceInfo = GenCelestial.GetLightSourceInfo(map, (GenCelestial.LightType) 0);
    if ((double) lightSourceInfo.vector.x < 0.0)
      rect.maxX -= Mathf.FloorToInt(lightSourceInfo.vector.x);
    else
      rect.minX -= Mathf.CeilToInt(lightSourceInfo.vector.x);
    if ((double) lightSourceInfo.vector.y < 0.0)
      rect.maxZ -= Mathf.FloorToInt(lightSourceInfo.vector.y);
    else
      rect.minZ -= Mathf.CeilToInt(lightSourceInfo.vector.y);
    DynamicDrawManagerOnVehicle.cachedRect[map] = (RealTime.frameCount, rect);
    return DynamicDrawManagerOnVehicle.cachedRect[map].rect;
  }
}
