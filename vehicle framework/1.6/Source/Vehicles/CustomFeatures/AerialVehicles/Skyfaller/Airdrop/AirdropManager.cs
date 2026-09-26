// Decompiled with JetBrains decompiler
// Type: Vehicles.AirdropManager
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable enable
namespace Vehicles;

public class AirdropManager : MapComponent
{
  private 
  #nullable disable
  List<DropShip> dropShips = new List<DropShip>();
  private List<Sustainer> sustainers = new List<Sustainer>();
  private readonly AirdropManager.EdgePoints[] dropZones = new AirdropManager.EdgePoints[4];

  public AirdropManager(Map map)
    : base(map)
  {
    this.CalculateDropZones(map);
  }

  public void Spawn(DropShip dropShip)
  {
    this.dropShips.Add(dropShip);
    dropShip.OnSpawned();
  }

  public void Remove(DropShip dropShip) => this.dropShips.Remove(dropShip);

  public DropZone GetDropZoneFor(Rot4 edge, int points)
  {
    IntVec3 randomPoint1 = this.dropZones[((Rot4) ref edge).AsInt].RandomPoint;
    AirdropManager.EdgePoints[] dropZones = this.dropZones;
    Rot4 opposite = ((Rot4) ref edge).Opposite;
    int asInt = ((Rot4) ref opposite).AsInt;
    IntVec3 randomPoint2 = dropZones[asInt].RandomPoint;
    return new DropZone(randomPoint1, randomPoint2, points);
  }

  private void CalculateDropZones(Map map)
  {
    this.dropZones[0] = new AirdropManager.EdgePoints(Rot4.North, map.Size.x);
    this.dropZones[1] = new AirdropManager.EdgePoints(Rot4.East, map.Size.z);
    this.dropZones[2] = new AirdropManager.EdgePoints(Rot4.South, map.Size.x);
    this.dropZones[3] = new AirdropManager.EdgePoints(Rot4.West, map.Size.z);
  }

  public virtual void MapComponentTick()
  {
    for (int index = this.dropShips.Count - 1; index >= 0; --index)
    {
      try
      {
        this.dropShips[index].Tick();
      }
      catch
      {
        this.dropShips.RemoveAt(index);
        throw;
      }
    }
  }

  public virtual void ExposeData()
  {
    Scribe_Collections.Look<DropShip>(ref this.dropShips, "dropShips", false, (LookMode) 2, Array.Empty<object>());
  }

  private record EdgePoints
  {
    private const int CellsPerAnchor = 50;
    private readonly Rot4 edge;
    private readonly IntVec3[] anchors;

    public EdgePoints(Rot4 edge, int length)
    {
      this.edge = edge;
      int count = Mathf.CeilToInt((float) length / 50f) - 1;
      this.anchors = new IntVec3[count + 2];
      this.CalculateAnchors(count, length);
    }

    public IntVec3 RandomPoint => this.anchors[Rand.Range(0, this.anchors.Length)];

    private void CalculateAnchors(int count, int length)
    {
      for (int index = 0; index <= count; ++index)
      {
        float num = (float) index / (float) count;
        int pos = Mathf.CeilToInt(Mathf.Lerp(0.0f, (float) length, num));
        SetPos(this.anchors, this.edge, index, pos, length);
      }

      static void SetPos(IntVec3[] array, Rot4 rot, int index, int pos, int length)
      {
        IntVec3[] intVec3Array = array;
        int index1 = index;
        IntVec3 intVec3;
        switch (((Rot4) ref rot).AsInt)
        {
          case 0:
            intVec3 = new IntVec3(pos, 0, length);
            break;
          case 1:
            intVec3 = new IntVec3(length, 0, pos);
            break;
          case 2:
            intVec3 = new IntVec3(pos, 0, 0);
            break;
          case 3:
            intVec3 = new IntVec3(0, 0, pos);
            break;
          default:
            throw new NotImplementedException(nameof (rot));
        }
        intVec3Array[index1] = intVec3;
      }
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(
    #nullable enable
    StringBuilder builder)
    {
      RuntimeHelpers.EnsureSufficientExecutionStack();
      builder.Append("RandomPoint = ");
      builder.Append(this.RandomPoint.ToString());
      return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      return (EqualityComparer<System.Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<Rot4>.Default.GetHashCode(this.edge)) * -1521134295 + EqualityComparer<IntVec3[]>.Default.GetHashCode(this.anchors);
    }

    [CompilerGenerated]
    public virtual bool Equals(AirdropManager.EdgePoints? other)
    {
      if ((object) this == (object) other)
        return true;
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<Rot4>.Default.Equals(this.edge, other.edge) && EqualityComparer<IntVec3[]>.Default.Equals(this.anchors, other.anchors);
    }

    [CompilerGenerated]
    protected EdgePoints(AirdropManager.EdgePoints original)
    {
      this.edge = original.edge;
      this.anchors = original.anchors;
    }
  }
}
