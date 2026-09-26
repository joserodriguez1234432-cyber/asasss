// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_MapHandling
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Reflection;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_MapHandling : IPatchCategory
{
  private static readonly FastInvokeHandler IsValidColonyPawn = MethodInvoker.GetHandler(AccessTools.Method(typeof (MapPawns), nameof (IsValidColonyPawn), (System.Type[]) null, (System.Type[]) null), false);

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (TileMutatorWorker_Coast), "CoastOffset"), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "CoastSizeMultiplier", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TileMutatorWorker_River), "GetRiverWidthAt", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "RiverNodeWidth", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TileFinder), "RandomSettlementTileFor", new System.Type[4]
    {
      typeof (PlanetLayer),
      typeof (Faction),
      typeof (bool),
      typeof (Predicate<PlanetTile>)
    }, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "AdjustSettlement", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Property(typeof (MapPawns), "AnyPawnBlockingMapRemoval").GetGetMethod(), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "AnyVehicleBlockingMapRemoval", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GasGrid), "GasCanMoveTo", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "GasCanMoveThroughVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MapInterface), "MapInterfaceUpdate", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "DebugUpdateVehicleRegions", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MapInterface), "MapInterfaceOnGUI_AfterMainTabs", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_MapHandling), "DebugOnGUIVehicleRegions", (System.Type[]) null));
  }

  private static void CoastSizeMultiplier(ref FloatRange __result)
  {
    __result = ModSettingsHelper.BeachMultiplier(__result);
  }

  private static void RiverNodeWidth(ref float __result)
  {
    __result *= ModSettingsHelper.RiverMultiplier;
  }

  public static void AdjustSettlement(ref PlanetTile __result)
  {
    if (TestWatcher.RunningTests || VehicleMod.settings.main.adjustSettlementRadius == 0)
      return;
    __result = WorldHelper.AdjustSettlement(__result);
  }

  public static void AnyVehicleBlockingMapRemoval(ref bool __result, Map ___map)
  {
    if (__result)
      return;
    if (LandingTargeter.Instance.IsTargeting && Current.Game.CurrentMap == ___map || MapHelper.AnyAerialVehiclesInRecon(___map))
    {
      __result = true;
    }
    else
    {
      foreach (VehiclePawn allClaimant in ___map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
      {
        if ((allClaimant.MovementPermissions & VehiclePermissions.Autonomous) != VehiclePermissions.None)
        {
          __result = true;
          break;
        }
        foreach (Pawn pawn in allClaimant.AllPawnsAboard)
        {
          if ((bool) Patch_MapHandling.IsValidColonyPawn.Invoke((object) ___map.mapPawns, new object[1]
          {
            (object) pawn
          }))
          {
            __result = true;
            return;
          }
        }
      }
    }
  }

  private static void GasCanMoveThroughVehicle(IntVec3 cell, ref bool __result, Map ___map)
  {
    if (!__result)
      return;
    VehiclePawn vehiclePawn = ___map.GetDetachedMapComponent<VehiclePositionManager>().ClaimedBy(cell);
    __result = vehiclePawn == null || vehiclePawn.VehicleDef.Fillage != 2;
  }

  public static void DebugUpdateVehicleRegions()
  {
    if (Find.CurrentMap == null || WorldRendererUtility.WorldRendered || !DebugHelper.AnyDebugSettings)
      return;
    DebugHelper.DebugDrawVehicleRegion(Find.CurrentMap);
  }

  public static void DebugOnGUIVehicleRegions()
  {
    if (Find.CurrentMap == null || WorldRendererUtility.WorldRendered || !DebugHelper.AnyDebugSettings)
      return;
    DebugHelper.DebugDrawVehiclePathCostsOverlay(Find.CurrentMap);
  }
}
