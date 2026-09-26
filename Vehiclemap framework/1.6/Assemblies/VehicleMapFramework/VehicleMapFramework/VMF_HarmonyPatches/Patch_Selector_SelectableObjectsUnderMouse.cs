// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Selector_SelectableObjectsUnderMouse
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Selector), "SelectableObjectsUnderMouse")]
[PatchLevel(Level.Safe)]
public static class Patch_Selector_SelectableObjectsUnderMouse
{
  public static bool Prefix(ref IEnumerable<object> __result)
  {
    Vector3 vector3 = UI.MouseMapPosition();
    VehiclePawnWithMap vehicle;
    if (!vector3.TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.All))
      return true;
    ref IEnumerable<object> local = ref __result;
    List<object> items = new List<object>();
    items.AddRange(Patch_Selector_SelectableObjectsUnderMouse.SelectableObjects(vehicle, vector3));
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    \u003C\u003Ez__ReadOnlyList<object> zReadOnlyList = new \u003C\u003Ez__ReadOnlyList<object>(items);
    local = (IEnumerable<object>) zReadOnlyList;
    return !__result.Any<object>();
  }

  private static IEnumerable<object> SelectableObjects(
    VehiclePawnWithMap vehicle,
    Vector3 mouseMapPosition)
  {
    TargetingParameters clickParams = new TargetingParameters()
    {
      mustBeSelectable = true,
      canTargetPawns = true,
      canTargetBuildings = true,
      canTargetItems = true,
      mapObjectTargetsMustBeAutoAttackable = false
    };
    Vector3 mouseVehicleMapPosition = mouseMapPosition.ToVehicleMapCoord(vehicle);
    if (GenGrid.InBounds(mouseVehicleMapPosition, vehicle.VehicleMap))
    {
      List<Thing> thingList = GenUIOnVehicle.ThingsUnderMouse(mouseVehicleMapPosition, 1f, clickParams, (ITargetingSource) null, vehicle);
      if (thingList.Count > 0 && thingList[0] is Pawn && (double) GenGeo.MagnitudeHorizontal(Vector3.op_Subtraction(thingList[0].DrawPos, mouseMapPosition)) < 0.40000000596046448)
      {
        for (int index = thingList.Count - 1; index >= 0; --index)
        {
          Thing thing = thingList[index];
          if (thing.def.category == 1 && (double) GenGeo.MagnitudeHorizontal(Vector3.op_Subtraction(thing.DrawPosHeld.Value, mouseMapPosition)) > 0.40000000596046448)
            thingList.Remove(thing);
        }
      }
      foreach (object obj in thingList)
        yield return obj;
      Zone zone = vehicle.CurrentLevel.zoneManager.ZoneAt(IntVec3Utility.ToIntVec3(mouseVehicleMapPosition));
      if (zone != null)
        yield return (object) zone;
      if (Find.CurrentMap == vehicle.VehicleMap && ((Thing) vehicle).Spawned)
        yield return (object) vehicle;
    }
  }
}
