// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WeatherEventMaker_WeatherEventMakerTick
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (WeatherEventMaker), "WeatherEventMakerTick")]
[PatchLevel(Level.Safe)]
public static class Patch_WeatherEventMaker_WeatherEventMakerTick
{
  public static void Prefix(Map map, ref float strength)
  {
    if (!map.IsVehicleMapOf(out VehiclePawnWithMap _))
      return;
    strength *= (float) map.Area / 40000f;
  }
}
