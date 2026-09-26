// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CaravanInventoryUtility_GetOwnerOf
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CaravanInventoryUtility), "GetOwnerOf")]
[PatchLevel(Level.Safe)]
public static class Patch_CaravanInventoryUtility_GetOwnerOf
{
  public static bool Prefix(Thing item, ref Pawn __result)
  {
    VehiclePawnWithMap vehicle;
    if (!item.IsOnVehicleMapOf(out vehicle))
      return true;
    __result = (Pawn) vehicle;
    return false;
  }
}
