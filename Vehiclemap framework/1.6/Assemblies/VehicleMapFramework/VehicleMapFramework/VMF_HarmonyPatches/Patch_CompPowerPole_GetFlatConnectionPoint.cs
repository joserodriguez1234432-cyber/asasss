// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompPowerPole_GetFlatConnectionPoint
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PowerPoles")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_CompPowerPole_GetFlatConnectionPoint
{
  public static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) GenTypes.AllSubclasses(ModCompat.PowerPoles.Building_LongDistanceCabled).Select<Type, MethodInfo>((Func<Type, MethodInfo>) (t => AccessTools.DeclaredMethod(t, "GetFlatConnectionPoint", (Type[]) null, (Type[]) null))).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m != (MethodInfo) null));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseRotation);
  }

  public static void Postfix(Thing __instance, ref Vector2 __result)
  {
    VehiclePawnWithMap vehicle;
    if (!__instance.IsOnVehicleMapOf(out vehicle))
      return;
    __result = Vector2Utility.ToVector2(Ext_Math.RotatePoint(Vector2Utility.ToVector3(__result), __instance.DrawPos, -VehicleMapUtility.get_ExtraAngle((VehiclePawn) vehicle)));
  }
}
