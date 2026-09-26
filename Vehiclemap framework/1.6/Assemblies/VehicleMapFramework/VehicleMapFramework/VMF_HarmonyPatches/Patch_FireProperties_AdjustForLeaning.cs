// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FireProperties_AdjustForLeaning
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AvoidFriendlyFire")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_FireProperties_AdjustForLeaning
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    FieldInfo fieldInfo = AccessTools.Field("AvoidFriendlyFire.FireProperties:Origin");
    MethodInfo methodInfo1 = AccessTools.PropertyGetter("AvoidFriendlyFire.FireProperties:Caster");
    MethodInfo methodInfo2 = AccessTools.PropertyGetter("AvoidFriendlyFire.FireProperties:CasterMap");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    CodeMatch codeMatch = CodeMatch.Calls((Patch_FireProperties_AdjustForLeaning.\u003C\u003EO.\u003C0\u003E__LeanShootingSourcesFromTo ?? (Patch_FireProperties_AdjustForLeaning.\u003C\u003EO.\u003C0\u003E__LeanShootingSourcesFromTo = new Action<IntVec3, IntVec3, Map, List<IntVec3>>(ShootLeanUtility.LeanShootingSourcesFromTo))).Method);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      codeMatch
    }).MatchStartBackwards(new CodeMatch[1]
    {
      CodeMatch.LoadsField(fieldInfo, false)
    }).SetAndAdvance(OpCodes.Callvirt, (object) methodInfo1).Insert(new CodeInstruction[1]
    {
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo2)
    }).SetAndAdvance(OpCodes.Callvirt, (object) methodInfo1).Insert(new CodeInstruction[1]
    {
      PatchHelper.get_CallvirtInstruction(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).MatchStartForward(new CodeMatch[1]{ codeMatch }).Set(OpCodes.Call, (object) (Patch_FireProperties_AdjustForLeaning.\u003C\u003EO.\u003C1\u003E__LeanShootingSourcesFromTo ?? (Patch_FireProperties_AdjustForLeaning.\u003C\u003EO.\u003C1\u003E__LeanShootingSourcesFromTo = new Action<IntVec3, IntVec3, Map, List<IntVec3>>(ShootLeanUtilityOnVehicle.LeanShootingSourcesFromTo))).Method).InstructionEnumeration();
  }
}
