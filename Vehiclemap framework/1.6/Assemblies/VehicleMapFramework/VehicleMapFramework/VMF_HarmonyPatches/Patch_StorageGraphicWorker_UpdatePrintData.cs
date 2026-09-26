// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StorageGraphicWorker_UpdatePrintData
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AdaptiveStorageFramework")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_StorageGraphicWorker_UpdatePrintData
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindLastIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_0)) + 1;
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[4]
    {
      CodeInstruction.LoadArgument(1, false),
      CodeInstruction.LoadLocal(0, false),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PrintExtraRotation),
      new CodeInstruction(OpCodes.Callvirt, (object) AccessTools.PropertySetter("AdaptiveStorage.PrintDatas.PrintData:ExtraRotation"))
    }));
    return (IEnumerable<CodeInstruction>) list.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_RotationForPrint);
  }
}
