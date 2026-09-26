// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StorytellerUtility_DefaultThreatPointsNow
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (StorytellerUtility), "DefaultThreatPointsNow")]
[PatchLevel(Level.Cautious)]
public static class Patch_StorytellerUtility_DefaultThreatPointsNow
{
  public static bool Prefix(IIncidentTarget target, ref float __result)
  {
    if ((!(target is Map map) || !VehicleMapUtility.get_IsVehicleMap(map)) && !target.PlayerPawnsForStoryteller.Any<Pawn>((Func<Pawn, bool>) (p => p is VehiclePawnWithMap)))
      return true;
    __result = VehicleMapUtility.DefaultThreatPointsNowForMapVehicles(target);
    return false;
  }
}
