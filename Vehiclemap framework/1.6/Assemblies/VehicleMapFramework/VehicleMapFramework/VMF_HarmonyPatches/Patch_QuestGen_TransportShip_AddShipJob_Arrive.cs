// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_QuestGen_TransportShip_AddShipJob_Arrive
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Reflection.Emit;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (QuestGen_TransportShip), "AddShipJob_Arrive")]
[PatchLevel(Level.Cautious)]
public static class Patch_QuestGen_TransportShip_AddShipJob_Arrive
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Isinst), (object) typeof (PocketMapParent), (string) null)
    });
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch(new OpCode?(OpCodes.Brfalse_S), (object) null, (string) null)
    });
    object obj = codeMatcher.Operand;
    codeMatcher.InsertAfter(new CodeInstruction[3]
    {
      CodeInstruction.LoadLocal(0, false),
      CodeInstruction.LoadField(typeof (PocketMapParent), "sourceMap", false),
      new CodeInstruction(OpCodes.Brfalse_S, obj)
    });
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }
}
