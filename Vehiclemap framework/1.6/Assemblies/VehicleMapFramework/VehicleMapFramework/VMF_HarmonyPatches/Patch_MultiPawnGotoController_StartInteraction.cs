// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MultiPawnGotoController_StartInteraction
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MultiPawnGotoController), "StartInteraction")]
[PatchLevel(Level.Safe)]
public static class Patch_MultiPawnGotoController_StartInteraction
{
  public static void Prefix(ref IntVec3 mouseCell)
  {
    VehiclePawnWithMap vehicle;
    if (!UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None))
      return;
    mouseCell = mouseCell.ToBaseMapCoord(vehicle);
  }
}
