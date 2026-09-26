using UnityEngine;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    public static class VRF_HoverSpeedDeriver
    {
        private const float HeliFactor_Move   = 0.55f;
        private const float HeliMin_Move      = 3.5f;
        private const float HeliMax_Move      = 12f;

        private const float HeliFactor_Rot    = 18f;
        private const float HeliMin_Rot       = 65f;
        private const float HeliMax_Rot       = 120f;

        private const float PlaneFactor_Move  = 0.50f;
        private const float PlaneMin_Move     = 7f;
        private const float PlaneMax_Move     = 30f;

        private const float PlaneFactor_Rot   = 4f;
        private const float PlaneMin_Rot      = 45f;
        private const float PlaneMax_Rot      = 80f;

        public static (float moveSpeed, float rotationSpeed) ForHelicopter(VehicleDef vdef)
        {
            float fs   = GetFlightSpeed(vdef);
            float move = Mathf.Clamp(fs * HeliFactor_Move, HeliMin_Move, HeliMax_Move);
            float rot  = Mathf.Clamp(move * HeliFactor_Rot, HeliMin_Rot, HeliMax_Rot);

            return (RoundTo(move, 0.5f), RoundTo(rot, 5f));
        }

        public static (float moveSpeed, float rotationSpeed) ForAirplane(VehicleDef vdef)
        {
            float fs   = GetFlightSpeed(vdef);
            float move = Mathf.Clamp(fs * PlaneFactor_Move, PlaneMin_Move, PlaneMax_Move);
            float rot  = Mathf.Clamp(move * PlaneFactor_Rot, PlaneMin_Rot, PlaneMax_Rot);

            return (RoundTo(move, 0.5f), RoundTo(rot, 5f));
        }

        public static (float moveSpeed, float rotationSpeed)? For(VehicleDef vdef)
        {
            if (vdef == null) return null;

            if (VRF_AerialVehicleClassifier.IsHelicopter(vdef))
                return ForHelicopter(vdef);

            if (VRF_AerialVehicleClassifier.IsAirplane(vdef))
                return ForAirplane(vdef);

            return null;
        }

        private static float GetFlightSpeed(VehicleDef vdef)
        {
            if (vdef == null) return 5f;
            float fs = vdef.GetStatValueAbstract(VehicleStatDefOf.FlightSpeed);
            return fs > 0f ? fs : 5f;
        }

        private static float RoundTo(float value, float step)
        {
            if (step <= 0f) return value;
            return Mathf.Round(value / step) * step;
        }
    }
}