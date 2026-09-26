// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ReservationManager_FirstRespectedReserver
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ReservationManager), "FirstRespectedReserver")]
[PatchLevel(Level.Safe)]
public static class Patch_ReservationManager_FirstRespectedReserver
{
  public static void Prefix(
    ref ReservationManager __instance,
    Map ___map,
    LocalTargetInfo target,
    Pawn claimant)
  {
    Map map;
    if (!Patch_ReservationManager_Reserve.ShouldReplace(___map, claimant, target, false, out map))
      return;
    __instance = map.reservationManager;
  }
}
