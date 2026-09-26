// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuOptionProvider_OrderVehicle_VehicleCanGoto
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (FloatMenuOptionProvider_OrderVehicle), "VehicleCanGoto")]
[PatchLevel(Level.Safe)]
public static class Patch_FloatMenuOptionProvider_OrderVehicle_VehicleCanGoto
{
  public static bool Prefix(VehiclePawn vehicle, IntVec3 gotoLoc, ref AcceptanceReport __result)
  {
    Map map;
    if (!((Thing) vehicle).TryGetTargetMap(out map) || ((Thing) vehicle).Map == map)
      return true;
    __result = vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(gotoLoc), (PathEndMode) 1, (Danger) 3, (TraverseMode) 0, map, out TargetInfo _, out TargetInfo _) ? AcceptanceReport.op_Implicit(true) : AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotMoveToCell", NamedArgument.op_Implicit(((Entity) vehicle).LabelCap)));
    return false;
  }
}
