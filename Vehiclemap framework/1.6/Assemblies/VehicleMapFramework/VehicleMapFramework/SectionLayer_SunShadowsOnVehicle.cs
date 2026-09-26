// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SectionLayer_SunShadowsOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class SectionLayer_SunShadowsOnVehicle : SectionLayer
{
  private readonly SectionLayer subLayer;
  internal static readonly Type t_SectionLayer_SunShadows = GenTypes.GetTypeInAnyAssembly("Verse.SectionLayer_SunShadows", "Verse");
  private static readonly Color32 LowVertexColor = new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0);

  public SectionLayer_SunShadowsOnVehicle(Section section)
    : base(section)
  {
    this.subLayer = (SectionLayer) Activator.CreateInstance(SectionLayer_SunShadowsOnVehicle.t_SectionLayer_SunShadows, (object) section);
    ((MapDrawLayer) this).relevantChangeTypes = ((MapDrawLayer) this.subLayer).relevantChangeTypes;
  }

  public void DrawLayer(Vector3 drawPos, float extraRotation)
  {
    if (!((MapDrawLayer) this).Visible)
      return;
    Quaternion quaternion = Quaternion.AngleAxis(extraRotation, Vector3.up);
    for (int index = 0; index < ((MapDrawLayer) this.subLayer).subMeshes.Count; ++index)
    {
      LayerSubMesh subMesh = ((MapDrawLayer) this.subLayer).subMeshes[index];
      if (subMesh.finalized && !subMesh.disabled)
        Graphics.DrawMesh(subMesh.mesh, drawPos, quaternion, subMesh.material, subMesh.renderLayer);
    }
  }

  public virtual void Regenerate()
  {
    if (!((MapDrawLayer) this).Map.IsVehicleMapOf(out VehiclePawnWithMap _))
      return;
    ((MapDrawLayer) this.subLayer).Regenerate();
    LayerSubMesh subMesh = ((MapDrawLayer) this.subLayer).GetSubMesh(MatBases.SunShadow);
    if (Rot4.op_Inequality(VehicleSectionLayerManager.RotForPrint, Rot4.North))
    {
      this.CastShadowNorth(subMesh);
      for (int index = 0; index < subMesh.verts.Count; ++index)
        subMesh.verts[index] = Vector3Utility.RotatedBy(subMesh.verts[index], VehicleSectionLayerManager.RotForPrint);
    }
    VehicleSectionLayerManager.FinalizeVerts(this.subLayer);
    subMesh.mesh.SetTriangles(subMesh.tris, 0);
    subMesh.mesh.SetColors(subMesh.colors);
  }

  private void CastShadowNorth(LayerSubMesh subMesh)
  {
    if (!MatBases.SunShadow.shader.isSupported)
      return;
    Building[] innerArray = ((MapDrawLayer) this).Map.edificeGrid.InnerArray;
    float num = Altitudes.AltitudeFor((AltitudeLayer) 13);
    CellRect cellRect = this.section.CellRect;
    CellIndices cellIndices = ((MapDrawLayer) this).Map.cellIndices;
    for (int minX = cellRect.minX; minX <= cellRect.maxX; ++minX)
    {
      for (int minZ = cellRect.minZ; minZ <= cellRect.maxZ; ++minZ)
      {
        Building building1 = innerArray[((CellIndices) ref cellIndices).CellToIndex(minX, minZ)];
        if (building1 != null && (double) ((Thing) building1).def.staticSunShadowHeight > 0.0)
        {
          float staticSunShadowHeight = ((Thing) building1).def.staticSunShadowHeight;
          Color32 color32;
          // ISSUE: explicit constructor call
          ((Color32) ref color32).\u002Ector((byte) 0, (byte) 0, (byte) 0, (byte) ((double) byte.MaxValue * (double) staticSunShadowHeight));
          if (minZ < ((MapDrawLayer) this).Map.Size.z - 1)
          {
            Building building2 = innerArray[((CellIndices) ref cellIndices).CellToIndex(minX, minZ + 1)];
            if (building2 == null || (double) ((Thing) building2).def.staticSunShadowHeight < (double) staticSunShadowHeight)
            {
              int count = subMesh.verts.Count;
              subMesh.verts.Add(new Vector3((float) minX, num, (float) (minZ + 1)));
              subMesh.verts.Add(new Vector3((float) (minX + 1), num, (float) (minZ + 1)));
              subMesh.verts.Add(new Vector3((float) minX, num, (float) (minZ + 1)));
              subMesh.verts.Add(new Vector3((float) (minX + 1), num, (float) (minZ + 1)));
              subMesh.colors.Add(SectionLayer_SunShadowsOnVehicle.LowVertexColor);
              subMesh.colors.Add(SectionLayer_SunShadowsOnVehicle.LowVertexColor);
              subMesh.colors.Add(color32);
              subMesh.colors.Add(color32);
              subMesh.tris.Add(count);
              subMesh.tris.Add(count + 2);
              subMesh.tris.Add(count + 1);
              subMesh.tris.Add(count + 1);
              subMesh.tris.Add(count + 2);
              subMesh.tris.Add(count + 3);
            }
          }
        }
      }
    }
  }
}
