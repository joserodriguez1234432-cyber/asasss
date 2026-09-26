// Decompiled with JetBrains decompiler
// Type: Vehicles.UpgradeTextEntry
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public struct UpgradeTextEntry
{
  public string label;
  public string description;
  public UpgradeEffectType effectType;

  public UpgradeTextEntry(string label, string description, UpgradeEffectType effectType = UpgradeEffectType.None)
  {
    this.label = label;
    this.description = description;
    this.effectType = effectType;
  }

  public UpgradeTextEntry(
    string label,
    string description,
    float value,
    UpgradeEffectType effectType)
  {
    this.label = label;
    this.description = description;
    if (effectType != UpgradeEffectType.Positive)
    {
      if (effectType == UpgradeEffectType.Negative)
        this.effectType = UpgradeTextEntry.EffectTypeFromValue(-value);
      else
        this.effectType = effectType;
    }
    else
      this.effectType = UpgradeTextEntry.EffectTypeFromValue(value);
  }

  private static UpgradeEffectType EffectTypeFromValue(float value)
  {
    if ((double) value > 0.0)
      return UpgradeEffectType.Positive;
    return (double) value < 0.0 ? UpgradeEffectType.Negative : UpgradeEffectType.None;
  }

  public static string FormatValue(
    float value,
    UpgradeType upgradeType,
    ToStringStyle toStringStyle,
    ToStringNumberSense toStringNumberSense = 1,
    string formatString = null)
  {
    string str = GenText.ToStringByStyle(value, toStringStyle, toStringNumberSense);
    if (toStringNumberSense != 2 && !GenText.NullOrEmpty(formatString))
      str = string.Format(formatString, (object) str);
    if (upgradeType == UpgradeType.Add && (double) value > 0.0)
      str = "+" + str;
    return str;
  }
}
