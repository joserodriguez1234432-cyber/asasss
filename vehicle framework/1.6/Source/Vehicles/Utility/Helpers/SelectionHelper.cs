// Decompiled with JetBrains decompiler
// Type: Vehicles.SelectionHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public static class SelectionHelper
{
  public static bool MultiSelectClicker(List<object> selectedObjects)
  {
    if (!selectedObjects.All<object>((Func<object, bool>) (x => x is Pawn)))
      return false;
    List<Pawn> pawnList = new List<Pawn>();
    foreach (object selectedObject in selectedObjects)
    {
      if (selectedObject is Pawn)
        pawnList.Add(selectedObject as Pawn);
    }
    if (pawnList.NotNullAndAny<Pawn>((Predicate<Pawn>) (x => ((Thing) x).Faction != Faction.OfPlayer || x is VehiclePawn)))
      return false;
    IntVec3 intVec3 = IntVec3Utility.ToIntVec3(UI.MouseMapPosition());
    if (selectedObjects.Count > 1 && selectedObjects.All<object>((Func<object, bool>) (x => x is Pawn)))
    {
      foreach (Thing thing in ((Thing) pawnList[0]).Map.thingGrid.ThingsAt(intVec3))
      {
        if (thing is VehiclePawn)
        {
          (thing as VehiclePawn).MultiplePawnFloatMenuOptions(pawnList);
          return true;
        }
      }
    }
    return false;
  }
}
