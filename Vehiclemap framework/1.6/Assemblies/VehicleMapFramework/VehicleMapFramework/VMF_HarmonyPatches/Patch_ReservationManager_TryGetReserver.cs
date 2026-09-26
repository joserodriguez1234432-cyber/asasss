// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ReservationManager_TryGetReserver
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ReservationManager), "TryGetReserver")]
[PatchLevel(Level.Safe)]
public static class Patch_ReservationManager_TryGetReserver
{
  public static bool Prefix(ref ReservationManager __instance, Map ___map, LocalTargetInfo target)
  {
    Map mapHeld;
    if ((mapHeld = ((LocalTargetInfo) ref target).Thing?.MapHeld) == null || ___map == mapHeld)
      return true;
    __instance = mapHeld.reservationManager;
    return false;
  }
}
