// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompVehicleDrawOffset
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompVehicleDrawOffset : VehicleComp
{
  public Vector3 drawOffset;
  public Vector3? drawOffsetNorth;
  public Vector3? drawOffsetEast;
  public Vector3? drawOffsetSouth;
  public Vector3? drawOffsetWest;
  private bool eastDiagonalRotated;
  private bool westDiagonalRotated;

  public virtual bool TickByRequest => true;

  public virtual void PostLoad() => this.Init();

  public virtual void PostGeneration() => this.Init();

  private void Init()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      Graphic_Vehicle vehicleGraphic = this.Vehicle.VehicleGraphic;
      this.eastDiagonalRotated = ((Graphic_Rgb) vehicleGraphic).EastDiagonalRotated;
      this.westDiagonalRotated = ((Graphic_Rgb) vehicleGraphic).WestDiagonalRotated;
    }));
  }

  public Vector3 DrawOffsetFull(Rot8 rot)
  {
    if (!((Rot8) ref rot).IsDiagonal)
      return this.DrawOffset(Rot8.op_Implicit(rot));
    if (this.eastDiagonalRotated)
    {
      if (Rot8.op_Equality(rot, Rot8.NorthEast))
        return this.DrawOffset(Rot4.North);
      if (Rot8.op_Equality(rot, Rot8.SouthEast))
        return this.DrawOffset(Rot4.South);
    }
    if (this.westDiagonalRotated)
    {
      if (Rot8.op_Equality(rot, Rot8.NorthWest))
        return this.DrawOffset(Rot4.North);
      if (Rot8.op_Equality(rot, Rot8.SouthWest))
        return this.DrawOffset(Rot4.South);
    }
    return this.DrawOffset(Rot8.op_Implicit(rot));
  }

  private Vector3 DrawOffset(Rot4 rot)
  {
    Vector3 vector3;
    switch (((Rot4) ref rot).AsInt)
    {
      case 0:
        vector3 = this.drawOffsetNorth ?? this.drawOffset;
        break;
      case 1:
        vector3 = this.drawOffsetEast ?? this.drawOffset;
        break;
      case 2:
        vector3 = this.drawOffsetSouth ?? this.drawOffset;
        break;
      case 3:
        vector3 = this.drawOffsetWest ?? this.drawOffset;
        break;
      default:
        vector3 = this.drawOffset;
        break;
    }
    return vector3;
  }
}
