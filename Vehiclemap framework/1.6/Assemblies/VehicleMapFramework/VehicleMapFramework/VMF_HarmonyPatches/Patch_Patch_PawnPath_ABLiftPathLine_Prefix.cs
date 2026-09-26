// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Patch_PawnPath_ABLiftPathLine_Prefix
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AsAboveSoBelow")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_Patch_PawnPath_ABLiftPathLine_Prefix
{
  public static bool Prefix(Pawn pathingPawn, ref bool __result)
  {
    if (!VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) pathingPawn))
      return true;
    __result = true;
    return false;
  }
}
