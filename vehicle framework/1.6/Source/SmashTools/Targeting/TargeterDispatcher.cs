// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.TargeterDispatcher
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools.Targeting;

public static class TargeterDispatcher
{
  private static readonly Stack<ITargeter> Targeters = new Stack<ITargeter>();

  private static ITargeter Current { get; set; }

  internal static void TargeterUpdate()
  {
    if (TargeterDispatcher.Current == null)
      return;
    try
    {
      TargeterDispatcher.Current.Update();
    }
    catch (Exception ex)
    {
      ITargeter targeter;
      TargeterDispatcher.Targeters.TryPop(ref targeter);
      Log.Error($"Root level exception in TargeterUpdate: {ex}");
    }
  }

  internal static void TargeterOnGUI()
  {
    if (TargeterDispatcher.Current == null)
      return;
    try
    {
      TargeterDispatcher.Current.OnGUI();
    }
    catch (Exception ex)
    {
      TargeterDispatcher.Targeters.Pop();
      TargeterDispatcher.UpdateCurrent();
      Log.Error($"Root level exception in TargeterOnGUI: {ex}");
    }
  }

  public static void Start(this ITargeter targeter)
  {
    TargeterDispatcher.Targeters.Push(targeter);
    targeter.OnStart();
    TargeterDispatcher.UpdateCurrent();
  }

  public static void Stop(this ITargeter targeter)
  {
    if (TargeterDispatcher.Targeters.Count == 0)
      throw new InvalidOperationException("Trying to stop targeter but the targeter stack is empty.");
    if (TargeterDispatcher.Targeters.Peek() == targeter)
    {
      TargeterDispatcher.Targeters.Pop();
    }
    else
    {
      Log.Error("Removing targeter out of sequence.");
      TargeterDispatcher.Remove(targeter);
    }
    targeter.OnStop();
    TargeterDispatcher.UpdateCurrent();
  }

  private static void UpdateCurrent()
  {
    TargeterDispatcher.Current = TargeterDispatcher.Targeters.Count > 0 ? TargeterDispatcher.Targeters.Peek() : (ITargeter) null;
  }

  private static void Remove(ITargeter targeter)
  {
    Stack<ITargeter> targeterStack = new Stack<ITargeter>();
    while (TargeterDispatcher.Targeters.Count > 0)
    {
      ITargeter targeter1 = TargeterDispatcher.Targeters.Pop();
      if (targeter1 != targeter)
        targeterStack.Push(targeter1);
      else
        break;
    }
    while (targeterStack.Count > 0)
      TargeterDispatcher.Targeters.Push(targeterStack.Pop());
  }
}
