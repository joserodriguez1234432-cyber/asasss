// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RGBMaterialPool_SetProperties
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (RGBMaterialPool), "SetProperties", new Type[] {typeof (IMaterialCacheTarget), typeof (PatternData), typeof (Func<Rot8, Texture2D>), typeof (Func<Rot8, Texture2D>)})]
[PatchLevel(Level.Mandatory)]
public static class Patch_RGBMaterialPool_SetProperties
{
  public static void Postfix(
    IMaterialCacheTarget target,
    Dictionary<IMaterialCacheTarget, Material[]> ___Cache)
  {
    GraphicOverlay graphicOverlay = target as GraphicOverlay;
    if (graphicOverlay == null)
      return;
    VehiclePawn vehicle = graphicOverlay.Vehicle;
    Material[] materialArray;
    if (vehicle == null || !((ThingWithComps) vehicle).AllComps.OfType<CompOpacityOverlay>().Any<CompOpacityOverlay>((Func<CompOpacityOverlay, bool>) (c => c.Props.identifier == graphicOverlay.data?.identifier)) || !___Cache.TryGetValue(target, out materialArray) || materialArray == null)
      return;
    foreach (Material material in materialArray)
    {
      if (material != null)
        material.shader = material.shader.OpacityShaderCorrespond();
    }
  }
}
