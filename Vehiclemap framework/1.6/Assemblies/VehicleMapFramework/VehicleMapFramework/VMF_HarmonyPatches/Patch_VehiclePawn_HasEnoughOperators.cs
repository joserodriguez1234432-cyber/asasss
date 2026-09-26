// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehiclePawn_HasEnoughOperators
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Vehicles;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_VehiclePawn_HasEnoughOperators
{
  public static bool Prefix(VehiclePawn __instance, ref bool __result)
  {
    if (!(__instance is VehiclePawnWithMap))
      return true;
    if (VehicleMod.settings.debug.debugDraftAnyVehicle || (__instance.MovementPermissions & 2) != null)
    {
      __result = true;
      return false;
    }
    bool flag1 = false;
    bool flag2 = true;
    foreach (VehicleRoleHandler handler in __instance.handlers)
    {
      if ((handler.role.HandlingTypes & 1) != null)
      {
        flag1 = true;
        if (!handler.RoleFulfilled)
          flag2 = false;
      }
    }
    if (!flag1)
    {
      __result = false;
      return false;
    }
    __result = flag2;
    return false;
  }
}
