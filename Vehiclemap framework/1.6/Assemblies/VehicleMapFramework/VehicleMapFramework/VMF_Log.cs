// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_Log
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Diagnostics;
using Verse;

#nullable disable
namespace VehicleMapFramework;

internal static class VMF_Log
{
  private const string LogLabel = "[VehicleMapFramework]";

  public static void Error(string message)
  {
    if (UnitTestDetector.IsTestingContext)
      Console.WriteLine(message);
    Log.Error($"{"[VehicleMapFramework]"} {message}\n{new StackTrace(2, true)}");
  }

  public static void Warning(string message) => Log.Warning("[VehicleMapFramework] " + message);

  public static void Message(string message) => Log.Message("[VehicleMapFramework] " + message);

  public static void Message(object obj) => Log.Message($"{"[VehicleMapFramework]"} {obj}");

  [Conditional("DEBUG")]
  public static void DebugMessage(string message) => Log.Message(message);

  [Conditional("DEBUG")]
  [Conditional("DEV")]
  public static void DebugWarning(string message) => Log.Warning(message);

  [Conditional("DEBUG")]
  [Conditional("DEV")]
  public static void DebugError(string message) => Log.Error(message);
}
