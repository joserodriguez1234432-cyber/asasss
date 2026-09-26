// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.UnityThread
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using SmashTools.Targeting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Performance;

[PublicAPI]
[StaticConstructorOnStartup]
public sealed class UnityThread : MonoBehaviour
{
  private readonly List<UnityThread.OnUpdate> onUpdateMethods = new List<UnityThread.OnUpdate>();
  private readonly List<UnityThread.OnGui> onGuiMethods = new List<UnityThread.OnGui>();
  private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

  private static UnityThread Instance { get; } = UnityThread.InjectToScene();

  private void Update()
  {
    Action result;
    while (this.actionQueue.TryDequeue(out result))
      result();
    TargeterDispatcher.TargeterUpdate();
    for (int index = this.onUpdateMethods.Count - 1; index >= 0; --index)
    {
      if (!this.onUpdateMethods[index]())
        this.onUpdateMethods.RemoveAt(index);
    }
  }

  private void OnGUI()
  {
    TargeterDispatcher.TargeterOnGUI();
    for (int index = this.onGuiMethods.Count - 1; index >= 0; --index)
    {
      try
      {
        if (!this.onGuiMethods[index]())
          this.onGuiMethods.RemoveAt(index);
      }
      catch (Exception ex)
      {
        this.onGuiMethods.RemoveAt(index);
        Log.Error($"Exception thrown from OnGUI.{Environment.NewLine}{ex}");
      }
    }
  }

  public static void RemoveUpdate(UnityThread.OnUpdate onUpdate)
  {
    if (!UnityData.IsInMainThread)
      Trace.Fail("Trying to remove update method to queue from another thread. This can only be done from the main thread.");
    else
      UnityThread.Instance.onUpdateMethods.Remove(onUpdate);
  }

  public static void StartUpdate(UnityThread.OnUpdate onUpdate)
  {
    if (!UnityData.IsInMainThread)
      Trace.Fail("Trying to add update method to queue from another thread. This can only be done from the main thread.");
    else
      UnityThread.Instance.onUpdateMethods.Add(onUpdate);
  }

  public static void StartGUI(UnityThread.OnGui onGui)
  {
    if (!UnityData.IsInMainThread)
      Trace.Fail("Trying to add OnGUI method to queue from another thread. This can only be done from the main thread.");
    else
      UnityThread.Instance.onGuiMethods.Add(onGui);
  }

  public static void RemoveOnGUI(UnityThread.OnGui onGui)
  {
    if (!UnityData.IsInMainThread)
      Trace.Fail("Trying to remove OnGUI method to queue from another thread. This can only be done from the main thread.");
    else
      UnityThread.Instance.onGuiMethods.Remove(onGui);
  }

  public static void ExecuteOnMainThread(Action action)
  {
    if (action == null)
      throw new ArgumentNullException(nameof (action));
    if (UnityData.IsInMainThread)
      action();
    else
      UnityThread.Instance.actionQueue.Enqueue(action);
  }

  public static void ExecuteOnMainThreadAndWait(Action action, int waitTimeout = 5000)
  {
    if (action == null)
      throw new ArgumentNullException(nameof (action));
    if (waitTimeout <= 0)
      throw new ArgumentException("waitTimeout must be greater than 0.");
    if (UnityData.IsInMainThread)
    {
      action();
    }
    else
    {
      UnityThread.ConcurrentAction concurrentAction = new UnityThread.ConcurrentAction(action);
      UnityThread.Instance.actionQueue.Enqueue(new Action(concurrentAction.InvokeAndDispose));
      concurrentAction.Wait(waitTimeout);
    }
  }

  private static UnityThread InjectToScene()
  {
    GameObject gameObject = new GameObject(nameof (UnityThread));
    UnityThread scene = gameObject.AddComponent<UnityThread>();
    Object.DontDestroyOnLoad((Object) gameObject);
    return scene;
  }

  internal static bool InUpdateQueue(UnityThread.OnUpdate update)
  {
    return UnityThread.Instance.onUpdateMethods.Contains(update);
  }

  public delegate bool OnUpdate();

  public delegate bool OnGui();

  private class ConcurrentAction : IDisposable
  {
    private readonly Action action;
    private readonly ManualResetEventSlim waitHandle = new ManualResetEventSlim();

    public ConcurrentAction(Action action) => this.action = action;

    public void InvokeAndDispose()
    {
      try
      {
        this.action();
      }
      finally
      {
        this.Dispose();
      }
    }

    public bool Wait(int waitTimeout) => this.waitHandle.Wait(waitTimeout);

    public void Dispose()
    {
      this.waitHandle.Dispose();
      GC.SuppressFinalize((object) this);
    }
  }
}
