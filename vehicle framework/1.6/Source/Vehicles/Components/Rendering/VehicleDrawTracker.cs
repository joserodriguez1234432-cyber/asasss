// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.VehicleDrawTracker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using SmashTools.Animations;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.Rendering;

[PublicAPI]
public class VehicleDrawTracker
{
  private readonly VehiclePawn vehicle;
  public readonly VehicleRenderer renderer;
  [AnimationProperty]
  [TweakField]
  public GraphicOverlayRenderer overlayRenderer;
  public readonly VehicleTweener tweener;
  public VehicleTrackMaker trackMaker;
  public Vehicle_RecoilTracker recoilTracker;
  private readonly List<IParallelRenderer> parallelRenderers = new List<IParallelRenderer>();

  public VehicleDrawTracker(VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    this.tweener = new VehicleTweener(vehicle);
    this.renderer = new VehicleRenderer(vehicle);
    this.overlayRenderer = new GraphicOverlayRenderer(vehicle);
    this.trackMaker = new VehicleTrackMaker(vehicle);
    this.recoilTracker = new Vehicle_RecoilTracker();
    this.AddRenderer((IParallelRenderer) this.renderer);
  }

  private bool RenderersInitialized { get; set; }

  internal IReadOnlyList<IParallelRenderer> ParallelRenderers
  {
    get => (IReadOnlyList<IParallelRenderer>) this.parallelRenderers;
  }

  public Vector3 DrawPos
  {
    get
    {
      this.tweener.PreDrawPosCalculation();
      Vector3 pos = this.tweener.TweenedPos;
      pos.y = ((BuildableDef) ((Thing) this.vehicle).def).Altitude;
      if ((double) this.recoilTracker.Recoil > 0.0)
        pos = pos.PointFromAngle(this.recoilTracker.Recoil, this.recoilTracker.Angle);
      return pos;
    }
  }

  public void AddRenderer(IParallelRenderer parallelRenderer)
  {
    parallelRenderer.SetDirty();
    this.parallelRenderers.Add(parallelRenderer);
  }

  public void RemoveRenderer(IParallelRenderer parallelRenderer)
  {
    this.parallelRenderers.Remove(parallelRenderer);
  }

  public void DynamicDrawPhaseAt(DrawPhase phase, in Vector3 drawLoc, Rot8 rot, float rotation)
  {
    TransformData transformData = new TransformData(drawLoc, rot, rotation);
    foreach (IParallelRenderer parallelRenderer in this.parallelRenderers)
    {
      if (phase != null)
      {
        if (phase - 1 > 1)
          throw new NotImplementedException("DrawPhase");
        parallelRenderer.DynamicDrawPhaseAt(phase, in transformData);
      }
      else if (parallelRenderer.IsDirty)
      {
        parallelRenderer.DynamicDrawPhaseAt(phase, in transformData);
        parallelRenderer.IsDirty = false;
      }
    }
  }

  public void ProcessPostTickVisuals(int ticksPassed)
  {
    if (!((Thing) this.vehicle).Spawned)
      return;
    this.trackMaker.ProcessPostTickVisuals(ticksPassed);
    this.recoilTracker.ProcessPostTickVisuals(ticksPassed);
  }

  public void Notify_Spawned() => this.tweener.ResetTweenedPosToRoot();
}
