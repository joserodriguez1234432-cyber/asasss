using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Vehicles;

namespace VehicleRaidFramework
{
    public class JobDriver_VRF_RepairVehicle : JobDriver
    {
        private const int TicksPerRepairCycle = 60;
        private int workTicks = 0;

        private VehiclePawn Vehicle => job.targetA.Thing as VehiclePawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);

            Toil gotoToil = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            yield return gotoToil;

            Toil repairToil = new Toil();
            repairToil.defaultCompleteMode = ToilCompleteMode.Never;
            repairToil.WithEffect(() => (Vehicle?.def as BuildableDef)?.repairEffect, TargetIndex.A);
            repairToil.activeSkill = () => SkillDefOf.Construction;
            repairToil.handlingFacing = true;
            repairToil.tickAction = () =>
            {
                Pawn actor = repairToil.actor;
                VehiclePawn vehicle = Vehicle;

                if (vehicle == null || vehicle.Destroyed || !vehicle.Spawned)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                if (vehicle.Faction != actor.Faction)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                if (vehicle.vehiclePather != null && vehicle.vehiclePather.Moving)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                workTicks++;
                if (workTicks < TicksPerRepairCycle)
                    return;

                workTicks = 0;

                bool anyDamaged = false;
                foreach (var component in vehicle.statHandler.ComponentsPrioritized)
                {
                    if (component.HealthPercent < 1f)
                    {
                        anyDamaged = true;
                        float repairRate = vehicle.GetStatValue(VehicleStatDefOf.RepairRate);
                        component.HealComponent(repairRate);
                        break;
                    }
                }

                if (!anyDamaged)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };

            repairToil.AddFailCondition(() =>
            {
                VehiclePawn v = Vehicle;
                return v == null || v.Destroyed || !v.Spawned;
            });

            yield return repairToil;
        }
    }
}
