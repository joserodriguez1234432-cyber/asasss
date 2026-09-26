// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StoreUtility_TryFindBestBetterStoreCellForWorker
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (StoreUtility), "TryFindBestBetterStoreCellForWorker")]
[PatchLevel(Level.Safe)]
public static class Patch_StoreUtility_TryFindBestBetterStoreCellForWorker
{
  public static bool Prefix(
    Thing t,
    Pawn carrier,
    Map map,
    Faction faction,
    ISlotGroup slotGroup,
    bool needAccurateResult,
    ref IntVec3 closestSlot,
    ref float closestDistSquared,
    ref StoragePriority foundPriority)
  {
    Map map1;
    switch (slotGroup?.Settings?.owner)
    {
      case StorageGroup storageGroup:
        map1 = storageGroup.Map;
        break;
      case IHaulDestination ihaulDestination:
        map1 = ihaulDestination.Map;
        break;
      case IHaulSource ihaulSource:
        map1 = ihaulSource.Map;
        break;
      default:
        map1 = (Map) null;
        break;
    }
    Map map2 = map1;
    if (map2 == null || map2 == map)
      return true;
    StoreAcrossMapsUtility.TryFindBestBetterStoreCellForWorker(t, carrier, map2, faction, slotGroup, needAccurateResult, ref closestSlot, ref closestDistSquared, ref foundPriority);
    return false;
  }
}
