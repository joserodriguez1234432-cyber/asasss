// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ThingSelectionUtility), "MultiSelectableThingsInScreenRectDistinct")]
[PatchLevel(Level.Safe)]
public static class Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct
{
  private static readonly FastInvokeHandler SelectableByMapClick = MethodInvoker.GetHandler(AccessTools.Method(typeof (ThingSelectionUtility), nameof (SelectableByMapClick), (Type[]) null, (Type[]) null), false);
  private static readonly HashSet<Thing> yieldedThings = new HashSet<Thing>();

  public static bool Prefix(ref IEnumerable<object> __result, Rect rect)
  {
    VehiclePawnWithMap vehicle;
    if (!UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.All))
      return true;
    __result = Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.MultiSelectableThings(vehicle, rect);
    return !__result.Any<object>();
  }

  private static IEnumerable<object> MultiSelectableThings(VehiclePawnWithMap vehicle, Rect rect)
  {
    Map focusedMap = vehicle.VehicleMap;
    CellRect mapRect = Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.GetMapRect(rect);
    Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.yieldedThings.Clear();
    foreach (List<Thing> thingList in ((IEnumerable<IntVec3>) (object) mapRect).Select<IntVec3, IntVec3>((Func<IntVec3, IntVec3>) (c => c.ToVehicleMapCoord(vehicle))).Where<IntVec3>((Func<IntVec3, bool>) (c2 => GenGrid.InBounds(c2, focusedMap))).Select<IntVec3, List<Thing>>((Func<IntVec3, List<Thing>>) (c2 => focusedMap.thingGrid.ThingsListAt(c2))).Where<List<Thing>>((Func<List<Thing>, bool>) (cellThings => cellThings != null)))
    {
      for (int index = 0; index < thingList.Count; ++index)
      {
        Thing thing = thingList[index];
        if ((bool) Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.SelectableByMapClick.Invoke((object) null, SingleParam.Get((object) thing)) && !thing.def.neverMultiSelect)
          Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.yieldedThings.Add(thing);
      }
    }
    Rect rectInWorldSpace = Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.GetRectInWorldSpace(rect);
    CellRect cellRect = ((CellRect) ref mapRect).ExpandedBy(1);
    foreach (IntVec3 edgeCell in ((CellRect) ref cellRect).EdgeCells)
    {
      IntVec3 vehicleMapCoord = edgeCell.ToVehicleMapCoord(vehicle);
      if (GenGrid.InBounds(vehicleMapCoord, focusedMap) && GridsUtility.GetItemCount(vehicleMapCoord, focusedMap) > 1)
      {
        foreach (Thing thing in focusedMap.thingGrid.ThingsAt(vehicleMapCoord))
        {
          if (thing.def.category == 2)
          {
            if ((bool) Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.SelectableByMapClick.Invoke((object) null, new object[1]
            {
              (object) thing
            }) && !thing.def.neverMultiSelect && !Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.yieldedThings.Contains(thing))
            {
              Vector3 vector3 = GenThing.TrueCenter(thing);
              Rect rect1;
              // ISSUE: explicit constructor call
              ((Rect) ref rect1).\u002Ector(vector3.x - 0.5f, vector3.z - 0.5f, 1f, 1f);
              if (((Rect) ref rect1).Overlaps(rectInWorldSpace))
                Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.yieldedThings.Add(thing);
            }
          }
        }
      }
    }
    return (IEnumerable<object>) Patch_ThingSelectionUtility_MultiSelectableThingsInScreenRectDistinct.yieldedThings;
  }

  private static CellRect GetMapRect(Rect rect)
  {
    Vector2 vector2_1;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_1).\u002Ector(((Rect) ref rect).x, (float) UI.screenHeight - ((Rect) ref rect).y);
    Vector2 vector2_2 = new Vector2(((Rect) ref rect).x + ((Rect) ref rect).width, (float) UI.screenHeight - (((Rect) ref rect).y + ((Rect) ref rect).height));
    Vector3 mapPosition1 = UI.UIToMapPosition(vector2_1);
    Vector3 mapPosition2 = UI.UIToMapPosition(vector2_2);
    return new CellRect()
    {
      minX = Mathf.FloorToInt(mapPosition1.x),
      minZ = Mathf.FloorToInt(mapPosition2.z),
      maxX = Mathf.FloorToInt(mapPosition2.x),
      maxZ = Mathf.FloorToInt(mapPosition1.z)
    };
  }

  private static Rect GetRectInWorldSpace(Rect rect)
  {
    Vector2 vector2_1;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_1).\u002Ector(((Rect) ref rect).x, (float) UI.screenHeight - ((Rect) ref rect).y);
    Vector2 vector2_2 = new Vector2(((Rect) ref rect).x + ((Rect) ref rect).width, (float) UI.screenHeight - (((Rect) ref rect).y + ((Rect) ref rect).height));
    Vector3 mapPosition1 = UI.UIToMapPosition(vector2_1);
    Vector3 mapPosition2 = UI.UIToMapPosition(vector2_2);
    return new Rect(mapPosition1.x, mapPosition2.z, mapPosition2.x - mapPosition1.x, mapPosition1.z - mapPosition2.z);
  }
}
