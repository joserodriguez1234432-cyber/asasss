using Verse;
using UnityEngine;
using Vehicles;
using RimWorld;

namespace VehicleRaid
{
    public class HoverVehicleProjectile : Projectile
    {
        public VehiclePawn vehicle;
        private bool cleanDestroy = false;
        public bool skipStandardDamage = false;
        private int lastTickRan = -1;

        public override Vector3 ExactPosition
        {
            get
            {
                if (vehicle != null && vehicle.Spawned)
                {
                    Vector3 pos = vehicle.DrawPos;
                    pos.y = Altitudes.AltitudeFor(AltitudeLayer.Projectile);
                    return pos;
                }
                return base.ExactPosition;
            }
        }

        public override void Tick()
        {
            CustomTick();
        }

        public override void TickInterval(int delta)
        {
            CustomTick();
        }

        public override void ImpactSomething()
        {
            // The dummy projectile stays attached to the vehicle and should never impact
            // overhead mountain roofs, thin roofs, or fog in vanilla flight checks.
        }

        public void CustomTick()
        {
            int currentTick = Find.TickManager != null ? Find.TickManager.TicksGame : -1;
            if (currentTick >= 0 && currentTick == lastTickRan)
            {
                return;
            }
            lastTickRan = currentTick;

            if (vehicle == null || !vehicle.Spawned || vehicle.Map != this.Map)
            {
                cleanDestroy = true;
                this.Destroy(DestroyMode.Vanish);
                return;
            }

            var hoverComp = vehicle.GetComp<CompVehicleHover>();
            if (hoverComp == null || !hoverComp.IsAirborne)
            {
                cleanDestroy = true;
                this.Destroy(DestroyMode.Vanish);
                return;
            }

            if (this.Position != vehicle.Position)
            {
                this.Position = vehicle.Position;
            }

            Vector3 targetPos = vehicle.DrawPos;
            targetPos.y = Altitudes.AltitudeFor(AltitudeLayer.Projectile);
            this.destination = targetPos;
            this.origin = targetPos;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref vehicle, "vehicle");
            Scribe_Values.Look(ref cleanDestroy, "cleanDestroy", false);
            Scribe_Values.Look(ref skipStandardDamage, "skipStandardDamage", false);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = true;

            if (vehicle == null || !vehicle.Spawned) return;

            CellRect rect = vehicle.OccupiedRect();
            IntVec3 hitCell = rect.RandomCell;
            vehicle.TakeDamage(dinfo);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (blockedByShield && vehicle != null && vehicle.Spawned && !skipStandardDamage)
            {
                var hoverComp = vehicle.GetComp<CompVehicleHover>();
                if (hoverComp != null && hoverComp.IsAirborne)
                {
                    CellRect rect = vehicle.OccupiedRect();
                    IntVec3 hitCell = rect.RandomCell;
                    DamageInfo dinfo = new DamageInfo(DamageDefOf.Bomb, 20f);
                    vehicle.TakeDamage(dinfo);
                }
            }

            cleanDestroy = true;
            this.Destroy(DestroyMode.Vanish);
        }

        public void CleanDestroy()
        {
            cleanDestroy = true;
            if (this.Spawned)
                this.Destroy(DestroyMode.Vanish);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!cleanDestroy && vehicle != null && vehicle.Spawned && !skipStandardDamage)
            {
                // Do not damage the vehicle if destroyed due to overhead mountain, roof, fog, or despawn
                bool isUnderThickRoofOrFog = false;
                if (this.Map != null && this.Position.InBounds(this.Map))
                {
                    RoofDef roof = this.Map.roofGrid?.RoofAt(this.Position);
                    if (roof != null && (roof.isThickRoof || HoverRoofUtil.IsBlockingRoof(roof)))
                        isUnderThickRoofOrFog = true;
                    if (this.Map.fogGrid != null && this.Map.fogGrid.IsFogged(this.Position))
                        isUnderThickRoofOrFog = true;
                }

                if (!isUnderThickRoofOrFog)
                {
                    if (VehicleRaidFramework.VRF_Log.Enabled)
                        Log.Warning($"[VRF] Dummy projectile NON-CLEAN destroy at {this.Position}! Mode={mode}\n{System.Environment.StackTrace}");
                    var hoverComp = vehicle.GetComp<CompVehicleHover>();
                    if (hoverComp != null && hoverComp.IsAirborne && hoverComp.State != HoverState.Crashing)
                    {
                        CellRect rect = vehicle.OccupiedRect();
                        IntVec3 hitCell = rect.RandomCell;
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Bomb, 30f);
                        vehicle.TakeDamage(dinfo);
                    }
                }
            }

            base.Destroy(mode);
        }
    }
}
