// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VerbUtility_ThingsToHit
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (VerbUtility), "ThingsToHit")]
[PatchLevel(Level.Cautious)]
public static class Patch_VerbUtility_ThingsToHit
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.m_GetThingList, MethodInfoCache.CachedMethodInfo.m_GetThingListAcrossMaps);
  }
}
