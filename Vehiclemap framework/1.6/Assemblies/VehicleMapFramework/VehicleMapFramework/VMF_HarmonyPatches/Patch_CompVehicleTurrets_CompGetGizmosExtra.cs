// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompVehicleTurrets_CompGetGizmosExtra
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Vehicles;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (CompVehicleTurrets), "CompGetGizmosExtra")]
[PatchLevel(Level.Safe)]
public static class Patch_CompVehicleTurrets_CompGetGizmosExtra
{
  public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, CompVehicleTurrets __instance)
  {
    foreach (Gizmo gizmo in gizmos)
    {
      if (gizmo is Command_Turret commandTurret)
      {
        VehicleTurret turret = commandTurret.turret;
        if (turret is VehicleTurret_Manual && !((Gizmo) commandTurret).Disabled && !VehicleMod.settings.debug.debugShootAnyTurret && !GenCollection.Any<VehicleRoleHandler>(((VehicleComp) __instance).Vehicle.handlers, (Predicate<VehicleRoleHandler>) (h =>
        {
          if (!((Enum) (object) h.role.handlingTypes).HasFlag((Enum) (object) (HandlingType) 2))
            return false;
          List<string> turretIds = h.role.TurretIds;
          // ISSUE: explicit non-virtual call
          return turretIds != null && __nonvirtual (turretIds.Contains(!GenText.NullOrEmpty(turret.groupKey) ? turret.groupKey : turret.key));
        })))
          ((Gizmo) commandTurret).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_NoRoles", NamedArgument.op_Implicit(((Entity) ((VehicleComp) __instance).Vehicle).LabelShort))));
      }
      yield return gizmo;
    }
  }
}
