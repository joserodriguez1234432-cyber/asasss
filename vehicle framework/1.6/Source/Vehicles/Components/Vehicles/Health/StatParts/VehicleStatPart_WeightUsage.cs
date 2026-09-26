// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatPart_WeightUsage
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleStatPart_WeightUsage : VehicleStatPart
{
  public LinearCurve usageCurve;
  public OperationType operation;
  public string formatString;

  protected float Modifier(VehiclePawn vehicle)
  {
    float x = 0.0f;
    float num;
    if (this.usageCurve != null)
    {
      float statValue = vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
      if ((double) statValue > 0.0)
        x = MassUtility.InventoryMass((Pawn) vehicle) / statValue;
      num = this.usageCurve.Evaluate(x);
    }
    else
      num = MassUtility.InventoryMass((Pawn) vehicle);
    return num;
  }

  public override float TransformValue(VehiclePawn vehicle, float value)
  {
    return this.operation.Apply(value, this.Modifier(vehicle));
  }

  public override string ExplanationPart(VehiclePawn vehicle)
  {
    float statValue = vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
    return TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_StatsReport_CargoWeight", NamedArgument.op_Implicit(!GenText.NullOrEmpty(this.formatString) ? string.Format(this.formatString, (object) MassUtility.InventoryMass((Pawn) vehicle), (object) statValue) : string.Format(this.statDef.formatString, (object) MassUtility.InventoryMass((Pawn) vehicle), (object) statValue))));
  }
}
