// Decompiled with JetBrains decompiler
// Type: SmashTools.PawnOverlayRenderer
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace SmashTools;

public static class PawnOverlayRenderer
{
  public static IEnumerable<CodeInstruction> ShowBodyTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo heldPawnPropertyGetter = AccessTools.PropertyGetter(typeof (IThingHolderWithDrawnPawn), "HeldPawnDrawPos_Y");
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, heldPawnPropertyGetter) && instructionList[i - 1].operand is LocalBuilder operand && operand.LocalIndex == 11)
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) 11);
        yield return new CodeInstruction(OpCodes.Ldarg_3, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (PawnOverlayRenderer), "GetShowBody", (Type[]) null, (Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  public static void GetShowBody(
    IThingHolderWithDrawnPawn thingHolderWithDrawnPawn,
    ref bool showBody)
  {
    if (!(thingHolderWithDrawnPawn is IThingHolderPawnOverlayer holderPawnOverlayer))
      return;
    showBody = holderPawnOverlayer.ShowBody;
  }

  public static bool LayingFacing(Pawn ___pawn, ref Rot4 __result)
  {
    if (!(((Thing) ___pawn).ParentHolder is IThingHolderPawnOverlayer parentHolder))
      return true;
    __result = parentHolder.PawnRotation;
    return false;
  }
}
