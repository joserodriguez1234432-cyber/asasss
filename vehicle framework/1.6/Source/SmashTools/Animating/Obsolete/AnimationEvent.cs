// Decompiled with JetBrains decompiler
// Type: SmashTools.AnimationEvent`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using UnityEngine;

#nullable disable
namespace SmashTools;

[PublicAPI]
public class AnimationEvent<T>
{
  public float triggerAt;
  public DynamicDelegate<T> method;
  public AnimationEvent<T>.AnimationTrigger type;
  public AnimationEvent<T>.AnimationFrequency frequency;

  public bool EventFrame(float t)
  {
    bool flag;
    switch (this.type)
    {
      case AnimationEvent<T>.AnimationTrigger.EqualTo:
        flag = Mathf.Approximately(t, this.triggerAt);
        break;
      case AnimationEvent<T>.AnimationTrigger.GreaterThan:
        flag = (double) t >= (double) this.triggerAt;
        break;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  public enum AnimationTrigger
  {
    EqualTo,
    GreaterThan,
  }

  public enum AnimationFrequency
  {
    OneShot,
    Continuous,
  }
}
