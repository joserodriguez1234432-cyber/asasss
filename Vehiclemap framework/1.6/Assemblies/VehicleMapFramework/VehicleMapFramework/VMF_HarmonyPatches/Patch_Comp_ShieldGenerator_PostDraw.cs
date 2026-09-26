// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Comp_ShieldGenerator_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_EnergyShield")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Comp_ShieldGenerator_PostDraw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (CodeInstructionExtensions.Calls(instruction, MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3Shifted))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return CodeInstruction.LoadField(typeof (ThingComp), "parent", false);
        yield return new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToThingBaseMapCoord);
      }
    }
  }
}
