// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WatchBuildingUtility_TryFindBestWatchCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (WatchBuildingUtility), "TryFindBestWatchCell")]
[PatchLevel(Level.Safe)]
public static class Patch_WatchBuildingUtility_TryFindBestWatchCell
{
  private static bool Prepare()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    return settings != null && settings.joyPatches;
  }

  public static void Prefix(Thing toWatch, Pawn pawn, ref VirtualTeleporter? __state)
  {
    Map mapHeld = toWatch.MapHeld;
    if (mapHeld == null || mapHeld == ((Thing) pawn).Map)
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter((Thing) pawn, mapHeld));
  }

  public static void Finalizer(VirtualTeleporter? __state) => __state?.Dispose();
}
