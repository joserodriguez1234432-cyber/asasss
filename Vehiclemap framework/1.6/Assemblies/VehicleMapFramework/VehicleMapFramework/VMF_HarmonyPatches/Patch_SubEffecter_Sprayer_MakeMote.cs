// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SubEffecter_Sprayer_MakeMote
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (SubEffecter_Sprayer), "MakeMote")]
[PatchLevel(Level.Safe)]
public static class Patch_SubEffecter_Sprayer_MakeMote
{
  public static void Prefix(SubEffecter_Sprayer __instance, TargetInfo A, TargetInfo B)
  {
    MoteSpawnLocType effectiveSpawnLocType = ((SubEffecter) __instance).EffectiveSpawnLocType;
    if (effectiveSpawnLocType == null && ((TargetInfo) ref A).HasThing || effectiveSpawnLocType == 5 && ((TargetInfo) ref B).HasThing)
      return;
    VehiclePawnWithMapCache.CacheMode = true;
  }

  public static void Finalizer() => VehiclePawnWithMapCache.CacheMode = false;
}
