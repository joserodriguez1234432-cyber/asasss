// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenDraw_DrawRadiusRing
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenDraw), "DrawRadiusRing", new Type[] {typeof (IntVec3), typeof (float), typeof (Color), typeof (Func<IntVec3, bool>)})]
[PatchLevel(Level.Safe)]
public static class Patch_GenDraw_DrawRadiusRing
{
  private static readonly List<IntVec3> ringDrawCells = new List<IntVec3>();

  public static bool Prefix(
    ref IntVec3 center,
    float radius,
    Color color,
    Func<IntVec3, bool> predicate)
  {
    Thing thing1 = (Thing) null;
    bool flag = false;
    foreach (object selectedObject in Find.Selector.SelectedObjects)
    {
      if (selectedObject is Thing thing2 && IntVec3.op_Equality(thing2.Position, center))
      {
        flag = true;
        thing1 = thing2;
        break;
      }
    }
    if (flag)
    {
      VehiclePawnWithMap vehicle;
      if (thing1.IsOnNonFocusedVehicleMapOf(out vehicle))
      {
        if (VehicleMapUtility.get_IsNonFocusedVehicleMap(Find.CurrentMap) && VehicleMapUtility.get_BaseMapOrCaravan(Find.CurrentMap) == VehicleMapUtility.get_BaseMapOrCaravan(vehicle.VehicleMap))
        {
          Patch_GenDraw_DrawRadiusRing.DrawRadiusRing(vehicle.VehicleMap, center, radius, color, predicate);
          return false;
        }
        center = center.ToBaseMapCoord(vehicle);
      }
    }
    else if (Command_FocusVehicleMap.FocusedVehicle != null)
      center = center.ToBaseMapCoord(Command_FocusVehicleMap.FocusedVehicle);
    return true;
  }

  private static void DrawRadiusRing(
    Map map,
    IntVec3 center,
    float radius,
    Color color,
    Func<IntVec3, bool> predicate = null)
  {
    if ((double) radius > (double) GenRadial.MaxRadialPatternRadius)
    {
      Log.ErrorOnce($"Cannot draw radius ring of radius {radius}: not enough squares in the precalculated list.", 71496514);
    }
    else
    {
      Patch_GenDraw_DrawRadiusRing.ringDrawCells.Clear();
      int num = GenRadial.NumCellsInRadius(radius);
      for (int index = 0; index < num; ++index)
      {
        IntVec3 intVec3 = IntVec3.op_Addition(center, GenRadial.RadialPattern[index]);
        if (predicate == null || predicate(intVec3))
          Patch_GenDraw_DrawRadiusRing.ringDrawCells.Add(intVec3);
      }
      List<IntVec3> ringDrawCells = Patch_GenDraw_DrawRadiusRing.ringDrawCells;
      Color color1 = color;
      Map map1 = map;
      float? altOffset = new float?();
      Map map2 = map1;
      GenDrawOnVehicle.DrawFieldEdges(ringDrawCells, color1, altOffset, map: map2);
    }
  }
}
