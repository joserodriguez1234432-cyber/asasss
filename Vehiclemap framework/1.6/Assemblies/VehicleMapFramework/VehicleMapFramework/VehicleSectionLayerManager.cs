// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleSectionLayerManager
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleSectionLayerManager(Map map) : MapComponent(map)
{
  private Dictionary<Section, Dictionary<Type, SectionLayer[]>> layersByRot;
  private Rot4 lastGeneratedRots = Rot4.North;
  internal static readonly List<Type> OrientedSectionLayerTypes = GenTypes.AllSubclassesNonAbstract(typeof (SectionLayer_Things)).Append<Type>(typeof (SectionLayer_SunShadowsOnVehicle)).ToList<Type>();
  public static readonly List<ModCompat.CompatBase> CompatClassesForDrawLayers = new List<ModCompat.CompatBase>();

  [UsedImplicitly]
  public static Rot4 RotForPrintCounter
  {
    get
    {
      Rot4 rotForPrint1 = VehicleSectionLayerManager.RotForPrint;
      if (!((Rot4) ref rotForPrint1).IsHorizontal)
        return VehicleSectionLayerManager.RotForPrint;
      Rot4 rotForPrint2 = VehicleSectionLayerManager.RotForPrint;
      return ((Rot4) ref rotForPrint2).Opposite;
    }
  }

  public static Rot4 RotForPrint { get; set; }

  public static bool CacheMode { get; set; }

  public virtual void FinalizeInit()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      if (!VehicleMapUtility.get_IsVehicleMap(this.map))
        return;
      this.layersByRot = new Dictionary<Section, Dictionary<Type, SectionLayer[]>>();
      for (int index1 = 0; index1 < this.map.Size.x; index1 += 17)
      {
        for (int index2 = 0; index2 < this.map.Size.z; index2 += 17)
        {
          Section key = this.map.mapDrawer.SectionAt(new IntVec3(index1, 0, index2));
          this.layersByRot[key] = new Dictionary<Type, SectionLayer[]>();
          foreach (Type type in GenTypes.AllSubclassesNonAbstract(typeof (SectionLayer)))
          {
            SectionLayer layer = key.GetLayer(type);
            if (layer != null)
            {
              if (VehicleSectionLayerManager.OrientedSectionLayerTypes.Contains(type))
              {
                this.layersByRot[key][type] = new SectionLayer[4]
                {
                  layer,
                  (SectionLayer) Activator.CreateInstance(type, (object) key),
                  (SectionLayer) Activator.CreateInstance(type, (object) key),
                  (SectionLayer) Activator.CreateInstance(type, (object) key)
                };
                for (int index3 = 0; index3 < 4; ++index3)
                  ((MapDrawLayer) this.layersByRot[key][type][index3]).Dirty = true;
              }
              else
                this.layersByRot[key][type] = new SectionLayer[1]
                {
                  layer
                };
            }
          }
        }
      }
    }));
  }

  public SectionLayer GetLayer(Section section, Type type, Rot8 rot)
  {
    SectionLayer[] sectionLayerArray;
    if (!this.layersByRot[section].TryGetValue(type, out sectionLayerArray))
      return (SectionLayer) null;
    if (!VehicleSectionLayerManager.OrientedSectionLayerTypes.Contains(type))
      return sectionLayerArray[0];
    Rot4 rot1 = rot.RotForVehicleDraw();
    SectionLayer layer = sectionLayerArray[((Rot4) ref rot1).AsInt];
    if (((MapDrawLayer) layer).Dirty)
    {
      try
      {
        VehicleSectionLayerManager.CacheMode = true;
        VehicleSectionLayerManager.RotForPrint = rot1;
        this.DirtyAdaptiveStorageGraphics(section, rot1);
        ((MapDrawLayer) layer).Regenerate();
        ((MapDrawLayer) layer).RefreshSubMeshBounds();
      }
      catch (Exception ex)
      {
        Log.Error($"Could not regenerate layer {Gen.ToStringSafe<SectionLayer>(layer)}: {ex}");
      }
      finally
      {
        VehicleSectionLayerManager.CacheMode = false;
        VehicleSectionLayerManager.RotForPrint = Rot4.North;
        ((MapDrawLayer) layer).Dirty = false;
      }
    }
    return layer;
  }

  public void UpdateAllSection()
  {
    Section[,] sectionArray = VehiclePawnWithMap.sections.Invoke(this.map.mapDrawer);
    int upperBound1 = sectionArray.GetUpperBound(0);
    int upperBound2 = sectionArray.GetUpperBound(1);
    for (int lowerBound1 = sectionArray.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
    {
      for (int lowerBound2 = sectionArray.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
      {
        Section section = sectionArray[lowerBound1, lowerBound2];
        this.UpdateSection(section);
        if ((section.dirtyFlags & MapMeshFlagDef.op_Implicit(MapMeshFlagDefOf.Buildings)) > 0UL)
          FrameDelay.DelayOne<SectionLayer>((Action<SectionLayer>) (layer => VehicleSectionLayerManager.FinalizeVerts(layer)), this.GetLayer(section, typeof (SectionLayer_EdgeShadows), new Rot8()));
      }
    }
  }

  private void UpdateSection(Section section)
  {
    if (section.dirtyFlags == 0UL)
      return;
    foreach (KeyValuePair<Type, SectionLayer[]> keyValuePair in this.layersByRot[section])
    {
      if (VehicleSectionLayerManager.OrientedSectionLayerTypes.Contains(keyValuePair.Key))
      {
        SectionLayer sectionLayer = keyValuePair.Value[0];
        ((MapDrawLayer) sectionLayer).Dirty = ((MapDrawLayer) sectionLayer).Dirty || (section.dirtyFlags & ((MapDrawLayer) sectionLayer).relevantChangeTypes) > 0UL;
        if (((MapDrawLayer) sectionLayer).Dirty)
        {
          this.DirtyAdaptiveStorageGraphics(section, Rot4.North);
          for (int index = 1; index < 4; ++index)
            ((MapDrawLayer) keyValuePair.Value[index]).Dirty = true;
        }
      }
    }
  }

  private static void TryRegenerate(SectionLayer layer, Rot4 rot)
  {
    try
    {
      VehicleSectionLayerManager.CacheMode = true;
      VehicleSectionLayerManager.RotForPrint = rot;
      ((MapDrawLayer) layer).Regenerate();
      ((MapDrawLayer) layer).RefreshSubMeshBounds();
    }
    catch (Exception ex)
    {
      Log.Error($"Could not regenerate layer {Gen.ToStringSafe<SectionLayer>(layer)}: {ex}");
    }
    finally
    {
      VehicleSectionLayerManager.CacheMode = false;
      VehicleSectionLayerManager.RotForPrint = Rot4.North;
      ((MapDrawLayer) layer).Dirty = false;
    }
  }

  private void DirtyAdaptiveStorageGraphics(Section section, Rot4 rot)
  {
    if (!ModCompat.CompatBase<ModCompat.AdaptiveStorage>.Active || Rot4.op_Equality(rot, this.lastGeneratedRots))
      return;
    this.lastGeneratedRots = rot;
    CellRect cellRect = section.CellRect;
    foreach (IntVec3 intVec3 in cellRect)
    {
      List<Thing> thingList = this.map.thingGrid.ThingsListAt(intVec3);
      int count = thingList.Count;
      for (int index = 0; index < count; ++index)
      {
        Thing thing = thingList[index];
        if (ModCompat.AdaptiveStorage.IsAdaptiveStorageClass(thing.def.thingClass) && (thing.def.seeThroughFog || !this.map.fogGrid.IsFogged(thing.Position)) && thing.def.drawerType != null && thing.def.drawerType != 1 && ((double) thing.def.hideAtSnowOrSandDepth >= 1.0 || (double) Math.Max(this.map.snowGrid.GetDepth(thing.Position), GridsUtility.GetSandDepth(thing.Position, this.map)) <= (double) thing.def.hideAtSnowOrSandDepth) && (thing.def.plant == null || thing.def.plant.showInFrozenWater || GridsUtility.GetTerrain(thing.Position, this.map) != TerrainDefOf.ThinIce) && thing.Position.x == intVec3.x && thing.Position.z == intVec3.z)
        {
          object obj = ModCompat.AdaptiveStorage.Renderer.Invoke((object) thing, Array.Empty<object>());
          if (obj != null)
            ModCompat.AdaptiveStorage.SetAllPrintDatasDirty.Invoke(obj, Array.Empty<object>());
        }
      }
    }
  }

  public static void FinalizeVerts(SectionLayer layer)
  {
    LayerSubMesh layerSubMesh = GenCollection.FirstOrDefault<LayerSubMesh>(((MapDrawLayer) layer).subMeshes, (Predicate<LayerSubMesh>) (subMesh => subMesh.finalized));
    if (layerSubMesh == null)
      return;
    for (int index = 0; index < layerSubMesh.verts.Count; ++index)
    {
      Vector3 vert = layerSubMesh.verts[index];
      vert.y /= 39.9999962f;
      layerSubMesh.verts[index] = vert;
    }
    layerSubMesh.mesh.SetVertices(layerSubMesh.verts);
  }

  public static void DrawLayer(
    VehicleSectionLayerManager component,
    Section section,
    Type layerType,
    Vector3 drawPos,
    Rot8 rot,
    float angle)
  {
    if ((object) layerType == null)
      return;
    SectionLayer layer = component.GetLayer(section, layerType, rot);
    if (layer == null)
      return;
    VehicleSectionLayerManager.DrawLayer(layer, drawPos, angle);
  }

  public static void DrawLayer(SectionLayer layer, Vector3 drawPos, float angle)
  {
    if (!((MapDrawLayer) layer).Visible)
      return;
    Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
    for (int index = 0; index < ((MapDrawLayer) layer).subMeshes.Count; ++index)
    {
      LayerSubMesh subMesh = ((MapDrawLayer) layer).subMeshes[index];
      if (subMesh.finalized && !subMesh.disabled)
        Graphics.DrawMesh(subMesh.mesh, drawPos, quaternion, subMesh.material, subMesh.renderLayer);
    }
  }
}
