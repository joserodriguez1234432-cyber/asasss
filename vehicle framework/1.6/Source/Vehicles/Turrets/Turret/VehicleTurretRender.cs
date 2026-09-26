// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTurretRender
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleTurretRender : ITweakFields
{
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? north;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? east;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? south;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? west;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? northEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? southEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? southWest;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? northWest;

  string ITweakFields.Label => "Render Properties";

  string ITweakFields.Category => string.Empty;

  public VehicleTurretRender()
  {
  }

  public VehicleTurretRender(VehicleTurretRender reference)
  {
    if (reference != null)
    {
      this.north = reference.north;
      this.east = reference.east;
      this.south = reference.south;
      this.west = reference.west;
      this.northEast = reference.northEast;
      this.southEast = reference.southEast;
      this.southWest = reference.southWest;
      this.northWest = reference.northWest;
    }
    this.PostLoad();
  }

  public void OnFieldChanged() => this.RecacheOffsets();

  public void PostLoad() => this.RecacheOffsets();

  public void RecacheOffsets()
  {
    Vector2 valueOrDefault = this.north.GetValueOrDefault();
    if (!this.north.HasValue)
      this.north = new Vector2?(this.south.HasValue ? VehicleTurretRender.Rotate(this.south.Value, 180f) : Vector2.zero);
    valueOrDefault = this.south.GetValueOrDefault();
    if (!this.south.HasValue)
      this.south = new Vector2?(VehicleTurretRender.Rotate(this.north.Value, 180f));
    valueOrDefault = this.east.GetValueOrDefault();
    if (!this.east.HasValue)
      this.east = new Vector2?(this.west.HasValue ? VehicleTurretRender.Flip(this.west.Value, true, false) : VehicleTurretRender.Rotate(this.north.Value, -90f));
    valueOrDefault = this.west.GetValueOrDefault();
    if (!this.west.HasValue)
      this.west = new Vector2?(this.east.HasValue ? VehicleTurretRender.Flip(this.east.Value, true, false) : VehicleTurretRender.Rotate(this.north.Value, 90f));
    valueOrDefault = this.northEast.GetValueOrDefault();
    if (!this.northEast.HasValue)
      this.northEast = new Vector2?(VehicleTurretRender.Rotate(this.north.Value, -45f));
    valueOrDefault = this.northWest.GetValueOrDefault();
    if (!this.northWest.HasValue)
      this.northWest = new Vector2?(VehicleTurretRender.Rotate(this.north.Value, 45f));
    valueOrDefault = this.southEast.GetValueOrDefault();
    if (!this.southEast.HasValue)
      this.southEast = new Vector2?(VehicleTurretRender.Rotate(this.south.Value, 45f));
    valueOrDefault = this.southWest.GetValueOrDefault();
    if (this.southWest.HasValue)
      return;
    this.southWest = new Vector2?(VehicleTurretRender.Rotate(this.south.Value, -45f));
  }

  private static Vector2 Rotate(Vector2 offset, float angle)
  {
    if ((double) angle % 45.0 == 0.0)
      return Vector2Utility.RotatedBy(offset, angle);
    Log.Error("Cannot rotate VehicleTurretRender.offset with an angle non-multiple of 45.");
    return offset;
  }

  private static Vector2 Flip(Vector2 offset, bool flipX, bool flipY)
  {
    Vector2 vector2 = offset;
    if (flipX)
      vector2.x *= -1f;
    if (flipY)
      vector2.y *= -1f;
    return vector2;
  }

  public Vector2 OffsetFor(Rot8 rot)
  {
    Vector2 vector2;
    switch (rot.AsInt)
    {
      case 0:
        vector2 = this.north ?? Vector2.zero;
        break;
      case 1:
        vector2 = this.east ?? Vector2.zero;
        break;
      case 2:
        vector2 = this.south ?? Vector2.zero;
        break;
      case 3:
        vector2 = this.west ?? Vector2.zero;
        break;
      case 4:
        vector2 = this.northEast ?? Vector2.zero;
        break;
      case 5:
        vector2 = this.southEast ?? Vector2.zero;
        break;
      case 6:
        vector2 = this.southWest ?? Vector2.zero;
        break;
      case 7:
        vector2 = this.northWest ?? Vector2.zero;
        break;
      default:
        vector2 = Vector2.zero;
        break;
    }
    return vector2;
  }

  public override string ToString()
  {
    return $"north: {this.north} east: {this.east} south: {this.south} west: {this.west} NE: {this.northEast} SE: {this.southEast} SW: {this.southWest} NW: {this.northWest}";
  }
}
