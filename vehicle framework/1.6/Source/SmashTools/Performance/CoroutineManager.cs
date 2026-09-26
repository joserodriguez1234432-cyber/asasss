// Decompiled with JetBrains decompiler
// Type: SmashTools.CoroutineManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class CoroutineManager : MonoBehaviour
{
  private const int MaxExecutionTimePerFrame = 0;
  private readonly ConcurrentQueue<CoroutineManager.Enumerator> enumerators = new ConcurrentQueue<CoroutineManager.Enumerator>();
  private float executionTimeElapsed;

  public bool Running { get; private set; }

  public bool NeedsRestart => !this.Running && this.enumerators.Count > 0;

  public static CoroutineManager Instance { get; } = CoroutineManager.InjectToScene();

  public static void QueueInvoke(Action action)
  {
    CoroutineManager.Instance.enumerators.Enqueue(new CoroutineManager.Enumerator(action));
    CoroutineManager.Instance.RunQueue();
  }

  public static void QueueInvoke(Func<IEnumerator> enumerator)
  {
    CoroutineManager.Instance.enumerators.Enqueue(new CoroutineManager.Enumerator(enumerator));
    CoroutineManager.Instance.RunQueue();
  }

  public static void QueueOrInvoke(Action action, float waitSeconds = 0.0f)
  {
    if ((double) waitSeconds > 0.0)
      CoroutineManager.QueueInvoke((Func<IEnumerator>) (() => CoroutineManager.YieldForInvoking(action, waitSeconds)));
    else
      action();
  }

  public static void StartCoroutine(Func<IEnumerator> enumerator)
  {
    CoroutineManager.Instance.StartCoroutine(enumerator());
  }

  private static IEnumerator YieldForInvoking(Action action, float waitSeconds)
  {
    action();
    yield return (object) new WaitForSeconds(waitSeconds);
  }

  public static IEnumerator YieldTillKeyDown(KeyCode keyCode)
  {
    while (!Input.GetKeyDown(keyCode))
      yield return (object) null;
  }

  private void RunQueue()
  {
    if (!this.NeedsRestart)
      return;
    this.StartCoroutine(this.ExecuteQueue());
  }

  public void Clear()
  {
    while (this.enumerators.Count > 0)
      this.enumerators.TryDequeue(out CoroutineManager.Enumerator _);
  }

  private IEnumerator ExecuteQueue()
  {
    this.Running = true;
    this.executionTimeElapsed = Time.realtimeSinceStartup;
    CoroutineManager.Enumerator result;
    while (this.enumerators.TryDequeue(out result))
    {
      if (result.Enumerate)
      {
        foreach (object obj in result)
          yield return obj;
        IEnumerator subEnumerator;
        if (subEnumerator is IDisposable disposable)
          disposable.Dispose();
        subEnumerator = (IEnumerator) null;
      }
      else
        result.Invoke();
      if ((double) this.executionTimeElapsed > (double) Time.realtimeSinceStartup + 0.0)
      {
        this.executionTimeElapsed = Time.realtimeSinceStartup;
        yield return (object) null;
      }
    }
    this.Running = false;
  }

  private static CoroutineManager InjectToScene()
  {
    GameObject gameObject = new GameObject(nameof (CoroutineManager));
    CoroutineManager scene = gameObject.AddComponent<CoroutineManager>();
    Object.DontDestroyOnLoad((Object) gameObject);
    return scene;
  }

  private class Enumerator
  {
    private readonly Action action;
    private readonly Func<IEnumerator> enumerator;

    public Enumerator(Func<IEnumerator> enumerator) => this.enumerator = enumerator;

    public Enumerator(Action action) => this.action = action;

    public bool Enumerate => this.enumerator != null;

    public IEnumerator GetEnumerator() => this.enumerator();

    public void Invoke() => this.action();

    public override string ToString()
    {
      return !this.Enumerate ? this.action.Method.Name : this.enumerator.Method.Name;
    }
  }
}
