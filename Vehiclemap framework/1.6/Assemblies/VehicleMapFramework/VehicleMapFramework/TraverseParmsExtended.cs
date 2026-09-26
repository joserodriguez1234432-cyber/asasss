// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TraverseParmsExtended
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public struct TraverseParmsExtended : IEquatable<TraverseParmsExtended>
{
  public TraverseParms traverseParms;
  public AbilityDef ability;

  public static implicit operator TraverseParmsExtended(TraverseParms m)
  {
    return new TraverseParmsExtended() { traverseParms = m };
  }

  public static bool operator ==(TraverseParmsExtended a, TraverseParmsExtended b)
  {
    return TraverseParms.op_Equality(a.traverseParms, b.traverseParms) && a.ability == b.ability;
  }

  public static bool operator !=(TraverseParmsExtended a, TraverseParmsExtended b) => !(a == b);

  public override bool Equals(object obj)
  {
    return obj is TraverseParmsExtended other && this.Equals(other);
  }

  public bool Equals(TraverseParmsExtended other)
  {
    return ((TraverseParms) ref this.traverseParms).Equals(other.traverseParms) && this.ability == other.ability;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine<TraverseParms, AbilityDef>(this.traverseParms, this.ability);
  }
}
