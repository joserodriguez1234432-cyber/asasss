using HarmonyLib;
using Verse;
using Vehicles;
using RimWorld;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace VehicleRaid.HoverFlight.AntiAirPatches
{
    [HarmonyPatch]
    public static class Patch_Rimatomics_HEL_Tick
    {
        private static Type buildingHELType;
        private static FieldInfo currentTargetField;
        private static FieldInfo beamAlphaField;
        private static FieldInfo burstWarmupField;
        private static FieldInfo burstCooldownField;
        private static FieldInfo powerCompField;
        private static PropertyInfo pulseSizeProperty;
        private static PropertyInfo rangeProperty;
        private static MethodInfo dissipateMethod;
        private static MethodInfo cooldownMethod;
        private static MethodInfo warmupMethod;
        
        private static MethodInfo getResearchMethod;
        private static FieldInfo buggerMeField;

        private static Dictionary<int, HELState> helStates = new Dictionary<int, HELState>();

        private class HELState
        {
            public VehiclePawn target;
            public int warmupTicks;
            public int cooldownTicks;
            public int beamTicks;
        }

        public static bool Prepare()
        {
            if (!ModsConfig.IsActive("Dubwise.Rimatomics")) return false;

            buildingHELType = AccessTools.TypeByName("Rimatomics.Building_HEL");
            if (buildingHELType == null) return false;

            var energyWeaponType = AccessTools.TypeByName("Rimatomics.Building_EnergyWeapon");
            if (energyWeaponType == null) return false;

            currentTargetField = AccessTools.Field(energyWeaponType, "currentTargetInt");
            beamAlphaField = AccessTools.Field(buildingHELType, "beamAlpha");
            burstWarmupField = AccessTools.Field(energyWeaponType, "burstWarmupTicksLeft");
            burstCooldownField = AccessTools.Field(energyWeaponType, "burstCooldownTicksLeft");
            powerCompField = AccessTools.Field(energyWeaponType, "powerComp");
            pulseSizeProperty = AccessTools.Property(energyWeaponType, "PulseSize");
            rangeProperty = AccessTools.Property(energyWeaponType, "Range");
            dissipateMethod = AccessTools.Method(energyWeaponType, "DissipateCharge");
            cooldownMethod = AccessTools.Method(energyWeaponType, "get_CooldownForShot");
            warmupMethod = AccessTools.Method(energyWeaponType, "get_WarmupForShot");

            var dubUtilsType = AccessTools.TypeByName("Rimatomics.DubUtils");
            if (dubUtilsType != null)
            {
                getResearchMethod = AccessTools.Method(dubUtilsType, "GetResearch");
                var researchType = AccessTools.TypeByName("Rimatomics.RimatomicsResearch");
                if (researchType != null)
                {
                    buggerMeField = AccessTools.Field(researchType, "BuggerMe");
                }
            }

            return currentTargetField != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("Rimatomics.Building_EnergyWeapon:Tick");
        }

        [HarmonyPostfix]
        public static void Postfix(Building __instance)
        {
            if (__instance.GetType() != buildingHELType) return;
            if (__instance.Map == null || __instance.Destroyed) return;

            int helId = __instance.thingIDNumber;

            if (!helStates.TryGetValue(helId, out HELState state))
            {
                state = new HELState();
                helStates[helId] = state;
            }

            LocalTargetInfo currentTarget = (LocalTargetInfo)currentTargetField.GetValue(__instance);
            if (currentTarget.IsValid && currentTarget.HasThing && currentTarget.Thing is Projectile && !(currentTarget.Thing is HoverVehicleProjectile))
            {
                state.target = null;
                return;
            }

            if (state.cooldownTicks > 0)
            {
                state.cooldownTicks--;
            }

            if (state.beamTicks > 0)
            {
                state.beamTicks--;
                if (beamAlphaField != null)
                {
                    beamAlphaField.SetValue(__instance, 1.0f);
                }
            }

            if (state.cooldownTicks > 0)
            {
                return;
            }

            if (powerCompField != null)
            {
                var powerComp = powerCompField.GetValue(__instance) as CompPowerTrader;
                if (powerComp == null || !powerComp.PowerOn)
                {
                    ClearTarget(__instance, state);
                    return;
                }
            }

            float range = 100f;
            if (rangeProperty != null)
            {
                try { range = (float)rangeProperty.GetValue(__instance); } catch { }
            }

            VehiclePawn bestTarget = FindBestHoverVehicle(__instance, range);

            if (bestTarget == null)
            {
                ClearTarget(__instance, state);
                return;
            }

            if (state.target != bestTarget)
            {
                state.target = bestTarget;
                
                float warmupSeconds = 1f;
                if (warmupMethod != null)
                {
                    try { warmupSeconds = (float)warmupMethod.Invoke(__instance, null); } catch { }
                }
                state.warmupTicks = GenTicks.SecondsToTicks(warmupSeconds);
                
                currentTargetField.SetValue(__instance, new LocalTargetInfo(bestTarget));
            }

            var topField = AccessTools.Field(__instance.GetType(), "top");
            if (topField != null)
            {
                var top = topField.GetValue(__instance);
                var targetInSightsProp = AccessTools.Property(top.GetType(), "TargetInSights");
                if (targetInSightsProp != null)
                {
                    bool inSights = (bool)targetInSightsProp.GetValue(top);
                    if (!inSights)
                    {
                        return;
                    }
                }
            }

            if (state.warmupTicks > 0)
            {
                state.warmupTicks--;
                
                if (state.warmupTicks == 59 || (state.warmupTicks > 0 && state.warmupTicks == GenTicks.SecondsToTicks(1f) - 1))
                {
                    try
                    {
                        var gunPropsMethod = AccessTools.Method(__instance.GetType(), "get_GunProps");
                        if (gunPropsMethod != null)
                        {
                            var gunProps = gunPropsMethod.Invoke(__instance, null);
                            var energyWepField = AccessTools.Field(gunProps.GetType(), "EnergyWep");
                            var energyWep = energyWepField.GetValue(gunProps);
                            var chargeSoundField = AccessTools.Field(energyWep.GetType(), "ChargeUpSound");
                            var chargeSound = (SoundDef)chargeSoundField.GetValue(energyWep);
                            chargeSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(__instance.Position, __instance.Map)));
                        }
                    }
                    catch { }
                }
                return;
            }

            bool godMode = Prefs.DevMode && DebugSettings.godMode;
            if (!godMode && dissipateMethod != null && pulseSizeProperty != null)
            {
                try
                {
                    float pulseSize = (float)pulseSizeProperty.GetValue(__instance);
                    bool hadCharge = (bool)dissipateMethod.Invoke(__instance, new object[] { pulseSize });
                    if (!hadCharge)
                    {
                        return; 
                    }
                }
                catch { return; }
            }

            state.beamTicks = 15;

            if (beamAlphaField != null)
            {
                beamAlphaField.SetValue(__instance, 1.0f);
            }

            ApplyDamage(bestTarget, __instance.Map);

            DestroyAssociatedDummy(bestTarget, __instance.Map);

            float cdSeconds = 3f;
            if (cooldownMethod != null)
            {
                try { cdSeconds = (float)cooldownMethod.Invoke(__instance, null); } catch { }
            }
            state.cooldownTicks = GenTicks.SecondsToTicks(cdSeconds);
            
            if (burstCooldownField != null)
            {
                burstCooldownField.SetValue(__instance, state.cooldownTicks);
            }

        }

        private static void ClearTarget(Building __instance, HELState state)
        {
            state.target = null;
            state.warmupTicks = 0;
            if (currentTargetField != null)
            {
                currentTargetField.SetValue(__instance, LocalTargetInfo.Invalid);
            }
            if (beamAlphaField != null)
            {
                beamAlphaField.SetValue(__instance, 0f);
            }
        }

        private static VehiclePawn FindBestHoverVehicle(Building __instance, float range)
        {
            var projDef = DefDatabase<ThingDef>.GetNamedSilentFail("VRF_HoverVehicleDummyProjectile");
            if (projDef == null) return null;

            VehiclePawn best = null;
            float bestDist = float.MaxValue;

            foreach (var thing in __instance.Map.listerThings.ThingsOfDef(projDef))
            {
                if (!(thing is HoverVehicleProjectile dummy)) continue;
                if (dummy.vehicle == null || !dummy.vehicle.Spawned) continue;

                bool isHostile = dummy.vehicle.Faction != null && dummy.vehicle.Faction.HostileTo(__instance.Faction);

                if (!isHostile)
                {
                    bool buggerMe = false;
                    if (getResearchMethod != null && buggerMeField != null)
                    {
                        try
                        {
                            object research = getResearchMethod.Invoke(null, null);
                            if (research != null)
                            {
                                buggerMe = (bool)buggerMeField.GetValue(research);
                            }
                        }
                        catch { }
                    }

                    if (!buggerMe)
                    {
                        continue;
                    }
                }

                float dist = dummy.Position.DistanceTo(__instance.Position);
                if (dist <= range && dist < bestDist)
                {
                    bestDist = dist;
                    best = dummy.vehicle;
                }
            }

            return best;
        }

        private static void ApplyDamage(VehiclePawn vehicle, Map map)
        {
            CellRect rect = vehicle.OccupiedRect();
            IntVec3 hitCell = rect.RandomCell;
            IntVec2 cell = new IntVec2(hitCell.x - vehicle.Position.x, hitCell.z - vehicle.Position.z);
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Bomb, 30f);
            vehicle.TakeDamage(dinfo, cell);

            if (vehicle.statHandler != null && !vehicle.statHandler.components.NullOrEmpty())
            {
                List<VehicleComponent> activeComponents = vehicle.statHandler.components
                    .Where(c => c.Health > 0)
                    .ToList();

                if (activeComponents.Count > 0)
                {
                    int componentsToDestroy = Math.Min(2, activeComponents.Count);
                    for (int i = 0; i < componentsToDestroy; i++)
                    {
                        int index = Rand.Range(0, activeComponents.Count);
                        VehicleComponent comp = activeComponents[index];
                        activeComponents.RemoveAt(index);

                        comp.SetHealth(0f);
                        Messages.Message("VRF_AntiAirInterceptComponent".Translate(vehicle.LabelShort, comp.props.label), MessageTypeDefOf.NegativeEvent);
                    }
                }
            }

            Vector3 drawPos = vehicle.DrawPos;
            FleckMaker.ThrowSmoke(drawPos, map, 1.5f);
            FleckMaker.ThrowMicroSparks(drawPos, map);
            FleckMaker.ThrowLightningGlow(drawPos, map, 1.5f);

            try
            {
                SoundDef.Named("Explosion_Stun")?.PlayOneShot(SoundInfo.InMap(new TargetInfo(vehicle.Position, map)));
            }
            catch { }
        }

        private static void DestroyAssociatedDummy(VehiclePawn vehicle, Map map)
        {
            var projDef = DefDatabase<ThingDef>.GetNamedSilentFail("VRF_HoverVehicleDummyProjectile");
            if (projDef == null) return;

            foreach (var thing in map.listerThings.ThingsOfDef(projDef).ToList())
            {
                if (thing is HoverVehicleProjectile dummy && dummy.vehicle == vehicle)
                {
                    dummy.skipStandardDamage = true;
                    if (dummy.Spawned)
                    {
                        dummy.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_Rimatomics_HEL_PreventShoot
    {
        public static bool Prepare()
        {
            return ModsConfig.IsActive("Dubwise.Rimatomics");
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("Rimatomics.Building_EnergyWeapon:TryStartShootSomething");
        }

        [HarmonyPrefix]
        public static bool Prefix(Building __instance)
        {
            var currentTargetField = AccessTools.Field(__instance.GetType(), "currentTargetInt");
            if (currentTargetField != null)
            {
                LocalTargetInfo currentTarget = (LocalTargetInfo)currentTargetField.GetValue(__instance);
                if (currentTarget.IsValid && currentTarget.HasThing && currentTarget.Thing is VehiclePawn)
                {
                    return false;
                }
            }
            return true;
        }
    }
}