// Decompiled with JetBrains decompiler
// Type: SmashTools.NumericBoxValuesAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class NumericBoxValuesAttribute : Attribute
{
  public float MinValue { get; set; } = float.MinValue;

  public float MaxValue { get; set; } = float.MaxValue;
}
