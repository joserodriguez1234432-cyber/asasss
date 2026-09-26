// Decompiled with JetBrains decompiler
// Type: Vehicles.GridOwners
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public static class GridOwners
{
  public static WorldGridOwners World { get; } = new WorldGridOwners();

  internal static void RecacheMoveableVehicleDefs()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    VehicleHarmony.AllMoveableVehicleDefs = DefDatabase<VehicleDef>.AllDefsListForReading.Where<VehicleDef>(GridOwners.\u003C\u003EO.\u003C0\u003E__ShouldCreateRegions ?? (GridOwners.\u003C\u003EO.\u003C0\u003E__ShouldCreateRegions = new Func<VehicleDef, bool>(PathingHelper.ShouldCreateRegions))).ToList<VehicleDef>();
    GridOwners.World.Init();
    if (GenList.NullOrEmpty<Map>((IList<Map>) Find.Maps))
      return;
    foreach (Map map in Find.Maps)
    {
      VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
      cachedMapComponent.GridOwners.Init();
      cachedMapComponent.ConstructComponents();
    }
  }

  [DebugOutput("Vehicle Framework", false, name = "Output GridOwners")]
  private static void OutputMapOwners()
  {
    Log.Message("------- GridOwners -------");
    Log.Message($"Vehicles = {DefDatabase<VehicleDef>.AllDefsListForReading.Count}");
    StringBuilder stringBuilder = new StringBuilder();
    OutputForGrid<WorldGridOwners.PathConfig>((GridOwnerList<WorldGridOwners.PathConfig>) GridOwners.World, stringBuilder);
    Log.Message($"World:\n{stringBuilder}");
    stringBuilder.Clear();
    foreach (Map map in Find.Maps)
    {
      VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
      stringBuilder.AppendLine($"  Id: {map.uniqueID}");
      OutputForGrid<MapGridOwners.PathConfig>((GridOwnerList<MapGridOwners.PathConfig>) cachedMapComponent.GridOwners, stringBuilder);
    }
    Log.Message($"Map:\n{stringBuilder}");
    Log.Message("-------");

    static void OutputForGrid<T>(GridOwnerList<T> gridOwnerList, StringBuilder stringBuilder) where T : IPathConfig
    {
      stringBuilder.AppendLine($"  Total Owners = {gridOwnerList.AllOwners.Length}");
      stringBuilder.AppendLine($"  Total Piggies = {gridOwnerList.AllPiggies.Count<VehicleDef>()}");
      stringBuilder.AppendLine("  List:");
      foreach (VehicleDef allOwner in gridOwnerList.AllOwners)
      {
        stringBuilder.AppendLine($"  Owner: {allOwner}");
        stringBuilder.AppendLine("  Piggies=(" + string.Join(",", gridOwnerList.GetPiggies(allOwner).Select<VehicleDef, string>((Func<VehicleDef, string>) (def => ((Def) def).defName))));
      }
    }
  }
}
