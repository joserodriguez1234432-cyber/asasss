// Decompiled with JetBrains decompiler
// Type: SmashTools.TaskManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using SmashTools.Performance;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class TaskManager
{
  [Pure]
  public static Task Run(Action action, CancellationToken token)
  {
    return ForgetAwaited(Task.Run(action, token));

    static async Task ForgetAwaited(Task task)
    {
      try
      {
        await task.ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Trace.Fail($"Exception thrown executing task.\n{ex}");
      }
    }
  }

  public static void FireAndForget(AsyncAction action, CancellationToken token)
  {
    TaskManager.Run(new Action(action.Invoke), token);
  }
}
