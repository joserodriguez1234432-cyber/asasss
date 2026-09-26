// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CastPositionFinder_TryFindCastPosition
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (CastPositionFinder), "TryFindCastPosition")]
[PatchLevel(Level.Safe)]
public static class Patch_CastPositionFinder_TryFindCastPosition
{
  public static bool Prefix(CastPositionRequest newReq, ref IntVec3 dest, ref bool __result)
  {
    if (((Thing) newReq.caster).Map == newReq.target.MapHeld || ((Thing) newReq.caster).BaseMap() != newReq.target.MapHeldBaseMap())
      return true;
    __result = CastPositionFinderOnVehicle.TryFindCastPosition(newReq, out dest);
    return false;
  }
}
