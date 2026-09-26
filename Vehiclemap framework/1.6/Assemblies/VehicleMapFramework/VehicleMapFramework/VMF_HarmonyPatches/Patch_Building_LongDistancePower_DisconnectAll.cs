// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Building_LongDistancePower_DisconnectAll
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_PowerPoles")]
[HarmonyPatch]
public static class Patch_Building_LongDistancePower_DisconnectAll
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(AccessTools.Method(ModCompat.PowerPoles.Building_LongDistancePower, "GetAllLinked", (Type[]) null, (Type[]) null), (Patch_Building_LongDistancePower_DisconnectAll.\u003C\u003EO.\u003C0\u003E__GetAllLinked ?? (Patch_Building_LongDistancePower_DisconnectAll.\u003C\u003EO.\u003C0\u003E__GetAllLinked = new Func<Building, bool, IEnumerable<Building>>(Patch_Building_LongDistancePower_GetAllLinked.GetAllLinked))).Method);
  }
}
