// Decompiled with JetBrains decompiler
// Type: Vehicles.DynamicShadowData
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;

#nullable disable
namespace Vehicles;

public struct DynamicShadowData
{
  public float width;
  public float height;
  public float alpha;

  public static DynamicShadowData CreateFrom(VehiclePawn vehicle)
  {
    DynamicShadowData from = new DynamicShadowData();
    Vector2 drawSize = vehicle.VehicleGraphic.data.drawSize;
    from.width = drawSize.x;
    from.height = drawSize.y;
    from.alpha = 1f;
    return from;
  }

  public bool Invalid => (double) this.width <= 0.0 && (double) this.height <= 0.0;
}
