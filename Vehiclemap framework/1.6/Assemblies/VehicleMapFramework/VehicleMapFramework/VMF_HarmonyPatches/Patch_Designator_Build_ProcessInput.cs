// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Designator_Build_ProcessInput
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Designator_Build), "ProcessInput")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Designator_Build_ProcessInput
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    MethodInfo g_Count = AccessTools.PropertyGetter(typeof (List<Thing>), "Count");
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(c, (MemberInfo) g_Count)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[4]
    {
      CodeInstruction.LoadArgument(0, false),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Designator_Map),
      CodeInstruction.LoadLocal(4, false),
      PatchHelper.get_CallInstruction((Patch_Designator_Build_ProcessInput.\u003C\u003EO.\u003C0\u003E__AddThingList ?? (Patch_Designator_Build_ProcessInput.\u003C\u003EO.\u003C0\u003E__AddThingList = new Func<List<Thing>, Map, ThingDef, List<Thing>>(Patch_ItemAvailability_ThingsAvailableAnywhere.AddThingList))).Method)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
