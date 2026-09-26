// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LaunchProtocol_GetArrivalOptions_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_LaunchProtocol_GetArrivalOptions_Delegate
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) AccessTools.InnerTypes(typeof (LaunchProtocol)).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t => AccessToolsExtensions.GetDeclaredMethods(t).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name.Contains("<GetArrivalOptions>") && ((IEnumerable<ParameterInfo>) m.GetParameters()).Any<ParameterInfo>((Func<ParameterInfo, bool>) (p => p.ParameterType == typeof (LocalTargetInfo)))))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (instruction.opcode == OpCodes.Ldfld && ((FieldInfo) instruction.operand).FieldType == typeof (MapParent))
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_LaunchProtocol_GetArrivalOptions_Delegate.\u003C\u003EO.\u003C0\u003E__VehicleMapParentOrMe ?? (Patch_LaunchProtocol_GetArrivalOptions_Delegate.\u003C\u003EO.\u003C0\u003E__VehicleMapParentOrMe = new Func<MapParent, MapParent>(Patch_LaunchProtocol_GetArrivalOptions.VehicleMapParentOrMe))).Method);
      }
    }
  }
}
