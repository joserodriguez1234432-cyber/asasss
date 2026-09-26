// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Params`1
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Runtime.CompilerServices;

#nullable disable
namespace VehicleMapFramework;

public static class Params<T> where T : struct, ITuple
{
  [ThreadStatic]
  private static object[] @params;

  public static object[] Get(T tuple)
  {
    if (Params<T>.@params == null)
      Params<T>.@params = new object[tuple.Length];
    for (int index = 0; index < tuple.Length; ++index)
      Params<T>.@params[index] = tuple[index];
    return Params<T>.@params;
  }
}
