using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Vehicles;
using Verse;

namespace VehicleRaidFramework
{
    /// <summary>
    /// FUEL FIX for non-player (raid) gravships converted into vehicles by VehicleMap Framework.
    ///
    /// Root cause:
    ///   - Building_GravEngine builds its fuel/facility cache with *colonist-only* listers
    ///     (AllBuildingsColonistOfClass), so an enemy engine reports TotalFuel = 0.
    ///   - VehicleMapFramework.CompFueledTravelGravship reads its Fuel / FuelCapacity from that
    ///     engine (via its Engine property, also resolved through player-only helpers), so the
    ///     vehicle reports "no fuel" and refuses to launch/move.
    ///
    /// Fix: fall back to reading the tanks that physically exist on the engine's own map
    /// (vanilla CompRefuelable tanks + Vanilla Gravship Expanded / PipeSystem CompResourceStorage
    /// tanks), regardless of faction, and fall back to finding the engine on the vehicle's
    /// interior map regardless of faction.
    /// </summary>
    public static class VRF_GravshipFuelHelper
    {
        public static PropertyInfo GetPropertySafe(Type type, string name)
        {
            if (type == null) return null;
            Type current = type;
            while (current != null && current != typeof(object))
            {
                var prop = current.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (prop != null) return prop;
                current = current.BaseType;
            }
            return null;
        }

        public static FieldInfo GetFieldSafe(Type type, string name)
        {
            if (type == null) return null;
            Type current = type;
            while (current != null && current != typeof(object))
            {
                var field = current.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                current = current.BaseType;
            }
            return null;
        }

        /// <summary>Finds any Building_GravEngine on a map, ignoring faction.</summary>
        public static Building_GravEngine FindEngine(Map map)
        {
            if (map == null) return null;
            try
            {
                return map.listerThings.AllThings.OfType<Building_GravEngine>().FirstOrDefault();
            }
            catch { return null; }
        }

        /// <summary>Sums fuel and capacity of every tank on the map, ignoring faction.</summary>
        public static void SumTanks(Map map, out float fuel, out float capacity)
        {
            fuel = 0f;
            capacity = 0f;
            if (map == null) return;

            List<Thing> things;
            try { things = map.listerThings.AllThings.ToList(); }
            catch { return; }

            foreach (Thing t in things)
            {
                if (!(t is ThingWithComps twc)) continue;

                // A. PipeSystem / VGE tanks (CompResourceStorage) - reflection, no hard dependency.
                foreach (ThingComp comp in twc.AllComps)
                {
                    if (comp.GetType().Name != "CompResourceStorage") continue;
                    try
                    {
                        Type ct = comp.GetType();
                        PropertyInfo amountProp = GetPropertySafe(ct, "AmountStored");
                        object storageProps = comp.props ?? GetPropertySafe(ct, "Props")?.GetValue(comp);
                        PropertyInfo capProp = GetPropertySafe(storageProps?.GetType(), "storageCapacity");
                        FieldInfo capFld = GetFieldSafe(storageProps?.GetType(), "storageCapacity");
                        float cap = capProp != null
                            ? Convert.ToSingle(capProp.GetValue(storageProps))
                            : (capFld != null ? Convert.ToSingle(capFld.GetValue(storageProps)) : 0f);
                        if (amountProp != null)
                            fuel += Convert.ToSingle(amountProp.GetValue(comp));
                        capacity += cap;
                    }
                    catch { }
                    break;
                }

                // B. Vanilla tanks (CompRefuelable) that actually feed the gravship.
                CompRefuelable refuelable = twc.GetComp<CompRefuelable>();
                if (refuelable == null) continue;
                if (!IsGravshipFuelProvider(twc)) continue;

                fuel += refuelable.Fuel;
                capacity += refuelable.Props?.fuelCapacity ?? 0f;
            }
        }

        public static bool IsGravshipFuelProvider(ThingWithComps twc)
        {
            try
            {
                foreach (ThingComp comp in twc.AllComps)
                {
                    Type ct = comp.GetType();
                    if (ct.Name != "CompGravshipFacility" && ct.Name != "CompGravshipFacilityPossibly") continue;
                    object facProps = comp.props ?? GetPropertySafe(ct, "Props")?.GetValue(comp);
                    FieldInfo ff = GetFieldSafe(facProps?.GetType(), "providesFuel");
                    if (ff != null && Convert.ToBoolean(ff.GetValue(facProps))) return true;
                    break;
                }
            }
            catch { }

            string dn = twc.def?.defName ?? "";
            return dn.Contains("Tank") || dn.Contains("Fuel") || dn.Contains("fuel");
        }

        /// <summary>Map the comp belongs to: the vehicle's interior map for a gravship vehicle.</summary>
        public static Map GetGravshipMap(ThingComp comp)
        {
            if (comp?.parent == null) return null;
            try
            {
                if (comp.parent is global::VehicleMapFramework.VehiclePawnWithMap vwm)
                    return vwm.VehicleMap;
            }
            catch { }
            return comp.parent.MapHeld;
        }
    }

    /// <summary>
    /// Building_GravEngine.TotalFuel returns 0 for non-player engines (colonist-only lister).
    /// Recompute from the tanks actually present on the engine's map.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Building_GravEngine_TotalFuel
    {
        public static bool Prepare()
        {
            return ModsConfig.OdysseyActive && AccessTools.PropertyGetter(typeof(Building_GravEngine), "TotalFuel") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Building_GravEngine), "TotalFuel");
        }

        public static void Postfix(Building_GravEngine __instance, ref float __result)
        {
            if (__result > 0f) return;
            if (__instance == null || !__instance.Spawned) return;
            if (__instance.Faction == Faction.OfPlayer) return;

            VRF_GravshipFuelHelper.SumTanks(__instance.Map, out float fuel, out _);
            if (fuel > 0f) __result = fuel;
        }
    }

    /// <summary>
    /// Same for the engine's fuel capacity (name differs between builds, so both are tried).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Building_GravEngine_FuelCapacity
    {
        private static MethodBase target;

        public static bool Prepare()
        {
            if (!ModsConfig.OdysseyActive) return false;
            target = AccessTools.PropertyGetter(typeof(Building_GravEngine), "MaxFuel")
                     ?? AccessTools.PropertyGetter(typeof(Building_GravEngine), "FuelCapacity")
                     ?? AccessTools.PropertyGetter(typeof(Building_GravEngine), "TotalFuelCapacity");
            return target != null;
        }

        public static MethodBase TargetMethod() => target;

        public static void Postfix(Building_GravEngine __instance, ref float __result)
        {
            if (__result > 0f) return;
            if (__instance == null || !__instance.Spawned) return;
            if (__instance.Faction == Faction.OfPlayer) return;

            VRF_GravshipFuelHelper.SumTanks(__instance.Map, out _, out float cap);
            if (cap > 0f) __result = cap;
        }
    }

    /// <summary>
    /// CompFueledTravelGravship.Engine resolves through player-only helpers, so it is null for
    /// raid gravships and every fuel read collapses to 0. Fall back to the engine physically
    /// present inside the vehicle map.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_CompFueledTravelGravship_Engine
    {
        private static MethodBase target;

        public static bool Prepare()
        {
            Type t = AccessTools.TypeByName("VehicleMapFramework.CompFueledTravelGravship");
            if (t == null) return false;
            target = AccessTools.DeclaredPropertyGetter(t, "Engine") ?? AccessTools.PropertyGetter(t, "Engine");
            return target != null;
        }

        public static MethodBase TargetMethod() => target;

        public static void Postfix(ThingComp __instance, ref Building_GravEngine __result)
        {
            if (__result != null) return;
            Map map = VRF_GravshipFuelHelper.GetGravshipMap(__instance);
            __result = VRF_GravshipFuelHelper.FindEngine(map);
        }
    }

    /// <summary>
    /// Final safety net: if the vehicle still reports 0 fuel / 0 capacity, read the interior tanks.
    /// Patches the override declared by CompFueledTravelGravship when present, otherwise the base
    /// CompFueledTravel property (filtered to gravship vehicles only).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_CompFueledTravelGravship_Fuel
    {
        private static MethodBase target;
        private static Type gravshipCompType;

        public static bool Prepare()
        {
            gravshipCompType = AccessTools.TypeByName("VehicleMapFramework.CompFueledTravelGravship");
            if (gravshipCompType == null) return false;
            target = AccessTools.DeclaredPropertyGetter(gravshipCompType, "Fuel")
                     ?? AccessTools.PropertyGetter(typeof(CompFueledTravel), "Fuel");
            return target != null;
        }

        public static MethodBase TargetMethod() => target;

        public static void Postfix(ThingComp __instance, ref float __result)
        {
            if (__result > 0f) return;
            if (gravshipCompType == null || !gravshipCompType.IsInstanceOfType(__instance)) return;

            Map map = VRF_GravshipFuelHelper.GetGravshipMap(__instance);
            VRF_GravshipFuelHelper.SumTanks(map, out float fuel, out _);
            if (fuel > 0f) __result = fuel;
        }
    }

    [HarmonyPatch]
    public static class Patch_CompFueledTravelGravship_FuelCapacity
    {
        private static MethodBase target;
        private static Type gravshipCompType;

        public static bool Prepare()
        {
            gravshipCompType = AccessTools.TypeByName("VehicleMapFramework.CompFueledTravelGravship");
            if (gravshipCompType == null) return false;
            target = AccessTools.DeclaredPropertyGetter(gravshipCompType, "FuelCapacity")
                     ?? AccessTools.PropertyGetter(typeof(CompFueledTravel), "FuelCapacity");
            return target != null;
        }

        public static MethodBase TargetMethod() => target;

        public static void Postfix(ThingComp __instance, ref float __result)
        {
            if (__result > 0f) return;
            if (gravshipCompType == null || !gravshipCompType.IsInstanceOfType(__instance)) return;

            Map map = VRF_GravshipFuelHelper.GetGravshipMap(__instance);
            VRF_GravshipFuelHelper.SumTanks(map, out _, out float cap);
            if (cap > 0f) __result = cap;
        }
    }

    /// <summary>
    /// CompGravshipFacility.providesFuel tanks are only registered on the engine for the player
    /// faction. Force a substructure/facility refresh for non-player engines when they spawn so
    /// the connection (and therefore the fuel readout in the inspect pane) is rebuilt.
    /// </summary>
    [HarmonyPatch(typeof(Building_GravEngine), "SpawnSetup")]
    public static class Patch_Building_GravEngine_SpawnSetup_NonPlayer
    {
        public static void Postfix(Building_GravEngine __instance)
        {
            if (__instance == null || __instance.Faction == Faction.OfPlayer) return;
            try { __instance.ForceSubstructureDirty(); }
            catch { }
        }
    }
}
