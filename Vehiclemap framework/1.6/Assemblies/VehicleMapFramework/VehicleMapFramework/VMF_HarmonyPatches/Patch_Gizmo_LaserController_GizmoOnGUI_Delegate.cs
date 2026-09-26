// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Gizmo_LaserController_GizmoOnGUI_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_SRALib")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Gizmo_LaserController_GizmoOnGUI_Delegate
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(GenTypes.GetTypeInAnyAssembly("SRA.Gizmo_LaserController", "SRA"), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m =>
    {
      if (!m.Name.Contains("<GizmoOnGUI>"))
        return false;
      return m.CallsMethod((MethodBase) MethodInfoCache.CachedMethodInfo.g_Thing_Position, (MethodBase) MethodInfoCache.CachedMethodInfo.m_Roofed);
    }))));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMapSpawned).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Map)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_BaseMap_Thing).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls((Patch_Gizmo_LaserController_GizmoOnGUI_Delegate.\u003C\u003EO.\u003C0\u003E__Roofed ?? (Patch_Gizmo_LaserController_GizmoOnGUI_Delegate.\u003C\u003EO.\u003C0\u003E__Roofed = new Func<IntVec3, Map, bool>(GridsUtility.Roofed))).Method)
    }).Set(OpCodes.Call, (object) (Patch_Gizmo_LaserController_GizmoOnGUI_Delegate.\u003C\u003EO.\u003C1\u003E__RoofedAcrossMaps ?? (Patch_Gizmo_LaserController_GizmoOnGUI_Delegate.\u003C\u003EO.\u003C1\u003E__RoofedAcrossMaps = new Func<IntVec3, Map, bool>(VehicleMapUtility.RoofedAcrossMaps))).Method).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Thing_Position)
    }).Set(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_PositionOnBaseMap).InstructionEnumeration();
  }
}
