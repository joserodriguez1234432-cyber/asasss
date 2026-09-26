// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_QuestNode_GetMap_IsAcceptableMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (QuestNode_GetMap), "IsAcceptableMap")]
[PatchLevel(Level.Cautious)]
public static class Patch_QuestNode_GetMap_IsAcceptableMap
{
  private static bool Prepare()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    return settings != null && settings.treatAsPlayerHome;
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(AccessTools.PropertyGetter(typeof (Map), "IsPocketMap"), (Patch_QuestNode_GetMap_IsAcceptableMap.\u003C\u003EO.\u003C0\u003E__IsPocketMap ?? (Patch_QuestNode_GetMap_IsAcceptableMap.\u003C\u003EO.\u003C0\u003E__IsPocketMap = new Func<Map, bool>(Patch_QuestNode_GetMap_IsAcceptableMap.IsPocketMap))).Method);
  }

  private static bool IsPocketMap(Map map)
  {
    return map != null && map.IsPocketMap && !VehicleMapUtility.get_IsVehicleMap(map);
  }
}
