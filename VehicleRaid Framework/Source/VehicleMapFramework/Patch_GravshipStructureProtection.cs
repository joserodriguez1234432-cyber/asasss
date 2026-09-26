using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VehicleRaidFramework.VehicleMapFramework
{
    /// <summary>
    /// Prevents claiming, deconstructing, and manipulating structures belonging to NPC/enemy Gravships
    /// (and any other VehicleMapFramework vehicle map owned by an NPC faction) unless God Mode is enabled.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Patch_GravshipStructureProtection
    {
        /// <summary>
        /// Checks if a thing (building, structure, hull segment) belongs to an NPC-controlled
        /// vehicle map or gravship.
        /// </summary>
        public static bool IsNPCVehicleStructure(Thing thing)
        {
            if (thing == null) return false;

            // 1. Direct check: the thing itself is an NPC vehicle
            if (thing is Vehicles.VehiclePawn vp && vp.Faction != null && vp.Faction != Faction.OfPlayer)
            {
                return true;
            }

            // 2. Check the map where the thing is located
            Map map = thing.MapHeld ?? thing.Map;
            if (map != null)
            {
                // Check Vehicle Map Framework map parent
                if (map.Parent is global::VehicleMapFramework.MapParent_Vehicle mpv)
                {
                    if (mpv.vehicle != null && mpv.vehicle.Faction != null && mpv.vehicle.Faction != Faction.OfPlayer)
                    {
                        return true;
                    }
                    if (mpv.Faction != null && mpv.Faction != Faction.OfPlayer)
                    {
                        return true;
                    }
                }

                // Check via VMF VehicleMapUtility if available
                try
                {
                    if (global::VehicleMapFramework.VehicleMapUtility.IsVehicleMapOf(map, out var vehiclePawn))
                    {
                        if (vehiclePawn != null && vehiclePawn.Faction != null && vehiclePawn.Faction != Faction.OfPlayer)
                        {
                            return true;
                        }
                    }
                }
                catch { }

                // Fallback: If it's a pocket map belonging to an NPC faction
                if (map.IsPocketMap && thing.Faction != null && thing.Faction != Faction.OfPlayer)
                {
                    return true;
                }
            }

            // 3. Building has non-player faction and is on a vehicle map
            if (thing is Building b && b.Faction != null && b.Faction != Faction.OfPlayer)
            {
                if (map != null && map.Parent is global::VehicleMapFramework.MapParent_Vehicle)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Blocks Building.ClaimableBy for structures on NPC vehicle maps (unless God Mode).
    /// Original signature: public virtual AcceptanceReport ClaimableBy(Faction by)
    /// </summary>
    [HarmonyPatch(typeof(Building), nameof(Building.ClaimableBy))]
    public static class Patch_Building_ClaimableBy_NPCVehicle
    {
        public static void Postfix(Building __instance, Faction by, ref AcceptanceReport __result)
        {
            if (!__result.Accepted || DebugSettings.godMode) return;

            if (by == Faction.OfPlayer && Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance))
            {
                __result = false;
            }
        }
    }

    /// <summary>
    /// Blocks Building.DeconstructibleBy for structures on NPC vehicle maps (unless God Mode).
    /// Original signature: public virtual AcceptanceReport DeconstructibleBy(Faction faction)
    /// </summary>
    [HarmonyPatch(typeof(Building), nameof(Building.DeconstructibleBy))]
    public static class Patch_Building_DeconstructibleBy_NPCVehicle
    {
        public static void Postfix(Building __instance, Faction faction, ref AcceptanceReport __result)
        {
            if (!__result.Accepted || DebugSettings.godMode) return;

            if (faction == Faction.OfPlayer && Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance))
            {
                __result = false;
            }
        }
    }

    /// <summary>
    /// Blocks Designator_Claim for structures on NPC vehicle maps (unless God Mode).
    /// Original signature: public override AcceptanceReport CanDesignateThing(Thing t)
    /// </summary>
    [HarmonyPatch(typeof(Designator_Claim), nameof(Designator_Claim.CanDesignateThing))]
    public static class Patch_Designator_Claim_CanDesignateThing_NPCVehicle
    {
        public static void Postfix(Thing t, ref AcceptanceReport __result)
        {
            if (!__result.Accepted || DebugSettings.godMode) return;

            if (Patch_GravshipStructureProtection.IsNPCVehicleStructure(t))
            {
                __result = false;
            }
        }
    }

    /// <summary>
    /// Blocks Designator_Deconstruct for structures on NPC vehicle maps (unless God Mode).
    /// Original signature: public override AcceptanceReport CanDesignateThing(Thing t)
    /// </summary>
    [HarmonyPatch(typeof(Designator_Deconstruct), nameof(Designator_Deconstruct.CanDesignateThing))]
    public static class Patch_Designator_Deconstruct_CanDesignateThing_NPCVehicle
    {
        public static void Postfix(Thing t, ref AcceptanceReport __result)
        {
            if (!__result.Accepted || DebugSettings.godMode) return;

            if (Patch_GravshipStructureProtection.IsNPCVehicleStructure(t))
            {
                __result = false;
            }
        }
    }

    /// <summary>
    /// Blocks VMF's Designator_RemoveVehicleSegment on NPC vehicles (unless God Mode).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Designator_RemoveVehicleSegment_CanDesignateThing_NPCVehicle
    {
        public static bool Prepare()
        {
            return AccessTools.TypeByName("VehicleMapFramework.Designator_RemoveVehicleSegment") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VehicleMapFramework.Designator_RemoveVehicleSegment:CanDesignateThing");
        }

        public static void Postfix(Thing t, ref AcceptanceReport __result)
        {
            if (!__result.Accepted || DebugSettings.godMode) return;

            if (Patch_GravshipStructureProtection.IsNPCVehicleStructure(t))
            {
                __result = false;
            }
        }
    }

    /// <summary>
    /// Suppresses the "Remove Segment" gizmo on structures of NPC vehicles (unless God Mode).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_CompRemoveSegmentGizmo_CompGetGizmosExtra_NPCVehicle
    {
        public static bool Prepare()
        {
            return AccessTools.TypeByName("VehicleMapFramework.CompRemoveSegmentGizmo") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VehicleMapFramework.CompRemoveSegmentGizmo:CompGetGizmosExtra");
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, ThingComp __instance)
        {
            if (!DebugSettings.godMode && __instance?.parent != null && Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance.parent))
            {
                yield break;
            }

            if (__result != null)
            {
                foreach (var g in __result)
                {
                    yield return g;
                }
            }
        }
    }

    /// <summary>
    /// Suppresses the "Disembark / Unload Pawn" gizmo from consoles/seats on NPC vehicles (unless God Mode).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_CompVehicleSeat_CompGetGizmosExtra_NPCVehicle
    {
        public static bool Prepare()
        {
            return AccessTools.TypeByName("VehicleMapFramework.CompVehicleSeat") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VehicleMapFramework.CompVehicleSeat:CompGetGizmosExtra");
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, ThingComp __instance)
        {
            if (__result == null) yield break;

            bool isNPC = !DebugSettings.godMode && __instance?.parent != null && Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance.parent);

            foreach (var g in __result)
            {
                if (isNPC && g != null && g.GetType().Name.Contains("Command_ActionPawnDrawer"))
                {
                    continue;
                }
                yield return g;
            }
        }
    }

    /// <summary>
    /// Suppresses the "Vehicle Mode" toggle gizmo on thrusters of NPC gravships (unless God Mode).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_CompLocalThruster_CompGetGizmosExtra_NPCVehicle
    {
        public static bool Prepare()
        {
            return AccessTools.TypeByName("VehicleMapFramework.CompLocalThruster") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VehicleMapFramework.CompLocalThruster:CompGetGizmosExtra");
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, ThingComp __instance)
        {
            if (!DebugSettings.godMode && __instance?.parent != null && Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance.parent))
            {
                yield break;
            }

            if (__result != null)
            {
                foreach (var g in __result)
                {
                    yield return g;
                }
            }
        }
    }

    /// <summary>
    /// Suppresses gizmos on Building_GravshipWheel for NPC gravships (unless God Mode).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Building_GravshipWheel_GetGizmos_NPCVehicle
    {
        public static bool Prepare()
        {
            return AccessTools.TypeByName("VehicleMapFramework.Building_GravshipWheel") != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("VehicleMapFramework.Building_GravshipWheel:GetGizmos");
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building __instance)
        {
            if (!DebugSettings.godMode && Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance))
            {
                yield break;
            }

            if (__result != null)
            {
                foreach (var g in __result)
                {
                    yield return g;
                }
            }
        }
    }

    /// <summary>
    /// Filters out unauthorized gizmos on NPC vehicle structures (such as 'Build copy', 'Vehicle Mode',
    /// 'Disembark pawn', etc.) when selected by the player, unless God Mode is enabled.
    /// </summary>
    [HarmonyPatch(typeof(Building), nameof(Building.GetGizmos))]
    public static class Patch_Building_GetGizmos_NPCVehicle
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building __instance)
        {
            if (__result == null) yield break;

            if (DebugSettings.godMode || !Patch_GravshipStructureProtection.IsNPCVehicleStructure(__instance))
            {
                foreach (var g in __result)
                {
                    yield return g;
                }
                yield break;
            }

            foreach (var gizmo in __result)
            {
                if (ShouldHideGizmoForNPC(gizmo))
                {
                    continue;
                }
                yield return gizmo;
            }
        }

        private static bool ShouldHideGizmoForNPC(Gizmo gizmo)
        {
            if (gizmo == null) return false;

            string typeName = gizmo.GetType().FullName ?? gizmo.GetType().Name;
            if (typeName.Contains("Command_ActionPawnDrawer") || typeName.Contains("Command_FlipBuilding"))
            {
                return true;
            }

            if (gizmo is Command cmd)
            {
                string label = cmd.defaultLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    // Filter "Construir copia" / "Build copy"
                    if (label.Equals("CommandBuildCopy".Translate(), StringComparison.OrdinalIgnoreCase) ||
                        label.IndexOf("Build copy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        label.IndexOf("Construir copia", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }

                    // Filter "Vehicle Mode"
                    if (label.Equals("VMF_VehicleMode".Translate(), StringComparison.OrdinalIgnoreCase) ||
                        label.IndexOf("Vehicle Mode", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        label.IndexOf("Modo vehículo", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        label.IndexOf("Modo vehiculo", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }

                    // Filter "Unload / Bajar / Desembarcar"
                    if (label.Equals("VF_DisembarkSinglePawn".Translate(), StringComparison.OrdinalIgnoreCase) ||
                        label.StartsWith("Unload ", StringComparison.OrdinalIgnoreCase) ||
                        label.StartsWith("Bajar ", StringComparison.OrdinalIgnoreCase) ||
                        label.StartsWith("Desembarcar ", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
