// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_StoreUtility_TryFindBestBetterStorageFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (StoreUtility), "TryFindBestBetterStorageFor")]
public static class Patch_StoreUtility_TryFindBestBetterStorageFor
{
  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo m_GetSlotGroup = (Patch_StoreUtility_TryFindBestBetterStorageFor.\u003C\u003EO.\u003C0\u003E__GetSlotGroup ?? (Patch_StoreUtility_TryFindBestBetterStorageFor.\u003C\u003EO.\u003C0\u003E__GetSlotGroup = new Func<IntVec3, Map, SlotGroup>(StoreUtility.GetSlotGroup))).Method;
    FieldInfo f_tmpDestMap = AccessTools.Field(typeof (StoreAcrossMapsUtility), "tmpDestMap");
    foreach (CodeInstruction instruction in instructions)
    {
      if (CodeInstructionExtensions.Calls(instruction, m_GetSlotGroup))
      {
        Label label = generator.DefineLabel();
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) f_tmpDestMap);
        yield return new CodeInstruction(OpCodes.Brfalse_S, (object) label);
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) f_tmpDestMap);
        yield return CodeInstructionExtensions.WithLabels(instruction, new Label[1]
        {
          label
        });
      }
      else
        yield return instruction;
    }
  }

  [PatchLevel(Level.Safe)]
  public static void Postfix(Pawn carrier, IHaulDestination haulDestination, IntVec3 foundCell)
  {
    if (haulDestination?.Map == null)
      return;
    TargetMapUtility.set_TargetInfo((Thing) carrier, new TargetInfo(foundCell, haulDestination.Map, false));
  }
}
