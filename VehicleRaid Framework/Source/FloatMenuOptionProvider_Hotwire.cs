using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Vehicles;

namespace VehicleRaidFramework
{
    public class FloatMenuOptionProvider_Hotwire : FloatMenuOptionProvider
    {
        public override bool Drafted => true;

        public override bool Undrafted => true;

        public override bool Multiselect => false;

        public override bool RequiresManipulation => true;

        public override bool AppliesInt(FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;
            if (pawn == null || pawn.Dead || pawn.Downed) return false;
            if (!pawn.IsColonistPlayerControlled) return false;
            if (pawn.skills == null) return false;
            return true;
        }

        public override bool TargetThingValid(Thing thing, FloatMenuContext context)
        {
            if (!(thing is VehiclePawn vehicle)) return false;
            if (!vehicle.Spawned || vehicle.Destroyed) return false;
            if (vehicle.Faction == null || vehicle.Faction.IsPlayer) return false;
            if (!vehicle.Faction.HostileTo(Faction.OfPlayer)) return false;
            return true;
        }

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            if (!(clickedThing is VehiclePawn vehicle)) yield break;

            Pawn pawn = context.FirstSelectedPawn;
            int required = VRF_HotwireUtility.GetRequiredSkill(vehicle);
            int pawnSkill = pawn.skills.GetSkill(SkillDefOf.Crafting).Level;

            if (VRF_HotwireUtility.VehicleHasNPCOccupants(vehicle))
            {
                yield return new FloatMenuOption(
                    "VRF_HotwireOption_HasOccupants".Translate(vehicle.LabelShort),
                    null,
                    MenuOptionPriority.Default);
                yield break;
            }

            if (pawnSkill < required)
            {
                yield return new FloatMenuOption(
                    "VRF_HotwireOption_NeedSkill".Translate(vehicle.LabelShort, required),
                    null,
                    MenuOptionPriority.Default);
                yield break;
            }

            if (!pawn.CanReach(vehicle, PathEndMode.Touch, Danger.Deadly))
            {
                yield return new FloatMenuOption(
                    "VRF_HotwireOption_NoPath".Translate(vehicle.LabelShort),
                    null,
                    MenuOptionPriority.Default);
                yield break;
            }

            yield return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption(
                    "VRF_HotwireOption".Translate(vehicle.LabelShort),
                    () =>
                    {
                        JobDef jobDef = DefDatabase<JobDef>.GetNamed("VRF_HotwireVehicle", false);
                        if (jobDef == null) return;
                        Job job = JobMaker.MakeJob(jobDef, vehicle);
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    },
                    MenuOptionPriority.Default),
                pawn,
                new LocalTargetInfo(vehicle));
        }
    }
}
