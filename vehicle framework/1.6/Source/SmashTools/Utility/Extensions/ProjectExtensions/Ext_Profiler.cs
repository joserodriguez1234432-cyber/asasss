// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.Ext_Profiler
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#nullable disable
namespace SmashTools.Performance;

[PublicAPI]
public static class Ext_Profiler
{
  private static readonly double SecondsThreshold = 0.1 * (double) Stopwatch.Frequency;
  private static readonly double MillisecondThreshold = 0.0001 * (double) Stopwatch.Frequency;
  private static readonly double MicrosecondThreshold = 1E-07 * (double) Stopwatch.Frequency;

  public static double ConvertTo(this double ticks, Profiler.Measurement measurement)
  {
    switch (measurement)
    {
      case Profiler.Measurement.Seconds:
        return Ext_Profiler.ToSeconds(ticks);
      case Profiler.Measurement.Milliseconds:
        return Ext_Profiler.ToMilliseconds(ticks);
      case Profiler.Measurement.Microseconds:
        return Ext_Profiler.ToMicroseconds(ticks);
      case Profiler.Measurement.Nanoseconds:
        return Ext_Profiler.ToNanoseconds(ticks);
      default:
        throw new NotImplementedException("Measurement");
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double ToSeconds(double ticks) => ticks / (double) Stopwatch.Frequency;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double ToMilliseconds(double ticks)
  {
    return ticks * 1000.0 / (double) Stopwatch.Frequency;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double ToMicroseconds(double ticks)
  {
    return ticks * 1000000.0 / (double) Stopwatch.Frequency;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double ToNanoseconds(double ticks)
  {
    return ticks * 1000000000.0 / (double) Stopwatch.Frequency;
  }

  public static double ToNearestMeasurement(this double ticks, out Profiler.Measurement measurement)
  {
    double num = Math.Abs(ticks);
    if (num >= Ext_Profiler.SecondsThreshold)
    {
      measurement = Profiler.Measurement.Seconds;
      return Ext_Profiler.ToSeconds(ticks);
    }
    if (num >= Ext_Profiler.MillisecondThreshold)
    {
      measurement = Profiler.Measurement.Milliseconds;
      return Ext_Profiler.ToMilliseconds(ticks);
    }
    measurement = Profiler.Measurement.Microseconds;
    return Ext_Profiler.ToMicroseconds(ticks);
  }

  public static string ToMeasurementString(double ticks)
  {
    Profiler.Measurement measurement;
    return $"{ticks.ToNearestMeasurement(out measurement):#0.#} {MeasurementSuffix(measurement)}";

    static string MeasurementSuffix(Profiler.Measurement measurement)
    {
      switch (measurement)
      {
        case Profiler.Measurement.Seconds:
          return "s";
        case Profiler.Measurement.Milliseconds:
          return "ms";
        case Profiler.Measurement.Microseconds:
          return "us";
        case Profiler.Measurement.Nanoseconds:
          return "ns";
        default:
          throw new NotImplementedException();
      }
    }
  }
}
