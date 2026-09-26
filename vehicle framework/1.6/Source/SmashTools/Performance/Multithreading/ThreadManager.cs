// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.ThreadManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using Verse;

#nullable disable
namespace SmashTools.Performance;

[PublicAPI]
public static class ThreadManager
{
  private const int MaxThreads = 10;
  private const int SingleThreadIdOffset = 100;
  private static readonly AccessTools.FieldRef<object, Thread> EventThreadFieldRef;
  private static int nextId = 100;
  private static readonly Dictionary<int, ushort> ThreadRefCounts = new Dictionary<int, ushort>(10);
  private static readonly List<DedicatedThread> Threads = new List<DedicatedThread>(10);
  private static readonly object ThreadListLock = new object();

  static ThreadManager()
  {
    ThreadManager.EventThreadFieldRef = AccessTools.FieldRefAccess<Thread>(typeof (LongEventHandler), "eventThread");
  }

  public static bool InMainOrEventThread
  {
    get
    {
      if (UnityData.IsInMainThread)
        return true;
      Thread thread = ThreadManager.EventThreadFieldRef.Invoke((object) null);
      return thread == null || Thread.CurrentThread.ManagedThreadId == thread.ManagedThreadId;
    }
  }

  public static bool AllThreadsTerminated
  {
    get
    {
      lock (ThreadManager.ThreadListLock)
        return ThreadManager.Threads.Count == 0;
    }
  }

  public static ListSnapshot<DedicatedThread> ThreadsSnapshot
  {
    get
    {
      lock (ThreadManager.ThreadListLock)
        return new ListSnapshot<DedicatedThread>(ThreadManager.Threads);
    }
  }

  [MustUseReturnValue]
  public static DedicatedThread CreateNew()
  {
    DedicatedThread dedicatedThread = ThreadManager.CreateNew(ThreadManager.nextId, DedicatedThread.ThreadType.Single);
    Interlocked.Increment(ref ThreadManager.nextId);
    return dedicatedThread;
  }

  [MustUseReturnValue]
  public static DedicatedThread GetOrCreateShared(int id)
  {
    return ThreadManager.CreateNew(id, DedicatedThread.ThreadType.Shared);
  }

  private static DedicatedThread CreateNew(int id, DedicatedThread.ThreadType type)
  {
    lock (ThreadManager.ThreadListLock)
    {
      if (ThreadManager.Threads.Count > 10)
        throw new InvalidOperationException("Trying to create more threads than is allowed by ThreadManager.");
      DedicatedThread dedicatedThread = type != DedicatedThread.ThreadType.Shared || id < 100 ? ThreadManager.GetThread(id) : throw new ArgumentException($"Shared thread ids must be between 0 and {100} to avoid conflicting with unshared thread ids.");
      if (dedicatedThread == null)
      {
        dedicatedThread = new DedicatedThread(id, type);
        ThreadManager.Threads.Add(dedicatedThread);
        ThreadManager.ThreadRefCounts.Add(id, (ushort) 1);
      }
      else
        ThreadManager.ThreadRefCounts[id]++;
      return dedicatedThread;
    }
  }

  [Pure]
  public static DedicatedThread GetThread(int id)
  {
    lock (ThreadManager.ThreadListLock)
    {
      foreach (DedicatedThread thread in ThreadManager.Threads)
      {
        if (thread.id == id)
          return thread;
      }
    }
    return (DedicatedThread) null;
  }

  public static void Release(this DedicatedThread dedicatedThread)
  {
    switch (dedicatedThread.type)
    {
      case DedicatedThread.ThreadType.Single:
        ThreadManager.DisposeThread(dedicatedThread);
        break;
      case DedicatedThread.ThreadType.Shared:
        ThreadManager.ReleaseReference(dedicatedThread);
        break;
      default:
        throw new NotImplementedException(dedicatedThread.type.ToString());
    }
  }

  private static void ReleaseReference(DedicatedThread dedicatedThread)
  {
    lock (ThreadManager.ThreadListLock)
    {
      int id = dedicatedThread.id;
      ThreadManager.ThreadRefCounts[id]--;
      if (ThreadManager.ThreadRefCounts[id] != (ushort) 0)
        return;
      ThreadManager.DisposeThread(dedicatedThread);
    }
  }

  private static void DisposeThread(DedicatedThread dedicatedThread)
  {
    lock (ThreadManager.ThreadListLock)
    {
      dedicatedThread.StopImmediately();
      if (!ThreadManager.Threads.Remove(dedicatedThread))
        Trace.Fail($"Failed to remove thread {dedicatedThread.id} from ThreadManager");
      ThreadManager.ThreadRefCounts.Remove(dedicatedThread.id);
    }
  }

  internal static void OnSceneChanged(Scene scene, LoadSceneMode mode)
  {
    ThreadManager.ReleaseAll();
    ComponentCache.ClearAll();
  }

  public static void ReleaseAll()
  {
    using (ListSnapshot<DedicatedThread> threadsSnapshot = ThreadManager.ThreadsSnapshot)
    {
      foreach (DedicatedThread dedicatedThread in threadsSnapshot)
        ThreadManager.ReleaseAndJoin(dedicatedThread);
    }
  }

  public static void ReleaseAndJoin(DedicatedThread dedicatedThread)
  {
    ThreadManager.DisposeThread(dedicatedThread);
    if (dedicatedThread.thread.Join(5000))
      return;
    Log.Error($"Thread {dedicatedThread.id} has failed to terminate.");
  }
}
