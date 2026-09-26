// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.Debouncer
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using UnityEngine;

#nullable disable
namespace SmashTools.Performance;

[PublicAPI]
public sealed class Debouncer
{
  private readonly float timeDelay;
  private readonly Debouncer.TimerImpl timer;

  public Debouncer(Action action, int milliseconds)
  {
    if (action == null)
      throw new ArgumentNullException(nameof (action));
    this.timeDelay = (float) milliseconds / 1000f;
    this.timer = new Debouncer.TimerImpl(action);
    this.timer.Reset(this.timeDelay);
  }

  public float TimeRemaining => this.timer.TimeLeft;

  public void Invoke()
  {
    this.timer.Reset(this.timeDelay);
    this.timer.EnsureScheduled();
  }

  public void Cancel() => this.timer.Cancel();

  private class TimerImpl(Action action)
  {
    private bool active;
    private float timeLeft;

    public float TimeLeft => this.timeLeft;

    private bool Expired => !this.active || (double) this.timeLeft <= 0.0;

    public void EnsureScheduled()
    {
      if (this.active)
        return;
      this.active = true;
      UnityThread.StartUpdate(new UnityThread.OnUpdate(this.Update));
    }

    public void Reset(float timeDelay) => this.timeLeft = timeDelay;

    public void Cancel()
    {
      this.active = false;
      this.timeLeft = 0.0f;
    }

    private bool Update()
    {
      if (!this.active)
        return false;
      this.timeLeft -= Time.deltaTime;
      if (!this.Expired)
        return true;
      this.timeLeft = 0.0f;
      this.active = false;
      action();
      return false;
    }
  }
}
