// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.KeyFrame
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Xml;
using System;
using System.Globalization;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public readonly struct KeyFrame : IXmlExport, IComparable<KeyFrame>
{
  private const float DefaultWeight = 0.333f;
  public readonly int frame;
  public readonly float value;
  public readonly float inTangent;
  public readonly float outTangent;
  public readonly float inWeight;
  public readonly float outWeight;
  public readonly WeightedMode weightedMode;

  public KeyFrame(int frame, float value)
    : this(frame, value, 0.0f, 0.0f)
  {
    this.frame = frame;
    this.value = value;
  }

  public KeyFrame(int frame, float value, float inTangent, float outTangent)
    : this(frame, value, inTangent, outTangent, 0.333f, 0.333f)
  {
    this.frame = frame;
    this.value = value;
    this.inTangent = inTangent;
    this.outTangent = outTangent;
  }

  public KeyFrame(
    int frame,
    float value,
    float inTangent,
    float outTangent,
    float inWeight,
    float outWeight)
    : this(frame, value, inTangent, outTangent, inWeight, outWeight, (WeightedMode) 0)
  {
    this.frame = frame;
    this.value = value;
    this.inTangent = inTangent;
    this.outTangent = outTangent;
    this.inWeight = inWeight;
    this.outWeight = outWeight;
  }

  public KeyFrame(
    int frame,
    float value,
    float inTangent,
    float outTangent,
    float inWeight,
    float outWeight,
    WeightedMode weightedMode)
  {
    this.frame = frame;
    this.value = value;
    this.inTangent = inTangent;
    this.outTangent = outTangent;
    this.inWeight = inWeight;
    this.outWeight = outWeight;
    this.weightedMode = weightedMode;
  }

  public static KeyFrame Invalid => new KeyFrame(-1, 0.0f);

  void IXmlExport.Export() => XmlExporter.WriteString(this.ToString());

  public override string ToString()
  {
    return $"({this.frame},{this.value.RoundTo(0.0001f)}," + $"{this.inTangent.RoundTo(0.0001f)},{this.outTangent.RoundTo(0.0001f)}," + $"{this.inWeight.RoundTo(0.0001f)},{this.outWeight.RoundTo(0.0001f)})";
  }

  int IComparable<KeyFrame>.CompareTo(KeyFrame other) => this.frame.CompareTo(other.frame);

  public static KeyFrame FromString(string entry)
  {
    entry = entry.Replace("(", "");
    entry = entry.Replace(")", "");
    string[] strArray = entry.Split(',', StringSplitOptions.None);
    if (strArray.Length == 6)
    {
      CultureInfo invariantCulture = CultureInfo.InvariantCulture;
      return new KeyFrame(Convert.ToInt32(strArray[0], (IFormatProvider) invariantCulture), Convert.ToSingle(strArray[1], (IFormatProvider) invariantCulture), Convert.ToSingle(strArray[2], (IFormatProvider) invariantCulture), Convert.ToSingle(strArray[3], (IFormatProvider) invariantCulture), Convert.ToSingle(strArray[4], (IFormatProvider) invariantCulture), Convert.ToSingle(strArray[5], (IFormatProvider) invariantCulture));
    }
    Log.Error($"Unable to parse AnimationCurve.KeyFrame. Invalid format: {entry}.");
    return KeyFrame.Invalid;
  }
}
