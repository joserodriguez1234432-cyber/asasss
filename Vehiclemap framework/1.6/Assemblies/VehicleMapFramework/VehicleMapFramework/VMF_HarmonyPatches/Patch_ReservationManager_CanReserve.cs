// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ReservationManager_CanReserve
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ReservationManager), "CanReserve")]
[PatchLevel(Level.Safe)]
public static class Patch_ReservationManager_CanReserve
{
  public static bool Prefix(
    Map ___map,
    Pawn claimant,
    LocalTargetInfo target,
    int maxPawns,
    int stackCount,
    ReservationLayerDef layer,
    bool ignoreOtherReservations,
    ref bool __result)
  {
    Map map;
    if (Patch_ReservationManager_Reserve.ShouldReplace(___map, claimant, target, true, out map))
    {
      __result = claimant.CanReserve(target, maxPawns, stackCount, layer, ignoreOtherReservations, map);
      return false;
    }
    if (((Thing) claimant).Map == ___map)
      return true;
    __result = claimant.CanReserve(target, maxPawns, stackCount, layer, ignoreOtherReservations, ___map);
    return false;
  }
}
