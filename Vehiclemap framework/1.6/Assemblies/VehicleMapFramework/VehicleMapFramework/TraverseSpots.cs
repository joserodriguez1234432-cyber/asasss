// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TraverseSpots
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public record struct TraverseSpots(TargetInfo exitSpot, TargetInfo enterSpot)
{
  public TargetInfo exitSpot = exitSpot;
  public TargetInfo enterSpot = enterSpot;

  [CompilerGenerated]
  public override readonly int GetHashCode()
  {
    return EqualityComparer<TargetInfo>.Default.GetHashCode(this.exitSpot) * -1521134295 + EqualityComparer<TargetInfo>.Default.GetHashCode(this.enterSpot);
  }

  [CompilerGenerated]
  public readonly bool Equals(TraverseSpots other)
  {
    return EqualityComparer<TargetInfo>.Default.Equals(this.exitSpot, other.exitSpot) && EqualityComparer<TargetInfo>.Default.Equals(this.enterSpot, other.enterSpot);
  }

  [CompilerGenerated]
  public readonly void Deconstruct(out TargetInfo exitSpot, out TargetInfo enterSpot)
  {
    exitSpot = this.exitSpot;
    enterSpot = this.enterSpot;
  }
}
