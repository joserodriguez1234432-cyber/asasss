// Decompiled with JetBrains decompiler
// Type: Vehicles.GraphicDataLayered
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class GraphicDataLayered : GraphicData
{
  public const int SubLayerCount = 10;
  private int layer;
  public AltitudeLayer? altLayerSpawned;
  private Vector3 originalDrawOffset;
  private Vector3? originalDrawOffsetNorth;
  private Vector3? originalDrawOffsetEast;
  private Vector3? originalDrawOffsetSouth;
  private Vector3? originalDrawOffsetWest;

  public virtual void CopyFrom(GraphicDataLayered graphicData)
  {
    this.CopyFrom((GraphicData) graphicData);
    this.layer = graphicData.layer;
    this.CacheDrawOffsets();
    this.RecacheLayerOffsets();
  }

  public void PostLoad() => this.CacheDrawOffsets();

  public virtual void Init(IMaterialCacheTarget target) => this.RecacheLayerOffsets();

  private void CacheDrawOffsets()
  {
    this.originalDrawOffset = this.drawOffset;
    this.originalDrawOffsetNorth = this.drawOffsetNorth;
    this.originalDrawOffsetEast = this.drawOffsetEast;
    this.originalDrawOffsetSouth = this.drawOffsetSouth;
    this.originalDrawOffsetWest = this.drawOffsetWest;
  }

  public void RecacheLayerOffsets()
  {
    if (this.layer == 0)
      return;
    float num = (float) this.layer * 0.00365853682f;
    this.drawOffset = this.originalDrawOffset;
    this.drawOffset.y += num;
    if (this.drawOffsetNorth.HasValue)
    {
      this.drawOffsetNorth = new Vector3?(this.originalDrawOffsetNorth.Value);
      this.drawOffsetNorth = new Vector3?(new Vector3(this.drawOffsetNorth.Value.x, this.drawOffsetNorth.Value.y + num, this.drawOffsetNorth.Value.z));
    }
    if (this.drawOffsetEast.HasValue)
    {
      this.drawOffsetEast = new Vector3?(this.originalDrawOffsetEast.Value);
      this.drawOffsetEast = new Vector3?(new Vector3(this.drawOffsetEast.Value.x, this.drawOffsetEast.Value.y + num, this.drawOffsetEast.Value.z));
    }
    if (this.drawOffsetSouth.HasValue)
    {
      this.drawOffsetSouth = new Vector3?(this.originalDrawOffsetSouth.Value);
      this.drawOffsetSouth = new Vector3?(new Vector3(this.drawOffsetSouth.Value.x, this.drawOffsetSouth.Value.y + num, this.drawOffsetSouth.Value.z));
    }
    if (!this.drawOffsetWest.HasValue)
      return;
    this.drawOffsetWest = new Vector3?(this.originalDrawOffsetWest.Value);
    this.drawOffsetWest = new Vector3?(new Vector3(this.drawOffsetWest.Value.x, this.drawOffsetWest.Value.y + num, this.drawOffsetWest.Value.z));
  }
}
