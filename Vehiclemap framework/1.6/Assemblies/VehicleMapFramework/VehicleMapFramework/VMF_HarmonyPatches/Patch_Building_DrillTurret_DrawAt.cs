// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_DrillTurret_DrawAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_DrillTurret")]
[HarmonyPatch]
public static class Patch_Building_DrillTurret_DrawAt
{
  private static Vector3? overridePos;

  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method("DrillTurret.Building_DrillTurret:DrawAt", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method("DrillTurret.Building_DrillTurret:computeDrawingParameters", (Type[]) null, (Type[]) null);
  }

  [PatchLevel(Level.Safe)]
  public static void Prefix(Thing __instance)
  {
    VehiclePawnWithMap vehicle;
    if (__instance.IsOnNonFocusedVehicleMapOf(out vehicle))
    {
      IntVec3 position = __instance.Position;
      Patch_Building_DrillTurret_DrawAt.overridePos = new Vector3?(((IntVec3) ref position).ToVector3ShiftedWithAltitude(Altitudes.AltitudeFor((AltitudeLayer) 22)).ToBaseMapCoord(vehicle));
    }
    else
      Patch_Building_DrillTurret_DrawAt.overridePos = new Vector3?();
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method = (Patch_Building_DrillTurret_DrawAt.\u003C\u003EO.\u003C0\u003E__ToVector3Override ?? (Patch_Building_DrillTurret_DrawAt.\u003C\u003EO.\u003C0\u003E__ToVector3Override = new \u003C\u003EF\u007B00000001\u007D<IntVec3, float, Vector3>(Patch_Building_DrillTurret_DrawAt.ToVector3Override))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3ShiftedWithAltitude, method);
  }

  public static Vector3 ToVector3Override(ref IntVec3 c, float AddedAltitude)
  {
    return Patch_Building_DrillTurret_DrawAt.overridePos ?? ((IntVec3) ref c).ToVector3ShiftedWithAltitude(AddedAltitude);
  }
}
