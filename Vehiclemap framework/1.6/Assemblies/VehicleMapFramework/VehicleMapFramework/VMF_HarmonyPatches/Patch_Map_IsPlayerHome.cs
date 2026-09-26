// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Map_IsPlayerHome
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Map_IsPlayerHome
{
  private static bool Prepare()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    return settings != null && settings.treatAsPlayerHome;
  }

  public static void Postfix(Map __instance, ref bool __result)
  {
    VehiclePawnWithMap vehicle;
    __result = __result || __instance.IsVehicleMapOf(out vehicle) && ((Thing) vehicle).Faction == Faction.OfPlayer;
  }
}
