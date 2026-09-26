// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriver_Ingest_ModifyCarriedThingDrawPosWorker
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JobDriver_Ingest), "ModifyCarriedThingDrawPosWorker")]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriver_Ingest_ModifyCarriedThingDrawPosWorker
{
  public static void Postfix(ref Vector3 drawPos, Pawn pawn, bool __result)
  {
    VehiclePawnWithMap vehicle;
    if (!__result || !((Thing) pawn).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    drawPos = Vector3Utility.WithY(drawPos.ToBaseMapCoord(vehicle), drawPos.y);
  }
}
