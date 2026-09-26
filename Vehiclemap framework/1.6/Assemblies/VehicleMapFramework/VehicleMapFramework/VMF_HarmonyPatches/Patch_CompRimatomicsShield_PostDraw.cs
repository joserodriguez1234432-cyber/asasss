// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompRimatomicsShield_PostDraw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Rimatomics")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_CompRimatomicsShield_PostDraw
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    Label label1;
    LocalBuilder localBuilder;
    Label label2;
    return new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_IntVec3_ToVector3Shifted)
    }).CreateLabelWithOffsets(1, ref label1).DeclareLocal(typeof (VehiclePawnWithMap), ref localBuilder).InsertAfterAndAdvance(new CodeInstruction[7]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (ThingComp), "parent", false),
      new CodeInstruction(OpCodes.Ldloca_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label1),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToBaseMapCoord2)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_Altitudes_AltitudeFor)
    }).CreateLabelWithOffsets(1, ref label2).InsertAfter(new CodeInstruction[4]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label2),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_YOffsetFull)
    }).InstructionEnumeration();
  }
}
