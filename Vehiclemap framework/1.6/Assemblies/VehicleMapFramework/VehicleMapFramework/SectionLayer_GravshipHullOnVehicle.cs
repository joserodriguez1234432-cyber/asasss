// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SectionLayer_GravshipHullOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class SectionLayer_GravshipHullOnVehicle : SectionLayer_GravshipHull
{
  private readonly List<LayerSubMesh>[] subMeshesByRot = new List<LayerSubMesh>[4];
  private static readonly Vector2[] UVs = new Vector2[4]
  {
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 1f),
    new Vector2(1f, 1f),
    new Vector2(1f, 0.0f)
  };
  private const float HullCornerScale = 2f;
  private const string TexPath_Corner_NW = "Things/Building/Linked/GravshipHull/AngledGravshipHull_northwest";
  private const string TexPath_Corner_NE = "Things/Building/Linked/GravshipHull/AngledGravshipHull_northeast";
  private const string TexPath_Corner_SW = "Things/Building/Linked/GravshipHull/AngledGravshipHull_southwest";
  private const string TexPath_Corner_SE = "Things/Building/Linked/GravshipHull/AngledGravshipHull_southeast";
  private const string TexPath_Diagonal_NW = "Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_northwest";
  private const string TexPath_Diagonal_NE = "Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_northeast";
  private const string TexPath_Diagonal_SW = "Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_southwest";
  private const string TexPath_Diagonal_SE = "Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_southeast";
  private const string TexPath_SubStructure_W = "VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Full_west_NoShadow";
  private const string TexPath_SubStructure_E = "VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Full_east_NoShadow";
  private const string TexPath_SubStructureExtra_W = "VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Tip_west_NoShadow";
  private const string TexPath_SubStructureExtra_E = "VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Tip_east_NoShadow";
  private const int BakedIndoorMaskRenderQueue = 3185;
  private static CachedMaterial mat_Corner_NW;
  private static CachedMaterial mat_Corner_NE;
  private static CachedMaterial mat_Corner_SW;
  private static CachedMaterial mat_Corner_SE;
  private static CachedMaterial mat_Diagonal_NW;
  private static CachedMaterial mat_Diagonal_NE;
  private static CachedMaterial mat_Diagonal_SW;
  private static CachedMaterial mat_Diagonal_SE;
  private static CachedMaterial mat_SubStructure_W;
  private static CachedMaterial mat_SubStructure_E;
  private static CachedMaterial mat_SubStructureExtra_W;
  private static CachedMaterial mat_SubStructureExtra_E;
  private static readonly float cornerAltitude = Altitudes.AltitudeFor((AltitudeLayer) 17);
  private static readonly float substructureAltitude = Altitudes.AltitudeFor((AltitudeLayer) 1);
  private static readonly float bakedAltitude = Altitudes.AltitudeFor((AltitudeLayer) 39);
  private static bool initalized;
  private static readonly IntVec3[] Directions = new IntVec3[8]
  {
    IntVec3.North,
    IntVec3.East,
    IntVec3.South,
    IntVec3.West,
    IntVec3.op_Addition(IntVec3.North, IntVec3.West),
    IntVec3.op_Addition(IntVec3.North, IntVec3.East),
    IntVec3.op_Addition(IntVec3.South, IntVec3.East),
    IntVec3.op_Addition(IntVec3.South, IntVec3.West)
  };
  private static readonly int[][] directionPairs = new int[4][]
  {
    new int[2]{ 0, 2 },
    new int[2]{ 1, 3 },
    new int[2]{ 4, 6 },
    new int[2]{ 5, 7 }
  };
  private static readonly bool[] tmpChecks = new bool[SectionLayer_GravshipHullOnVehicle.Directions.Length];

  public SectionLayer_GravshipHullOnVehicle(Section section)
    : base(section)
  {
    for (int index = 0; index < 4; ++index)
      this.subMeshesByRot[index] = new List<LayerSubMesh>();
  }

  private static Shader WallShader => ShaderDatabase.CutoutOverlay;

  private static Shader SubstructureShader => ShaderDatabase.Transparent;

  public virtual bool Visible => ModsConfig.OdysseyActive;

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

  public void DrawMeshes(List<LayerSubMesh> meshes, Vector3 drawPos, float extraRotation)
  {
    if (!((MapDrawLayer) this).Visible)
      return;
    int count = meshes.Count;
    for (int index = 0; index < count; ++index)
    {
      LayerSubMesh mesh = meshes[index];
      if (mesh.finalized && !mesh.disabled)
        Graphics.DrawMesh(mesh.mesh, drawPos, Quaternion.AngleAxis(extraRotation, Vector3.up), mesh.material, mesh.renderLayer);
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
        IntVec3 intVec3_1 = IntVec3Utility.RotatedBy(IntVec3.South, VehicleSectionLayerManager.RotForPrintCounter);
        CellRect cellRect = ((SectionLayer) this).section.CellRect;
        foreach (IntVec3 intVec3_2 in cellRect)
        {
          SectionLayer_GravshipHull.CornerType cornerType;
          Color color;
          if (SectionLayer_GravshipHullOnVehicle.ShouldDrawCornerPiece(intVec3_2, map, terrainGrid, out cornerType, out color))
          {
            CachedMaterial material = SectionLayer_GravshipHullOnVehicle.GetMaterial(cornerType);
            IntVec3 intVec3_3 = IntVec3Utility.RotatedBy(SectionLayer_GravshipHullOnVehicle.GetOffset(cornerType), VehicleSectionLayerManager.RotForPrintCounter);
            this.AddQuad(material.Material, IntVec3.op_Addition(intVec3_2, intVec3_3), 2f, SectionLayer_GravshipHullOnVehicle.cornerAltitude, color);
            TerrainDef terrainDef = terrainGrid.FoundationAt(IntVec3.op_Addition(intVec3_2, intVec3_1));
            bool substructureToSouth = terrainDef != null && terrainDef.IsSubstructure;
            this.AddSubstructure(cornerType, intVec3_2, substructureToSouth);
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

  private static void EnsureInitialized()
  {
    if (SectionLayer_GravshipHullOnVehicle.initalized)
      return;
    SectionLayer_GravshipHullOnVehicle.initalized = true;
    SectionLayer_GravshipHullOnVehicle.mat_Corner_NW = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_northwest", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Corner_NE = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_northeast", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Corner_SW = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_southwest", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Corner_SE = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_southeast", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Diagonal_NW = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_northwest", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Diagonal_NE = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_northeast", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Diagonal_SW = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_southwest", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_Diagonal_SE = new CachedMaterial("Things/Building/Linked/GravshipHull/AngledGravshipHull_Partial_southeast", SectionLayer_GravshipHullOnVehicle.WallShader);
    SectionLayer_GravshipHullOnVehicle.mat_SubStructure_W = new CachedMaterial("VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Full_west_NoShadow", SectionLayer_GravshipHullOnVehicle.SubstructureShader);
    SectionLayer_GravshipHullOnVehicle.mat_SubStructure_E = new CachedMaterial("VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Full_east_NoShadow", SectionLayer_GravshipHullOnVehicle.SubstructureShader);
    SectionLayer_GravshipHullOnVehicle.mat_SubStructureExtra_W = new CachedMaterial("VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Tip_west_NoShadow", SectionLayer_GravshipHullOnVehicle.SubstructureShader);
    SectionLayer_GravshipHullOnVehicle.mat_SubStructureExtra_E = new CachedMaterial("VehicleMapFramework/Things/SubstructureProps/SubstructureCorner_Tip_east_NoShadow", SectionLayer_GravshipHullOnVehicle.SubstructureShader);
    SectionLayer_GravshipHullOnVehicle.mat_SubStructure_W.Material.mainTexture.wrapMode = (TextureWrapMode) 1;
    SectionLayer_GravshipHullOnVehicle.mat_SubStructure_E.Material.mainTexture.wrapMode = (TextureWrapMode) 1;
    SectionLayer_GravshipHullOnVehicle.mat_SubStructureExtra_W.Material.mainTexture.wrapMode = (TextureWrapMode) 1;
    SectionLayer_GravshipHullOnVehicle.mat_SubStructureExtra_E.Material.mainTexture.wrapMode = (TextureWrapMode) 1;
  }

  private static bool IsIndoorMasked(IntVec3 c, Map map) => GridsUtility.Roofed(c, map);

  private static bool IsCornerSubstructure(
    IntVec3 c,
    SectionLayer_GravshipHull.CornerType cornerType)
  {
    bool flag;
    switch (cornerType - 1)
    {
      case 0:
      case 4:
        flag = SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.North)) || SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.West));
        break;
      case 1:
      case 5:
        flag = SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.North)) || SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.East));
        break;
      case 2:
      case 6:
        flag = SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.South)) || SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.West));
        break;
      case 3:
      case 7:
        flag = SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.South)) || SectionLayer_GravshipMask.IsValidSubstructure(IntVec3.op_Addition(c, IntVec3.East));
        break;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  private static bool IsCornerIndoorMasked(
    IntVec3 c,
    SectionLayer_GravshipHull.CornerType cornerType,
    Map map)
  {
    switch ((int) cornerType)
    {
      case 1:
      case 5:
        return SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.North), map) || SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.West), map);
      case 2:
      case 6:
        return SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.North), map) || SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.East), map);
      case 3:
      case 7:
        return SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.South), map) || SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.West), map);
      case 4:
      case 8:
        return SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.South), map) || SectionLayer_GravshipHullOnVehicle.IsIndoorMasked(IntVec3.op_Addition(c, IntVec3.East), map);
      default:
        return false;
    }
  }

  private static CachedMaterial GetMaterial(SectionLayer_GravshipHull.CornerType edgeType)
  {
    SectionLayer_GravshipHullOnVehicle.EnsureInitialized();
    switch (edgeType - 1)
    {
      case 0:
        return SectionLayer_GravshipHullOnVehicle.mat_Corner_NW;
      case 1:
        return SectionLayer_GravshipHullOnVehicle.mat_Corner_NE;
      case 2:
        return SectionLayer_GravshipHullOnVehicle.mat_Corner_SW;
      case 3:
        return SectionLayer_GravshipHullOnVehicle.mat_Corner_SE;
      case 4:
        return SectionLayer_GravshipHullOnVehicle.mat_Diagonal_NW;
      case 5:
        return SectionLayer_GravshipHullOnVehicle.mat_Diagonal_NE;
      case 6:
        return SectionLayer_GravshipHullOnVehicle.mat_Diagonal_SW;
      case 7:
        return SectionLayer_GravshipHullOnVehicle.mat_Diagonal_SE;
      default:
        throw new ArgumentOutOfRangeException(nameof (edgeType), (object) edgeType, (string) null);
    }
  }

  private static IntVec3 GetOffset(SectionLayer_GravshipHull.CornerType cornerType)
  {
    IntVec3 offset;
    switch (cornerType - 1)
    {
      case 0:
      case 4:
        offset = new IntVec3(-1, 0, 0);
        break;
      case 1:
      case 5:
        offset = new IntVec3(0, 0, 0);
        break;
      case 2:
      case 6:
        offset = new IntVec3(-1, 0, -1);
        break;
      case 3:
      case 7:
        offset = new IntVec3(0, 0, -1);
        break;
      default:
        offset = IntVec3.Zero;
        break;
    }
    return offset;
  }

  private static void AddQuad(
    LayerSubMesh sm,
    Vector3 c,
    float scale,
    float altitude,
    Color color)
  {
    c = Vector3Utility.RotatedBy(c, VehicleSectionLayerManager.RotForPrint);
    Vector2[] uvs = SectionLayer_GravshipHullOnVehicle.UVs;
    Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
    int asInt = ((Rot4) ref rotForPrint).AsInt;
    Vector2 vector2 = Vector2.op_UnaryNegation(uvs[asInt]);
    int count = sm.verts.Count;
    for (int index = 0; index < 4; ++index)
    {
      sm.verts.Add(new Vector3(c.x + SectionLayer_GravshipHullOnVehicle.UVs[index].x * scale + vector2.x, altitude, c.z + SectionLayer_GravshipHullOnVehicle.UVs[index].y * scale + vector2.y));
      sm.uvs.Add(Vector2.op_Implicit(SectionLayer_GravshipHullOnVehicle.UVs[index % 4]));
      sm.colors.Add(Color32.op_Implicit(color));
    }
    sm.tris.Add(count);
    sm.tris.Add(count + 1);
    sm.tris.Add(count + 2);
    sm.tris.Add(count);
    sm.tris.Add(count + 2);
    sm.tris.Add(count + 3);
  }

  private void AddQuad(Material mat, IntVec3 c, float scale, float altitude, Color color)
  {
    SectionLayer_GravshipHullOnVehicle.AddQuad(((MapDrawLayer) this).GetSubMesh(mat), ((IntVec3) ref c).ToVector3(), scale, altitude, color);
  }

  private void AddSubstructure(
    SectionLayer_GravshipHull.CornerType cornerType,
    IntVec3 c,
    bool substructureToSouth)
  {
    switch (cornerType - 1)
    {
      case 0:
      case 4:
        this.AddQuad(SectionLayer_GravshipHullOnVehicle.mat_SubStructure_W.Material, c, 1f, SectionLayer_GravshipHullOnVehicle.substructureAltitude, Color.white);
        if (substructureToSouth)
          break;
        this.AddQuad(SectionLayer_GravshipHullOnVehicle.mat_SubStructureExtra_W.Material, IntVec3.op_Addition(c, IntVec3Utility.RotatedBy(IntVec3.South, VehicleSectionLayerManager.RotForPrintCounter)), 1f, SectionLayer_GravshipHullOnVehicle.substructureAltitude, Color.white);
        break;
      case 1:
      case 5:
        this.AddQuad(SectionLayer_GravshipHullOnVehicle.mat_SubStructure_E.Material, c, 1f, SectionLayer_GravshipHullOnVehicle.substructureAltitude, Color.white);
        if (substructureToSouth)
          break;
        this.AddQuad(SectionLayer_GravshipHullOnVehicle.mat_SubStructureExtra_E.Material, IntVec3.op_Addition(c, IntVec3Utility.RotatedBy(IntVec3.South, VehicleSectionLayerManager.RotForPrintCounter)), 1f, SectionLayer_GravshipHullOnVehicle.substructureAltitude, Color.white);
        break;
    }
  }

  public static bool ShouldDrawCornerPiece(
    IntVec3 pos,
    Map map,
    TerrainGrid terrGrid,
    out SectionLayer_GravshipHull.CornerType cornerType,
    out Color color)
  {
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    ^(int&) ref cornerType = 0;
    color = Color.white;
    TerrainDef terrainDef = terrGrid.FoundationAt(pos);
    if (terrainDef != null && terrainDef.IsSubstructure)
      return false;
    for (int index = 0; index < SectionLayer_GravshipHullOnVehicle.Directions.Length; ++index)
      SectionLayer_GravshipHullOnVehicle.tmpChecks[index] = ((Thing) GridsUtility.GetEdificeSafe(IntVec3.op_Addition(pos, IntVec3Utility.RotatedBy(SectionLayer_GravshipHullOnVehicle.Directions[index], VehicleSectionLayerManager.RotForPrintCounter)), map))?.def == ThingDefOf.GravshipHull;
    SectionLayer_GravshipHull.CornerType cornerType1;
    if (SectionLayer_GravshipHullOnVehicle.tmpChecks[0])
    {
      if (SectionLayer_GravshipHullOnVehicle.tmpChecks[3] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[2] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[1])
      {
        cornerType1 = SectionLayer_GravshipHullOnVehicle.tmpChecks[4] ? (SectionLayer_GravshipHull.CornerType) 1 : (SectionLayer_GravshipHull.CornerType) 5;
        goto label_17;
      }
      if (SectionLayer_GravshipHullOnVehicle.tmpChecks[1] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[2] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[3])
      {
        cornerType1 = SectionLayer_GravshipHullOnVehicle.tmpChecks[5] ? (SectionLayer_GravshipHull.CornerType) 2 : (SectionLayer_GravshipHull.CornerType) 6;
        goto label_17;
      }
    }
    SectionLayer_GravshipHull.CornerType cornerType2;
    if (SectionLayer_GravshipHullOnVehicle.tmpChecks[2])
    {
      if (SectionLayer_GravshipHullOnVehicle.tmpChecks[1] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[0] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[3])
      {
        cornerType2 = SectionLayer_GravshipHullOnVehicle.tmpChecks[6] ? (SectionLayer_GravshipHull.CornerType) 4 : (SectionLayer_GravshipHull.CornerType) 8;
        goto label_16;
      }
      if (SectionLayer_GravshipHullOnVehicle.tmpChecks[3] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[0] && !SectionLayer_GravshipHullOnVehicle.tmpChecks[1])
      {
        cornerType2 = SectionLayer_GravshipHullOnVehicle.tmpChecks[7] ? (SectionLayer_GravshipHull.CornerType) 3 : (SectionLayer_GravshipHull.CornerType) 7;
        goto label_16;
      }
    }
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    cornerType2 = (SectionLayer_GravshipHull.CornerType) ^(int&) ref cornerType;
label_16:
    cornerType1 = cornerType2;
label_17:
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    ^(int&) ref cornerType = (int) cornerType1;
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    if (^(int&) ref cornerType == 0)
      return false;
    foreach (IEnumerable<int> directionPair in SectionLayer_GravshipHullOnVehicle.directionPairs)
    {
      List<int> list = directionPair.Where<int>((Func<int, bool>) (num2 => SectionLayer_GravshipHullOnVehicle.tmpChecks[num2])).ToList<int>();
      if (list.Count > 0)
      {
        int index = list.First<int>();
        color = ((Thing) GridsUtility.GetEdificeSafe(IntVec3.op_Addition(pos, IntVec3Utility.RotatedBy(SectionLayer_GravshipHullOnVehicle.Directions[index], VehicleSectionLayerManager.RotForPrintCounter)), map)).DrawColor;
        break;
      }
    }
    return true;
  }
}
