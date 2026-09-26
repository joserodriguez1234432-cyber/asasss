using System.Linq;
using Vehicles;

namespace VehicleRaidFramework
{
    public static class VRF_AerialVehicleClassifier
    {

        public static bool IsHelicopter(VehicleDef vdef)
        {
            if (vdef == null || vdef.type != VehicleType.Air) return false;

            var launcher = GetLauncherProps(vdef);
            if (launcher == null) return false;

            if (!HasVerticalTakeoff(launcher)) return false;

            if (!HasControlInFlight(launcher)) return false;

            if (IsSingleNodeVehicle(launcher)) return false;

            return true;
        }

        public static bool IsAirplane(VehicleDef vdef)
        {
            if (vdef == null || vdef.type != VehicleType.Air) return false;

            var launcher = GetLauncherProps(vdef);
            if (launcher == null) return false;

            return HasRunwayRestriction(launcher);
        }

        public static bool IsSiegePod(VehicleDef vdef)
        {
            if (vdef == null || vdef.type != VehicleType.Air) return false;

            var launcher = GetLauncherProps(vdef);
            if (launcher == null) return false;

            bool singleNode  = IsSingleNodeVehicle(launcher);
            bool noControl   = !HasControlInFlight(launcher);
            bool noRunway    = !HasRunwayRestriction(launcher);
            bool noVertLift  = !HasVerticalTakeoff(launcher);

            if (singleNode) return true;

            if (noControl && noRunway && noVertLift) return true;

            return false;
        }

        public static string GetAerialCategory(VehicleDef vdef)
        {
            if (vdef == null || vdef.type != VehicleType.Air) return "NotAerial";
            if (IsHelicopter(vdef)) return "Helicopter";
            if (IsAirplane(vdef))   return "Airplane";
            if (IsSiegePod(vdef))   return "SiegePod";
            return "AerialUnknown";
        }

        private static CompProperties_VehicleLauncher GetLauncherProps(VehicleDef vdef)
        {
            var comps = vdef?.comps;
            if (comps == null) return null;
            for (int i = 0; i < comps.Count; i++)
            {
                if (comps[i] is CompProperties_VehicleLauncher launcher)
                    return launcher;
            }
            return null;
        }

        private static bool HasVerticalTakeoff(CompProperties_VehicleLauncher launcher)
        {
            if (launcher.launchProtocol == null) return false;

            return launcher.launchProtocol is PropellerTakeoff
                || (launcher.launchProtocol is VTOLTakeoff && launcher.launchProtocol is not PropellerTakeoff);
        }

        private static bool HasRunwayRestriction(CompProperties_VehicleLauncher launcher)
        {
            if (launcher.launchProtocol == null) return false;

            if (launcher.launchProtocol is DirectionalTakeoff directional)
            {
                bool horzRunway = directional.launchProperties?.horizontal?.restriction is LaunchRestriction_Runway;
                bool vertRunway = directional.launchProperties?.vertical?.restriction   is LaunchRestriction_Runway;
                return horzRunway || vertRunway;
            }

            if (launcher.launchProtocol is DefaultTakeoff defaultTakeoff)
                return defaultTakeoff.launchProperties?.restriction is LaunchRestriction_Runway;

            try { return launcher.launchProtocol.LaunchProperties?.restriction is LaunchRestriction_Runway; }
            catch { return false; }
        }

        private static bool HasControlInFlight(CompProperties_VehicleLauncher launcher)
        {
            return launcher.controlInFlight;
        }

        private static bool IsSingleNodeVehicle(CompProperties_VehicleLauncher launcher)
        {
            if (launcher.launchProtocol == null) return false;
            return launcher.launchProtocol.MaxFlightNodes == 1;
        }
    }
}