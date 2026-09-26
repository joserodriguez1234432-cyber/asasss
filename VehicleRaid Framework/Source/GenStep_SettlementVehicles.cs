using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Vehicles;
using Verse.AI.Group;

namespace VehicleRaidFramework
{
    public class GenStep_SettlementVehicles : GenStep
    {
        public override int SeedPart => 135792468;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!MapGenerator.TryGetVar<CellRect>("SettlementRect", out var settlementRect))
            {
                return;
            }

            Faction faction = map.ParentFaction;
            if (faction == null) return;

            var vDef = DefDatabase<VRF_SettlementVehicleDef>.AllDefsListForReading
                .FirstOrDefault(d => d.faction == faction.def || (d.faction == null && faction.def == FactionDefOf.Empire && d.defName.Contains("Empire")));

            if (vDef == null || vDef.vehicles.NullOrEmpty()) return;

            float points = vDef.totalCombatPoints;
            List<VehiclePawn> spawnedVehicles = new List<VehiclePawn>();

            foreach (var entry in vDef.vehicles.Where(e => e.forceCount > 0))
            {
                for (int i = 0; i < entry.forceCount; i++)
                {
                    SpawnVehicle(map, settlementRect, entry, faction, spawnedVehicles);
                    points -= (entry.vehicleKind?.combatPower ?? 100f);
                }
            }

            int safetyLimit = 20;
            while (points > 0 && safetyLimit > 0)
            {
                if (vDef.vehicles.TryRandomElementByWeight(e => e.weight, out var entry))
                {
                    float cost = entry.vehicleKind?.combatPower ?? 100f;
                    if (cost <= points || points > -50)
                    {
                        SpawnVehicle(map, settlementRect, entry, faction, spawnedVehicles);
                        points -= cost;
                    }
                    else
                    {
                        safetyLimit--;
                    }
                }
                else break;
            }

            if (spawnedVehicles.Count > 0)
            {
                Lord lord = map.lordManager.lords.FirstOrDefault(l => l.faction == faction && l.LordJob is LordJob_DefendBase);
                if (lord == null)
                {
                    lord = LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, settlementRect.CenterCell, 25000), map);
                }

                foreach (var v in spawnedVehicles)
                {
                    if (!lord.ownedPawns.Contains(v))
                        lord.AddPawn(v);
                }
            }
        }

        private void SpawnVehicle(Map map, CellRect settlementRect, SettlementVehicleEntry entry, Faction faction, List<VehiclePawn> spawnedList)
        {
            var vehicleDef = entry.vehicleKind?.race as VehicleDef;
            if (vehicleDef == null) return;

            if (TryFindSpawnCell(map, settlementRect, vehicleDef, out IntVec3 cell, out Rot4 spawnRot))
            {
                VehiclePawn vehicle = VehicleSpawner.GenerateVehicle(vehicleDef, faction);
                if (vehicle == null) return;

                if (entry.colorConfig != null)
                    VehicleRaidUtility.ApplyColorConfig(vehicle, entry.colorConfig, faction);

                GenSpawn.Spawn(vehicle, cell, map, spawnRot);

                PawnKindDef crewKind = faction.def.basicMemberKind ?? faction.def.pawnGroupMakers.SelectMany(x => x.options).Select(x => x.kind).FirstOrDefault(x => x.RaceProps.Humanlike) ?? PawnKindDefOf.AncientSoldier;
                foreach (var role in vehicleDef.properties.roles)
                {
                    for (int i = 0; i < role.Slots; i++)
                    {
                        Pawn crew = PawnGenerator.GeneratePawn(new PawnGenerationRequest(crewKind, faction, PawnGenerationContext.NonPlayer, map.Tile));
                        vehicle.TryAddPawn(crew, vehicle.GetHandler(role.key));
                    }
                }

                if (!entry.cargoItems.NullOrEmpty())
                {
                    foreach (var stack in entry.cargoItems)
                    {
                        if (stack.thingDef != null)
                        {
                            Thing item = ThingMaker.MakeThing(stack.thingDef);
                            item.stackCount = stack.count.RandomInRange;
                            vehicle.inventory.innerContainer.TryAdd(item);
                        }
                    }
                }

                if (entry.isMortar)
                {
                   map.GetComponent<VRF_LeaderManager>()?.RegisterMortar(vehicle);
                }

                Patch_VehicleNPCOnOff.UpdateVehiclePower(vehicle);

                spawnedList.Add(vehicle);
            }
        }

        private static bool IsCellValidForVehicle(IntVec3 cell, VehicleDef vehicleDef, Map map)
        {
            if (!cell.InBounds(map)) return false;
            if (cell.Roofed(map)) return false;

            TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
            if (terrain == null) return false;
            if (terrain.passability == Traversability.Impassable) return false;
            if (terrain.IsWater) return false;

            Building edifice = map.edificeGrid[cell];
            if (edifice != null) return false;

            List<Thing> things = map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || !t.Spawned) continue;
                if (t.def.passability == Traversability.Impassable) return false;
                if (t.def.fillPercent > 0.4f) return false;
            }

            int cost = VehiclePathGrid.CalculatePathCostFor(vehicleDef, map, cell);
            return cost < VehiclePathGrid.ImpassableCost;
        }

        private bool TryFindSpawnCell(Map map, CellRect settlementRect, VehicleDef vehicleDef, out IntVec3 cell, out Rot4 spawnRot)
        {
            Rot4[] rotations = { Rot4.North, Rot4.East, Rot4.South, Rot4.West };
            CellRect searchArea = settlementRect.ExpandedBy(15);

            for (int attempt = 0; attempt < 100; attempt++)
            {
                IntVec3 c = searchArea.RandomCell;
                if (!c.InBounds(map) || settlementRect.Contains(c)) continue;

                foreach (Rot4 rot in rotations)
                {
                    CellRect vehicleRect = vehicleDef.VehicleRect(c, rot);
                    CellRect padded      = vehicleRect.ExpandedBy(1);

                    if (!padded.InBounds(map)) continue;

                    bool valid = true;
                    foreach (IntVec3 cl in padded.Cells)
                    {
                        if (!IsCellValidForVehicle(cl, vehicleDef, map))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        cell     = c;
                        spawnRot = rot;
                        return true;
                    }
                }
            }

            cell     = IntVec3.Invalid;
            spawnRot = Rot4.North;
            return false;
        }
    }
}


