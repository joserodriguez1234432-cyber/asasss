// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegionLink
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleRegionLink : IPoolable
{
  private const float WeightColorCeiling = 30f;
  public VehicleRegion regionA;
  public VehicleRegion regionB;
  public EdgeSpan span;
  public IntVec3 anchor;
  public IntVec3 portal;
  private static readonly LinearPool<SimpleColor> ColorWeights = new LinearPool<SimpleColor>()
  {
    range = new FloatRange(0.0f, 30f),
    items = new List<SimpleColor>(6)
    {
      (SimpleColor) 0,
      (SimpleColor) 2,
      (SimpleColor) 5,
      (SimpleColor) 7,
      (SimpleColor) 1,
      (SimpleColor) 4
    }
  };

  public bool InPool { get; set; }

  public bool IsValid => this.regionA != null || this.regionB != null;

  public IntVec3 Root => this.span.root;

  public IntVec3 End => VehicleRegionLink.SpanEnd(in this.span);

  public void SetNew(EdgeSpan span)
  {
    this.Reset();
    this.span = span;
    this.anchor = VehicleRegionCostCalculator.RegionLinkCenter(this);
  }

  public void Reset()
  {
    this.regionA = (VehicleRegion) null;
    this.regionB = (VehicleRegion) null;
  }

  public void Register(VehicleRegion region, Rot4 dir)
  {
    if (this.regionA == region || this.regionB == region)
      return;
    if (this.regionA == null || !this.regionA.valid)
    {
      this.regionA = region;
    }
    else
    {
      if (this.regionB != null && this.regionB.valid)
        return;
      this.regionB = region;
    }
  }

  public void Deregister(VehicleRegion region)
  {
    if (this.regionA == region)
    {
      this.regionA = (VehicleRegion) null;
    }
    else
    {
      if (this.regionB != region)
        return;
      this.regionB = (VehicleRegion) null;
    }
  }

  public bool LinksRegions(VehicleRegion regionA, VehicleRegion regionB)
  {
    if (this.regionA == regionA && this.regionB == regionB)
      return true;
    return this.regionA == regionB && this.regionB == regionA;
  }

  public void DrawWeight(Map map, VehicleRegionLink regionLink, float weight, int duration = 50)
  {
  }

  public VehicleRegion GetOtherRegion(VehicleRegion region)
  {
    return region == this.regionA ? this.regionB : this.regionA;
  }

  public VehicleRegion GetInFacingRegion(VehicleRegionLink regionLink)
  {
    if (this.regionA == regionLink.regionA || this.regionA == regionLink.regionB)
      return this.regionA;
    if (this.regionB == regionLink.regionB || this.regionB == regionLink.regionA)
      return this.regionB;
    Log.Warning($"Attempting to fetch region between links {this.Root} and {regionLink.Root}, " + $"but they do not share a region.\n--- Regions ---\n{this.regionA}\n{this.regionB}\n" + $"{regionLink.regionA}\n{regionLink.regionB}\n");
    return (VehicleRegion) null;
  }

  public ulong UniqueHashCode() => ((EdgeSpan) ref this.span).UniqueHashCode();

  public override string ToString()
  {
    return $"({this.regionA.Id},{this.regionB.Id}, regions=[spawn={this.span}, hash={this.UniqueHashCode()}])";
  }

  public static SimpleColor WeightColor(float weight)
  {
    return VehicleRegionLink.ColorWeights.Evaluate(weight);
  }

  private static IntVec3 SpanEnd(in EdgeSpan edgeSpan)
  {
    SpanDirection dir = edgeSpan.dir;
    if (dir == null)
      return new IntVec3(edgeSpan.root.x, 0, edgeSpan.root.z + edgeSpan.length);
    if (dir == 1)
      return new IntVec3(edgeSpan.root.x + edgeSpan.length, 0, edgeSpan.root.z);
    throw new ArgumentException("dir");
  }
}
