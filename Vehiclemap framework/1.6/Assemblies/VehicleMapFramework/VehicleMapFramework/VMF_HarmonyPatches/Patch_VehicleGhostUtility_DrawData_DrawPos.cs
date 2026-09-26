// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleGhostUtility_DrawData_DrawPos
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[VFVersionalPatch]
[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
public static class Patch_VehicleGhostUtility_DrawData_DrawPos
{
  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("Vehicles.VehicleGhostUtility+DrawData", (string) null);
    FieldInfo fieldInfo = AccessTools.Field(typeInAnyAssembly, "rot");
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(fieldInfo, false)
    }).InsertAfterAndAdvance(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeInAnyAssembly, "vehicle", false),
      PatchHelper.get_CallInstruction(new Func<Rot8, VehiclePawn, Rot8>(BaseRot).Method)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(fieldInfo, false)
    }).InsertAfter(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction(new Func<Rot8, Rot8>(FocusedRot).Method)
    }).InstructionEnumeration();

    static Rot8 BaseRot(Rot8 rot, VehiclePawn vehicle)
    {
      VehiclePawnWithMap focusedVehicle = Command_FocusVehicleMap.FocusedVehicle;
      if (focusedVehicle != null)
        return rot.Rotated(focusedVehicle.FullRotation);
      Map map;
      VehiclePawnWithMap vehicle1;
      return ((Thing) vehicle).TryGetTargetMap(out map) && map.IsVehicleMapOf(out vehicle1) ? rot.Rotated(vehicle1.FullRotation) : rot;
    }

    static Rot8 FocusedRot(Rot8 rot)
    {
      VehiclePawnWithMap focusedVehicle = Command_FocusVehicleMap.FocusedVehicle;
      return focusedVehicle != null ? rot.Rotated(focusedVehicle.FullRotation) : rot;
    }
  }

  [PatchLevel(Level.Safe)]
  public static void Postfix(VehiclePawn ___vehicle, ref Vector3 __result)
  {
    if (___vehicle != null)
    {
      Map map;
      __result = ((Thing) ___vehicle).TryGetTargetMap(out map) ? Vector3Utility.WithY(__result.ToBaseMapCoord(map), __result.y) : __result;
    }
    else
    {
      VehiclePawnWithMap vehicle;
      if (Command_FocusVehicleMap.FocusedVehicle != null || !UI.MouseMapPosition().TryGetVehicleMap(Find.CurrentMap, out vehicle))
        return;
      __result = __result.ToBaseMapCoord(vehicle);
    }
  }
}
