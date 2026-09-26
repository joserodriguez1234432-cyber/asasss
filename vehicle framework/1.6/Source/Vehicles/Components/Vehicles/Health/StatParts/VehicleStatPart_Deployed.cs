// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatPart_Deployed
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class VehicleStatPart_Deployed : VehicleStatPart
{
  public override float TransformValue(VehiclePawn vehicle, float value)
  {
    return vehicle.CompVehicleTurrets != null && vehicle.CompVehicleTurrets.Deployed ? 0.0f : value;
  }

  public override string ExplanationPart(VehiclePawn vehicle)
  {
    return vehicle.CompVehicleTurrets != null && vehicle.CompVehicleTurrets.Deployed ? TaggedString.op_Implicit(Translator.Translate("VF_StatsReport_Deployed")) : (string) null;
  }
}
