// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TradeUtility_LaunchThingsOfType
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

[HarmonyPatch(typeof (TradeUtility), "LaunchThingsOfType")]
[PatchLevel(Level.Sensitive)]
public static class Patch_TradeUtility_LaunchThingsOfType
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    FieldInfo f_Map_thingGrid = AccessTools.Field(typeof (Map), "thingGrid");
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Ldfld && CodeInstructionExtensions.OperandIs(c, (MemberInfo) f_Map_thingGrid))) - 1;
    list.RemoveAt(index);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[3]
    {
      CodeInstruction.LoadLocal(2, false),
      new CodeInstruction(OpCodes.Callvirt, (object) AccessTools.PropertyGetter(typeof (IEnumerator<Building_OrbitalTradeBeacon>), "Current")),
      new CodeInstruction(OpCodes.Callvirt, (object) MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
