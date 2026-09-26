// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleJobLimitations
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Globalization;
using Verse;

#nullable disable
namespace Vehicles;

[HeaderTitle(Label = "VF_JobLimitations", Translate = true)]
[VehicleSettingsClass]
public class VehicleJobLimitations
{
  public string defName;
  [PostToSettings(Label = "VF_MaxWorkers", Translate = true)]
  public int maxWorkers;

  public VehicleJobLimitations(string defName, int maxWorkers)
  {
    this.defName = defName;
    this.maxWorkers = maxWorkers;
  }

  public bool IsValid => !GenText.NullOrEmpty(this.defName);

  public static VehicleJobLimitations Invalid => new VehicleJobLimitations(string.Empty, 0);

  public static VehicleJobLimitations FromString(string entry)
  {
    entry = entry.TrimStart('(').TrimEnd(')');
    string[] strArray = entry.Split(',');
    try
    {
      CultureInfo invariantCulture = CultureInfo.InvariantCulture;
      return new VehicleJobLimitations(Convert.ToString(strArray[0], (IFormatProvider) invariantCulture), Convert.ToInt32(strArray[1], (IFormatProvider) invariantCulture));
    }
    catch (Exception ex)
    {
      SmashLog.Error($"{entry} is not a valid <struct>VehicleJobLimitations</struct> format. Exception: {ex}");
      return VehicleJobLimitations.Invalid;
    }
  }

  public override string ToString() => $"({this.defName},{this.maxWorkers})";
}
