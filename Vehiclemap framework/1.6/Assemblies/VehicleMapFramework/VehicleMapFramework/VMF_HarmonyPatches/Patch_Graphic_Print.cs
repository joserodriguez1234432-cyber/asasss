// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_Print
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Graphic), "Print")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Graphic_Print
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_3)) - 1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[3]
    {
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_RotateForPrintNegate),
      CodeInstruction.LoadArgument(2, false),
      PatchHelper.get_CallInstruction((Patch_Graphic_Print.\u003C\u003EO.\u003C0\u003E__EdgeSpacerOffset ?? (Patch_Graphic_Print.\u003C\u003EO.\u003C0\u003E__EdgeSpacerOffset = new Func<Vector3, Thing, Vector3>(Patch_Graphic_Print.EdgeSpacerOffset))).Method)
    }));
    return (IEnumerable<CodeInstruction>) list.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_RotationForPrint);
  }

  private static Vector3 EdgeSpacerOffset(Vector3 vector, Thing thing)
  {
    CompVehicleEnterSpot comp = ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(thing);
    VehiclePawnWithMap vehicle;
    if (comp != null && comp.ShouldOffsetOnEdge && thing.IsOnVehicleMapOf(out vehicle))
    {
      VehicleMapProps vehicleMapProps = vehicle.VehicleMapProps;
      if (vehicleMapProps != null)
      {
        Rot4 rotation = thing.Rotation;
        Rot4 opposite = ((Rot4) ref rotation).Opposite;
        return Vector3.op_Addition(vector, Vector3.op_Multiply(Vector2Utility.ToVector3(((Rot4) ref opposite).AsVector2), vehicleMapProps.EdgeSpaceValue(Rot8.op_Implicit(VehicleSectionLayerManager.RotForPrint), opposite)));
      }
    }
    return vector;
  }
}
