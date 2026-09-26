// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimRenderer_DrawSingle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_AnimRenderer_DrawSingle
{
  public static Func<object, Vector3> f_RootPositionOffset;

  public static Vector3 RootPositionOffset(object instance)
  {
    return Patch_AnimRenderer_DrawSingle.f_RootPositionOffset(instance);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo from = AccessTools.PropertyGetter(AccessTools.TypeByName("AM.AnimRenderer"), "RootPosition");
    FastInvokeHandler f_RootPosition = MethodInvoker.GetHandler(from, false);
    Vector3 result = new Vector3();
    Patch_AnimRenderer_DrawSingle.f_RootPositionOffset = (Func<object, Vector3>) (instance => result = (Vector3) f_RootPosition.Invoke(instance, Array.Empty<object>()));
    VehiclePawnWithMap vehicle;
    Patch_AnimRenderer_DrawSingle.f_RootPositionOffset += (Func<object, Vector3>) (instance => ModCompat.MeleeAnimation.AnimRenderer_Map.Invoke(instance).IsNonFocusedVehicleMapOf(out vehicle) && ModCompat.MeleeAnimation.AnimRenderer_cellData.Invoke(ModCompat.MeleeAnimation.AnimRenderer_Def.Invoke(instance)).Count > 0 ? result.ToBaseMapCoord(vehicle) : result);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_AnimRenderer_DrawSingle.\u003C\u003EO.\u003C0\u003E__RootPositionOffset ?? (Patch_AnimRenderer_DrawSingle.\u003C\u003EO.\u003C0\u003E__RootPositionOffset = new Func<object, Vector3>(Patch_AnimRenderer_DrawSingle.RootPositionOffset))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, method);
  }
}
