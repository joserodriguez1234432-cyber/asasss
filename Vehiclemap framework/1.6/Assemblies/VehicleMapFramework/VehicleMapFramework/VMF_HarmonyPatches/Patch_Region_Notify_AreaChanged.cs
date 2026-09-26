// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Region_Notify_AreaChanged
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Region), "Notify_AreaChanged")]
[PatchLevel(Level.Safe)]
public static class Patch_Region_Notify_AreaChanged
{
  public static void Postfix(Area a)
  {
    if (!(a is Area_Allowed))
      return;
    CrossMapReachabilityCache.ClearCacheFor(a.Map);
  }
}
