using Vehicles;

namespace VehicleRaidFramework
{
    public static class TDMS_Compatibility
    {
        public static bool IsAutonomousVehicle(VehiclePawn vehicle)
        {
            if (vehicle?.VehicleDef == null) return false;
            if (!vehicle.VehicleDef.defName.StartsWith("DMS_")) return false;

            if (vehicle.handlers != null)
            {
                foreach (var h in vehicle.handlers)
                {
                    if (h.role != null && (h.role.HandlingTypes & HandlingType.Movement) != 0)
                        return false;
                }
            }
            return true;
        }
    }
}
