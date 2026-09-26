// Decompiled with JetBrains decompiler
// Type: Vehicles.FlyingObject
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using SmashTools.Rendering;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class FlyingObject : MoteThrown
{
  private readonly List<FlyingObject.DrawProps> objects = new List<FlyingObject.DrawProps>();

  public void Add(IParallelRenderer renderer, Rot8 orientation, float rotation)
  {
    this.objects.Add(new FlyingObject.DrawProps(renderer, orientation, rotation));
  }

  public void Launch(Map map, Vector3 rootPos, float rotationRate, float speed, float angle)
  {
    ((Mote) this).rotationRate = rotationRate;
    ((Mote) this).exactPosition = rootPos;
    this.SetVelocity(angle, speed);
    GenSpawn.Spawn((Thing) this, IntVec3Utility.ToIntVec3(rootPos), map, (WipeMode) 0);
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    ((Mote) this).exactPosition.y = Altitudes.AltitudeFor(((BuildableDef) ((Thing) this).def).altitudeLayer);
    foreach (FlyingObject.DrawProps drawProps in this.objects)
    {
      TransformData transformData = new TransformData(((Mote) this).exactPosition, drawProps.orientation, drawProps.rotation + ((Mote) this).exactRotation);
      drawProps.renderer.DynamicDrawPhaseAt((DrawPhase) 2, in transformData, true);
    }
  }

  private readonly struct DrawProps(IParallelRenderer renderer, Rot8 orientation, float rotation)
  {
    public readonly IParallelRenderer renderer = renderer;
    public readonly Rot8 orientation = orientation;
    public readonly float rotation = rotation;
  }
}
