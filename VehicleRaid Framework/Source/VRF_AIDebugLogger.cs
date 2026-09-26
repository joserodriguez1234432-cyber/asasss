using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework
{

    internal static class VRF_Log
    {
        internal static bool Enabled
        {
            get
            {
                return VRF_Mod.Settings?.VerboseAILogging ?? false;
            }
        }

        internal static void Msg(string message)
        {
            if (Enabled) Log.Message($"[VRF_AI] {message}");
        }

        internal static void Warn(string message)
        {
            Log.Warning($"[VRF_AI][WARN] {message}");
        }
    }

    [HarmonyPatch(typeof(VRF_JobGiver_DynamicAssault), "TryGiveJob")]
    internal static class Patch_LogJobGiven_DynamicAssault
    {
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, Job __result)
        {
            if (!VRF_Log.Enabled) return;
            if (!(pawn is VehiclePawn v)) return;

            if (__result == null)
            {
                VRF_Log.Msg($"DynamicAssault.TryGiveJob [{v.LabelShort}] → NULL " +
                             $"canMove={CrewManager.CanMove(v)} " +
                             $"boarding={CrewManager.IsAnyPawnBoarding(v)} " +
                             $"outOfAmmo={CrewManager.IsOutOfAmmo(v)} " +
                             $"alreadyMoving={v.vehiclePather?.Moving}");
            }
            else
            {
                Thing target = __result.targetA.Thing;
                VRF_Log.Msg($"DynamicAssault.TryGiveJob [{v.LabelShort}] → Job='{__result.def.defName}' " +
                             $"target={(target != null ? $"{target.LabelShort}@{target.Position}" : __result.targetA.Cell.ToString())} " +
                             $"expiry={__result.expiryInterval} " +
                             $"canMove={CrewManager.CanMove(v)} " +
                             $"enemy={(v.mindState?.enemyTarget != null ? $"{v.mindState.enemyTarget.LabelShort}@{v.mindState.enemyTarget.Position}" : "none")}");
            }
        }
    }

    [HarmonyPatch(typeof(VRF_JobGiver_VehicleExitMap), "TryGiveJob")]
    internal static class Patch_LogJobGiven_ExitMap
    {
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, Job __result)
        {
            if (!VRF_Log.Enabled) return;
            if (!(pawn is VehiclePawn v)) return;

            VRF_Log.Msg($"ExitMap.TryGiveJob [{v.LabelShort}] → " +
                         (__result == null
                             ? $"NULL canMove={CrewManager.CanMove(v)}"
                             : $"Job='{__result.def.defName}' dest={__result.targetA.Cell}"));
        }
    }

    [HarmonyPatch(typeof(LordToil_VehicleSearchAndDestroy), "UpdateAllDuties")]
    internal static class Patch_LogDutyUpdate_SearchAndDestroy
    {
        [HarmonyPrefix]
        static void Prefix(LordToil_VehicleSearchAndDestroy __instance)
        {
            if (!VRF_Log.Enabled) return;

            Lord lord = __instance.lord;
            if (lord == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"[SearchAndDestroy.UpdateAllDuties] lord={lord.faction?.def?.defName} " +
                          $"toil={lord.CurLordToil?.GetType().Name} " +
                          $"pawnCount={lord.ownedPawns.Count}");

            foreach (Pawn p in lord.ownedPawns)
            {
                if (p is VehiclePawn v)
                {
                    sb.AppendLine($"  VEH [{v.LabelShort}] " +
                                  $"pos={v.Position} " +
                                  $"duty={v.mindState?.duty?.def?.defName ?? "null"} " +
                                  $"dutyFocus={v.mindState?.duty?.focus.Cell} " +
                                  $"job={v.CurJobDef?.defName ?? "null"} " +
                                  $"moving={v.vehiclePather?.Moving} " +
                                  $"drafed={v.ignition?.Drafted} " +
                                  $"hasDriver={CrewManager.HasOperationalDriver(v)} " +
                                  $"hasCrew={v.AllPawnsAboard.Count} " +
                                  $"fuel={v.GetComp<CompFueledTravel>()?.Fuel:F0}/{v.GetComp<CompFueledTravel>()?.FuelCapacity:F0} " +
                                  $"hp={v.HitPoints}/{v.MaxHitPoints}");
                }
                else
                {
                    sb.AppendLine($"  INF [{p.LabelShort}] " +
                                  $"pos={p.Position} " +
                                  $"duty={p.mindState?.duty?.def?.defName ?? "null"} " +
                                  $"job={p.CurJobDef?.defName ?? "null"}");
                }
            }
            VRF_Log.Msg(sb.ToString());
        }
    }

    [HarmonyPatch(typeof(LordToil_VehicleExitMap), "UpdateAllDuties")]
    internal static class Patch_LogDutyUpdate_ExitMap
    {
        [HarmonyPrefix]
        static void Prefix(LordToil_VehicleExitMap __instance)
        {
            if (!VRF_Log.Enabled) return;

            Lord lord = __instance.lord;
            if (lord == null) return;

            VRF_Log.Msg($"[ExitMap.UpdateAllDuties] lord={lord.faction?.def?.defName} " +
                         $"pawnCount={lord.ownedPawns.Count} " +
                         $"exitToilStartTick={__instance.ExitToilStartTick} " +
                         $"nowTick={Find.TickManager.TicksGame}");

            foreach (Pawn p in lord.ownedPawns)
            {
                if (!(p is VehiclePawn v)) continue;
                VRF_Log.Msg($"  VEH [{v.LabelShort}] " +
                             $"duty={v.mindState?.duty?.def?.defName ?? "null"} " +
                             $"job={v.CurJobDef?.defName ?? "null"} " +
                             $"hasDriver={CrewManager.HasOperationalDriver(v)} " +
                             $"boarding={CrewManager.IsAnyPawnBoarding(v)} " +
                             $"siegeDrop={VRF_TransportUtil.IsSiegeDropVehicle(v)}");
            }
        }
    }

    [HarmonyPatch(typeof(Lord), "GotoToil")]
    internal static class Patch_LogLordTransition
    {
        [HarmonyPrefix]
        static void Prefix(Lord __instance, LordToil newLordToil)
        {
            if (!VRF_Log.Enabled) return;
            if (!(__instance.LordJob is LordJob_VehicleRaid)) return;

            VRF_Log.Msg($"[LordTransition] faction={__instance.faction?.def?.defName} " +
                         $"FROM={__instance.CurLordToil?.GetType().Name ?? "null"} " +
                         $"TO={newLordToil?.GetType().Name ?? "null"} " +
                         $"pawnCount={__instance.ownedPawns.Count} " +
                         $"tick={Find.TickManager.TicksGame}");
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    internal static class Patch_LogJobStart
    {
        [HarmonyPrefix]
        static void Prefix(Pawn_JobTracker __instance, Job newJob, JobCondition lastJobEndCondition)
        {
            if (!VRF_Log.Enabled) return;

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!(pawn is VehiclePawn v)) return;
            if (v.GetLord()?.LordJob is not LordJob_VehicleRaid) return;

            string prevJobName = __instance.curJob?.def?.defName ?? "null";
            string newJobName  = newJob?.def?.defName ?? "null";
            Thing  tgt         = newJob?.targetA.Thing;

            VRF_Log.Msg($"[JobStart] [{v.LabelShort}] {prevJobName}→{newJobName} " +
                         $"prevEndCondition={lastJobEndCondition} " +
                         $"target={(tgt != null ? $"{tgt.LabelShort}@{tgt.Position}" : newJob?.targetA.Cell.ToString())} " +
                         $"duty={v.mindState?.duty?.def?.defName ?? "null"} " +
                         $"pos={v.Position} " +
                         $"moving={v.vehiclePather?.Moving}");
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
    internal static class Patch_LogJobEnd
    {
        [HarmonyPrefix]
        static void Prefix(Pawn_JobTracker __instance, JobCondition condition)
        {
            if (!VRF_Log.Enabled) return;

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!(pawn is VehiclePawn v)) return;
            if (v.GetLord()?.LordJob is not LordJob_VehicleRaid) return;

            VRF_Log.Msg($"[JobEnd] [{v.LabelShort}] job='{v.CurJobDef?.defName ?? "null"}' " +
                         $"condition={condition} " +
                         $"pos={v.Position} " +
                         $"duty={v.mindState?.duty?.def?.defName ?? "null"}");
        }
    }

    [HarmonyPatch(typeof(Patch_VehicleNPCOnOff), "UpdateVehiclePower")]
    internal static class Patch_LogPowerChange
    {
        [HarmonyPrefix]
        static void Prefix(VehiclePawn vehicle)
        {
            if (!VRF_Log.Enabled || vehicle == null) return;
            if (vehicle.GetLord()?.LordJob is not LordJob_VehicleRaid) return;

            bool wasDrafted  = vehicle.ignition?.Drafted ?? false;
            bool shouldBeOn  = Patch_VehicleNPCOnOff.ShouldVehicleBeOn(vehicle);

            if (wasDrafted != shouldBeOn)
            {
                VRF_Log.Msg($"[PowerChange] [{vehicle.LabelShort}] " +
                             $"drafted={wasDrafted}→{shouldBeOn} " +
                             $"hasDriver={CrewManager.HasOperationalDriver(vehicle)} " +
                             $"hasFuel={vehicle.GetComp<CompFueledTravel>()?.Fuel > 0} " +
                             $"hasEngine={CrewManager.HasFunctionalEngine(vehicle)} " +
                             $"pos={vehicle.Position} " +
                             $"duty={vehicle.mindState?.duty?.def?.defName ?? "null"} " +
                             $"job={vehicle.CurJobDef?.defName ?? "null"}");
            }
        }
    }

    [HarmonyPatch(typeof(Patch_CustomSettlementInjector), "Postfix")]
    internal static class Patch_LogSettlementSpawnEntry
    {
    }

    [HarmonyPatch(typeof(VRF_SettlementSpawnHelper), "SpawnVehiclesDeferred")]
    internal static class Patch_LogSpawnDeferred
    {
        [HarmonyPrefix]
        static void Prefix(Map map, CellRect settlementRect, Faction faction,
            System.Collections.Generic.List<(PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry, VehicleDef vdef)> toSpawn)
        {
            if (!VRF_Log.Enabled) return;
            if (map == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"[SpawnDeferred] map={map.info?.parent?.def?.defName ?? "?"} " +
                          $"faction={faction?.def?.defName} " +
                          $"rect={settlementRect} " +
                          $"spawnCount={toSpawn?.Count ?? 0}");

            if (toSpawn != null)
            {
                foreach (var (kind, entry, vdef) in toSpawn)
                {
                    float budget = (entry?.combatPowerOverride > 0) ? entry.combatPowerOverride : (kind?.combatPower ?? 0f);
                    sb.AppendLine($"  → '{vdef?.defName}' " +
                                  $"kind='{kind?.defName}' " +
                                  $"isSiegeDrop={entry?.isSiegeDrop} " +
                                  $"helicopterMode={entry?.helicopterMode} " +
                                  $"budget={budget:F0}");
                }
            }
            VRF_Log.Msg(sb.ToString());
        }
    }

    [HarmonyPatch(typeof(Lord), "AddPawn")]
    internal static class Patch_LogLordAddPawn
    {
        [HarmonyPostfix]
        static void Postfix(Lord __instance, Pawn p)
        {
            if (!VRF_Log.Enabled) return;
            if (!(__instance.LordJob is LordJob_VehicleRaid)) return;
            if (!(p is VehiclePawn v)) return;

            VRF_Log.Msg($"[LordAddPawn] lord={__instance.faction?.def?.defName} " +
                         $"toil={__instance.CurLordToil?.GetType().Name} " +
                         $"vehicle='{v.LabelShort}' pos={v.Position} " +
                         $"hp={v.HitPoints}/{v.MaxHitPoints} " +
                         $"crew={v.AllPawnsAboard.Count} " +
                         $"totalPawnsInLord={__instance.ownedPawns.Count}");
        }
    }

    [HarmonyPatch(typeof(Lord), "RemovePawn")]
    internal static class Patch_LogLordRemovePawn
    {
        [HarmonyPostfix]
        static void Postfix(Lord __instance, Pawn p)
        {
            if (!VRF_Log.Enabled) return;
            if (!(__instance.LordJob is LordJob_VehicleRaid)) return;
            if (!(p is VehiclePawn v)) return;

            VRF_Log.Msg($"[LordRemovePawn] lord={__instance.faction?.def?.defName} " +
                         $"vehicle='{v.LabelShort}' pos={v.Position} " +
                         $"dead={v.Dead} destroyed={v.Destroyed} " +
                         $"hasDriver={CrewManager.HasOperationalDriver(v)} " +
                         $"remainingPawns={__instance.ownedPawns.Count}");
        }
    }

    [HarmonyPatch(typeof(DelayedDutyRefresh), "Schedule")]
    internal static class Patch_LogDelayedRefreshScheduled
    {
        [HarmonyPostfix]
        static void Postfix(Lord lord, Map map)
        {
            if (!VRF_Log.Enabled) return;
            if (!(lord?.LordJob is LordJob_VehicleRaid)) return;

            VRF_Log.Msg($"[DelayedRefresh Scheduled] lord={lord.faction?.def?.defName} " +
                         $"toil={lord.CurLordToil?.GetType().Name} " +
                         $"pawnCount={lord.ownedPawns.Count} " +
                         $"triggerTick={Find.TickManager.TicksGame + 60}");
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.MapPreTick))]
    internal static class Patch_LogPeriodicStateDump
    {
        private const int DumpInterval = 500;

        [HarmonyPostfix]
        static void Postfix(Map __instance)
        {
            if (!VRF_Log.Enabled) return;
            if (!__instance.IsHashIntervalTick(DumpInterval)) return;

            bool hasVRFLord = false;
            foreach (Lord lord in __instance.lordManager.lords)
            {
                if (lord.LordJob is LordJob_VehicleRaid) { hasVRFLord = true; break; }
            }
            if (!hasVRFLord) return;

            int tick = Find.TickManager.TicksGame;
            var sb   = new StringBuilder();
            sb.AppendLine($"════ [VRF Periodic Dump] tick={tick} map={__instance.info?.parent?.def?.defName} ════");

            foreach (Lord lord in __instance.lordManager.lords)
            {
                if (!(lord.LordJob is LordJob_VehicleRaid vrfJob)) continue;

                sb.AppendLine($"  LORD faction={lord.faction?.def?.defName} " +
                               $"toil={lord.CurLordToil?.GetType().Name} " +
                               $"ticksInToil={lord.ticksInToil} " +
                               $"naturalRaidLord={(vrfJob.naturalRaidLord != null ? vrfJob.naturalRaidLord.LordJob?.GetType().Name : "null")} " +
                               $"pawnCount={lord.ownedPawns.Count}");

                foreach (Pawn pawn in lord.ownedPawns)
                {
                    if (pawn is VehiclePawn v)
                    {
                        var turretSb = new StringBuilder();
                        var tc = v.CompVehicleTurrets;
                        if (tc?.Turrets != null)
                        {
                            foreach (var t in tc.Turrets)
                            {
                                string ammoStr = "noAmmo";
                                if (t.def?.ammunition != null)
                                {
                                    ThingDef ammoD = System.Linq.Enumerable.FirstOrDefault(t.def.ammunition.AllowedThingDefs);
                                    int ammoCount = ammoD != null
                                        ? v.inventory.innerContainer.TotalStackCountOfDef(ammoD)
                                        : 0;
                                    ammoStr = $"{ammoCount}x{ammoD?.defName ?? "?"}";
                                }
                                turretSb.Append($"[{t.def?.defName}:{ammoStr}]");
                            }
                        }

                        var hoverComp = v.GetComp<VehicleRaid.CompVehicleHover>();
                        string hoverStr = hoverComp != null
                            ? $"hover={hoverComp.State} airborne={hoverComp.IsAirborne}"
                            : "noHover";

                        string enemyStr = v.mindState?.enemyTarget != null
                            ? $"{v.mindState.enemyTarget.LabelShort}@{v.mindState.enemyTarget.Position}"
                            : "none";

                        string destStr = v.vehiclePather?.Destination.IsValid == true
                            ? v.vehiclePather.Destination.Cell.ToString()
                            : "noDest";

                        sb.AppendLine($"    VEH [{v.LabelShort}] " +
                                       $"pos={v.Position} " +
                                       $"dest={destStr} " +
                                       $"duty={v.mindState?.duty?.def?.defName ?? "null"} " +
                                       $"dutyFocus={v.mindState?.duty?.focus.Cell} " +
                                       $"job={v.CurJobDef?.defName ?? "null"} " +
                                       $"jobTarget={v.CurJob?.targetA.Cell} " +
                                       $"moving={v.vehiclePather?.Moving} " +
                                       $"drafted={v.ignition?.Drafted} " +
                                       $"hp={v.HitPoints}/{v.MaxHitPoints} " +
                                       $"crew={v.AllPawnsAboard.Count} " +
                                       $"fuel={v.GetComp<CompFueledTravel>()?.Fuel:F0} " +
                                       $"enemy={enemyStr} " +
                                       $"turrets={turretSb} " +
                                       $"{hoverStr}");
                    }
                    else
                    {
                        sb.AppendLine($"    INF [{pawn.LabelShort}] " +
                                       $"pos={pawn.Position} " +
                                       $"duty={pawn.mindState?.duty?.def?.defName ?? "null"} " +
                                       $"job={pawn.CurJobDef?.defName ?? "null"} " +
                                       $"hp={pawn.HitPoints}/{pawn.MaxHitPoints}");
                    }
                }
            }
            sb.Append($"════════════════════════════════════════════");
            VRF_Log.Msg(sb.ToString());
        }
    }

    [HarmonyPatch(typeof(Pawn_MindState), "Notify_EngagedTarget")]
    internal static class Patch_LogEngagedTarget
    {
        [HarmonyPostfix]
        static void Postfix(Pawn_MindState __instance)
        {
            if (!VRF_Log.Enabled) return;

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!(pawn is VehiclePawn v)) return;
            if (v.GetLord()?.LordJob is not LordJob_VehicleRaid) return;

            Thing enemy = __instance.enemyTarget;
            string enemyStr = enemy != null ? $"{enemy.LabelShort}@{enemy.Position}" : "null";

            VRF_Log.Msg($"[EngagedTarget] [{v.LabelShort}] target={enemyStr} " +
                         $"job={v.CurJobDef?.defName ?? "null"} " +
                         $"duty={v.mindState?.duty?.def?.defName ?? "null"} " +
                         $"pos={v.Position}");
        }
    }

    [HarmonyPatch(typeof(VRF_SettlementSpawnHelper), "TryMarkInjected")]
    internal static class Patch_LogSpawnGuard
    {
        [HarmonyPostfix]
        static void Postfix(Map map, bool __result)
        {
            if (!VRF_Log.Enabled) return;
            if (map == null) return;

            VRF_Log.Msg($"[SpawnGuard] map={map.info?.parent?.def?.defName} id={map.uniqueID} " +
                         $"faction={map.ParentFaction?.def?.defName} " +
                         $"allowed={__result}");
        }
    }

    [HarmonyPatch(typeof(LordToil_VehicleHoldPosition), "Init")]
    internal static class Patch_LogHoldPositionInit
    {
        [HarmonyPostfix]
        static void Postfix(LordToil_VehicleHoldPosition __instance)
        {
            if (!VRF_Log.Enabled) return;

            Lord lord = __instance.lord;
            VRF_Log.Msg($"[HoldPosition.Init] lord={lord?.faction?.def?.defName} " +
                         $"holdSpot={__instance.FlagLoc} " +
                         $"pawnCount={lord?.ownedPawns.Count} " +
                         $"naturalRaidLord={((lord?.LordJob as LordJob_VehicleRaid)?.naturalRaidLord?.LordJob?.GetType().Name ?? "null")}");
        }
    }

    [HarmonyPatch(typeof(LordJob_VehicleRaid), "CreateGraph")]
    internal static class Patch_LogCreateGraph
    {
        [HarmonyPostfix]
        static void Postfix(LordJob_VehicleRaid __instance, StateGraph __result)
        {
            if (!VRF_Log.Enabled) return;

            VRF_Log.Msg($"[CreateGraph] faction={__instance.lord?.faction?.def?.defName} " +
                         $"startingToil={__result?.StartingToil?.GetType().Name} " +
                         $"toilCount={__result?.lordToils?.Count} " +
                         $"transitionCount={__result?.transitions?.Count}");
        }
    }
}
