// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_GravEngine_DeSpawn
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Odyssey")]
[HarmonyPatch(typeof (Building_GravEngine), "DeSpawn")]
[PatchLevel(Level.Safe)]
public static class Patch_Building_GravEngine_DeSpawn
{
  public static void Prefix(Building_GravEngine __instance)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) __instance).IsOnVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned || GravshipVehicleUtility.GravshipProcessInProgress)
      return;
    IntVec3 loc = ((Thing) __instance).Position;
    Rot4 rot = ((Thing) __instance).Rotation;
    LongEventHandler.QueueLongEvent((Action) (() => GravshipVehicleUtility.PlaceGravshipVehicleUnSpawned(__instance, loc, rot, vehicle, true)), TaggedString.op_Implicit(Translator.Translate("VMF_GravshipVehicleDestroyed")), false, (Action<Exception>) null, false, false, (Action) null);
  }
}
