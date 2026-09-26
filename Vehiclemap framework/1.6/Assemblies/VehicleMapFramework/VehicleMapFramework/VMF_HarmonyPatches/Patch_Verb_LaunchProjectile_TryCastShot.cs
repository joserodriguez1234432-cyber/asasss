// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_LaunchProjectile_TryCastShot
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Verb_LaunchProjectile), "TryCastShot")]
public static class Patch_Verb_LaunchProjectile_TryCastShot
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(Thing ___caster)
  {
    VehiclePawnWithMap vehicle;
    if (!___caster.IsOnVehicleMapOf(out vehicle) || ((Thing) vehicle).Spawned && vehicle.VehicleMap != Find.CurrentMap)
      return;
    VehiclePawnWithMapCache.CacheMode = true;
  }

  [PatchLevel(Level.Safe)]
  public static void Finalizer() => VehiclePawnWithMapCache.CacheMode = false;

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing), (MethodInfoCache.CachedMethodInfo.g_LocalTargetInfo_Cell, MethodInfoCache.CachedMethodInfo.m_CellOnBaseMapSpawned), (MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned));
  }
}
