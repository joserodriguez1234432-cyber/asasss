// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (FloatMenuOptionProvider_ExtinguishFires), "GetSingleOption")]
[PatchLevel(Level.Sensitive)]
public static class Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    LocalBuilder localBuilder1;
    LocalBuilder localBuilder2;
    LocalBuilder localBuilder3;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (FloatMenuContext), "FirstSelectedPawn"))
    }).RemoveInstruction().SetInstruction(CodeInstruction.LoadField(typeof (FloatMenuContext), "map", false)).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (FloatMenuContext), "ClickedCell"))
    }).DeclareLocal(typeof (FloatMenuContext), ref localBuilder1).DeclareLocal(typeof (Pawn), ref localBuilder2).DeclareLocal(typeof (VirtualTeleporter?), ref localBuilder3).InsertAndAdvance(new CodeInstruction[8]
    {
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder1),
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder2),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1),
      PatchHelper.get_CallInstruction((Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption.\u003C\u003EO.\u003C0\u003E__Teleport ?? (Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption.\u003C\u003EO.\u003C0\u003E__Teleport = new Func<Pawn, FloatMenuContext, VirtualTeleporter?>(Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption.Teleport))).Method),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder3),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder1)
    }).MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Call), (object) null, (string) null)
    }).InsertAfter(new CodeInstruction[3]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder2),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder3),
      PatchHelper.get_CallInstruction((Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption.\u003C\u003EO.\u003C1\u003E__Dispose ?? (Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption.\u003C\u003EO.\u003C1\u003E__Dispose = new Action<Pawn, VirtualTeleporter?>(Patch_FloatMenuOptionProvider_ExtinguishFires_GetSingleOption.Dispose))).Method)
    }).InstructionEnumeration();
  }

  internal static VirtualTeleporter? Teleport(Pawn pawn, FloatMenuContext context)
  {
    return ((Thing) pawn).Map == context.map ? new VirtualTeleporter?() : new VirtualTeleporter?(new VirtualTeleporter((Thing) pawn, context.map));
  }

  internal static void Dispose(Pawn pawn, VirtualTeleporter? __state)
  {
    if (pawn != null)
      pawn.RemoveDepartMap();
    __state?.Dispose();
  }
}
