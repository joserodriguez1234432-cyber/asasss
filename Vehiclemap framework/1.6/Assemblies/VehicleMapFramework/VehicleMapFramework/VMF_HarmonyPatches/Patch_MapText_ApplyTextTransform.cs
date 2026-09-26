// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapText_ApplyTextTransform
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TextTool")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_MapText_ApplyTextTransform
{
  public static void Postfix(Thing __instance, Vector2 screenCenter)
  {
    VehiclePawnWithMap vehicle;
    if (!__instance.IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    GUI.matrix = Matrix4x4.op_Multiply(GUI.matrix, Matrix4x4.op_Multiply(Matrix4x4.TRS(Vector2.op_Implicit(screenCenter), Quaternion.Euler(0.0f, 0.0f, VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle)), Vector3.one), Matrix4x4.TRS(Vector2.op_Implicit(Vector2.op_UnaryNegation(screenCenter)), Quaternion.identity, Vector3.one)));
  }
}
