// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PawnRenderer_BodyAngle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (PawnRenderer), "BodyAngle")]
[PatchLevel(Level.Safe)]
public static class Patch_PawnRenderer_BodyAngle
{
  public static void Postfix(Pawn ___pawn, ref float __result)
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) ___pawn).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    __result = Ext_Math.RotateAngle(__result, VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
  }
}
