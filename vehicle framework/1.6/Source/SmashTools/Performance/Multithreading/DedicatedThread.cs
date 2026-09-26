// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.DedicatedThread
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Verse;

#nullable disable
namespace SmashTools.Performance;

[PublicAPI]
public class DedicatedThread
{
  internal readonly Thread thread;
  public readonly int id;
  public readonly DedicatedThread.ThreadType type;
  private bool shouldTerminate;
  private readonly ManualResetEventSlim workHandle;
  private readonly ManualResetEventSlim suspendHandle;
  private readonly ConcurrentQueue<AsyncAction> queue;

  internal DedicatedThread(int id, DedicatedThread.ThreadType type)
  {
    this.id = id;
    this.type = type;
    this.workHandle = new ManualResetEventSlim(false);
    this.suspendHandle = new ManualResetEventSlim(false);
    this.queue = new ConcurrentQueue<AsyncAction>();
    this.thread = new Thread(new ThreadStart(this.Execute))
    {
      IsBackground = true,
      Priority = ThreadPriority.BelowNormal
    };
    this.thread.Start();
  }

  public int QueueCount => this.queue.Count;

  public bool IsSuspended => this.State == DedicatedThread.ThreadState.Suspended;

  public bool IsTerminated => this.State == DedicatedThread.ThreadState.Terminated;

  public bool IsBlocked => !this.workHandle.IsSet;

  public DedicatedThread.ThreadState State { get; private set; }

  public void Suspend()
  {
    if (Thread.CurrentThread.ManagedThreadId == this.thread.ManagedThreadId)
      throw new InvalidOperationException("Attempting to suspend thread from inside the thread.");
    if (this.State != DedicatedThread.ThreadState.Running)
      return;
    this.suspendHandle.Reset();
    this.State = DedicatedThread.ThreadState.Suspending;
    this.UnpauseConsumer();
    this.suspendHandle.Wait();
  }

  public void Unsuspend()
  {
    switch (this.State)
    {
      case DedicatedThread.ThreadState.Running:
        break;
      case DedicatedThread.ThreadState.Suspending:
        throw new InvalidOperationException("Unsuspending a thread which is still spinning up to suspend.");
      case DedicatedThread.ThreadState.Suspended:
        this.State = DedicatedThread.ThreadState.Running;
        this.UnpauseConsumer();
        break;
      default:
        throw new InvalidOperationException("Unsuspending a thread which was not suspended.");
    }
  }

  internal void Snapshot(List<AsyncAction> items)
  {
    items.AddRange((IEnumerable<AsyncAction>) this.queue);
  }

  public void Enqueue(AsyncAction action)
  {
    bool flag;
    switch (this.State)
    {
      case DedicatedThread.ThreadState.Suspending:
      case DedicatedThread.ThreadState.Suspended:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    if (flag)
      throw new InvalidOperationException($"Thread {this.id} has been enqueued an item while suspended. It will not execute.");
    this.queue.Enqueue(action);
    this.UnpauseConsumer();
  }

  internal void EnqueueSilently(AsyncAction action) => this.queue.Enqueue(action);

  private void UnpauseConsumer()
  {
    if (this.State == DedicatedThread.ThreadState.Terminated)
      return;
    this.workHandle.Set();
  }

  public void Stop()
  {
    this.shouldTerminate = true;
    this.UnpauseConsumer();
  }

  public void StopImmediately()
  {
    if (this.State == DedicatedThread.ThreadState.Terminated)
      return;
    this.State = DedicatedThread.ThreadState.Stopping;
    this.Stop();
  }

  private void Execute()
  {
    this.State = DedicatedThread.ThreadState.Running;
    try
    {
      while (!this.shouldTerminate)
      {
        this.workHandle.Wait();
        this.workHandle.Reset();
        while (this.State != DedicatedThread.ThreadState.Stopping)
        {
          AsyncAction result;
          if (this.queue.TryDequeue(out result))
          {
            try
            {
              if (result.IsValid)
                result.Invoke();
            }
            catch (Exception ex)
            {
              Log.Error($"Exception thrown while executing {result} on DedicatedThread " + $"#{this.id:D3}.\nException={ex}");
              result.ExceptionThrown(ex);
            }
            finally
            {
              result.ReturnToPool();
            }
          }
          else
            break;
        }
        if (this.State == DedicatedThread.ThreadState.Suspending)
        {
          this.State = DedicatedThread.ThreadState.Suspended;
          this.suspendHandle.Set();
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown from thread={this.thread.ManagedThreadId}.\n{ex}");
    }
    finally
    {
      this.State = DedicatedThread.ThreadState.Terminated;
      this.workHandle.Dispose();
      this.suspendHandle.Dispose();
    }
  }

  public enum ThreadType
  {
    Single,
    Shared,
  }

  public enum ThreadState
  {
    Uninitialized,
    Running,
    Suspending,
    Suspended,
    Stopping,
    Terminated,
  }
}
