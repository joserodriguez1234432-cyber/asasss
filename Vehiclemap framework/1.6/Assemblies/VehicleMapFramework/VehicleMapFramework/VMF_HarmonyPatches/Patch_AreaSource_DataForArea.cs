// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AreaSource_DataForArea
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (AreaSource), "DataForArea")]
[PatchLevel(Level.Safe)]
public static class Patch_AreaSource_DataForArea
{
  private static readonly AccessTools.FieldRef<PathFinderMapData, AreaSource> areas = AccessTools.FieldRefAccess<PathFinderMapData, AreaSource>(nameof (areas));

  public static void Prefix(ref AreaSource __instance, Area area, Map ___map)
  {
    Map map;
    if (area.Map == ___map || area.Map != (map = ___map.BaseMap()))
      return;
    __instance = Patch_AreaSource_DataForArea.areas.Invoke(map.pathFinder.MapData);
  }
}
