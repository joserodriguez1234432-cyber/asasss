// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenWorld_TileAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenWorld), "TileAt")]
[PatchLevel(Level.Safe)]
public static class Patch_GenWorld_TileAt
{
  public static void Prefix()
  {
    if (!WorldRendererUtility.DrawingMap)
      return;
    Map currentMap = Find.CurrentMap;
    if (currentMap == null || !VehicleMapUtility.get_IsVehicleMap(currentMap))
      return;
    ((Component) Find.WorldCamera)?.gameObject.SetActive(true);
  }
}
