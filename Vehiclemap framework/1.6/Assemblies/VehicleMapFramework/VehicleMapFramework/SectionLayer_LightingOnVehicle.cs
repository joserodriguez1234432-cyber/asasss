// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SectionLayer_LightingOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class SectionLayer_LightingOnVehicle : SectionLayer
{
  private int firstCenterInd;
  private CellRect sectRect;
  private static Material LightOverlayInverseMultiply;
  private static MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
  private static readonly int RestoreFactor = Shader.PropertyToID("_RestoreFactor");
  private static readonly int MaxRestore = Shader.PropertyToID("_MaxRestore");
  private static readonly int ColorPreservation = Shader.PropertyToID("_ColorPreservation");
  [TweakValue("SectionLayer_LightingOnVehicle._RestoreFactor", 0.0f, 10f)]
  [UsedImplicitly]
  private static float restoreFactor = 1.5f;
  [TweakValue("SectionLayer_LightingOnVehicle._MaxRestore", 0.0f, 1f)]
  [UsedImplicitly]
  private static float maxRestore = 0.85f;
  private const int MinGlow = 100;
  private readonly bool[] expand = new bool[4];
  private const byte RoofedAreaMinSkyCover = 100;
  private const int ExpandSize = 10;

  static SectionLayer_LightingOnVehicle()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      SectionLayer_LightingOnVehicle.LightOverlayInverseMultiply = MaterialPool.MatFrom(VMF_DefOf.VMF_LightOverlayInverseMultiply.Shader);
      SectionLayer_LightingOnVehicle.materialPropertyBlock = new MaterialPropertyBlock();
    }));
  }

  public virtual bool Visible
  {
    get
    {
      if (!DebugViewSettings.drawLightingOverlay)
        return false;
      return Find.CurrentMap != ((MapDrawLayer) this).Map || VehicleMapFramework.VehicleMapFramework.settings.drawPlanet;
    }
  }

  public SectionLayer_LightingOnVehicle(Section section)
    : base(section)
  {
    ((MapDrawLayer) this).relevantChangeTypes = MapMeshFlagDef.op_Implicit(MapMeshFlagDefOf.Roofs) | MapMeshFlagDef.op_Implicit(MapMeshFlagDefOf.GroundGlow);
  }

  public virtual void DrawLayer()
  {
  }

  public void DrawLayer(Vector3 drawPos)
  {
    VehiclePawnWithMap vehicle;
    if (!((MapDrawLayer) this).Visible || !((MapDrawLayer) this).Map.IsVehicleMapOf(out vehicle))
      return;
    Map map = ((MapDrawLayer) this).Map.BaseMap();
    Quaternion quaternion = Quaternion.AngleAxis(VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle), Vector3.up);
    for (int index = 0; index < ((MapDrawLayer) this).subMeshes.Count; ++index)
    {
      LayerSubMesh subMesh = ((MapDrawLayer) this).subMeshes[index];
      if (subMesh.finalized && !subMesh.disabled)
      {
        if (Object.op_Equality((Object) subMesh.material, (Object) SectionLayer_LightingOnVehicle.LightOverlayInverseMultiply))
        {
          SectionLayer_LightingOnVehicle.materialPropertyBlock.SetColor(ShaderPropertyIDs.Color, map.skyManager.CurSky.colors.sky);
          SectionLayer_LightingOnVehicle.materialPropertyBlock.SetFloat(SectionLayer_LightingOnVehicle.RestoreFactor, SectionLayer_LightingOnVehicle.restoreFactor);
          SectionLayer_LightingOnVehicle.materialPropertyBlock.SetFloat(SectionLayer_LightingOnVehicle.MaxRestore, SectionLayer_LightingOnVehicle.maxRestore);
        }
        else
          SectionLayer_LightingOnVehicle.materialPropertyBlock.SetColor(ShaderPropertyIDs.Color, Color.white);
        Graphics.DrawMesh(subMesh.mesh, drawPos, quaternion, subMesh.material, 0, (Camera) null, 0, SectionLayer_LightingOnVehicle.materialPropertyBlock);
      }
    }
  }

  public string GlowReportAt(IntVec3 c)
  {
    Color32[] colors32 = ((MapDrawLayer) this).GetSubMesh(SectionLayer_LightingOnVehicle.LightOverlayInverseMultiply).mesh.colors32;
    int botLeft;
    int topLeft;
    int topRight;
    int botRight;
    int center;
    this.CalculateVertexIndices(c.x, c.z, out botLeft, out topLeft, out topRight, out botRight, out center);
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.Append("BL=" + colors32[botLeft].ToString());
    stringBuilder.Append("\nTL=" + colors32[topLeft].ToString());
    stringBuilder.Append("\nTR=" + colors32[topRight].ToString());
    stringBuilder.Append("\nBR=" + colors32[botRight].ToString());
    stringBuilder.Append("\nCenter=" + colors32[center].ToString());
    return stringBuilder.ToString();
  }

  public virtual void Regenerate()
  {
    if (!VehicleMapUtility.get_IsVehicleMap(((MapDrawLayer) this).Map))
      return;
    LayerSubMesh subMesh1 = ((MapDrawLayer) this).GetSubMesh(SectionLayer_LightingOnVehicle.LightOverlayInverseMultiply);
    LayerSubMesh subMesh2 = ((MapDrawLayer) this).GetSubMesh(MatBases.LightOverlay);
    if (subMesh1.verts.Count == 0)
      this.MakeBaseGeometry(subMesh1, Altitudes.AltitudeFor((AltitudeLayer) 32 /*0x20*/).YOffset());
    if (subMesh2.verts.Count == 0)
      this.MakeBaseGeometry(subMesh2, Altitudes.AltitudeFor((AltitudeLayer) 32 /*0x20*/).YOffset());
    Color32[] color32Array1 = new Color32[subMesh1.verts.Count];
    CellRect cellRect1;
    // ISSUE: explicit constructor call
    ((CellRect) ref cellRect1).\u002Ector(this.section.botLeft.x, this.section.botLeft.z, 17, 17);
    ((CellRect) ref cellRect1).ClipInsideMap(((MapDrawLayer) this).Map);
    int maxX = cellRect1.maxX;
    int maxZ = cellRect1.maxZ;
    int width = ((CellRect) ref this.sectRect).Width;
    Map map = ((MapDrawLayer) this).Map;
    int x = map.Size.x;
    Building[] innerArray = map.edificeGrid.InnerArray;
    int length = innerArray.Length;
    RoofGrid roofGrid = map.roofGrid;
    CellIndices cellIndices = map.cellIndices;
    int botLeft1;
    int num1;
    int num2;
    int num3;
    this.CalculateVertexIndices(cellRect1.minX, cellRect1.minZ, out botLeft1, out int _, out num1, out num2, out num3);
    int num4 = ((CellIndices) ref cellIndices).CellToIndex(new IntVec3(cellRect1.minX, 0, cellRect1.minZ));
    int[] numArray1 = new int[4]{ -x - 1, -x, -1, 0 };
    int[] numArray2 = new int[4]{ -1, -1, 0, 0 };
    ColorInt colorInt1;
    for (int minZ = cellRect1.minZ; minZ <= maxZ + 1; ++minZ)
    {
      int num5 = num4 / x;
      int minX = cellRect1.minX;
      while (minX <= maxX + 1)
      {
        ColorInt colorInt2;
        // ISSUE: explicit constructor call
        ((ColorInt) ref colorInt2).\u002Ector(0, 0, 0, 0);
        int num6 = 0;
        bool flag = false;
        for (int index1 = 0; index1 < 4; ++index1)
        {
          int index2 = num4 + numArray1[index1];
          if (index2 >= 0 && index2 < length && index2 / x == num5 + numArray2[index1])
          {
            Building building = innerArray[index2];
            if (roofGrid.RoofAt(index2) != null)
            {
              if (building != null)
              {
                ThingDef def = ((Thing) building).def;
                if (def != null && def.holdsRoof && ((BuildableDef) def).altitudeLayer != 14)
                  goto label_13;
              }
              flag = true;
            }
label_13:
            if (building != null)
            {
              ThingDef def = ((Thing) building).def;
              if (def != null && def.blockLight)
                continue;
            }
            colorInt2 = ColorInt.op_Addition(colorInt2, map.glowGrid.VisualGlowAt(index2));
            ++num6;
            if (!flag && Mathf.Max(colorInt2.r, Mathf.Max(colorInt2.g, colorInt2.b)) > 100)
              flag = true;
          }
        }
        if (num6 > 0)
        {
          Color32[] color32Array2 = color32Array1;
          int index = botLeft1;
          colorInt1 = ColorInt.op_Division(colorInt2, num6);
          Color32 color32 = ((ColorInt) ref colorInt1).ProjectToColor32();
          color32Array2[index] = color32;
        }
        else
          color32Array1[botLeft1] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 0);
        if (flag && color32Array1[botLeft1].a < (byte) 100)
          color32Array1[botLeft1].a = (byte) 100;
        ++minX;
        ++botLeft1;
        ++num4;
      }
      int num7 = maxX + 2 - this.sectRect.minX;
      if (this.expand[3])
        num7 -= 10;
      int num8 = botLeft1 - num7;
      int num9 = num4 - num7;
      botLeft1 = num8 + (width + 1);
      num4 = num9 + map.Size.x;
    }
    int botLeft2;
    int center;
    this.CalculateVertexIndices(cellRect1.minX, cellRect1.minZ, out botLeft2, out num3, out num2, out num1, out center);
    for (int minZ = cellRect1.minZ; minZ <= maxZ; ++minZ)
    {
      int minX = cellRect1.minX;
      while (minX <= maxX)
      {
        colorInt1 = new ColorInt();
        ColorInt colorInt3 = ColorInt.op_Addition(ColorInt.op_Addition(ColorInt.op_Addition(ColorInt.op_Addition(colorInt1, color32Array1[botLeft2]), color32Array1[botLeft2 + 1]), color32Array1[botLeft2 + width + 1]), color32Array1[botLeft2 + width + 2]);
        color32Array1[center] = new Color32((byte) (colorInt3.r / 4), (byte) (colorInt3.g / 4), (byte) (colorInt3.b / 4), (byte) (colorInt3.a / 4));
        ++minX;
        ++botLeft2;
        ++center;
      }
      int num10 = 0;
      if (this.expand[3])
        ++num10;
      if (this.expand[1])
        ++num10;
      botLeft2 += num10 * 10 + 1;
      center += num10 * 10;
    }
    CellRect cellRect2;
    // ISSUE: explicit constructor call
    ((CellRect) ref cellRect2).\u002Ector(this.section.botLeft.x, this.section.botLeft.z, 17, 17);
    ((CellRect) ref cellRect2).ClipInsideMap(((MapDrawLayer) this).Map);
    CellRect cellRect3 = ((CellRect) ref cellRect2).MovedBy(IntVec3.op_UnaryNegation(((CellRect) ref cellRect2).Min));
    CellRect cellRect4 = cellRect3;
    if (((IEnumerable<bool>) this.expand).Any<bool>((Func<bool, bool>) (e => e)))
    {
      for (int index3 = 0; index3 < 10; ++index3)
      {
        if (this.expand[0])
          ++cellRect3.maxZ;
        if (this.expand[1])
          ++cellRect3.maxX;
        if (this.expand[2])
          --cellRect3.minZ;
        if (this.expand[3])
          --cellRect3.minX;
        CellRect cellRect5 = cellRect3;
        --cellRect5.maxX;
        --cellRect5.maxZ;
        for (int index4 = 0; index4 < 4; ++index4)
        {
          Rot4 rot4;
          // ISSUE: explicit constructor call
          ((Rot4) ref rot4).\u002Ector(index4);
          if (this.expand[index4])
          {
            CellRect edgeRect1 = ((CellRect) ref cellRect3).GetEdgeRect(rot4);
            TrimCorner(ref edgeRect1, index4);
            foreach (IntVec3 c1 in edgeRect1)
            {
              IntVec3 c2 = ((CellRect) ref cellRect4).ClosestCellTo(c1);
              Color32 color32 = color32Array1[IndexGetterCorner(c2)];
              int index5 = IndexGetterCorner(c1);
              IntVec3 intVec3 = IntVec3.op_Subtraction(c2, c1);
              float lengthHorizontal = ((IntVec3) ref intVec3).LengthHorizontal;
              int num11 = Mathf.Max((int) color32.r, Mathf.Max((int) color32.g, (int) color32.b));
              float num12 = Mathf.Max((float) num11 - 25.5f * lengthHorizontal, 0.0f) / (float) num11;
              color32Array1[index5] = new Color32((byte) ((double) color32.r * (double) num12), (byte) ((double) color32.g * (double) num12), (byte) ((double) color32.b * (double) num12), (byte) ((double) color32.a * (double) num12));
            }
            CellRect edgeRect2 = ((CellRect) ref cellRect5).GetEdgeRect(rot4);
            TrimCorner(ref edgeRect2, index4);
            foreach (IntVec3 c in edgeRect2)
            {
              int index6 = IndexGetterCorner(c);
              int index7 = IndexGetterCenter(c);
              ColorInt colorInt4 = ColorInt.op_Addition(ColorInt.op_Addition(ColorInt.op_Addition(ColorInt.op_Addition(new ColorInt(), color32Array1[index6]), color32Array1[index6 + 1]), color32Array1[index6 + width + 1]), color32Array1[index6 + width + 2]);
              color32Array1[index7] = new Color32((byte) (colorInt4.r / 4), (byte) (colorInt4.g / 4), (byte) (colorInt4.b / 4), (byte) (colorInt4.a / 4));
            }
          }
        }
      }
    }
    subMesh1.mesh.colors32 = color32Array1;
    subMesh2.mesh.colors32 = color32Array1;

    void TrimCorner(ref CellRect edgeRect, int index)
    {
      if (!this.expand[(index + 1) % 4])
        return;
      switch (index)
      {
        case 0:
          --edgeRect.maxX;
          break;
        case 1:
          ++edgeRect.minZ;
          break;
        case 2:
          ++edgeRect.minX;
          break;
        case 3:
          --edgeRect.maxZ;
          break;
      }
    }

    int IndexGetterCorner(IntVec3 c)
    {
      return ((this.expand[2] ? 10 : 0) + c.z) * (width + 1) + (this.expand[3] ? 10 : 0) + c.x;
    }

    int IndexGetterCenter(IntVec3 c)
    {
      return this.firstCenterInd + ((this.expand[2] ? 10 : 0) + c.z) * width + (this.expand[3] ? 10 : 0) + c.x;
    }
  }

  private void MakeBaseGeometry(LayerSubMesh sm, float altitude)
  {
    this.sectRect = new CellRect(this.section.botLeft.x, this.section.botLeft.z, 17, 17);
    ((CellRect) ref this.sectRect).ClipInsideMap(((MapDrawLayer) this).Map);
    IntVec3 min = ((CellRect) ref this.sectRect).Min;
    IntVec3 max = ((CellRect) ref this.sectRect).Max;
    if (!GenGrid.InBounds(IntVec3.op_Addition(max, IntVec3.North), ((MapDrawLayer) this).Map))
    {
      this.expand[0] = true;
      this.sectRect.maxZ += 10;
    }
    if (!GenGrid.InBounds(IntVec3.op_Addition(max, IntVec3.East), ((MapDrawLayer) this).Map))
    {
      this.expand[1] = true;
      this.sectRect.maxX += 10;
    }
    if (!GenGrid.InBounds(IntVec3.op_Addition(min, IntVec3.South), ((MapDrawLayer) this).Map))
    {
      this.expand[2] = true;
      this.sectRect.minZ -= 10;
    }
    if (!GenGrid.InBounds(IntVec3.op_Addition(min, IntVec3.West), ((MapDrawLayer) this).Map))
    {
      this.expand[3] = true;
      this.sectRect.minX -= 10;
    }
    for (int minZ = this.sectRect.minZ; minZ <= this.sectRect.maxZ + 1; ++minZ)
    {
      for (int minX = this.sectRect.minX; minX <= this.sectRect.maxX + 1; ++minX)
        sm.verts.Add(new Vector3((float) minX, altitude, (float) minZ));
    }
    this.firstCenterInd = sm.verts.Count;
    for (int minZ = this.sectRect.minZ; minZ <= this.sectRect.maxZ; ++minZ)
    {
      for (int minX = this.sectRect.minX; minX <= this.sectRect.maxX; ++minX)
        sm.verts.Add(new Vector3((float) minX + 0.5f, altitude, (float) minZ + 0.5f));
    }
    sm.tris.Capacity = ((CellRect) ref this.sectRect).Area * 4 * 3;
    for (int minZ = this.sectRect.minZ; minZ <= this.sectRect.maxZ; ++minZ)
    {
      for (int minX = this.sectRect.minX; minX <= this.sectRect.maxX; ++minX)
      {
        int botLeft;
        int topLeft;
        int topRight;
        int botRight;
        int center;
        this.CalculateVertexIndices(minX, minZ, out botLeft, out topLeft, out topRight, out botRight, out center);
        sm.tris.Add(botLeft);
        sm.tris.Add(center);
        sm.tris.Add(botRight);
        sm.tris.Add(botLeft);
        sm.tris.Add(topLeft);
        sm.tris.Add(center);
        sm.tris.Add(topLeft);
        sm.tris.Add(topRight);
        sm.tris.Add(center);
        sm.tris.Add(topRight);
        sm.tris.Add(botRight);
        sm.tris.Add(center);
      }
    }
    sm.FinalizeMesh((MeshParts) 3);
  }

  private void CalculateVertexIndices(
    int worldX,
    int worldZ,
    out int botLeft,
    out int topLeft,
    out int topRight,
    out int botRight,
    out int center)
  {
    int num1 = worldX - this.sectRect.minX;
    int num2 = worldZ - this.sectRect.minZ;
    botLeft = num2 * (((CellRect) ref this.sectRect).Width + 1) + num1;
    topLeft = (num2 + 1) * (((CellRect) ref this.sectRect).Width + 1) + num1;
    topRight = (num2 + 1) * (((CellRect) ref this.sectRect).Width + 1) + num1 + 1;
    botRight = num2 * (((CellRect) ref this.sectRect).Width + 1) + num1 + 1;
    center = this.firstCenterInd + num2 * ((CellRect) ref this.sectRect).Width + num1;
  }
}
