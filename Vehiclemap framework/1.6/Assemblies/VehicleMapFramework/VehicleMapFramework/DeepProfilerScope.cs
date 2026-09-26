// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.DeepProfilerScope
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public readonly struct DeepProfilerScope : IDisposable
{
  private readonly bool force;
  private readonly bool enabled;
  private readonly bool logVerbose;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DeepProfilerScope(string label, bool force = false)
  {
    this.force = false;
    this.enabled = false;
    this.logVerbose = false;
    if (force)
    {
      this.force = true;
      this.enabled = DeepProfiler.enabled;
      this.logVerbose = Prefs.LogVerbose;
      DeepProfiler.enabled = true;
      Prefs.LogVerbose = true;
    }
    DeepProfiler.Start(label);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  void IDisposable.Dispose()
  {
    DeepProfiler.End();
    if (!this.force)
      return;
    DeepProfiler.enabled = this.enabled;
    Prefs.LogVerbose = this.logVerbose;
  }
}
