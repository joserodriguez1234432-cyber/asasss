// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapDrawLayer_FinalizeMesh
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (MapDrawLayer), "FinalizeMesh")]
[PatchLevel(Level.Mandatory)]
public static class Patch_MapDrawLayer_FinalizeMesh
{
  public static void Prefix(
    MapDrawLayer __instance,
    MeshParts tags,
    Map ___map,
    List<LayerSubMesh> ___subMeshes)
  {
    if (!VehicleMapUtility.get_IsVehicleMap(___map) || (tags & 1) == null)
      return;
    bool flag = __instance is SectionLayer_TerrainOnVehicle;
    float num = flag ? Altitudes.AltitudeFor((AltitudeLayer) 5, -0.1f).YOffset() : 0.0f;
    foreach (LayerSubMesh subMesh in ___subMeshes)
    {
      for (int index = 0; index < subMesh.verts.Count; ++index)
      {
        Vector3 vert = subMesh.verts[index];
        if (flag)
          vert.y = num;
        else
          vert.y /= 39.9999962f;
        subMesh.verts[index] = vert;
      }
    }
  }
}
