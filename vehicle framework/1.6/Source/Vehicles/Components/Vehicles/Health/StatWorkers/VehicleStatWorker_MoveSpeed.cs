// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatWorker_MoveSpeed
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;

#nullable disable
namespace Vehicles;

public class VehicleStatWorker_MoveSpeed : VehicleStatWorker
{
  public override bool ShouldShowFor(VehicleDef vehicleDef)
  {
    return !Mathf.Approximately(vehicleDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed), 0.0f) && base.ShouldShowFor(vehicleDef);
  }
}
