// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.ObjectCountWatcher`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace SmashTools.Performance;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ObjectCountWatcher<T> : IDisposable
{
  public ObjectCountWatcher() => ObjectCounter.StartWatcher<T>();

  public int Count => ObjectCounter.GetWatchedCount<T>();

  void IDisposable.Dispose() => ObjectCounter.EndWatcher<T>();
}
