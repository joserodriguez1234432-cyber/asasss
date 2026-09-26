// Decompiled with JetBrains decompiler
// Type: Vehicles.GraphicOverlayRenderer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Animations;
using SmashTools.Rendering;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class GraphicOverlayRenderer
{
  private readonly VehiclePawn vehicle;
  [TweakField]
  [AnimationProperty(Name = "Overlays")]
  private readonly List<GraphicOverlay> overlays = new List<GraphicOverlay>();
  [TweakField]
  private readonly List<GraphicOverlay> extraOverlays = new List<GraphicOverlay>();
  private readonly Dictionary<string, List<GraphicOverlay>> extraOverlayLookup = new Dictionary<string, List<GraphicOverlay>>();

  public GraphicOverlayRenderer(VehiclePawn vehicle) => this.vehicle = vehicle;

  public List<GraphicOverlay> Overlays => this.overlays;

  public List<GraphicOverlay> ExtraOverlays => this.extraOverlays;

  public List<GraphicOverlay> AllOverlaysListForReading { get; } = new List<GraphicOverlay>();

  private List<GraphicOverlay> RotatorOverlays { get; } = new List<GraphicOverlay>();

  public void Init()
  {
    if (GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) this.vehicle.VehicleDef.drawProperties.overlays))
      return;
    this.overlays.Clear();
    foreach (GraphicDataOverlay graphicOverlay1 in this.vehicle.VehicleDef.drawProperties.graphicOverlays)
    {
      GraphicOverlay graphicOverlay2 = GraphicOverlay.Create(graphicOverlay1, this.vehicle);
      this.overlays.Add(graphicOverlay2);
      this.AllOverlaysListForReading.Add(graphicOverlay2);
      this.vehicle.DrawTracker.AddRenderer((IParallelRenderer) graphicOverlay2);
    }
    this.RecacheRotatorOverlays();
  }

  private void RecacheRotatorOverlays()
  {
    this.RotatorOverlays.Clear();
    foreach (GraphicOverlay graphicOverlay in this.AllOverlaysListForReading)
    {
      if (graphicOverlay.Graphic is Graphic_Rotator)
        this.RotatorOverlays.Add(graphicOverlay);
    }
  }

  public void AddOverlay(string key, GraphicOverlay graphicOverlay)
  {
    this.extraOverlayLookup.AddOrAppend<string, List<GraphicOverlay>, GraphicOverlay>(key, graphicOverlay);
    this.extraOverlays.Add(graphicOverlay);
    this.AllOverlaysListForReading.Add(graphicOverlay);
    this.vehicle.DrawTracker.AddRenderer((IParallelRenderer) graphicOverlay);
    this.RecacheRotatorOverlays();
  }

  public void RemoveOverlays(string key)
  {
    if (!this.extraOverlayLookup.ContainsKey(key))
      return;
    foreach (GraphicOverlay graphicOverlay in this.extraOverlayLookup[key])
    {
      this.extraOverlays.Remove(graphicOverlay);
      this.AllOverlaysListForReading.Remove(graphicOverlay);
      this.vehicle.DrawTracker.RemoveRenderer((IParallelRenderer) graphicOverlay);
      graphicOverlay.Destroy();
    }
    this.extraOverlayLookup.Remove(key);
    this.RecacheRotatorOverlays();
  }

  public void SetAcceleration(float rotation)
  {
    foreach (GraphicOverlay rotatorOverlay in this.RotatorOverlays)
    {
      rotatorOverlay.acceleration = ((Graphic_Rotator) rotatorOverlay.Graphic).ModifyIncomingRotation(rotation);
      rotatorOverlay.Transform.rotation = (rotatorOverlay.Transform.rotation + rotatorOverlay.acceleration).ClampAngle();
    }
  }
}
