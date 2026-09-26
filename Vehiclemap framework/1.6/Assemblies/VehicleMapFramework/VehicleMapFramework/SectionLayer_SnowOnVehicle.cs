// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SectionLayer_SnowOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class SectionLayer_SnowOnVehicle(Section section) : SectionLayer_Snow(section)
{
  private readonly float[] adjValuesTmp = new float[9];
  private static readonly List<float> opacityListTmp = new List<float>();
  private static readonly CachedTexture PollutedSnowTex = new CachedTexture("Other/SnowPolluted");
  private static Material SnowMat;
  private static readonly Func<SnowGrid, NativeArray<float>> DepthGrid_Unsafe = AccessTools.MethodDelegate<Func<SnowGrid, NativeArray<float>>>(AccessTools.PropertyGetter(typeof (SnowGrid), nameof (DepthGrid_Unsafe)), (object) null, true, (Type[]) null);

  static SectionLayer_SnowOnVehicle()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      SectionLayer_SnowOnVehicle.SnowMat = MaterialPool.MatFrom("Other/Snow", VMF_DefOf.VMF_SnowWithZ.Shader);
      SectionLayer_SnowOnVehicle.SnowMat.SetTexture(Shader.PropertyToID("_MacroTex"), (Texture) ContentFinder<Texture2D>.Get("Other/SnowMacro", true));
      SectionLayer_SnowOnVehicle.SnowMat.SetTexture(Shader.PropertyToID("_AlphaAddTex"), (Texture) TexGame.AlphaAddTex);
    }));
  }

  public void DrawLayer(Vector3 drawPos)
  {
    VehiclePawnWithMap vehicle;
    if (!((MapDrawLayer) this).Visible || !((MapDrawLayer) this).Map.IsVehicleMapOf(out vehicle))
      return;
    Quaternion quaternion = Quaternion.AngleAxis(VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle), Vector3.up);
    for (int index = 0; index < ((MapDrawLayer) this).subMeshes.Count; ++index)
    {
      LayerSubMesh subMesh = ((MapDrawLayer) this).subMeshes[index];
      if (subMesh.finalized && !subMesh.disabled)
        Graphics.DrawMesh(subMesh.mesh, drawPos, quaternion, subMesh.material, subMesh.renderLayer);
    }
  }

  public virtual void DrawLayer()
  {
  }

  private bool Filled(int index)
  {
    Building building = ((MapDrawLayer) this).Map.edificeGrid[index];
    return building != null && ((Thing) building).def.Fillage == 2;
  }

  public virtual void Regenerate()
  {
    if (!VehicleMapUtility.get_IsVehicleMap(((MapDrawLayer) this).Map))
      return;
    LayerSubMesh subMesh = ((MapDrawLayer) this).GetSubMesh(SectionLayer_SnowOnVehicle.SnowMat);
    if (ModsConfig.BiotechActive)
      subMesh.material.SetTexture(ShaderPropertyIDs.PollutedTex, (Texture) SectionLayer_SnowOnVehicle.PollutedSnowTex.Texture);
    if (subMesh.mesh.vertexCount == 0)
    {
      SectionLayerGeometryMaker_Solid.MakeBaseGeometry(((SectionLayer) this).section, subMesh, (AltitudeLayer) 2);
      VehicleSectionLayerManager.FinalizeVerts((SectionLayer) this);
    }
    SectionLayer_SnowOnVehicle.opacityListTmp.Clear();
    subMesh.Clear((MeshParts) 4);
    NativeArray<float> nativeArray = SectionLayer_SnowOnVehicle.DepthGrid_Unsafe(((MapDrawLayer) this).Map.snowGrid);
    CellRect cellRect = ((SectionLayer) this).section.CellRect;
    bool flag = false;
    CellIndices cellIndices = ((MapDrawLayer) this).Map.cellIndices;
    for (int minX = cellRect.minX; minX <= cellRect.maxX; ++minX)
    {
      for (int minZ = cellRect.minZ; minZ <= cellRect.maxZ; ++minZ)
      {
        SectionLayer_SnowOnVehicle.opacityListTmp.Clear();
        float num1 = nativeArray[((CellIndices) ref cellIndices).CellToIndex(minX, minZ)];
        for (int index = 0; index < 9; ++index)
        {
          IntVec3 intVec3 = IntVec3.op_Addition(new IntVec3(minX, 0, minZ), GenAdj.AdjacentCellsAndInsideForUV[index]);
          this.adjValuesTmp[index] = GenGrid.InBounds(intVec3, ((MapDrawLayer) this).Map) ? nativeArray[((CellIndices) ref cellIndices).CellToIndex(intVec3)] : num1;
        }
        for (int index1 = 0; index1 < 9; ++index1)
        {
          float num2 = 0.0f;
          for (int index2 = 0; index2 < SectionLayer_Snow.vertexWeights[index1].Count; ++index2)
            num2 += this.adjValuesTmp[SectionLayer_Snow.vertexWeights[index1][index2]];
          float num3 = num2 / (float) SectionLayer_Snow.vertexWeights[index1].Count;
          if ((double) num3 > 0.0099999997764825821)
            flag = true;
          SectionLayer_SnowOnVehicle.opacityListTmp.Add(num3);
        }
        for (int index = 0; index < 9; ++index)
          this.adjValuesTmp[index] = ((MapDrawLayer) this).Map.pollutionGrid.IsPolluted(IntVec3.op_Addition(new IntVec3(minX, 0, minZ), GenAdj.AdjacentCellsAndInsideForUV[index])) ? 1f : 0.0f;
        for (int index3 = 0; index3 < 9; ++index3)
        {
          float num4 = 0.0f;
          for (int index4 = 0; index4 < SectionLayer_Snow.vertexWeights[index3].Count; ++index4)
            num4 += this.adjValuesTmp[SectionLayer_Snow.vertexWeights[index3][index4]];
          float num5 = num4 / (float) SectionLayer_Snow.vertexWeights[index3].Count;
          float num6 = SectionLayer_SnowOnVehicle.opacityListTmp[index3];
          subMesh.colors.Add(new Color32(Convert.ToByte(num5 * (float) byte.MaxValue), byte.MaxValue, byte.MaxValue, Convert.ToByte(num6 * (float) byte.MaxValue)));
        }
      }
    }
    if (flag)
    {
      subMesh.disabled = false;
      subMesh.FinalizeMesh((MeshParts) 4);
    }
    else
      subMesh.disabled = true;
  }
}
