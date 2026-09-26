// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompShip_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_TraderShips")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompShip_PostDraw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.LoadsConstant(instruction, 0.0))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_CompShip_PostDraw.\u003C\u003EO.\u003C0\u003E__Rotation ?? (Patch_CompShip_PostDraw.\u003C\u003EO.\u003C0\u003E__Rotation = new Func<ThingComp, float>(Patch_CompShip_PostDraw.Rotation))).Method);
      }
      else
        yield return instruction;
    }
  }

  private static float Rotation(ThingComp comp)
  {
    Rot8 rot8 = ((Thing) comp.parent).BaseFullRotationDoor();
    return ((Rot8) ref rot8).AsAngle;
  }
}
