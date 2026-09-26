// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Avatar_GetBestTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PerspectiveShift")]
[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Avatar_GetBestTarget
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_GetThingList)
    }).SetOperandAndAdvance((object) MethodInfoCache.CachedMethodInfo.m_GetThingListAcrossMaps).Insert(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction((Patch_Avatar_GetBestTarget.\u003C\u003EO.\u003C0\u003E__RemoveMapVehicles ?? (Patch_Avatar_GetBestTarget.\u003C\u003EO.\u003C0\u003E__RemoveMapVehicles = new Func<List<Thing>, List<Thing>>(Patch_Avatar_GetBestTarget.RemoveMapVehicles))).Method)
    }).InstructionEnumeration();
  }

  private static List<Thing> RemoveMapVehicles(List<Thing> list)
  {
    list.RemoveAll((Predicate<Thing>) (t => t is VehiclePawnWithMap));
    return list;
  }
}
