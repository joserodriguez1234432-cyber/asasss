// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuMakerMap_GetOptions
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (FloatMenuMakerMap), "GetOptions")]
[PatchLevel(Level.Sensitive)]
public static class Patch_FloatMenuMakerMap_GetOptions
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls((Patch_FloatMenuMakerMap_GetOptions.\u003C\u003EO.\u003C0\u003E__InBounds ?? (Patch_FloatMenuMakerMap_GetOptions.\u003C\u003EO.\u003C0\u003E__InBounds = new Func<Vector3, Map, bool>(GenGrid.InBounds))).Method)
    }).Set(OpCodes.Call, (object) (Patch_FloatMenuMakerMap_GetOptions.\u003C\u003EO.\u003C1\u003E__InBounds ?? (Patch_FloatMenuMakerMap_GetOptions.\u003C\u003EO.\u003C1\u003E__InBounds = new Func<Vector3, Map, bool>(Patch_FloatMenuMakerMap_GetOptions.InBounds))).Method).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls((Patch_FloatMenuMakerMap_GetOptions.\u003C\u003EO.\u003C2\u003E__InBounds ?? (Patch_FloatMenuMakerMap_GetOptions.\u003C\u003EO.\u003C2\u003E__InBounds = new Func<IntVec3, Map, bool>(GenGrid.InBounds))).Method)
    }).Insert(new CodeInstruction[4]
    {
      new CodeInstruction(OpCodes.Pop, (object) null),
      CodeInstruction.LoadArgument(2, false),
      new CodeInstruction(OpCodes.Ldind_Ref, (object) null),
      CodeInstruction.LoadField(typeof (FloatMenuContext), "map", false)
    }).InstructionEnumeration();
  }

  private static bool InBounds(Vector3 clickPos, Map map)
  {
    return !VehicleMapUtility.get_IsVehicleMap(map) ? GenGrid.InBounds(clickPos, map) : clickPos.TryGetVehicleMap(map, out VehiclePawnWithMap _, VehicleMapFlag.None);
  }
}
