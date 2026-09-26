// Decompiled with JetBrains decompiler
// Type: SmashTools.Trace
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Diagnostics;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class Trace
{
  [Conditional("TRACE")]
  public static void IsTrue(bool condition, string message = null)
  {
    if (condition)
      return;
    Trace.Fail(message);
  }

  [Conditional("TRACE")]
  public static void IsFalse(bool condition, string message = null)
  {
    if (!condition)
      return;
    Trace.Fail(message);
  }

  [Conditional("TRACE")]
  public static void IsNull<T>(T obj, string message = null) where T : class
  {
    if ((object) obj == null)
      return;
    Trace.Fail(message);
  }

  [Conditional("TRACE")]
  public static void IsNotNull<T>(T obj, string message = null) where T : class
  {
    if ((object) obj != null)
      return;
    Trace.Fail(message);
  }

  [Conditional("TRACE")]
  public static void Fail(string message = null)
  {
    Log.Error($"{message ?? "Assertion Failed"}\nStackTrace:\n{StackTraceUtility.ExtractStackTrace()}");
  }
}
