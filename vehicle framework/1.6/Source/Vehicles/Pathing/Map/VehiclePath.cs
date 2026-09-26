// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclePath
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class VehiclePath : IDisposable
{
  private const int InitialPathSize = 128 /*0x80*/;
  private readonly List<IntVec3> nodes = new List<IntVec3>(128 /*0x80*/);

  public bool Found { get; private set; }

  public bool UsedHeuristics { get; private set; }

  public IntVec3 LastNode => this.nodes[0];

  public int Current { get; private set; }

  public int NodesLeft => this.Current + 1;

  public bool Finished => this.NodesLeft <= 0;

  public int NodesConsumedCount => this.nodes.Count - this.NodesLeft;

  public IReadOnlyList<IntVec3> Nodes => (IReadOnlyList<IntVec3>) this.nodes;

  public static VehiclePath NotFound => new VehiclePath();

  public void Init(bool usedHeuristics)
  {
    this.UsedHeuristics = usedHeuristics;
    this.Current = this.nodes.Count - 1;
    this.Found = true;
  }

  public void AddNode(IntVec3 cell) => this.nodes.Add(cell);

  public IntVec3 ConsumeNextNode()
  {
    IntVec3 intVec3 = this.Peek(1);
    --this.Current;
    return intVec3;
  }

  public IntVec3 Peek(int nodesAhead) => this.nodes[this.Current - nodesAhead];

  public void DrawPath(VehiclePawn vehicle)
  {
    if (!this.Found || this.Finished)
      return;
    float num = Altitudes.AltitudeFor((AltitudeLayer) 18);
    IntVec3 intVec3;
    for (int nodesAhead = 0; nodesAhead < this.NodesLeft - 1; ++nodesAhead)
    {
      intVec3 = this.Peek(nodesAhead);
      Vector3 vector3Shifted1 = ((IntVec3) ref intVec3).ToVector3Shifted();
      vector3Shifted1.y = num;
      intVec3 = this.Peek(nodesAhead + 1);
      Vector3 vector3Shifted2 = ((IntVec3) ref intVec3).ToVector3Shifted();
      vector3Shifted2.y = num;
      GenDraw.DrawLineBetween(vector3Shifted1, vector3Shifted2);
    }
    if (vehicle == null)
      return;
    Vector3 drawPos = ((Thing) vehicle).DrawPos;
    drawPos.y = num;
    intVec3 = this.Peek(0);
    Vector3 vector3Shifted = ((IntVec3) ref intVec3).ToVector3Shifted();
    vector3Shifted.y = num;
    Vector3 vector3 = Vector3.op_Subtraction(drawPos, vector3Shifted);
    if ((double) ((Vector3) ref vector3).sqrMagnitude <= 0.0099999997764825821)
      return;
    GenDraw.DrawLineBetween(drawPos, vector3Shifted);
  }

  public void Dispose()
  {
    this.Current = -1;
    this.UsedHeuristics = false;
    this.Found = false;
    this.nodes.Clear();
    AsyncPool<VehiclePath>.Return(this);
  }
}
