// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_VehiclePathing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_VehiclePathing : IPatchCategory
{
  private static readonly List<VehiclePawn> MultiSelectGotoList = new List<VehiclePawn>();

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (FloatMenuOptionProvider_DraftedMove), "PawnCanGoto", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "MultiselectVehicleGotoBlocked", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (FloatMenuOptionProvider), "Applies", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "DontVanillaDraftMoveVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Selector), "HandleMultiselectGoto", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "MultiselectGotoDraggingBlocked", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_JobTracker), "IsCurrentJobPlayerInterruptible", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "JobInterruptibleForVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_PathFollower), "StartPath", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "StartVehiclePath", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GenAdj), "AdjacentTo8WayOrInside", new System.Type[2]
    {
      typeof (IntVec3),
      typeof (Thing)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "AdjacentTo8WayOrInsideVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GenAdj), "OccupiedRect", new System.Type[1]
    {
      typeof (Thing)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "OccupiedRectVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pathing), "RecalculatePerceivedPathCostAt", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_VehiclePathing), "RecalculatePerceivedPathCostForVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TerrainGrid), "DoTerrainChangedEffects", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "SetTerrainAndUpdateVehiclePathCosts", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Thing), "DeSpawn", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_VehiclePathing), "DeSpawnAndUpdateVehicleRegionsTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Thing), "SpawnSetup", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_VehiclePathing), "SpawnAndUpdateVehicleRegionsTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertySetter(typeof (Thing), "Position"), postfix: new HarmonyMethod(typeof (Patch_VehiclePathing), "SetPositionAndUpdateVehicleRegions", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertySetter(typeof (Thing), "Rotation"), new HarmonyMethod(typeof (Patch_VehiclePathing), "SetRotationAndUpdateVehicleRegionsClipping", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ThingGrid), "Register", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "MonitorThingGridRegisterStart", (System.Type[]) null), finalizer: new HarmonyMethod(typeof (Patch_VehiclePathing), "MonitorThingGridRegisterEnd", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ThingGrid), "Deregister", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "MonitorThingGridDeregisterStart", (System.Type[]) null), finalizer: new HarmonyMethod(typeof (Patch_VehiclePathing), "MonitorThingGridDeregisterEnd", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GenStep_RocksFromGrid), "Generate", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_VehiclePathing), "DisableRegionUpdatingRockGen", (System.Type[]) null));
  }

  private static bool MultiselectVehicleGotoBlocked(Pawn pawn, ref AcceptanceReport __result)
  {
    if (!(pawn is VehiclePawn))
      return true;
    __result = AcceptanceReport.op_Implicit(false);
    return false;
  }

  private static bool DontVanillaDraftMoveVehicles(
    ref bool __result,
    FloatMenuOptionProvider __instance,
    FloatMenuContext context)
  {
    if (__instance is FloatMenuOptionProvider_Vehicle || context.IsMultiselect || !(context.FirstSelectedPawn is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  private static bool MultiselectGotoDraggingBlocked(FloatMenuContext context)
  {
    if (context.IsMultiselect)
    {
      if (context.allSelectedPawns.All<Pawn>((Func<Pawn, bool>) (pawn => pawn is VehiclePawn)))
      {
        Patch_VehiclePathing.MultiSelectGotoList.AddRange(context.allSelectedPawns.Cast<VehiclePawn>());
        IntVec3 result;
        if (!PathingHelper.TryFindNearestStandableCell(Patch_VehiclePathing.MultiSelectGotoList.FirstOrDefault<VehiclePawn>(), context.ClickedCell, out result))
          return false;
        VehicleOrientationController.StartOrienting(Patch_VehiclePathing.MultiSelectGotoList, result, context.ClickedCell);
        Patch_VehiclePathing.MultiSelectGotoList.Clear();
        return false;
      }
      for (int index = context.allSelectedPawns.Count - 1; index >= 0; --index)
      {
        if (context.allSelectedPawns[index] is VehiclePawn)
          context.allSelectedPawns.RemoveAt(index);
      }
    }
    return true;
  }

  private static bool JobInterruptibleForVehicle(
    Pawn_JobTracker __instance,
    Pawn ___pawn,
    ref bool __result)
  {
    if (!(___pawn is VehiclePawn))
      return true;
    if (__instance.curJob == null || __instance.curDriver == null)
    {
      __result = true;
      return false;
    }
    bool flag;
    if (__instance != null)
    {
      Job curJob = __instance.curJob;
      if (curJob != null)
      {
        JobDef def = curJob.def;
        if (def != null && def.playerInterruptible)
          goto label_7;
      }
      JobDriver curDriver = __instance.curDriver;
      if (curDriver == null || !curDriver.PlayerInterruptable)
        goto label_8;
label_7:
      flag = true;
      goto label_9;
    }
label_8:
    flag = false;
label_9:
    __result = flag;
    return false;
  }

  private static bool StartVehiclePath(LocalTargetInfo dest, PathEndMode peMode, Pawn ___pawn)
  {
    if (!(___pawn is VehiclePawn vehiclePawn))
      return true;
    vehiclePawn.vehiclePather.StartPath(dest, peMode);
    return false;
  }

  private static bool AdjacentTo8WayOrInsideVehicle(IntVec3 root, Thing t, ref bool __result)
  {
    if (!(t is VehiclePawn vehiclePawn))
      return true;
    IntVec2 size = ((Thing) vehiclePawn).def.size;
    Rot4 rotation = ((Thing) vehiclePawn).Rotation;
    Ext_Vehicles.AdjustForVehicleOccupiedRect(ref size, ref rotation);
    __result = GenAdj.AdjacentTo8WayOrInside(root, ((Thing) vehiclePawn).Position, rotation, size);
    return false;
  }

  private static bool OccupiedRectVehicles(Thing t, ref CellRect __result)
  {
    if (!(t is VehiclePawn vehicle))
      return true;
    __result = vehicle.VehicleRect();
    return false;
  }

  private static void RecalculatePerceivedPathCostForVehicle(IntVec3 c, PathingContext ___normal)
  {
    PathingHelper.RecalculatePerceivedPathCostAt(c, ___normal.map);
  }

  private static void SetTerrainAndUpdateVehiclePathCosts(ref IntVec3 c, Map ___map)
  {
    if (Current.ProgramState != 2)
      return;
    PathingHelper.RecalculatePerceivedPathCostAt(c, ___map);
  }

  private static IEnumerable<CodeInstruction> DeSpawnAndUpdateVehicleRegionsTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo coverGridDeregisterMethod = AccessTools.Method(typeof (TickManager), "DeRegisterAllTickabilityFor", (System.Type[]) null, (System.Type[]) null);
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, coverGridDeregisterMethod))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldloc_0, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_VehiclePathing), "DeSpawnAndNotifyVehicleRegions", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static IEnumerable<CodeInstruction> SpawnAndUpdateVehicleRegionsTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo coverGridDeregisterMethod = AccessTools.Method(typeof (CoverGrid), "Register", (System.Type[]) null, (System.Type[]) null);
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, coverGridDeregisterMethod))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldarg_1, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_VehiclePathing), "SpawnAndNotifyVehicleRegions", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static void SetPositionAndUpdateVehicleRegions(Thing __instance)
  {
    if (!__instance.Spawned)
      return;
    if (__instance is VehiclePawn vehiclePawn)
      vehiclePawn.ReclaimPosition();
    PathingHelper.ThingAffectingRegionsOrientationChanged(__instance, __instance.Map);
  }

  private static bool SetRotationAndUpdateVehicleRegionsClipping(
    Thing __instance,
    Rot4 value,
    ref Rot4 ___rotationInt)
  {
    if (!(__instance is VehiclePawn vehiclePawn))
      return true;
    vehiclePawn.SetRotationInt(value, ref ___rotationInt);
    return false;
  }

  private static void SetRotationAndUpdateVehicleRegions(Thing __instance)
  {
    if (!__instance.Spawned || __instance.def.size.x == 1 && __instance.def.size.z == 1)
      return;
    PathingHelper.ThingAffectingRegionsOrientationChanged(__instance, __instance.Map);
  }

  private static void MonitorThingGridRegisterStart(ThingGrid __instance)
  {
    Monitor.Enter((object) __instance);
  }

  private static void MonitorThingGridRegisterEnd(ThingGrid __instance)
  {
    Monitor.Exit((object) __instance);
  }

  private static void MonitorThingGridDeregisterStart(ThingGrid __instance)
  {
    Monitor.Enter((object) __instance);
  }

  private static void MonitorThingGridDeregisterEnd(ThingGrid __instance)
  {
    Monitor.Exit((object) __instance);
  }

  private static void DisableRegionUpdatingRockGen(Map map)
  {
    if (map.TileInfo.WaterCovered)
      return;
    map.GetCachedMapComponent<VehiclePathingSystem>().DisableAllRegionUpdaters();
  }

  private static void SpawnAndNotifyVehicleRegions(Thing thing, Map map)
  {
    PathingHelper.ThingAffectingRegionsStateChange(thing, map, true);
  }

  private static void DeSpawnAndNotifyVehicleRegions(Thing thing, Map map)
  {
    PathingHelper.ThingAffectingRegionsStateChange(thing, map, false);
  }
}
