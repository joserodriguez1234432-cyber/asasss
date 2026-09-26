// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StoreUtility_TryFindBestBetterStoreCellFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (StoreUtility), "TryFindBestBetterStoreCellFor")]
[PatchLevel(Level.Safe)]
public static class Patch_StoreUtility_TryFindBestBetterStoreCellFor
{
  public static void Postfix(
    Thing t,
    Pawn carrier,
    Map map,
    StoragePriority currentPriority,
    Faction faction,
    ref IntVec3 foundCell,
    bool needAccurateResult,
    ref bool __result)
  {
    ((Thing) carrier).RemoveTargetInfo();
    StoragePriority currentPriority1 = ((IntVec3) ref foundCell).IsValid ? StoreUtility.GetSlotGroup(foundCell, map)?.Settings?.Priority ?? currentPriority : currentPriority;
    __result |= StoreAcrossMapsUtility.TryFindBestBetterStoreCellFor(t, carrier, map, currentPriority1, faction, ref foundCell, needAccurateResult);
    if (StoreAcrossMapsUtility.tmpDestMap == null)
      return;
    TargetMapUtility.set_TargetInfo((Thing) carrier, new TargetInfo(foundCell, StoreAcrossMapsUtility.tmpDestMap ?? map, false));
  }
}
