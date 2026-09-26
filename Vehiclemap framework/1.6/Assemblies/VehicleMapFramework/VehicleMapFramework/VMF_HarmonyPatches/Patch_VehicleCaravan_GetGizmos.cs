// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleCaravan_GetGizmos
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleCaravan), "GetGizmos")]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleCaravan_GetGizmos
{
  public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, List<VehiclePawn> ___vehicles)
  {
    foreach (Gizmo gizmo in values)
    {
      VehiclePawn vehiclePawn;
      if (gizmo is Command_Action commandAction && ((Command) commandAction).defaultLabel == TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroup")) && (vehiclePawn = ___vehicles.FirstOrDefault<VehiclePawn>()) != null)
      {
        if (vehiclePawn.CompVehicleLauncher is CompVehicleLauncherWithMap compVehicleLauncher1)
        {
          gizmo.Disabled = false;
          string disableReason;
          if (!compVehicleLauncher1.CanLaunchWithCargoCapacityWithMap(out disableReason))
            gizmo.Disable(disableReason);
        }
        string disableReason1;
        if (!gizmo.Disabled && vehiclePawn.CompVehicleLauncher is CompVehicleLauncherGravshipVehicle compVehicleLauncher2 && !compVehicleLauncher2.CanLaunchGravship(out disableReason1, out Building_GravEngine _, out CompPilotConsole _, out Pawn _, out Pawn _))
          gizmo.Disable(disableReason1);
      }
      yield return gizmo;
    }
  }
}
