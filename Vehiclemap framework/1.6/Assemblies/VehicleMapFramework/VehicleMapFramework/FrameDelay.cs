// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.FrameDelay
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class FrameDelay : GameComponent
{
  private static List<FrameDelay.IJob> currentJobs = new List<FrameDelay.IJob>();
  private static List<FrameDelay.IJob> nextJobs = new List<FrameDelay.IJob>();
  private static readonly object lockObj = new object();
  private readonly Game game;

  public FrameDelay(Game game)
  {
    this.game = game;
    // ISSUE: explicit constructor call
    base.\u002Ector();
  }

  public static void DelayOne<T>(Action<T> action, T state)
  {
    lock (FrameDelay.lockObj)
      FrameDelay.nextJobs.Add((FrameDelay.IJob) FrameDelay.Job<T>.Get(action, state));
  }

  public virtual void GameComponentUpdate()
  {
    lock (FrameDelay.lockObj)
    {
      if (FrameDelay.nextJobs.Count == 0)
        return;
      List<FrameDelay.IJob> nextJobs = FrameDelay.nextJobs;
      List<FrameDelay.IJob> currentJobs = FrameDelay.currentJobs;
      FrameDelay.currentJobs = nextJobs;
      FrameDelay.nextJobs = currentJobs;
    }
    for (int index = 0; index < FrameDelay.currentJobs.Count; ++index)
    {
      try
      {
        FrameDelay.currentJobs[index].Execute();
      }
      catch (Exception ex)
      {
        VMF_Log.Error($"Error in FrameDelay: {ex}");
      }
      finally
      {
        FrameDelay.currentJobs[index].Return();
      }
    }
    FrameDelay.currentJobs.Clear();
  }

  private interface IJob
  {
    void Execute();

    void Return();
  }

  private class Job<T> : FrameDelay.IJob
  {
    private Action<T> action;
    private T state;

    public void Execute() => this.action(this.state);

    public void Return()
    {
      this.state = default (T);
      this.action = (Action<T>) null;
      SimplePool<FrameDelay.Job<T>>.Return(this);
    }

    public static FrameDelay.Job<T> Get(Action<T> action, T state)
    {
      FrameDelay.Job<T> job = SimplePool<FrameDelay.Job<T>>.Get();
      job.state = state;
      job.action = action;
      return job;
    }
  }
}
