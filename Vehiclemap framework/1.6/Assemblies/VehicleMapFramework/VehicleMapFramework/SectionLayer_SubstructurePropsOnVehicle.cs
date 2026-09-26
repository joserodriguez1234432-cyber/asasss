// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SectionLayer_SubstructurePropsOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class SectionLayer_SubstructurePropsOnVehicle : SectionLayer_SubstructureProps
{
  public readonly List<LayerSubMesh>[] subMeshesByRot = new List<LayerSubMesh>[4];
  private static readonly CachedMaterial OShape = new CachedMaterial("Terrain/Surfaces/Substructure/SubstructureBorder_OShape", ShaderDatabase.Transparent);
  private static readonly CachedMaterial UShape = new CachedMaterial("Terrain/Surfaces/Substructure/SubstructureBorder_UShape", ShaderDatabase.Transparent);
  private static readonly CachedMaterial CornerInner = new CachedMaterial("Terrain/Surfaces/Substructure/SubstructureBorder_CornerInner", ShaderDatabase.Transparent);
  private static readonly CachedMaterial CornerOuter = new CachedMaterial("Terrain/Surfaces/Substructure/SubstructureBorder_CornerOuter", ShaderDatabase.Transparent);
  private static readonly CachedMaterial Flat = new CachedMaterial("Terrain/Surfaces/Substructure/SubstructureBorder_Flat", ShaderDatabase.Transparent);
  private static readonly CachedMaterial Bottom = new CachedMaterial("VehicleMapFramework/Things/SubstructureProps/SubstructureProps_Bottom_NoShadow", ShaderDatabase.Transparent);
  private static readonly Vector2[] UVs = new Vector2[4]
  {
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 1f),
    new Vector2(1f, 1f),
    new Vector2(1f, 0.0f)
  };
  private static readonly Dictionary<SectionLayer_SubstructurePropsOnVehicle.EdgeDirections, (CachedMaterial, Rot4)[]> EdgeMats = new Dictionary<SectionLayer_SubstructurePropsOnVehicle.EdgeDirections, (CachedMaterial, Rot4)[]>()
  {
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.South)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.West)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.North)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.East)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.CornerOuter, Rot4.West)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.CornerOuter, Rot4.North)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.CornerOuter, Rot4.East)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.CornerOuter, Rot4.South)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South,
      new (CachedMaterial, Rot4)[2]
      {
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.South),
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.North)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[2]
      {
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.West),
        (SectionLayer_SubstructurePropsOnVehicle.Flat, Rot4.East)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.UShape, Rot4.West)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.UShape, Rot4.North)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.UShape, Rot4.East)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.UShape, Rot4.South)
      }
    },
    {
      SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South | SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West,
      new (CachedMaterial, Rot4)[1]
      {
        (SectionLayer_SubstructurePropsOnVehicle.OShape, Rot4.North)
      }
    }
  };

  public SectionLayer_SubstructurePropsOnVehicle(Section section)
    : base(section)
  {
    for (int index = 0; index < 4; ++index)
      this.subMeshesByRot[index] = new List<LayerSubMesh>();
    SectionLayer_SubstructurePropsOnVehicle.Bottom.Material.mainTexture.wrapMode = (TextureWrapMode) 1;
  }

  public virtual CellRect GetBoundaryRect()
  {
    CellRect boundaryRect = ((SectionLayer) this).GetBoundaryRect();
    VehiclePawnWithMap vehicle;
    if (((SectionLayer) this).section.map.IsVehicleMapOf(out vehicle))
    {
      int num = Mathf.Max(((Thing) vehicle).def.size.x, ((Thing) vehicle).def.size.z);
      boundaryRect = ((CellRect) ref boundaryRect).ExpandedBy(num);
    }
    return boundaryRect;
  }

  public virtual void DrawLayer()
  {
  }

  public void DrawLayer(Rot8 rot, Vector3 drawPos, float extraRotation)
  {
    float extraRotation1 = Ext_Math.RotateAngle(-((Rot8) ref rot).AsRotationAngle, extraRotation);
    List<LayerSubMesh>[] subMeshesByRot = this.subMeshesByRot;
    Rot4 rot4 = rot.RotForVehicleDraw();
    int asInt = ((Rot4) ref rot4).AsInt;
    this.DrawMeshes(subMeshesByRot[asInt], drawPos, extraRotation1);
  }

  public void DrawMeshes(List<LayerSubMesh> _subMeshes, Vector3 drawPos, float extraRotation)
  {
    if (!((MapDrawLayer) this).Visible)
      return;
    int count = _subMeshes.Count;
    for (int index = 0; index < count; ++index)
    {
      LayerSubMesh subMesh = _subMeshes[index];
      if (subMesh.finalized && !subMesh.disabled)
        Graphics.DrawMesh(subMesh.mesh, drawPos, Quaternion.AngleAxis(extraRotation, Vector3.up), subMesh.material, subMesh.renderLayer);
    }
  }

  public virtual void Regenerate()
  {
    if (!ModsConfig.OdysseyActive || !((MapDrawLayer) this).Map.IsVehicleMapOf(out VehiclePawnWithMap _))
      return;
    VehicleSectionLayerManager.RotForPrint = Rot4.North;
    for (int index = 0; index < 4; ++index)
    {
      try
      {
        ((MapDrawLayer) this).subMeshes = this.subMeshesByRot[index];
        ((MapDrawLayer) this).ClearSubMeshes((MeshParts) 63 /*0x3F*/);
        Map map = ((MapDrawLayer) this).Map;
        TerrainGrid terrainGrid = map.terrainGrid;
        CellRect cellRect = ((SectionLayer) this).section.CellRect;
        float altitude = Altitudes.AltitudeFor((AltitudeLayer) 3);
        LayerSubMesh subMesh = ((MapDrawLayer) this).GetSubMesh(SectionLayer_SubstructurePropsOnVehicle.Bottom.Material);
        IntVec3 intVec3 = IntVec3Utility.RotatedBy(IntVec3.South, VehicleSectionLayerManager.RotForPrintCounter);
        foreach (IntVec3 c in cellRect)
        {
          SectionLayer_SubstructurePropsOnVehicle.EdgeDirections edgeEdgeDirections;
          SectionLayer_SubstructurePropsOnVehicle.CornerDirections cornerDirections;
          if (this.ShouldDrawPropsOn(c, terrainGrid, out edgeEdgeDirections, out cornerDirections))
          {
            this.DrawEdges(c, edgeEdgeDirections, altitude);
            this.DrawCorners(c, cornerDirections, edgeEdgeDirections, altitude);
            SectionLayer_GravshipHull.CornerType cornerType;
            SectionLayer_GravshipHullOnVehicle.ShouldDrawCornerPiece(IntVec3.op_Addition(c, intVec3), map, terrainGrid, out cornerType, out Color _);
            bool flag = cornerType == 1 || cornerType == 5 || cornerType == 2 || cornerType == 6;
            if (edgeEdgeDirections.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South) && !flag)
              this.AddQuad(subMesh, IntVec3.op_Addition(c, intVec3), altitude, Rot4.North);
          }
        }
        ((MapDrawLayer) this).FinalizeMesh((MeshParts) 63 /*0x3F*/);
      }
      finally
      {
        Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
        VehicleSectionLayerManager.RotForPrint = ((Rot4) ref rotForPrint).Rotated((RotationDirection) 1);
      }
    }
    VehicleSectionLayerManager.RotForPrint = Rot4.North;
  }

  private void DrawEdges(
    IntVec3 c,
    SectionLayer_SubstructurePropsOnVehicle.EdgeDirections edgeDirs,
    float altitude)
  {
    (CachedMaterial, Rot4)[] tupleArray;
    if (!SectionLayer_SubstructurePropsOnVehicle.EdgeMats.TryGetValue(edgeDirs, out tupleArray))
      return;
    for (int index = 0; index < tupleArray.Length; ++index)
    {
      (CachedMaterial cachedMaterial, Rot4 rotation) = tupleArray[index];
      this.AddQuad(((MapDrawLayer) this).GetSubMesh(cachedMaterial.Material), c, altitude, rotation);
    }
  }

  private void DrawCorners(
    IntVec3 c,
    SectionLayer_SubstructurePropsOnVehicle.CornerDirections cornerDirections,
    SectionLayer_SubstructurePropsOnVehicle.EdgeDirections edgeDirs,
    float altitude)
  {
    if (cornerDirections.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.CornerDirections.NorthWest) && !edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North) && !edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West))
      this.AddQuad(((MapDrawLayer) this).GetSubMesh(SectionLayer_SubstructurePropsOnVehicle.CornerInner.Material), c, altitude, Rot4.South);
    if (cornerDirections.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.CornerDirections.NorthEast) && !edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.North) && !edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East))
      this.AddQuad(((MapDrawLayer) this).GetSubMesh(SectionLayer_SubstructurePropsOnVehicle.CornerInner.Material), c, altitude, Rot4.West);
    if (cornerDirections.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.CornerDirections.SouthEast) && !edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South) && !edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.East))
      this.AddQuad(((MapDrawLayer) this).GetSubMesh(SectionLayer_SubstructurePropsOnVehicle.CornerInner.Material), c, altitude, Rot4.North);
    if (!cornerDirections.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.CornerDirections.SouthWest) || edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.South) || edgeDirs.HasFlag((Enum) SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.West))
      return;
    this.AddQuad(((MapDrawLayer) this).GetSubMesh(SectionLayer_SubstructurePropsOnVehicle.CornerInner.Material), c, altitude, Rot4.East);
  }

  private void AddQuad(LayerSubMesh sm, IntVec3 c, float altitude, Rot4 rotation)
  {
    c = IntVec3Utility.RotatedBy(c, VehicleSectionLayerManager.RotForPrint);
    Vector2[] uvs = SectionLayer_SubstructurePropsOnVehicle.UVs;
    Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
    int asInt = ((Rot4) ref rotForPrint).AsInt;
    Vector2 vector2 = Vector2.op_UnaryNegation(uvs[asInt]);
    int count = sm.verts.Count;
    int num = Mathf.Abs(4 - ((Rot4) ref rotation).AsInt);
    for (int index = 0; index < 4; ++index)
    {
      sm.verts.Add(new Vector3((float) c.x + SectionLayer_SubstructurePropsOnVehicle.UVs[index].x + vector2.x, altitude, (float) c.z + SectionLayer_SubstructurePropsOnVehicle.UVs[index].y + vector2.y));
      sm.uvs.Add(Vector2.op_Implicit(SectionLayer_SubstructurePropsOnVehicle.UVs[(num + index) % 4]));
    }
    sm.tris.Add(count);
    sm.tris.Add(count + 1);
    sm.tris.Add(count + 2);
    sm.tris.Add(count);
    sm.tris.Add(count + 2);
    sm.tris.Add(count + 3);
  }

  private bool ShouldDrawPropsOn(
    IntVec3 c,
    TerrainGrid terrGrid,
    out SectionLayer_SubstructurePropsOnVehicle.EdgeDirections edgeEdgeDirections,
    out SectionLayer_SubstructurePropsOnVehicle.CornerDirections cornerDirections)
  {
    edgeEdgeDirections = SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.None;
    cornerDirections = SectionLayer_SubstructurePropsOnVehicle.CornerDirections.None;
    TerrainDef terrainDef1 = terrGrid.FoundationAt(c);
    if (terrainDef1 == null || !terrainDef1.IsSubstructure)
      return false;
    Rot4 rotForPrint;
    for (int index1 = 0; index1 < GenAdj.CardinalDirections.Length; ++index1)
    {
      IntVec3 intVec3_1 = c;
      IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
      int num = index1;
      rotForPrint = VehicleSectionLayerManager.RotForPrint;
      int asInt = ((Rot4) ref rotForPrint).AsInt;
      int index2 = GenMath.PositiveMod(num - asInt, 4);
      IntVec3 intVec3_2 = cardinalDirections[index2];
      IntVec3 intVec3_3 = IntVec3.op_Addition(intVec3_1, intVec3_2);
      if (!GenGrid.InBounds(intVec3_3, ((MapDrawLayer) this).Map))
      {
        edgeEdgeDirections |= (SectionLayer_SubstructurePropsOnVehicle.EdgeDirections) (1 << index1);
      }
      else
      {
        TerrainDef terrainDef2 = terrGrid.FoundationAt(intVec3_3);
        if (terrainDef2 == null || !terrainDef2.IsSubstructure)
          edgeEdgeDirections |= (SectionLayer_SubstructurePropsOnVehicle.EdgeDirections) (1 << index1);
      }
    }
    for (int index3 = 0; index3 < GenAdj.DiagonalDirections.Length; ++index3)
    {
      IntVec3 intVec3_4 = c;
      IntVec3[] diagonalDirections = GenAdj.DiagonalDirections;
      int num = index3;
      rotForPrint = VehicleSectionLayerManager.RotForPrint;
      int asInt = ((Rot4) ref rotForPrint).AsInt;
      int index4 = GenMath.PositiveMod(num - asInt, 4);
      IntVec3 intVec3_5 = diagonalDirections[index4];
      IntVec3 intVec3_6 = IntVec3.op_Addition(intVec3_4, intVec3_5);
      if (!GenGrid.InBounds(intVec3_6, ((MapDrawLayer) this).Map))
      {
        cornerDirections |= (SectionLayer_SubstructurePropsOnVehicle.CornerDirections) (1 << index3);
      }
      else
      {
        TerrainDef terrainDef3 = terrGrid.FoundationAt(intVec3_6);
        if (terrainDef3 == null || !terrainDef3.IsSubstructure)
          cornerDirections |= (SectionLayer_SubstructurePropsOnVehicle.CornerDirections) (1 << index3);
      }
    }
    return edgeEdgeDirections != SectionLayer_SubstructurePropsOnVehicle.EdgeDirections.None || cornerDirections != 0;
  }

  [Flags]
  private enum EdgeDirections
  {
    None = 0,
    North = 1,
    East = 2,
    South = 4,
    West = 8,
  }

  [Flags]
  private enum CornerDirections
  {
    None = 0,
    SouthWest = 1,
    NorthWest = 2,
    NorthEast = 4,
    SouthEast = 8,
  }
}
