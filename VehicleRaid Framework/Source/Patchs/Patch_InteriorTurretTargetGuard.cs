using HarmonyLib;
using RimWorld;
using Verse;

namespace VehicleRaidFramework
{
    /// <summary>
    /// Guards Building_TurretGun.Tick for turrets inside a gravship interior map.
    ///
    /// Root cause:
    ///   VehicleMapFramework patches Building_Turret.Tick (base class) with a transpiler that
    ///   replaces every call to thing.get_Map → BaseMapOrCaravan(thing). This breaks the vanilla
    ///   guard in Building_Turret.Tick that resets forcedTarget when the target is on a different
    ///   map — after the patch both sides evaluate to the exterior map, so the guard never fires.
    ///
    ///   Consequence: a gravship interior turret retains a forcedTarget that is a hover vehicle
    ///   on the EXTERIOR map. When TryStartShootSomething later calls roofGrid.RoofAt(position)
    ///   using that exterior Position against the small interior map's array, it produces:
    ///       IndexOutOfRangeException: Index was outside the bounds of the array.
    ///
    /// Fix:
    ///   Prefix Building_TurretGun.Tick. If the turret is inside a vehicle interior map and its
    ///   currentTargetInt or forcedTarget lives on a different map, clear those fields before the
    ///   (VMF-patched) Tick body runs. The turret will re-acquire a valid target next interval
    ///   through TryFindNewTarget, which VMF already patches for cross-map awareness.
    ///
    /// Note: Assembly-CSharp is publicized via Krafs.Publicizer, so private/protected fields
    /// are accessible directly without reflection.
    /// </summary>
    [HarmonyPatch(typeof(Building_TurretGun), "Tick")]
    public static class Patch_InteriorTurretTargetGuard
    {
        static void Prefix(Building_TurretGun __instance)
        {
            Map turretMap = __instance.Map;
            if (turretMap == null || !IsVehicleInteriorMap(turretMap)) return;

            // ── Clear currentTargetInt if it refers to something outside the interior map ──
            if (__instance.currentTargetInt.IsValid
                && IsTargetOffMap(__instance.currentTargetInt, turretMap))
            {
                __instance.currentTargetInt = LocalTargetInfo.Invalid;
                __instance.burstWarmupTicksLeft = 0;
            }

            // ── Clear forcedTarget if it refers to something outside the interior map ─────
            // Building_Turret.forcedTarget is protected; publicizer makes it accessible.
            if (__instance.forcedTarget.IsValid
                && IsTargetOffMap(__instance.forcedTarget, turretMap))
            {
                // Call private ResetForcedTarget() so burst warmup is also zeroed and
                // TryStartShootSomething is retriggered cleanly.
                __instance.ResetForcedTarget();
            }
        }

        // Returns true when a valid thing-target lives on a map other than the turret's.
        // Cell-only targets out of interior bounds are also rejected — they would crash too.
        private static bool IsTargetOffMap(LocalTargetInfo target, Map interiorMap)
        {
            if (!target.IsValid) return false;

            if (target.HasThing)
            {
                Thing t = target.Thing;
                if (t == null || t.Destroyed) return false;
                return t.Spawned && t.Map != null && t.Map != interiorMap;
            }

            // Cell-only target outside the interior map's bounds
            return !target.Cell.InBounds(interiorMap);
        }

        // Detects a VehicleMapFramework interior map without a hard type reference.
        // VMF's interior-map parent type name always contains "VehicleMap".
        private static bool IsVehicleInteriorMap(Map map)
        {
            if (map?.Parent == null) return false;
            string typeName = map.Parent.GetType().Name;
            return typeName.IndexOf("VehicleMap", System.StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("VehiclePawnWithMap", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
