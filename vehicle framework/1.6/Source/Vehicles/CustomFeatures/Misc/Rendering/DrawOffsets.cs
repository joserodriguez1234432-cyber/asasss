// Decompiled with JetBrains decompiler
// Type: Vehicles.DrawOffsets
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class DrawOffsets
{
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3 defaultOffset;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? north;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? east;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? south;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? west;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? northEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? southEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? southWest;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? northWest;

  public Vector3 OffsetFor(Rot8 rot)
  {
    switch (rot.AsInt)
    {
      case 0:
        return this.north ?? this.defaultOffset;
      case 1:
        return !this.east.HasValue && this.west.HasValue ? new Vector3(-this.west.Value.x, this.west.Value.y, this.west.Value.z) : this.east ?? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle);
      case 2:
        return !this.south.HasValue && this.north.HasValue ? new Vector3(this.north.Value.x, this.north.Value.y, -this.north.Value.z) : this.south ?? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle);
      case 3:
        return !this.west.HasValue && this.east.HasValue ? new Vector3(-this.east.Value.x, this.east.Value.y, this.east.Value.z) : this.west ?? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle);
      case 4:
        Vector3? northEast = this.northEast;
        if (northEast.HasValue)
          return northEast.GetValueOrDefault();
        ref Vector3? local1 = ref this.northWest;
        if (local1.HasValue)
          return local1.GetValueOrDefault().MirrorHorizontal();
        ref Vector3? local2 = ref this.north;
        return !local2.HasValue ? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle) : Vector3Utility.RotatedBy(local2.GetValueOrDefault(), 45f);
      case 5:
        Vector3? southEast = this.southEast;
        if (southEast.HasValue)
          return southEast.GetValueOrDefault();
        ref Vector3? local3 = ref this.southWest;
        if (local3.HasValue)
          return local3.GetValueOrDefault().MirrorHorizontal();
        ref Vector3? local4 = ref this.south;
        return !local4.HasValue ? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle) : Vector3Utility.RotatedBy(local4.GetValueOrDefault(), -45f);
      case 6:
        Vector3? southWest = this.southWest;
        if (southWest.HasValue)
          return southWest.GetValueOrDefault();
        ref Vector3? local5 = ref this.southEast;
        if (local5.HasValue)
          return local5.GetValueOrDefault().MirrorHorizontal();
        ref Vector3? local6 = ref this.south;
        return !local6.HasValue ? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle) : Vector3Utility.RotatedBy(local6.GetValueOrDefault(), 45f);
      case 7:
        Vector3? northWest = this.northWest;
        if (northWest.HasValue)
          return northWest.GetValueOrDefault();
        ref Vector3? local7 = ref this.northEast;
        if (local7.HasValue)
          return local7.GetValueOrDefault().MirrorHorizontal();
        ref Vector3? local8 = ref this.north;
        return !local8.HasValue ? Vector3Utility.RotatedBy(this.defaultOffset, rot.AsAngle) : Vector3Utility.RotatedBy(local8.GetValueOrDefault(), -45f);
      default:
        throw new NotImplementedException("Rot8");
    }
  }
}
