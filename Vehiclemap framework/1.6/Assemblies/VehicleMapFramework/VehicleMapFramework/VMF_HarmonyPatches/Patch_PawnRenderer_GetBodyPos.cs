// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PawnRenderer_GetBodyPos
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (PawnRenderer), "GetBodyPos")]
[PatchLevel(Level.Safe)]
public static class Patch_PawnRenderer_GetBodyPos
{
  public static void Postfix(PawnPosture posture, Pawn ___pawn, ref Vector3 __result)
  {
    Corpse corpse = ___pawn.Corpse;
    if (corpse != null && ((Thing) corpse).IsOnNonFocusedVehicleMapOf(out VehiclePawnWithMap _))
    {
      ((Thing) corpse).TryGetDrawPos(ref __result);
    }
    else
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) ___pawn).IsOnNonFocusedVehicleMapOf(out vehicle))
        return;
      if (RestUtility.CurrentBed(___pawn) != null)
      {
        __result = Vector3Utility.WithYOffset(__result.ToBaseMapCoord(vehicle), -0.0240384638f);
      }
      else
      {
        if (posture == null)
          return;
        __result = Vector3Utility.WithYOffset(__result.YOffsetFull(vehicle), 0.08f);
      }
    }
  }
}
