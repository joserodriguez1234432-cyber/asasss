// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapComponent_OgDHM_IsIgnored
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DoNotHitMe")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapComponent_OgDHM_IsIgnored
{
  public static void Prefix(ref MapComponent __instance, Thing thing, Map ___map)
  {
    if (thing.Map == null || thing.Map == ___map)
      return;
    __instance = thing.Map.GetComponent(__instance.GetType());
  }
}
