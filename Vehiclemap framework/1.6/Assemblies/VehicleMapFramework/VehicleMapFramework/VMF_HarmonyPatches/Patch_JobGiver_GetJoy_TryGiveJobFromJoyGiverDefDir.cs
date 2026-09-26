// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_GetJoy_TryGiveJobFromJoyGiverDefDirect
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JobGiver_GetJoy), "TryGiveJobFromJoyGiverDefDirect")]
[PatchLevel(Level.Safe)]
public static class Patch_JobGiver_GetJoy_TryGiveJobFromJoyGiverDefDirect
{
  private static bool Prepare()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    return settings != null && settings.joyPatches;
  }

  public static void Postfix(Pawn pawn, Job __result)
  {
    Map mapHeld = __result != null ? ((LocalTargetInfo) ref __result.targetA).Thing?.MapHeld : (Map) null;
    if (mapHeld == null || mapHeld == ((Thing) pawn).Map || ((LocalTargetInfo) ref __result.targetB).HasThing)
      return;
    TargetMapUtility.set_TargetInfo((Thing) pawn, new TargetInfo(((LocalTargetInfo) ref __result.targetB).Cell, mapHeld, false));
  }
}
