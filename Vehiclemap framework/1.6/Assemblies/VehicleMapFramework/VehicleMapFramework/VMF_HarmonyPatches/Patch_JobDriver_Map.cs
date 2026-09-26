// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_Map
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriver_Map
{
  public static void Postfix(JobDriver __instance, ref Map __result)
  {
    if (__instance is JobDriver_Wait)
      return;
    Map map = ((GlobalTargetInfo) ref __instance.job.globalTarget).Map ?? TargetMapUtility.get_TargetMap((Thing) __instance.pawn);
    if (map == null)
      return;
    __result = map;
  }
}
