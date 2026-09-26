// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_HaulDestinationManager_Notify_HaulDestinationChangedPriority
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (HaulDestinationManager), "Notify_HaulDestinationChangedPriority")]
[PatchLevel(Level.Mandatory)]
public static class Patch_HaulDestinationManager_Notify_HaulDestinationChangedPriority
{
  public static void Postfix(Map ___map)
  {
    ComponentCache.GetCachedMapComponent<CrossMapHaulDestinationManager>(___map).Notify_HaulDestinationChangedPriority();
  }
}
