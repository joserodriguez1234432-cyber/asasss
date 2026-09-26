// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.StaticConstructorOnStartupPriorityUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public static class StaticConstructorOnStartupPriorityUtility
{
  static StaticConstructorOnStartupPriorityUtility()
  {
    List<Type> typeList = GenTypes.AllTypesWithAttribute<StaticConstructorOnStartupPriorityAttribute>();
    GenCollection.SortByDescending<Type, int>(typeList, (Func<Type, int>) (t => t.GetCustomAttribute<StaticConstructorOnStartupPriorityAttribute>().priority));
    foreach (Type type in typeList)
    {
      try
      {
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);
      }
      catch (Exception ex)
      {
        Log.Error($"Error in static constructor of {type}: {ex}");
      }
    }
  }
}
