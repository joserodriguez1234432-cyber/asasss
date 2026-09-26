// Decompiled with JetBrains decompiler
// Type: Vehicles.SettingsValueInfo
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;

#nullable disable
namespace Vehicles;

public struct SettingsValueInfo
{
  public float minValue;
  public float maxValue;
  public float endValue;
  public string endSymbol;
  public int roundDecimalPlaces;
  public float increment;
  public string minValueDisplay;
  public string maxValueDisplay;
  public UISettingsType settingsType;

  public bool IsValid => this.settingsType != 0;
}
