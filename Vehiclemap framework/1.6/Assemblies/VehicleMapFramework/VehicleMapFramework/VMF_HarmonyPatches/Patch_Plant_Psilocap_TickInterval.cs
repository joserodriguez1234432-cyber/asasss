// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Plant_Psilocap_TickInterval
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Plant_Psilocap), "TickInterval")]
[PatchLevel(Level.Cautious)]
public static class Patch_Plant_Psilocap_TickInterval
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_AllPawnsSpawned, MethodInfoCache.CachedMethodInfo.m_AllPawnsSpawned_Reverse);
  }
}
