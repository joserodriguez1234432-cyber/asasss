// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_ThreatDisabled
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Pawn), "ThreatDisabled")]
[HarmonyAfter(new string[] {"SmashPhil.VehicleFramework"})]
[PatchLevel(Level.Safe)]
public static class Patch_Pawn_ThreatDisabled
{
  public static void Postfix(Pawn __instance, ref bool __result)
  {
    if (!__result || !(__instance is VehiclePawnWithMap vehiclePawnWithMap))
      return;
    __result = GenCollection.Empty<Pawn>(vehiclePawnWithMap.VehicleMap.mapPawns.FreeHumanlikesSpawnedOfFaction(((Thing) vehiclePawnWithMap).Faction));
  }
}
