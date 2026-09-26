using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Vehicles;

namespace VehicleRaidFramework
{
    public class JobDriver_VRF_Hotwire : JobDriver
    {
        private const int TicksPerCycle = 60;
        private const int TotalCycles = 5;

        private int cyclesDone = 0;

        private VehiclePawn Vehicle => job.targetA.Thing as VehiclePawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() =>
            {
                VehiclePawn v = Vehicle;
                if (v == null || !v.Spawned) return true;
                if (v.Faction != null && v.Faction.IsPlayer) return true;
                return VRF_HotwireUtility.VehicleHasNPCOccupants(v);
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil workToil = new Toil();
            workToil.defaultCompleteMode = ToilCompleteMode.Never;
            workToil.activeSkill = () => SkillDefOf.Crafting;
            workToil.handlingFacing = true;
            workToil.WithEffect(() => (Vehicle?.def as BuildableDef)?.repairEffect, TargetIndex.A);
            workToil.WithProgressBar(TargetIndex.A, () => (float)cyclesDone / TotalCycles);
            workToil.tickAction = () =>
            {
                pawn.skills?.Learn(SkillDefOf.Crafting, 0.11f);
                pawn.rotationTracker.FaceTarget(job.targetA);

                if (Find.TickManager.TicksGame % TicksPerCycle == 0)
                {
                    cyclesDone++;
                    if (cyclesDone >= TotalCycles)
                        ReadyForNextToil();
                }
            };
            workToil.AddFailCondition(() =>
            {
                VehiclePawn v = Vehicle;
                return v == null || v.Destroyed || !v.Spawned;
            });
            yield return workToil;

            Toil claimToil = ToilMaker.MakeToil();
            claimToil.initAction = () =>
            {
                VehiclePawn v = Vehicle;
                if (v == null || v.Destroyed) return;

                v.SetFaction(Faction.OfPlayer);

                Patch_VehicleNPCOnOff.UpdateVehiclePower(v);

                Lord lord = v.GetLord();
                lord?.RemovePawn(v);

                Messages.Message("VRF_HotwireSuccess".Translate(v.LabelShort), v, MessageTypeDefOf.PositiveEvent);
            };
            yield return claimToil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cyclesDone, "cyclesDone", 0);
        }
    }
}
