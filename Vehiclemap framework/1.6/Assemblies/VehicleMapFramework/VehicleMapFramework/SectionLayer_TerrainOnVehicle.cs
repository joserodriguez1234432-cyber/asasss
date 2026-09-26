// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SectionLayer_TerrainOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class SectionLayer_TerrainOnVehicle(Section section) : SectionLayer_Terrain(section)
{
  private Material baseTerrainMat;
  private static readonly Dictionary<Material, Material> terrainMatCache = new Dictionary<Material, Material>();

  public void DrawLayer(Vector3 drawPos)
  {
    VehiclePawnWithMap vehicle;
    if (!((MapDrawLayer) this).Visible || !((MapDrawLayer) this).Map.IsVehicleMapOf(out vehicle))
      return;
    Quaternion quaternion = Quaternion.AngleAxis(VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle), Vector3.up);
    for (int index = 0; index < ((MapDrawLayer) this).subMeshes.Count; ++index)
    {
      LayerSubMesh subMesh = ((MapDrawLayer) this).subMeshes[index];
      if (subMesh.finalized && !subMesh.disabled && Object.op_Inequality((Object) subMesh.material, (Object) MatBases.ShadowMask))
        Graphics.DrawMesh(subMesh.mesh, drawPos, quaternion, subMesh.material, subMesh.renderLayer);
    }
  }

  public virtual void DrawLayer()
  {
    VehiclePawnWithMap vehicle;
    if (!((MapDrawLayer) this).Map.IsVehicleMapOf(out vehicle))
      return;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector((float) vehicle.VehicleMap.Size.x, 0.0f, (float) vehicle.VehicleMap.Size.z);
    Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(Vector3.op_Division(vector3, 2f), Quaternion.identity, vector3), this.baseTerrainMat, 0);
  }

  public virtual Material GetMaterialFor(CellTerrain cellTerrain)
  {
    return SectionLayer_TerrainOnVehicle.GetMaterialWithZ(base.GetMaterialFor(cellTerrain));
  }

  public static Material GetMaterialWithZ(Material source)
  {
    Material materialWithZ;
    if (!SectionLayer_TerrainOnVehicle.terrainMatCache.TryGetValue(source, out materialWithZ))
    {
      Material material = new Material(source)
      {
        shader = VMF_DefOf.VMF_TerrainHardWithZ.Shader
      };
      materialWithZ = SectionLayer_TerrainOnVehicle.terrainMatCache[source] = material;
    }
    return materialWithZ;
  }

  public virtual void Regenerate()
  {
    VehiclePawnWithMap vehicle;
    if (!((MapDrawLayer) this).Map.IsVehicleMapOf(out vehicle))
      return;
    this.baseTerrainMat = SolidColorMaterials.NewSolidColorMaterial(((Thing) vehicle).DrawColor, ShaderDatabase.TerrainHard);
    base.Regenerate();
  }
}
