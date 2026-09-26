// Decompiled with JetBrains decompiler
// Type: Vehicles.DebugActions
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using LudeonTK;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal static class DebugActions
{
  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static void ClearRegionCache()
  {
    LongEventHandler.QueueLongEvent((Action) (() =>
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      foreach (Map map in Find.Maps)
      {
        VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
        foreach (VehicleDef moveableVehicleDef in VehicleHarmony.AllMoveableVehicleDefs)
          cachedMapComponent[moveableVehicleDef].VehicleReachability.ClearCache();
      }
    }), "Clearing Region Cache", false, (Action<Exception>) null, true, false, (Action) null);
  }

  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static void FlashPathCosts()
  {
    List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
    debugMenuOptionList.Add(new DebugMenuOption("Vanilla", (DebugMenuOptionMode) 0, (Action) (() =>
    {
      FlashPathCostsFor((VehicleDef) null);
      Find.WindowStack.WindowOfType<Dialog_RadioButtonMenu>()?.Close(true);
    })));
    foreach (VehicleDef vehicleDef1 in (IEnumerable<VehicleDef>) DefDatabase<VehicleDef>.AllDefsListForReading.OrderBy<VehicleDef, bool>((Func<VehicleDef, bool>) (def => ((Def) def).modContentPack.ModMetaData.SamePackageId("SmashPhil.VehicleFramework", true))).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (def => ((Def) def).modContentPack.Name)).ThenBy<VehicleDef, string>((Func<VehicleDef, string>) (d => ((Def) d).defName)))
    {
      VehicleDef vehicleDef = vehicleDef1;
      debugMenuOptionList.Add(new DebugMenuOption(((Def) vehicleDef).defName, (DebugMenuOptionMode) 0, (Action) (() => FlashPathCostsFor(vehicleDef))));
    }
    Find.WindowStack.Add((Window) new Dialog_DebugOptionListLister((IEnumerable<DebugMenuOption>) debugMenuOptionList, "Vehicle Defs"));

    static void FlashPathCostsFor(VehicleDef vehicleDef)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      Map currentMap = Find.CurrentMap;
      if (currentMap == null)
        return;
      if (vehicleDef == null)
      {
        foreach (IntVec3 allCell in currentMap.AllCells)
        {
          int num = currentMap.pathing.Normal.pathGrid.Cost(allCell);
          currentMap.debugDrawer.FlashCell(allCell, (float) num / 500f, num.ToString(), 50);
        }
      }
      else
      {
        VehiclePathingSystem cachedMapComponent = currentMap.GetCachedMapComponent<VehiclePathingSystem>();
        foreach (IntVec3 allCell in currentMap.AllCells)
        {
          int num = cachedMapComponent[vehicleDef].VehiclePathGrid.PerceivedPathCostAt(allCell);
          currentMap.debugDrawer.FlashCell(allCell, (float) num / 500f, num.ToString(), 50);
        }
      }
    }
  }

  [DebugAction("Vehicle Framework", null, false, false, false, false, false, 0, false)]
  private static void RegenerateAllGrids()
  {
    LongEventHandler.QueueLongEvent((Action) (() =>
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      foreach (Map map in Find.Maps)
        MapComponentCache<VehiclePathingSystem>.GetComponent(map).RegenerateGrids(deferment: VehiclePathingSystem.GridDeferment.Forced);
    }), "Regenerating Regions", true, (Action<Exception>) null, true, false, (Action) null);
  }
}
