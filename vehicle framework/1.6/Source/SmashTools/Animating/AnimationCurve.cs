// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationCurve
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using SmashTools.Xml;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

[PublicAPI]
public sealed class AnimationCurve : IXmlExport
{
  public List<KeyFrame> points = new List<KeyFrame>();

  public KeyFrame LeftBound => this.points.Count <= 0 ? KeyFrame.Invalid : this.points[0];

  public KeyFrame RightBound
  {
    get => this.points.Count <= 0 ? KeyFrame.Invalid : this.points[this.points.Count - 1];
  }

  public FloatRange RangeX
  {
    get => new FloatRange((float) this.LeftBound.frame, (float) this.RightBound.frame);
  }

  public FloatRange RangeY => throw new NotImplementedException();

  public int PointsCount => this.points.Count;

  public bool IsValid => !this.points.NullOrEmpty<KeyFrame>();

  public float this[int frame] => this.Function((float) frame);

  public bool Add(int frame, float value)
  {
    foreach (KeyFrame point in this.points)
    {
      if (point.frame == frame)
        return false;
    }
    this.points.Add(new KeyFrame(frame, value));
    this.points.Sort();
    return true;
  }

  public void Set(int frame, float value)
  {
    for (int index = 0; index < this.points.Count; ++index)
    {
      KeyFrame point = this.points[index];
      if (point.frame == frame)
      {
        this.points[index] = new KeyFrame(point.frame, value);
        return;
      }
      if (point.frame > frame)
      {
        this.points.Insert(index, new KeyFrame(frame, value));
        return;
      }
    }
    this.Add(frame, value);
  }

  public void Remove(int frame)
  {
    for (int index = 0; index < this.points.Count; ++index)
    {
      if (this.points[index].frame == frame)
      {
        this.points.RemoveAt(index);
        break;
      }
    }
  }

  public bool KeyFrameAt(float frame)
  {
    foreach (KeyFrame point in this.points)
    {
      if (Mathf.Approximately((float) point.frame, frame))
        return true;
      if ((double) point.frame > (double) frame)
        return false;
    }
    return false;
  }

  public float Function(float time)
  {
    if (this.points.NullOrEmpty<KeyFrame>() || this.RightBound.frame <= 0)
      return 0.0f;
    if (this.points.Count == 1 || (double) time <= (double) this.LeftBound.frame)
      return this.LeftBound.value;
    return (double) time >= (double) this.RightBound.frame ? this.RightBound.value : this.CubicSpline(time);
  }

  private float CubicSpline(float time)
  {
    KeyFrame keyFrame1 = KeyFrame.Invalid;
    KeyFrame keyFrame2 = KeyFrame.Invalid;
    for (int index = 0; index < this.points.Count; ++index)
    {
      if (Mathf.Approximately((float) this.points[index].frame, time))
        return this.points[index].value;
      if ((double) this.points[index].frame <= (double) time)
      {
        keyFrame1 = this.points[index];
        keyFrame2 = this.points[index + 1];
      }
      else
        break;
    }
    if (Mathf.Approximately(keyFrame1.outTangent, float.PositiveInfinity))
      return keyFrame1.value;
    if (Mathf.Approximately(keyFrame1.outTangent, float.NegativeInfinity))
      return keyFrame2.value;
    if (Mathf.Approximately(keyFrame2.inTangent, float.PositiveInfinity))
      return keyFrame1.value;
    if (Mathf.Approximately(keyFrame2.inTangent, float.NegativeInfinity))
      return keyFrame2.value;
    float num1 = (float) (keyFrame2.frame - keyFrame1.frame);
    float num2 = (time - (float) keyFrame1.frame) / num1;
    float num3 = num2 * num2;
    float num4 = num2 * num2 * num2;
    float num5 = keyFrame1.outTangent * num1;
    float num6 = keyFrame2.inTangent * num1;
    float num7 = (float) (2.0 * (double) num4 - 3.0 * (double) num3 + 1.0) * keyFrame1.value;
    float num8 = (num4 - 2f * num3 + num2) * num5;
    float num9 = (num4 - num3) * num6;
    float num10 = (float) (-2.0 * (double) num4 + 3.0 * (double) num3) * keyFrame2.value;
    if (keyFrame1.weightedMode == 2 || keyFrame1.weightedMode == 3)
      num8 *= keyFrame1.outWeight;
    if (keyFrame2.weightedMode == 1 || keyFrame2.weightedMode == 3)
      num9 *= keyFrame2.inWeight;
    return num7 + num8 + num9 + num10;
  }

  private float Lerp(float frame)
  {
    KeyFrame point1 = this.points[0];
    KeyFrame point2 = this.points[this.points.Count - 1];
    for (int index = 0; index < this.points.Count; ++index)
    {
      if ((double) frame <= (double) this.points[index].frame)
      {
        point2 = this.points[index];
        if (index > 0)
        {
          point1 = this.points[index - 1];
          break;
        }
        break;
      }
    }
    float num = (frame - (float) point1.frame) / (float) (point2.frame - point1.frame);
    return Mathf.LerpUnclamped(point1.value, point2.value, num);
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteCollection<KeyFrame>("points", (IEnumerable<KeyFrame>) this.points);
  }
}
