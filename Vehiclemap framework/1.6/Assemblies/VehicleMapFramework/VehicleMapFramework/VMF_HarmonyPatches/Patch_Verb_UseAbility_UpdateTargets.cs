// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Verb_UseAbility_UpdateTargets
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_RimWorldOfMagic")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Verb_UseAbility_UpdateTargets
{
  private static readonly List<Thing> tmpList = new List<Thing>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, (ILGenerator) null);
    MethodInfo methodInfo = AccessTools.PropertyGetter(typeof (ListerThings), "AllThings");
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo)
    });
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.Repeat((Action<CodeMatcher>) (c => c.InsertAfterAndAdvance(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(0, false),
      PatchHelper.get_CallInstruction((Patch_Verb_UseAbility_UpdateTargets.\u003C\u003EO.\u003C0\u003E__AddThingList ?? (Patch_Verb_UseAbility_UpdateTargets.\u003C\u003EO.\u003C0\u003E__AddThingList = new Func<List<Thing>, Verb, List<Thing>>(Patch_Verb_UseAbility_UpdateTargets.AddThingList))).Method)
    })), (Action<string>) null);
    return (IEnumerable<CodeInstruction>) codeMatcher.Instructions();
  }

  private static List<Thing> AddThingList(List<Thing> list, Verb verb)
  {
    List<VehiclePawnWithMap> source = VehiclePawnWithMapCache.AllVehiclesOn(verb.caster.BaseMap());
    if (source.Count == 0)
      return list;
    Patch_Verb_UseAbility_UpdateTargets.tmpList.Clear();
    Patch_Verb_UseAbility_UpdateTargets.tmpList.AddRange((IEnumerable<Thing>) list);
    Patch_Verb_UseAbility_UpdateTargets.tmpList.AddRange(source.SelectMany<VehiclePawnWithMap, Thing>((Func<VehiclePawnWithMap, IEnumerable<Thing>>) (v => (IEnumerable<Thing>) v.VehicleMap.listerThings.AllThings)));
    return Patch_Verb_UseAbility_UpdateTargets.tmpList;
  }
}
