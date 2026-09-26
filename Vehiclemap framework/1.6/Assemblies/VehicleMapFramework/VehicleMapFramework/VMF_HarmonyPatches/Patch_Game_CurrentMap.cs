// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Game_CurrentMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Game_CurrentMap
{
  public static bool ForceSet { get; set; }

  public static void Prefix(ref Map value)
  {
    if (Patch_Game_CurrentMap.ForceSet)
    {
      Patch_Game_CurrentMap.ForceSet = false;
    }
    else
    {
      VehiclePawnWithMap vehicle;
      if (!value.IsVehicleMapOf(out vehicle))
        return;
      if (ModCompat.CompatBase<ModCompat.MultiFloors>.Active)
        vehicle.CurrentLevel = value;
      if (((Thing) vehicle).Spawned)
      {
        value = ((Thing) vehicle).Map;
      }
      else
      {
        if (!VehicleMapFramework.VehicleMapFramework.settings.drawPlanet)
          return;
        Patch_Map_MapUpdate.lastRenderedTick = -1;
      }
    }
  }
}
