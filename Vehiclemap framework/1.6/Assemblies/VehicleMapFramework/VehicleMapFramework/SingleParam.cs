// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SingleParam
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;

#nullable disable
namespace VehicleMapFramework;

public static class SingleParam
{
  [ThreadStatic]
  private static object[] singleParam;

  public static object[] Get(object param)
  {
    if (SingleParam.singleParam == null)
      SingleParam.singleParam = new object[1];
    SingleParam.singleParam[0] = param;
    return SingleParam.singleParam;
  }
}
