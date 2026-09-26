// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.VehicleRenderer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools.Rendering;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.Rendering;

public sealed class VehicleRenderer : IParallelRenderer
{
  private readonly VehiclePawn vehicle;
  private PreRenderResults results;

  public VehicleRenderer(VehiclePawn vehicle) => this.vehicle = vehicle;

  [Obsolete("Not currently implemented, still WIP. Do not reference.", true)]
  public PawnFirefoamDrawer FirefoamOverlays => throw new NotImplementedException();

  bool IParallelRenderer.IsDirty { get; set; }

  public void DynamicDrawPhaseAt(DrawPhase phase, in TransformData transformData, bool forceDraw = false)
  {
    switch ((int) phase)
    {
      case 0:
        for (int index = 0; index < 4; ++index)
          ((Graphic) this.vehicle.VehicleGraphic).MeshAt(new Rot4(index));
        break;
      case 1:
        this.results = this.ParallelGetPreRenderResults(ref transformData);
        break;
      case 2:
        if (!this.results.valid)
          this.results = this.ParallelGetPreRenderResults(ref transformData);
        this.Draw();
        this.results = new PreRenderResults();
        break;
      default:
        throw new NotImplementedException();
    }
  }

  private PreRenderResults ParallelGetPreRenderResults(
    [RequiresLocation, In] ref TransformData transformData,
    bool forceDraw = false)
  {
    return this.vehicle.VehicleGraphic.ParallelGetPreRenderResults(ref transformData, forceDraw, (Thing) this.vehicle);
  }

  private void Draw()
  {
    Graphics.DrawMesh(this.results.mesh, this.results.position, this.results.quaternion, this.results.material, 0);
    ((Graphic) this.vehicle.VehicleGraphic.ShadowGraphic)?.Draw(this.results.position, (Rot4) this.vehicle.FullRotation, (Thing) this.vehicle, 0.0f);
    if (!((Thing) this.vehicle).Spawned || this.vehicle.Dead)
      return;
    this.vehicle.vehiclePather.PatherDraw();
  }

  void IParallelRenderer.DynamicDrawPhaseAt(
    DrawPhase phase,
    in TransformData transformData,
    bool forceDraw = false)
  {
    this.DynamicDrawPhaseAt(phase, in transformData, forceDraw);
  }
}
