// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_MaintenanceBay_DynamicDrawPhaseAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ExosuitFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Building_MaintenanceBay_DynamicDrawPhaseAt
{
  public static void Prefix(Building __instance, Pawn ___cachePawn)
  {
    if (___cachePawn == null)
      return;
    Pawn pawn = ___cachePawn;
    Rot4 rot4 = ((Thing) __instance).BaseRotationVehicleDraw();
    Rot4 opposite = ((Rot4) ref rot4).Opposite;
    ((Thing) pawn).Rotation = opposite;
  }
}
