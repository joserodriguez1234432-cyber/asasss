// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.AnimatedSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using UnityEngine;

#nullable disable
namespace UpdateLogTool;

public abstract class AnimatedSegment : ImageSegment
{
  protected abstract int DefaultFramesPerSecond { get; }

  protected virtual int DefaultDelayOnReset { get; }

  protected virtual int CurrentFrame(int maxFrames, int fps, int delay)
  {
    return Mathf.Clamp(Mathf.FloorToInt(Time.time * (float) fps) % (maxFrames + delay) - delay, 0, maxFrames);
  }
}
