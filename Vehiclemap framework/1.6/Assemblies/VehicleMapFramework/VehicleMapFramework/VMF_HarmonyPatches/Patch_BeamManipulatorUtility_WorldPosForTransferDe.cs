// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamManipulatorUtility_WorldPosForTransferDestination
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_BeamManipulatorUtility_WorldPosForTransferDestination
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).End().MatchStartBackwards(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.Method("ManipulatorBeam.BeamManipulatorUtility:WorldPosForCell", (Type[]) null, (Type[]) null))
    }).InsertAfter(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(0, false),
      PatchHelper.get_CallInstruction((Patch_BeamManipulatorUtility_WorldPosForTransferDestination.\u003C\u003EO.\u003C0\u003E__ToBaseMapWorldPos ?? (Patch_BeamManipulatorUtility_WorldPosForTransferDestination.\u003C\u003EO.\u003C0\u003E__ToBaseMapWorldPos = new Func<Vector3, object, Vector3>(Patch_BeamManipulatorUtility_WorldPosForTransferDestination.ToBaseMapWorldPos))).Method)
    }).InstructionEnumeration();
  }

  public static Vector3 ToBaseMapWorldPos(Vector3 original, object transfer)
  {
    if (transfer != null)
    {
      Thing thing = ModCompat.ManipulatorBeamEmitter.thing.Invoke(transfer);
      if (thing != null)
      {
        Map targetMap = TargetMapUtility.get_TargetMap(thing);
        return targetMap == null ? thing.DrawPos : Vector3Utility.WithY(original.ToBaseMapCoord(targetMap), Altitudes.AltitudeFor((AltitudeLayer) 39));
      }
    }
    return original;
  }
}
