// Decompiled with JetBrains decompiler
// Type: SmashTools.SliderValuesAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class SliderValuesAttribute : Attribute
{
  public float MinValue { get; set; }

  public float MaxValue { get; set; }

  public float EndValue { get; set; }

  public string EndSymbol { get; set; }

  public int RoundDecimalPlaces { get; set; }

  public float Increment { get; set; }

  public string MinValueDisplay { get; set; }

  public string MaxValueDisplay { get; set; }
}
