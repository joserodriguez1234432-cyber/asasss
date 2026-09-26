// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapGridOwners_PathConfig_MatchesReachability
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MapGridOwners.PathConfig), "Vehicles.IPathConfig.MatchesReachability")]
[PatchLevel(Level.Mandatory)]
public static class Patch_MapGridOwners_PathConfig_MatchesReachability
{
  public static void Postfix(VehicleDef ___vehicleDef, IPathConfig other, ref bool __result)
  {
    ref bool local = ref __result;
    int num;
    if (__result && !IsUniqueVehicleDefPlaceholder(___vehicleDef) && other is MapGridOwners.PathConfig)
    {
      MapGridOwners.PathConfig pathConfig = (MapGridOwners.PathConfig) other;
      num = !IsUniqueVehicleDefPlaceholder(ModCompat.VehicleFramework.vehicleDef.Invoke(ref pathConfig)) ? 1 : 0;
    }
    else
      num = 0;
    local = num != 0;

    static bool IsUniqueVehicleDefPlaceholder(VehicleDef def)
    {
      VehicleMapProps_Unique modExtension = ((Def) def).GetModExtension<VehicleMapProps_Unique>();
      return modExtension != null && modExtension.baseDef != null;
    }
  }
}
