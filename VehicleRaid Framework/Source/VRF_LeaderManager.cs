using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Vehicles;
using RimWorld;
using SmashTools;
using Verse.AI;
using Verse.AI.Group;

namespace VehicleRaidFramework
{
    public class VRF_LeaderManager : MapComponent
    {
        private Dictionary<VehiclePawn, int> leaderVehicles = new Dictionary<VehiclePawn, int>();
        private HashSet<VehiclePawn> mortarVehicles = new HashSet<VehiclePawn>();
        private HashSet<VehiclePawn> settlementMortarVehicles = new HashSet<VehiclePawn>();

        private Dictionary<Faction, VehiclePawn> cachedBestLeaders = new Dictionary<Faction, VehiclePawn>();
        private int lastCacheTick = -1;

        public VRF_LeaderManager(Map map) : base(map) { }

        public void RegisterLeader(VehiclePawn vehicle, int priority)
        {
            if (vehicle == null) return;
            leaderVehicles[vehicle] = priority;
            ForceRefresh();
        }

        public void RegisterMortar(VehiclePawn vehicle)
        {
            if (vehicle == null) return;
            mortarVehicles.Add(vehicle);
        }

        public void RegisterSettlementMortar(VehiclePawn vehicle)
        {
            if (vehicle == null) return;
            settlementMortarVehicles.Add(vehicle);
        }

        public bool IsMortar(VehiclePawn vehicle)
        {
            if (vehicle == null) return false;
            return mortarVehicles.Contains(vehicle) || settlementMortarVehicles.Contains(vehicle);
        }

        public void ForceRefresh()
        {
            lastCacheTick = -1;
            cachedBestLeaders.Clear();
        }

        public void NotifyRaidStarted()
        {
            hasActiveVehicleRaid = true;
            lastRaidCheckTick = Find.TickManager.TicksGame;
        }

        private bool hasActiveVehicleRaid = false;
        private int  lastRaidCheckTick   = -999;
        private const int RaidCheckInterval = 300;

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            int tick = Find.TickManager.TicksGame;

            if (tick - lastRaidCheckTick >= RaidCheckInterval)
            {
                lastRaidCheckTick = tick;
                bool hadRaid = hasActiveVehicleRaid;
                hasActiveVehicleRaid = false;
                var lords = map.lordManager.lords;
                for (int i = 0; i < lords.Count; i++)
                {
                    if (lords[i].LordJob is LordJob_VehicleRaid)
                    {
                        hasActiveVehicleRaid = true;
                        break;
                    }
                }

                if (hadRaid && !hasActiveVehicleRaid)
                {
                    leaderVehicles.Clear();
                    mortarVehicles.Clear();
                    cachedBestLeaders.Clear();
                    lastCacheTick = -1;
                }
            }

            if (tick % 600 == 0)
                settlementMortarVehicles.RemoveWhere(v => v == null || v.Dead || !v.Spawned);

            if (!hasActiveVehicleRaid) return;
            if (tick % 60 != 0) return;

            {
                var lords = map.lordManager.lords;
                for (int i = 0; i < lords.Count; i++)
                {
                    Lord lord = lords[i];
                    if (!(lord.LordJob is LordJob_VehicleRaid)) continue;

                    for (int j = lord.ownedPawns.Count - 1; j >= 0; j--)
                    {
                        Pawn pawn = lord.ownedPawns[j];
                        if (pawn is VehiclePawn v)
                        {
                            string dName = v.mindState.duty?.def?.defName;
                            if (dName != null && (dName.IndexOf("Exit", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                  dName.IndexOf("Steal", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                  dName.IndexOf("Kidnap", StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                  dName.IndexOf("Flee", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  dName.IndexOf("Panic", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                var exitDef = VRF_DutyDefOf.VRF_VehicleExitMap ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleExitMap", false) ?? DutyDefOf.ExitMapBest;
                                if (v.mindState.duty.def != exitDef) v.mindState.duty = new PawnDuty(exitDef);
                                continue;
                            }

                            if (lord.CurLordToil is LordToil_VehicleHoldPosition)
                            {
                                if (tick % 600 == 0)
                                    Log.Message($"[VRF_Debug] LeaderManager — skipping duty override for {v.LabelShort} (lord in HoldPosition)");
                                continue;
                            }

                            if (CrewManager.HasOperationalDriver(v) || CrewManager.AnyFriendlyInfantryNearby(v) || CrewManager.IsAnyPawnBoarding(v))
                            {
                                var dutyDef = VRF_DutyDefOf.VRF_VehicleSearchAndDestroy ?? DefDatabase<DutyDef>.GetNamed("VRF_VehicleSearchAndDestroy", false);
                                if (v.mindState.duty == null || v.mindState.duty.def != dutyDef)
                                {
                                    v.mindState.duty = new PawnDuty(dutyDef);
                                }
                            }
                        }
                    }

                    if (lord.CurLordToil is LordToil_VehicleHoldPosition)
                        continue;

                    Faction raidFaction = lord.faction;
                    if (raidFaction != null)
                    {
                        foreach (Pawn p in map.mapPawns.FreeHumanlikesSpawnedOfFaction(raidFaction))
                        {
                            if (p.GetLord() == null && !p.Dead && !p.Downed)
                            {
                                lord.AddPawn(p);
                                var infantryDuty = VRF_DutyDefOf.VRF_InfantryAssault ?? DefDatabase<DutyDef>.GetNamed("VRF_InfantryAssault", false);
                                if (infantryDuty != null)
                                {
                                    p.mindState.duty = new PawnDuty(infantryDuty);
                                }
                            }
                        }
                    }
                }
            }
        }

        public VehiclePawn GetBestLeader(Faction faction, IEnumerable<Pawn> candidates = null)
        {
            if (faction == null) return null;

            if (candidates != null)
            {
                VehiclePawn bestInCandidates = null;
                int bestPriorityInCandidates = int.MaxValue;
                VehiclePawn firstAnyVehicleInCandidates = null;

                foreach (Pawn p in candidates)
                {
                    if (p is VehiclePawn v && v.Faction == faction && !v.Dead && !v.Destroyed && v.Spawned && CrewManager.HasOperationalDriver(v))
                    {
                        if (firstAnyVehicleInCandidates == null) firstAnyVehicleInCandidates = v;

                        if (leaderVehicles.TryGetValue(v, out int priority))
                        {
                            if (priority < bestPriorityInCandidates)
                            {
                                bestPriorityInCandidates = priority;
                                bestInCandidates = v;
                            }
                        }
                    }
                }

                if (bestInCandidates != null) return bestInCandidates;
                if (firstAnyVehicleInCandidates != null) return firstAnyVehicleInCandidates;
            }

            if (candidates == null && Find.TickManager.TicksGame == lastCacheTick)
            {
                if (cachedBestLeaders.TryGetValue(faction, out var cached)) return cached;
            }

            VehiclePawn bestLeader = null;
            int bestPriority = int.MaxValue;

            foreach (var kvp in leaderVehicles)
            {
                VehiclePawn v = kvp.Key;
                if (v != null && !v.Dead && !v.Destroyed && v.Spawned && v.Faction == faction && v.Map == map && CrewManager.HasOperationalDriver(v))
                {
                    if (kvp.Value < bestPriority)
                    {
                        bestPriority = kvp.Value;
                        bestLeader = v;
                    }
                }
            }

            if (bestLeader == null)
            {
                foreach (Pawn p in map.mapPawns.AllPawnsSpawned)
                {
                    if (p is VehiclePawn v && v.Faction == faction && !v.Dead && !v.Destroyed && v.Spawned && CrewManager.HasOperationalDriver(v))
                    {
                        bestLeader = v;
                        break;
                    }
                }
            }

            if (candidates == null)
            {
                lastCacheTick = Find.TickManager.TicksGame;
                cachedBestLeaders[faction] = bestLeader;
            }

            return bestLeader;
        }

        private List<VehiclePawn> tmpLeaderKeys;
        private List<int> tmpLeaderValues;

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                leaderVehicles.RemoveAll(kvp => kvp.Key == null || kvp.Key.Dead || !kvp.Key.Spawned);
                mortarVehicles.RemoveWhere(v => v == null || v.Dead || !v.Spawned);
                settlementMortarVehicles.RemoveWhere(v => v == null || v.Dead || !v.Spawned);
            }
            
            Scribe_Collections.Look(ref leaderVehicles, "leaderVehicles", LookMode.Reference, LookMode.Value, ref tmpLeaderKeys, ref tmpLeaderValues);
            Scribe_Collections.Look(ref mortarVehicles, "mortarVehicles", LookMode.Reference);
            Scribe_Collections.Look(ref settlementMortarVehicles, "settlementMortarVehicles", LookMode.Reference);
            
            if (leaderVehicles == null) leaderVehicles = new Dictionary<VehiclePawn, int>();
            if (mortarVehicles == null) mortarVehicles = new HashSet<VehiclePawn>();
            if (settlementMortarVehicles == null) settlementMortarVehicles = new HashSet<VehiclePawn>();
        }
    }
}

