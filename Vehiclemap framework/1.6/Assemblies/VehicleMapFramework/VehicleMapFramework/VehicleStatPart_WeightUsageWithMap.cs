// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleStatPart_WeightUsageWithMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleStatPart_WeightUsageWithMap : VehicleStatPart_WeightUsage
{
  private float Modifier(VehiclePawnWithMap vehicle)
  {
    float num1 = 0.0f;
    float num2;
    if (this.usageCurve != null)
    {
      float statValue = vehicle.GetStatValue(VMF_DefOf.MaximumPayload);
      if ((double) statValue > 0.0)
        num1 = VehicleMapUtility.VehicleMapMass(vehicle) * VehicleMapFramework.VehicleMapFramework.settings.weightFactor / statValue;
      num2 = this.usageCurve.Evaluate(num1);
    }
    else
      num2 = VehicleMapUtility.VehicleMapMass(vehicle) * VehicleMapFramework.VehicleMapFramework.settings.weightFactor;
    return num2;
  }

  public virtual float TransformValue(VehiclePawn vehicle, float value)
  {
    return vehicle is VehiclePawnWithMap vehicle1 ? MathOp.Apply(this.operation, value, this.Modifier(vehicle1)) : value;
  }

  public virtual string ExplanationPart(VehiclePawn vehicle)
  {
    if (!(vehicle is VehiclePawnWithMap vehicle1))
      return (string) null;
    string stringByStyle = GenText.ToStringByStyle(vehicle.GetStatValue(VMF_DefOf.MaximumPayload), (ToStringStyle) 2, (ToStringNumberSense) 1);
    return TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_StatsReport_MaximumPayload", NamedArgument.op_Implicit(string.Format(GenText.NullOrEmpty(this.formatString) ? ((VehicleStatPart) this).statDef.formatString : this.formatString, (object) VehicleMapUtility.VehicleMapMass(vehicle1), (object) stringByStyle))));
  }
}
