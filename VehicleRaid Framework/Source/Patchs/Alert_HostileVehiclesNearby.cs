using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    public class Alert_HostileVehiclesNearby : Alert
    {
        private readonly List<Pawn> cachedVehicles = new List<Pawn>();

        public Alert_HostileVehiclesNearby()
        {
            defaultPriority = AlertPriority.High;
        }

        public override string GetLabel()
        {
            int count = cachedVehicles.Count;
            return count == 1
                ? "VRF_Alert_HostileVehicle".Translate()
                : "VRF_Alert_HostileVehicles".Translate(count);
        }

        public override TaggedString GetExplanation()
        {
            return "VRF_Alert_HostileVehiclesDesc".Translate();
        }

        public override AlertReport GetReport()
        {
            cachedVehicles.Clear();

            foreach (Map map in Find.Maps)
            {
                if (map.mapPawns.FreeColonistsSpawnedCount == 0) continue;

                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (!(pawn is VehiclePawn)) continue;
                    if (pawn.Dead || pawn.Destroyed) continue;
                    if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer)) continue;

                    Lord lord = pawn.GetLord();
                    if (lord?.CurLordToil is LordToil_VehicleExitMap) continue;

                    cachedVehicles.Add(pawn);
                }
            }

            if (cachedVehicles.Count == 0)
                return AlertReport.Inactive;

            return AlertReport.CulpritsAre(cachedVehicles);
        }
    }
}