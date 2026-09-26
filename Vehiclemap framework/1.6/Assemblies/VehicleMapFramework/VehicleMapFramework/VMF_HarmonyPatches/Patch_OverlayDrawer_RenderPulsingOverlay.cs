// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_OverlayDrawer_RenderPulsingOverlay
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (OverlayDrawer), "RenderPulsingOverlay", new Type[] {typeof (Thing), typeof (Material), typeof (int), typeof (Mesh), typeof (bool)})]
public static class Patch_OverlayDrawer_RenderPulsingOverlay
{
  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_BaseFullRotation_Thing), (MethodInfoCache.CachedMethodInfo.g_Rot4_AsVector2, MethodInfoCache.CachedMethodInfo.m_AsFundVector2));
  }
}
