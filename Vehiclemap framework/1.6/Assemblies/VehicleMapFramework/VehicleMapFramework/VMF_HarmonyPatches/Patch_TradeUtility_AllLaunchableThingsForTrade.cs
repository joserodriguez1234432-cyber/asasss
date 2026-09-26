// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TradeUtility_AllLaunchableThingsForTrade
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_TradeUtility_AllLaunchableThingsForTrade
{
  [UsedImplicitly]
  public static Building_OrbitalTradeBeacon beacon;

  private static MethodInfo TargetMethod()
  {
    return AccessTools.Method(AccessTools.FirstInner(typeof (TradeUtility), (Func<Type, bool>) (t => t.Name.Contains("AllLaunchableThingsForTrade"))), "MoveNext", (Type[]) null, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_GetThingList = (Patch_TradeUtility_AllLaunchableThingsForTrade.\u003C\u003EO.\u003C0\u003E__GetThingList ?? (Patch_TradeUtility_AllLaunchableThingsForTrade.\u003C\u003EO.\u003C0\u003E__GetThingList = new Func<IntVec3, Map, List<Thing>>(GridsUtility.GetThingList))).Method;
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, m_GetThingList))
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_TradeUtility_AllLaunchableThingsForTrade.\u003C\u003EO.\u003C1\u003E__BuildingMap ?? (Patch_TradeUtility_AllLaunchableThingsForTrade.\u003C\u003EO.\u003C1\u003E__BuildingMap = new Func<Map, Map>(Patch_TradeUtility_AllLaunchableThingsForTrade.BuildingMap))).Method);
      }
      yield return instruction;
      if (instruction.opcode == OpCodes.Stloc_2)
      {
        yield return CodeInstruction.LoadLocal(2, false);
        yield return CodeInstruction.StoreField(typeof (Patch_TradeUtility_AllLaunchableThingsForTrade), "beacon");
      }
    }
  }

  private static Map BuildingMap(Map map)
  {
    return ((Thing) Patch_TradeUtility_AllLaunchableThingsForTrade.beacon)?.Map ?? map;
  }
}
