using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    public static class SeaVehicleSpawnUtility
    {
        public static bool TryFindSeaGroupEntryPoint(Map map, List<VehicleDef> vehicleDefs, out IntVec3 cell)
        {
            VehicleDef widest = null;
            int widestSize = 0;
            foreach (var v in vehicleDefs)
            {
                int s = Mathf.Max(v.size.x, v.size.z);
                if (s > widestSize) { widestSize = s; widest = v; }
            }

            if (widest == null)
            {
                cell = IntVec3.Invalid;
                return false;
            }

            return FindWaterCellManual(map, widest, out cell);
        }

        public static bool TryFindSeaEntryCell(Map map, VehicleDef vehicleDef, out IntVec3 cell)
        {
            return FindWaterCellManual(map, vehicleDef, out cell);
        }

        public static bool IsWater(IntVec3 c, Map map)
        {
            if (!c.InBounds(map)) return false;
            TerrainDef terrain = c.GetTerrain(map);
            if (terrain == null) return false;
            return terrain.IsWater || terrain.IsRiver || terrain.defName.Contains("Water") || terrain.defName.Contains("Ocean") || terrain.defName.Contains("Marsh");
        }

        private static bool FindWaterCellManual(Map map, VehicleDef vehicleDef, out IntVec3 cell)
        {
            List<KeyValuePair<IntVec3, float>> scoredEdgeCells = new List<KeyValuePair<IntVec3, float>>();
            CellRect mapRect = CellRect.WholeMap(map);
            
            HashSet<IntVec3> globallyVisited = new HashSet<IntVec3>();
            Dictionary<IntVec3, float> validWaterScores = new Dictionary<IntVec3, float>();
            HashSet<IntVec3> invalidWaterCache = new HashSet<IntVec3>();

            foreach (IntVec3 c in mapRect.EdgeCells)
            {
                if (IsWater(c, map) && !c.Fogged(map))
                {
                    if (validWaterScores.TryGetValue(c, out float cachedScore))
                    {
                        scoredEdgeCells.Add(new KeyValuePair<IntVec3, float>(c, cachedScore));
                        continue;
                    }
                    if (invalidWaterCache.Contains(c))
                    {
                        continue;
                    }

                    if (TryGetWaterPathScore(c, map, globallyVisited, validWaterScores, invalidWaterCache, out float score))
                    {
                        scoredEdgeCells.Add(new KeyValuePair<IntVec3, float>(c, score));
                    }
                }
            }

            if (scoredEdgeCells.Count > 0)
            {
                scoredEdgeCells.Sort((a, b) => a.Value.CompareTo(b.Value));
                
                float bestScore = scoredEdgeCells[0].Value;
                List<IntVec3> bestCells = new List<IntVec3>();
                foreach (var kvp in scoredEdgeCells)
                {
                    if (kvp.Value <= bestScore + 5f)
                    {
                        bestCells.Add(kvp.Key);
                    }
                }

                bestCells.Shuffle();
                foreach (IntVec3 c in bestCells)
                {
                    if (vehicleDef != null)
                    {
                        IntVec3 padded = c.PadForHitbox(map, vehicleDef);
                        if (IsWater(padded, map))
                        {
                            cell = padded;
                            return true;
                        }
                    }
                    else
                    {
                        cell = c;
                        return true;
                    }
                }

                cell = bestCells.RandomElement();
                return true;
            }

            cell = IntVec3.Invalid;
            return false;
        }

        private static Vector3 GetColonyCenter(Map map)
        {
            float x = 0;
            float z = 0;
            int count = 0;
            foreach (Pawn pawn in map.mapPawns.FreeColonists)
            {
                x += pawn.Position.x;
                z += pawn.Position.z;
                count++;
            }
            if (count > 0)
            {
                return new Vector3(x / count, 0, z / count);
            }
            return map.Center.ToVector3();
        }

        private static bool TryGetWaterPathScore(IntVec3 startCell, Map map, HashSet<IntVec3> globallyVisited, Dictionary<IntVec3, float> validScores, HashSet<IntVec3> invalidCache, out float score)
        {
            score = float.MaxValue;
            int waterCellsFound = 0;
            Vector3 centerPos = GetColonyCenter(map);
            float closestDistSqToCenter = float.MaxValue;
            float maxAllowedDistance = Mathf.Max(map.Size.x, map.Size.z) * 0.35f;
            float maxAllowedDistSq = maxAllowedDistance * maxAllowedDistance;

            Queue<IntVec3> queue = new Queue<IntVec3>();
            HashSet<IntVec3> localVisited = new HashSet<IntVec3>();

            queue.Enqueue(startCell);
            localVisited.Add(startCell);
            globallyVisited.Add(startCell);
            bool reachedInland = false;

            while (queue.Count > 0)
            {
                IntVec3 curr = queue.Dequeue();
                waterCellsFound++;

                float distSq = (curr.x - centerPos.x) * (curr.x - centerPos.x) + (curr.z - centerPos.z) * (curr.z - centerPos.z);
                if (distSq < closestDistSqToCenter)
                {
                    closestDistSqToCenter = distSq;
                    if (closestDistSqToCenter <= maxAllowedDistSq)
                    {
                        reachedInland = true;
                    }
                }

                if (waterCellsFound >= 100 && reachedInland)
                {
                    float finalDist = Mathf.Sqrt(closestDistSqToCenter);
                    foreach (var v in localVisited) validScores[v] = finalDist;
                    score = finalDist;
                    return true;
                }

                if (waterCellsFound > 1500)
                {
                    break; 
                }

                for (int i = 0; i < 4; i++)
                {
                    IntVec3 next = curr + GenAdj.CardinalDirections[i];
                    if (next.InBounds(map) && localVisited.Add(next) && IsWater(next, map))
                    {
                        globallyVisited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            bool isValid = waterCellsFound >= 100 && reachedInland;
            if (isValid)
            {
                float finalDist = Mathf.Sqrt(closestDistSqToCenter);
                foreach (var v in localVisited) validScores[v] = finalDist;
                score = finalDist;
            }
            else
            {
                foreach (var v in localVisited) invalidCache.Add(v);
            }

            return isValid;
        }
    }
}
