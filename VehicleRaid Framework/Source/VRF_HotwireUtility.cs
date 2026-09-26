using System;
using RimWorld;
using Verse;
using Vehicles;

namespace VehicleRaidFramework
{
    public static class VRF_HotwireUtility
    {
        public const int BaseSkillRequired = 5;
        public const int MaxSkillRequired = 20;
        public const float CombatPointsPerSkillLevel = 500f;

        public static int GetRequiredSkill(VehiclePawn vehicle)
        {
            float combatPower = GetVehicleCombatPower(vehicle);
            int bonus = (int)Math.Floor(combatPower / CombatPointsPerSkillLevel);
            int required = BaseSkillRequired + bonus;
            return Math.Min(required, MaxSkillRequired);
        }

        public static float GetVehicleCombatPower(VehiclePawn vehicle)
        {
            if (vehicle?.def == null) return 0f;

            foreach (PawnKindDef kind in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (kind.race == vehicle.def)
                    return kind.combatPower;
            }
            return 0f;
        }

        public static bool VehicleHasNPCOccupants(VehiclePawn vehicle)
        {
            if (vehicle?.handlers == null) return false;
            foreach (VehicleRoleHandler handler in vehicle.handlers)
            {
                foreach (Pawn p in handler.thingOwner)
                {
                    if (p != null && !p.Dead)
                        return true;
                }
            }
            return false;
        }

        public static bool CanHotwire(Pawn pawn, VehiclePawn vehicle, out string failReason)
        {
            failReason = null;

            if (vehicle == null || !vehicle.Spawned || vehicle.Destroyed)
            {
                failReason = "VRF_HotwireFail_NoVehicle".Translate();
                return false;
            }

            if (vehicle.Faction == null || vehicle.Faction.IsPlayer)
            {
                failReason = "VRF_HotwireFail_AlreadyOwned".Translate();
                return false;
            }

            if (VehicleHasNPCOccupants(vehicle))
            {
                failReason = "VRF_HotwireFail_HasOccupants".Translate();
                return false;
            }

            int required = GetRequiredSkill(vehicle);
            int pawnSkill = pawn.skills?.GetSkill(SkillDefOf.Crafting)?.Level ?? 0;
            if (pawnSkill < required)
            {
                failReason = "VRF_HotwireFail_SkillRequired".Translate(required);
                return false;
            }

            return true;
        }
    }
}
