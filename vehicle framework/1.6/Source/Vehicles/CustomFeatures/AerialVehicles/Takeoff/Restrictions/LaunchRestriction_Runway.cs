// Decompiled with JetBrains decompiler
// Type: Vehicles.LaunchRestriction_Runway
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class LaunchRestriction_Runway : LaunchRestriction
{
  private static List<IntVec3> invalidCells = new List<IntVec3>();
  public Color colorValid = Color.white;
  public Color colorInvalid = Color.red;
  public IntVec2 width = IntVec2.Zero;
  public IntVec2 height = IntVec2.Zero;
  public SimpleDictionary<ThingCategory, float> thingCategories;
  public SimpleDictionary<ThingCategoryDef, float> thingCategoryDefs;
  public float fillPercent = 0.2f;

  private bool ShouldCheckValidators
  {
    get
    {
      return !GenDictionary.NullOrEmpty<ThingCategory, float>((Dictionary<ThingCategory, float>) this.thingCategories) || !GenDictionary.NullOrEmpty<ThingCategoryDef, float>((Dictionary<ThingCategoryDef, float>) this.thingCategoryDefs) || (double) this.fillPercent > 0.0;
    }
  }

  private CellRect RunwayRect(IntVec3 position, Rot4 rot)
  {
    IntVec2 width = this.width;
    IntVec2 height = this.height;
    if (Rot4.op_Equality(rot, Rot4.West))
    {
      width.x = -this.width.x;
      width.z = -this.width.z;
    }
    else if (Rot4.op_Equality(rot, Rot4.South))
    {
      height.x = -this.height.x;
      height.z = -this.height.z;
    }
    return CellRect.FromLimits(position.x + width.x, position.z + height.x, position.x + width.z, position.z + height.z);
  }

  public override bool CanStartProtocol(VehiclePawn vehicle, Map map, IntVec3 position, Rot4 rot)
  {
    if (map == null || !this.ShouldCheckValidators)
      return true;
    CellRect cellRect = this.RunwayRect(position, rot);
    foreach (IntVec3 cell in cellRect)
    {
      if (!GenGrid.InBounds(cell, map) || GenCollection.Any<Thing>(map.thingGrid.ThingsListAtFast(cell), (Predicate<Thing>) (thing => thing != vehicle && this.InvalidFor(thing))) || !cell.Walkable(vehicle.VehicleDef, map))
        return false;
    }
    return true;
  }

  public override void DrawRestrictionsTargeter(
    VehiclePawn vehicle,
    Map map,
    IntVec3 position,
    Rot4 rot)
  {
    if (map == null)
      return;
    CellRect source = this.RunwayRect(position, rot);
    if (this.ShouldCheckValidators)
    {
      LaunchRestriction_Runway.invalidCells.Clear();
      foreach (IntVec3 cell in source)
      {
        if (GenGrid.InBounds(cell, map))
        {
          if (GenCollection.Any<Thing>(map.thingGrid.ThingsListAtFast(cell), (Predicate<Thing>) (thing => thing != vehicle && this.InvalidFor(thing))))
            LaunchRestriction_Runway.invalidCells.Add(cell);
          else if (!cell.Walkable(vehicle.VehicleDef, map))
            LaunchRestriction_Runway.invalidCells.Add(cell);
        }
      }
    }
    GenDraw.DrawFieldEdges(((IEnumerable<IntVec3>) (object) source).ToList<IntVec3>(), this.colorValid, new float?(), (HashSet<IntVec3>) null, 2900);
    if (GenList.NullOrEmpty<IntVec3>((IList<IntVec3>) LaunchRestriction_Runway.invalidCells))
      return;
    GenDraw.DrawFieldEdges(LaunchRestriction_Runway.invalidCells, this.colorInvalid, new float?(), (HashSet<IntVec3>) null, 2900);
  }

  private bool InvalidFor(Thing thing)
  {
    bool flag = false;
    if ((double) this.fillPercent >= 0.0)
      flag |= (double) thing.def.fillPercent >= (double) this.fillPercent;
    if (!GenDictionary.NullOrEmpty<ThingCategory, float>((Dictionary<ThingCategory, float>) this.thingCategories) && this.thingCategories.ContainsKey(thing.def.category))
      flag |= (double) thing.def.fillPercent >= (double) this.thingCategories[thing.def.category];
    if (!GenDictionary.NullOrEmpty<ThingCategoryDef, float>((Dictionary<ThingCategoryDef, float>) this.thingCategoryDefs) && !GenList.NullOrEmpty<ThingCategoryDef>((IList<ThingCategoryDef>) thing.def.thingCategories))
    {
      foreach (ThingCategoryDef thingCategory in thing.def.thingCategories)
      {
        if (this.thingCategoryDefs.ContainsKey(thingCategory))
          flag |= (double) thing.def.fillPercent >= (double) this.thingCategoryDefs[thingCategory];
      }
    }
    return flag;
  }
}
