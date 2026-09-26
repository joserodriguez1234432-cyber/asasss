using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    public static class HoverNPC_AirplaneCombatPlanner
    {
        private static FieldInfo _fi_rotationSpeed;
        private static FieldInfo _fi_ticksUntilAutoFire;
        private static bool      _reflectionCached = false;

        private static void CacheReflection()
        {
            if (_reflectionCached) return;
            _reflectionCached = true;
            Type tt = typeof(VehicleTurret);
            _fi_rotationSpeed     = AccessTools.Field(tt, "rotationSpeed")
                                 ?? AccessTools.Field(tt, "turretRotationSpeed");
            _fi_ticksUntilAutoFire = AccessTools.Field(tt, "ticksUntilAutoFire")
                                  ?? AccessTools.Field(tt, "autoFire");
        }

        public static Vector3 CalcEngagementPoint(
            VehiclePawn    vehicle,
            CompVehicleHover hoverComp,
            Thing          enemy)
        {
            if (vehicle == null || hoverComp == null || enemy == null) return Vector3.zero;
            if (hoverComp.FlightType != FlightType.Airplane)          return Vector3.zero;

            CacheReflection();

            TurretProfile best = GetBestTurretProfile(vehicle);

            float maxRange  = best.maxRange  > 0 ? best.maxRange  : (vehicle.CompVehicleTurrets?.MaxRange ?? 40f);
            float minRange  = best.minRange  > 0 ? best.minRange  : GetEffectiveMinRange(vehicle);
            float idealDist = Mathf.Clamp(maxRange * 0.65f, minRange + 4f, maxRange - 4f);

            float reloadTicks  = best.reloadTicks;
            float cooldownLeft = best.cooldownLeft;
            float totalWaitTicks = Mathf.Max(reloadTicks, cooldownLeft);

            float speedPerTick   = hoverComp.Props.hoverMoveSpeed / 60f;
            float travelDuringWait = speedPerTick * totalWaitTicks;

            Vector2 hoverPos  = hoverComp.realPos;
            Vector2 enemyPos2 = new Vector2(enemy.DrawPos.x, enemy.DrawPos.z);
            Vector2 toEnemy   = (enemyPos2 - hoverPos);
            float   distToEnemy = toEnemy.magnitude;

            float angleToEnemy = Mathf.Atan2(toEnemy.x, toEnemy.y) * Mathf.Rad2Deg;

            float rotSpeed     = hoverComp.Props.hoverRotationSpeed;
            float rotSpeedTick = rotSpeed / 60f;                   
            float angleDelta   = Mathf.Abs(Mathf.DeltaAngle(hoverComp.currentFlyAngle, angleToEnemy));
            float turnTicks    = rotSpeedTick > 0f ? angleDelta / rotSpeedTick : 0f;

            float turretAimTicks = CalcTurretAimTicks(vehicle, hoverComp, enemy, best);

            float engagementDelay = Mathf.Max(turnTicks, turretAimTicks, totalWaitTicks);

            float approachTravel = speedPerTick * engagementDelay;

            float orbitDist = Mathf.Clamp(
                idealDist + approachTravel * 0.4f,
                minRange  + 2f,
                maxRange  - 2f);

            Vector2 dir = toEnemy.normalized;
            if (dir.sqrMagnitude < 0.01f) dir = new Vector2(0f, 1f);

            Vector2 orbitCenter = enemyPos2 - dir * orbitDist;

            Map map = vehicle.Map;
            if (map != null)
            {
                float mg = 4f;
                orbitCenter.x = Mathf.Clamp(orbitCenter.x, mg, map.Size.x - mg);
                orbitCenter.y = Mathf.Clamp(orbitCenter.y, mg, map.Size.z - mg);
            }

            IntVec3 orbitCell = new IntVec3(Mathf.RoundToInt(orbitCenter.x - 0.5f), 0, Mathf.RoundToInt(orbitCenter.y - 0.5f));
            if (map != null && orbitCell.InBounds(map))
            {
                RoofDef roof = map.roofGrid.RoofAt(orbitCell);
                if (roof != null && HoverRoofUtil.IsBlockingRoof(roof))
                {
                    IntVec3 open = FindNearestOpenCell(orbitCell, map, 12);
                    if (open.IsValid) orbitCell = open;
                }
            }

            return orbitCell.ToVector3Shifted();
        }

        public static bool IsReadyToFire(
            VehiclePawn    vehicle,
            CompVehicleHover hoverComp,
            Thing          enemy)
        {
            if (vehicle == null || hoverComp == null || enemy == null) return false;

            CacheReflection();

            TurretProfile best = GetBestTurretProfile(vehicle);
            float maxRange = best.maxRange > 0 ? best.maxRange : (vehicle.CompVehicleTurrets?.MaxRange ?? 40f);
            float minRange = best.minRange > 0 ? best.minRange : GetEffectiveMinRange(vehicle);

            Vector2 hoverPos  = hoverComp.realPos;
            Vector2 enemyPos2 = new Vector2(enemy.DrawPos.x, enemy.DrawPos.z);
            float   dist      = Vector2.Distance(hoverPos, enemyPos2);

            if (dist < minRange || dist > maxRange) return false;

            IntVec3 hoverCell = new IntVec3(Mathf.RoundToInt(hoverPos.x - 0.5f), 0, Mathf.RoundToInt(hoverPos.y - 0.5f));
            if (!GenSight.LineOfSight(hoverCell, enemy.Position, vehicle.Map, true)) return false;

            float angleToEnemy  = Mathf.Atan2(enemyPos2.x - hoverPos.x, enemyPos2.y - hoverPos.y) * Mathf.Rad2Deg;
            float facingDelta   = Mathf.Abs(Mathf.DeltaAngle(hoverComp.currentFlyAngle, angleToEnemy));
            float rotTolerance  = Mathf.Max(hoverComp.Props.hoverRotationSpeed * 0.5f, 30f);
            if (facingDelta > rotTolerance) return false;

            if (best.cooldownLeft > 5) return false;

            return true;
        }

        private static float CalcTurretAimTicks(
            VehiclePawn      vehicle,
            CompVehicleHover hoverComp,
            Thing            enemy,
            TurretProfile    profile)
        {
            if (profile.turret == null) return 0f;
            if (_fi_rotationSpeed == null) return 0f;

            float turretRotSpeed = 0f;
            try { turretRotSpeed = Convert.ToSingle(_fi_rotationSpeed.GetValue(profile.turret)); }
            catch { return 0f; }

            if (turretRotSpeed <= 0f) return 0f;

            float turretAngle  = profile.turret.TurretRotation;
            Vector2 hoverPos   = hoverComp.realPos;
            Vector2 enemyPos2  = new Vector2(enemy.DrawPos.x, enemy.DrawPos.z);
            float   reqAngle   = Mathf.Atan2(enemyPos2.x - hoverPos.x, enemyPos2.y - hoverPos.y) * Mathf.Rad2Deg;

            float delta = Mathf.Abs(Mathf.DeltaAngle(turretAngle, reqAngle));
            return delta / turretRotSpeed * 60f;
        }

        private static TurretProfile GetBestTurretProfile(VehiclePawn vehicle)
        {
            var profile = new TurretProfile();
            var turretComp = vehicle.CompVehicleTurrets;
            if (turretComp?.turrets == null) return profile;

            float bestDps = -1f;
            foreach (VehicleTurret turret in turretComp.turrets)
            {
                if (turret == null) continue;

                float r = turret.MaxRange;
                if (r <= 0f) continue;

                float dmg    = turret.ProjectileDef?.projectile?.GetDamageAmount(null) ?? 10f;
                float reload = GetReloadTicks(turret);
                float dps    = reload > 0f ? dmg / reload : dmg;

                if (dps > bestDps)
                {
                    bestDps          = dps;
                    profile.turret   = turret;
                    profile.maxRange = r;
                    profile.minRange = turret.MinRange;
                    profile.reloadTicks  = reload;
                    profile.cooldownLeft = GetCooldownLeft(turret);
                }
            }

            return profile;
        }

        private static float GetReloadTicks(VehicleTurret turret)
        {
            try
            {
                var fi = AccessTools.Field(typeof(VehicleTurret), "MaxTicks")
                      ?? AccessTools.Field(typeof(VehicleTurret), "reloadTicks")
                      ?? AccessTools.Field(typeof(VehicleTurret), "maxTicks");
                if (fi != null) return Convert.ToSingle(fi.GetValue(turret));
            }
            catch { }

            var defFi = AccessTools.Field(typeof(VehicleTurret), "turretDef");
            if (defFi != null)
            {
                var tDef = defFi.GetValue(turret);
                if (tDef != null)
                {
                    var maxTicksFi = AccessTools.Field(tDef.GetType(), "maxTicks")
                                  ?? AccessTools.Field(tDef.GetType(), "reloadTimer")
                                  ?? AccessTools.Field(tDef.GetType(), "chargeTime");
                    if (maxTicksFi != null)
                        try { return Convert.ToSingle(maxTicksFi.GetValue(tDef)); } catch { }
                }
            }

            return 120f; 
        }

        private static float GetCooldownLeft(VehicleTurret turret)
        {
            if (_fi_ticksUntilAutoFire == null) return 0f;
            try { return Convert.ToSingle(_fi_ticksUntilAutoFire.GetValue(turret)); }
            catch { return 0f; }
        }

        private static float GetEffectiveMinRange(VehiclePawn vehicle)
        {
            var turretComp = vehicle.CompVehicleTurrets;
            if (turretComp?.turrets == null) return 0f;
            float max = 0f;
            foreach (var t in turretComp.turrets)
                if (t != null && t.MinRange > max) max = t.MinRange;
            return max;
        }

        private static IntVec3 FindNearestOpenCell(IntVec3 center, Map map, int radius)
        {
            foreach (IntVec3 c in GenRadial.RadialCellsAround(center, radius, false))
            {
                if (!c.InBounds(map)) continue;
                RoofDef r = map.roofGrid.RoofAt(c);
                if (r != null && HoverRoofUtil.IsBlockingRoof(r)) continue;
                if (!c.Standable(map)) continue;
                return c;
            }
            return IntVec3.Invalid;
        }

        private struct TurretProfile
        {
            public VehicleTurret turret;
            public float maxRange;
            public float minRange;
            public float reloadTicks;
            public float cooldownLeft;
        }
    }
}