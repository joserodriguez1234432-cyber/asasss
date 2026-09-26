// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.Ext_Mesh
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#nullable disable
namespace SmashTools.Rendering;

[PublicAPI]
internal static class Ext_Mesh
{
  [MustUseReturnValue]
  public static Mesh BakeLineRendererMesh(List<LineSegment> segments)
  {
    if (segments == null)
      throw new ArgumentNullException(nameof (segments), "Line segments cannot be null.");
    if (segments.Count == 0)
      throw new ArgumentException("Line segments cannot be empty.", nameof (segments));
    Mesh mesh1 = new Mesh();
    ((Object) mesh1).name = "Line segments mesh";
    Mesh mesh2 = mesh1;
    try
    {
      Ext_Mesh.RecalculateLineRenderer(mesh2, segments);
      return mesh2;
    }
    catch
    {
      Object.Destroy((Object) mesh2);
      throw;
    }
  }

  public static void RecalculateLineRenderer([NotNull] Mesh mesh, List<LineSegment> segments)
  {
    if (!Object.op_Implicit((Object) mesh))
      throw new ArgumentNullException(nameof (mesh), "Mesh cannot be null.");
    if (segments == null)
      throw new ArgumentNullException(nameof (segments), "Line segments cannot be null.");
    int num = segments.Count != 0 ? segments.Count : throw new ArgumentException("Line segments cannot be empty.", nameof (segments));
    int length = num + 1;
    if (length >= (int) ushort.MaxValue)
      mesh.indexFormat = (IndexFormat) 1;
    Vector3[] vector3Array = new Vector3[length];
    int[] numArray = new int[length];
    Color[] colorArray = new Color[length];
    vector3Array[0] = segments[0].from;
    colorArray[0] = segments[0].color;
    numArray[0] = 0;
    for (int index1 = 0; index1 < num; ++index1)
    {
      LineSegment segment = segments[index1];
      int index2 = index1 + 1;
      vector3Array[index2] = segment.to;
      colorArray[index2] = segment.color;
      numArray[index2] = index2;
    }
    mesh.Clear();
    mesh.SetVertices(vector3Array);
    mesh.SetIndices(numArray, (MeshTopology) 4, 0);
    mesh.SetColors(colorArray);
    mesh.RecalculateBounds();
  }
}
