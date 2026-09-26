using System.Collections.Generic;
using Verse;
using Vehicles;

namespace VehicleRaid
{
    // Keeps the render path proportional to hover vehicles, rather than all pawns on a map.
    // Optimizado: uso de HashSet para O(1) y prevención de memory leaks en mapas destruidos.
    internal static class HoverVehicleRegistry
    {
        private static readonly Dictionary<Map, HashSet<VehiclePawn>> vehiclesByMap =
            new Dictionary<Map, HashSet<VehiclePawn>>();

        private static readonly Dictionary<Map, List<VehiclePawn>> iterationCache =
            new Dictionary<Map, List<VehiclePawn>>();
        private static readonly HashSet<Map> dirtyMaps = new HashSet<Map>();

        private static readonly IReadOnlyList<VehiclePawn> Empty = new VehiclePawn[0];

        public static IReadOnlyList<VehiclePawn> Get(Map map)
        {
            if (map == null) return Empty;

            if (dirtyMaps.Contains(map))
            {
                if (vehiclesByMap.TryGetValue(map, out var set))
                {
                    if (!iterationCache.TryGetValue(map, out var list))
                    {
                        list = new List<VehiclePawn>(set.Count);
                        iterationCache[map] = list;
                    }
                    list.Clear();
                    list.AddRange(set);
                }
                dirtyMaps.Remove(map);
            }

            if (iterationCache.TryGetValue(map, out var cachedList))
                return cachedList;

            return Empty;
        }

        public static void Register(VehiclePawn vehicle)
        {
            if (vehicle?.Map == null) return;
            Map map = vehicle.Map;

            if (!vehiclesByMap.TryGetValue(map, out var vehicles))
            {
                vehicles = new HashSet<VehiclePawn>();
                vehiclesByMap[map] = vehicles;
            }

            if (vehicles.Add(vehicle))
            {
                dirtyMaps.Add(map);
            }
        }

        public static void Deregister(VehiclePawn vehicle, Map map)
        {
            if (vehicle == null || map == null) return;

            if (vehiclesByMap.TryGetValue(map, out var vehicles))
            {
                if (vehicles.Remove(vehicle))
                {
                    dirtyMaps.Add(map);
                }

                if (vehicles.Count == 0)
                {
                    vehiclesByMap.Remove(map);
                    iterationCache.Remove(map);
                    dirtyMaps.Remove(map);
                }
            }
        }

        public static void CleanStaleMaps()
        {
            var activeMaps = Find.Maps;
            if (activeMaps == null) return;

            List<Map> toRemove = null;
            foreach (var kvp in vehiclesByMap)
            {
                if (!activeMaps.Contains(kvp.Key))
                {
                    toRemove ??= new List<Map>();
                    toRemove.Add(kvp.Key);
                }
            }

            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                {
                    Map m = toRemove[i];
                    vehiclesByMap.Remove(m);
                    iterationCache.Remove(m);
                    dirtyMaps.Remove(m);
                }
            }
        }

        public static void Clear()
        {
            vehiclesByMap.Clear();
            iterationCache.Clear();
            dirtyMaps.Clear();
        }
    }
}
