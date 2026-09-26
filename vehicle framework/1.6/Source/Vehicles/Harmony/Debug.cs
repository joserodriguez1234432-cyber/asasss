// Decompiled with JetBrains decompiler
// Type: Vehicles.Debug
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public static class Debug
{
  public static void Message(string text)
  {
    if (!VehicleMod.settings.debug.debugLogging)
      return;
    Log.Message(text);
  }

  public static void Warning(string text)
  {
    if (!VehicleMod.settings.debug.debugLogging)
      return;
    Log.Warning(text);
  }

  public static void Error(string text)
  {
    if (!VehicleMod.settings.debug.debugLogging)
      return;
    Log.Error(text);
  }
}
