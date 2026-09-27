using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Vehicles;

namespace VehicleRaidFramework
{
    /// <summary>
    /// Safe compatibility layer for Vehicle Map Framework (OELS.VehicleMapFramework).
    /// Prevents ReflectionTypeLoadException and CS0400 when Vehicle Map Framework is not referenced or installed.
    /// </summary>
    public static class VRF_VehicleMapCompat
    {
        public const string VMF_PackageId = "OELS.VehicleMapFramework";

        private static bool? _isVMFActive;
        public static bool IsVMFActive
        {
            get
            {
                if (!_isVMFActive.HasValue)
                {
                    _isVMFActive = ModsConfig.IsActive(VMF_PackageId) ||
                                   AccessTools.TypeByName("VehicleMapFramework.VehiclePawnWithMap") != null;
                }
                return _isVMFActive.Value;
            }
        }

        public static bool IsVehicleWithMap(Thing thing)
        {
            if (thing == null) return false;
            return IsVehicleWithMapType(thing.GetType());
        }

        public static bool IsVehicleWithMapType(Type t)
        {
            if (t == null) return false;
            while (t != null && t != typeof(object))
            {
                if (t.FullName == "VehicleMapFramework.VehiclePawnWithMap") return true;
                t = t.BaseType;
            }
            return false;
        }

        public static bool IsVehicleMapParent(MapParent parent)
        {
            if (parent == null) return false;
            Type t = parent.GetType();
            while (t != null && t != typeof(object))
            {
                if (t.FullName == "VehicleMapFramework.MapParent_Vehicle") return true;
                t = t.BaseType;
            }
            return false;
        }

        public static bool IsVehicleRoleBuildable(object role)
        {
            if (role == null) return false;
            Type t = role.GetType();
            while (t != null && t != typeof(object))
            {
                if (t.FullName == "VehicleMapFramework.VehicleRoleBuildable") return true;
                t = t.BaseType;
            }
            return t.Name.IndexOf("Buildable", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsVehicleRoleHandlerBuildable(object handler)
        {
            if (handler == null) return false;
            Type t = handler.GetType();
            while (t != null && t != typeof(object))
            {
                if (t.FullName == "VehicleMapFramework.VehicleRoleHandlerBuildable") return true;
                t = t.BaseType;
            }
            return false;
        }

        public static Map GetInteriorVehicleMap(VehiclePawn vehicle)
        {
            if (vehicle == null) return null;
            try
            {
                var prop = vehicle.GetType().GetProperty("VehicleMap");
                return prop?.GetValue(vehicle, null) as Map;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsVehicleMap(Map map, out VehiclePawn vehiclePawn)
        {
            vehiclePawn = null;
            if (map == null) return false;
            try
            {
                Type vmuType = AccessTools.TypeByName("VehicleMapFramework.VehicleMapUtility");
                if (vmuType != null)
                {
                    var method = vmuType.GetMethod("IsVehicleMapOf", new Type[] { typeof(Map), typeof(VehiclePawn).MakeByRefType() });
                    if (method != null)
                    {
                        object[] args = new object[] { map, null };
                        bool res = (bool)method.Invoke(null, args);
                        vehiclePawn = args[1] as VehiclePawn;
                        return res;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
