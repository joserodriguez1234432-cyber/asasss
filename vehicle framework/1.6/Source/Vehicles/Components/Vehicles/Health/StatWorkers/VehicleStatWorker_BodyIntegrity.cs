// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatWorker_BodyIntegrity
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;

#nullable disable
namespace Vehicles;

public class VehicleStatWorker_BodyIntegrity : VehicleStatWorker
{
  public override float TransformValue(VehiclePawn vehicle, float value)
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    foreach (VehicleComponent component in vehicle.statHandler.components)
    {
      num1 += component.Health;
      num2 += component.MaxHealth;
    }
    value = (double) num2 > 0.0 ? (num1 / num2).RoundTo(1f / 1000f) : throw new InvalidOperationException($"Total health of VehicleDef {vehicle.VehicleDef} is less than or equal to 0.");
    return base.TransformValue(vehicle, value);
  }
}
