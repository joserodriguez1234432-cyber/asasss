// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WASDGameComponent_RenderPawn
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_WASDedPawn")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_WASDGameComponent_RenderPawn
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    FieldInfo fieldInfo = AccessTools.Field("wasdedPawn.WASDGameComponent:lasPos3");
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.LoadsField(fieldInfo, false)
    }).Repeat((Action<CodeMatcher>) (c => c.InsertAfterAndAdvance(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_ToNonFocusedThingMapCoord)
    })), (Action<string>) null).InstructionEnumeration();
  }
}
