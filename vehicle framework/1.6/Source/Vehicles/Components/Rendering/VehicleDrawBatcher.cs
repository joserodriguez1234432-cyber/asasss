// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleDrawBatcher
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleDrawBatcher
{
  private static readonly Color32[] DefaultColors = new Color32[4]
  {
    new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
  };
  private static readonly Vector2[] DefaultUvs = new Vector2[4]
  {
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 1f),
    new Vector2(1f, 1f),
    new Vector2(1f, 0.0f)
  };
  private static readonly Vector2[] DefaultUvsFlipped = new Vector2[4]
  {
    new Vector2(1f, 0.0f),
    new Vector2(1f, 1f),
    new Vector2(0.0f, 1f),
    new Vector2(0.0f, 0.0f)
  };

  public static void Batch(
    MapDrawLayer layer,
    Vector3 center,
    Vector2 size,
    Material mat,
    float rot = 0.0f,
    bool flipUv = false,
    Vector2[] uvs = null,
    Color32[] colors = null,
    float topVerticesAltitudeBias = 0.01f,
    float uvzPayload = 0.0f)
  {
    if (colors == null)
      colors = VehicleDrawBatcher.DefaultColors;
    if (uvs == null)
      uvs = flipUv ? VehicleDrawBatcher.DefaultUvsFlipped : VehicleDrawBatcher.DefaultUvs;
    LayerSubMesh subMesh = layer.GetSubMesh(mat);
    int count = subMesh.verts.Count;
    subMesh.verts.Add(new Vector3(-0.5f * size.x, 0.0f, -0.5f * size.y));
    subMesh.verts.Add(new Vector3(-0.5f * size.x, topVerticesAltitudeBias, 0.5f * size.y));
    subMesh.verts.Add(new Vector3(0.5f * size.x, topVerticesAltitudeBias, 0.5f * size.y));
    subMesh.verts.Add(new Vector3(0.5f * size.x, 0.0f, -0.5f * size.y));
    if ((double) rot != 0.0)
    {
      float num1 = rot * ((float) Math.PI / 180f) * -1f;
      for (int index = 0; index < 4; ++index)
      {
        float x = subMesh.verts[count + index].x;
        float z = subMesh.verts[count + index].z;
        float num2 = Mathf.Cos(num1);
        float num3 = Mathf.Sin(num1);
        float num4 = (float) ((double) x * (double) num2 - (double) z * (double) num3);
        float num5 = (float) ((double) x * (double) num3 + (double) z * (double) num2);
        subMesh.verts[count + index] = new Vector3(num4, subMesh.verts[count + index].y, num5);
      }
    }
    for (int index1 = 0; index1 < 4; ++index1)
    {
      List<Vector3> verts = subMesh.verts;
      int num = count + index1;
      List<Vector3> vector3List = verts;
      int index2 = num;
      vector3List[index2] = Vector3.op_Addition(vector3List[index2], center);
      subMesh.uvs.Add(new Vector3(uvs[index1].x, uvs[index1].y, uvzPayload));
      subMesh.colors.Add(colors[index1]);
    }
    subMesh.tris.Add(count);
    subMesh.tris.Add(count + 1);
    subMesh.tris.Add(count + 2);
    subMesh.tris.Add(count);
    subMesh.tris.Add(count + 2);
    subMesh.tris.Add(count + 3);
  }
}
