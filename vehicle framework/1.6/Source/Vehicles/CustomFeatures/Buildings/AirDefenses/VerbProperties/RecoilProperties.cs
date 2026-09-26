// Decompiled with JetBrains decompiler
// Type: Vehicles.RecoilProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;

#nullable disable
namespace Vehicles;

public class RecoilProperties
{
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float distanceTotal = 0.75f;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float distancePerTick = 0.15f;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 2f, Increment = 0.05f, RoundDecimalPlaces = 2)]
  public float speedMultiplierPostRecoil = 0.25f;
}
