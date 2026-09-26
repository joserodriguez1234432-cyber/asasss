// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_WorldObjects
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld.Planet;
using SmashTools.Patching;
using System.Reflection;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_WorldObjects : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CameraJumper), "GetAdjustedTarget", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldObjects), "GetAdjustedTargetForAerialVehicle", (System.Type[]) null));
  }

  private static void AerialVehicleInFlightAltimeter(ISelectable sel)
  {
    if (!(sel is AerialVehicleInFlight aerialVehicle))
      return;
    AltitudeMeter.DrawAltitudeMeter(aerialVehicle);
  }

  private static void GetAdjustedTargetForAerialVehicle(
    GlobalTargetInfo target,
    ref GlobalTargetInfo __result)
  {
    if (!((GlobalTargetInfo) ref target).HasThing || !(((GlobalTargetInfo) ref target).Thing.ParentHolder is VehicleRoleHandler parentHolder))
      return;
    AerialVehicleInFlight aerialVehicle = parentHolder.vehicle.GetAerialVehicle();
    if (aerialVehicle == null)
      return;
    __result = GlobalTargetInfo.op_Implicit((WorldObject) aerialVehicle);
  }
}
