// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TravellingTransporters_Tick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (TravellingTransporters), "TickInterval")]
[PatchLevel(Level.Safe)]
public static class Patch_TravellingTransporters_Tick
{
  private static readonly AccessTools.FieldRef<TransportersArrivalAction_LandInSpecificCell, MapParent> mapParent = AccessTools.FieldRefAccess<TransportersArrivalAction_LandInSpecificCell, MapParent>(nameof (mapParent));

  public static void Postfix(TravellingTransporters __instance)
  {
    if (!(__instance.arrivalAction is TransportersArrivalAction_LandInSpecificCell arrivalAction) || !(Patch_TravellingTransporters_Tick.mapParent.Invoke(arrivalAction) is MapParent_Vehicle mapParentVehicle))
      return;
    __instance.destinationTile = ((WorldObject) mapParentVehicle).Tile;
  }
}
