// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_BillDoorsFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
[StaticConstructorOnStartup]
public static class Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing
{
  private static IntVec3 locCache;
  private static readonly ConcurrentSet<IntVec3> cellCache;
  private static readonly ConcurrentSet<IntVec3> badCellCache;
  private static readonly Material redMat;
  private static readonly Material greenMat;

  static Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing()
  {
    if (!ModCompat.BillDoorsFramework)
      return;
    Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.redMat = DebugMatsSpectrum.Mat(0, false);
    Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.redMat.color = ColorExtension.ToTransparent(Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.redMat.color, 0.1f);
    Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.greenMat = DebugMatsSpectrum.Mat(50, false);
    Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.greenMat.color = ColorExtension.ToTransparent(Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.greenMat.color, 0.1f);
    Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.cellCache = new ConcurrentSet<IntVec3>();
    Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.badCellCache = new ConcurrentSet<IntVec3>();
  }

  public static bool Prefix(
    BuildableDef checkingDef,
    IntVec3 loc,
    Map map,
    ref AcceptanceReport __result)
  {
    __result = AcceptanceReport.op_Implicit(true);
    if (KeyBindingDefOf.ShowEyedropper.IsDown)
    {
      if (IntVec3.op_Inequality(Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.locCache, loc))
      {
        ((ConcurrentDictionary<IntVec3, byte>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.cellCache).Clear();
        ((ConcurrentDictionary<IntVec3, byte>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.badCellCache).Clear();
        VehiclePawnWithMap vehicle;
        if (map.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned)
        {
          loc = loc.ToBaseMapCoord(vehicle);
          map = ((Thing) vehicle).Map;
        }
        foreach (VerbProperties verb in ((ThingDef) checkingDef).building.turretGunDef.Verbs)
        {
          Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.locCache = loc;
          Parallel.ForEach<IntVec3>(GenRadial.RadialCellsAround(loc, verb.minRange, verb.range), (Action<IntVec3>) (cell =>
          {
            if (GenSightOnVehicle.LineOfSight(loc, cell, map, false))
              Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.cellCache.Add(cell);
            else
              Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.badCellCache.Add(cell);
          }));
        }
      }
      if (((IEnumerable<KeyValuePair<IntVec3, byte>>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.cellCache).Any<KeyValuePair<IntVec3, byte>>())
      {
        GenDraw.DrawFieldEdges(((ConcurrentDictionary<IntVec3, byte>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.cellCache).Keys.ToList<IntVec3>(), 2900);
        foreach (IntVec3 key in (IEnumerable<IntVec3>) ((ConcurrentDictionary<IntVec3, byte>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.cellCache).Keys)
          CellRenderer.RenderCell(key, Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.greenMat);
      }
      if (((IEnumerable<KeyValuePair<IntVec3, byte>>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.badCellCache).Any<KeyValuePair<IntVec3, byte>>())
      {
        foreach (IntVec3 key in (IEnumerable<IntVec3>) ((ConcurrentDictionary<IntVec3, byte>) Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.badCellCache).Keys)
          CellRenderer.RenderCell(key, Patch_PlaceWorker_ShowVerbRadiusBySight_AllowsPlacing.redMat);
      }
    }
    foreach (VerbProperties verb in ((ThingDef) checkingDef).building.turretGunDef.Verbs)
    {
      if ((double) verb.range > 0.0)
        GenDraw.DrawRadiusRing(loc, verb.range);
      if ((double) verb.minRange > 0.0)
        GenDraw.DrawRadiusRing(loc, verb.minRange);
    }
    return false;
  }
}
