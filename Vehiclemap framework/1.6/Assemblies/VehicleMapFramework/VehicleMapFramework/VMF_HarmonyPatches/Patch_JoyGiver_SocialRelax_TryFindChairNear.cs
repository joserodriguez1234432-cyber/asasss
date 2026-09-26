// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JoyGiver_SocialRelax_TryFindChairNear
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JoyGiver_SocialRelax), "TryFindChairNear")]
[PatchLevel(Level.Cautious)]
public static class Patch_JoyGiver_SocialRelax_TryFindChairNear
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo from = AccessTools.Method(typeof (GridsUtility), "GetEdifice", (Type[]) null, (Type[]) null);
    MethodInfo to = AccessTools.Method(typeof (GridsUtility), "GetEdificeSafe", (Type[]) null, (Type[]) null);
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, to);
  }
}
