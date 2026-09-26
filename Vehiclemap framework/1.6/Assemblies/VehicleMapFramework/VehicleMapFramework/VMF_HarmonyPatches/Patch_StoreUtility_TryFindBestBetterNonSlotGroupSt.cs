// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StoreUtility_TryFindBestBetterNonSlotGroupStorageFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (StoreUtility), "TryFindBestBetterNonSlotGroupStorageFor")]
[PatchLevel(Level.Safe)]
public static class Patch_StoreUtility_TryFindBestBetterNonSlotGroupStorageFor
{
  public static void Postfix(
    Thing t,
    Pawn carrier,
    Map map,
    StoragePriority currentPriority,
    Faction faction,
    ref IHaulDestination haulDestination,
    bool acceptSamePriority,
    bool requiresDestReservation,
    ref bool __result)
  {
    StoragePriority storagePriority;
    if (haulDestination == null)
    {
      storagePriority = currentPriority;
    }
    else
    {
      StorageSettings parentStoreSettings = ((IStoreSettingsParent) haulDestination).GetParentStoreSettings();
      storagePriority = parentStoreSettings != null ? parentStoreSettings.Priority : currentPriority;
    }
    StoragePriority currentPriority1 = storagePriority;
    __result |= StoreAcrossMapsUtility.TryFindBestBetterNonSlotGroupStorageFor(t, carrier, map, currentPriority1, faction, ref haulDestination, acceptSamePriority, requiresDestReservation);
  }
}
