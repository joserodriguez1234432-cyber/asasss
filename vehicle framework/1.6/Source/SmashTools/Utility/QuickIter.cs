// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.QuickIter
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Verse;

#nullable disable
namespace SmashTools.Performance;

public static class QuickIter
{
  public static void EnumerateAllTypes(QuickIter.TypeProcessor processor)
  {
    List<Type> typeList = GenTypes.AllTypes;
    Parallel.ForEach<Tuple<int, int>>((Partitioner<Tuple<int, int>>) Partitioner.Create(0, typeList.Count), (Action<Tuple<int, int>, ParallelLoopState>) ((range, _) =>
    {
      for (int index = range.Item1; index < range.Item2; ++index)
        processor(typeList[index]);
    }));
  }

  public static void EnumerateAllModTypes(QuickIter.TypeProcessor processor)
  {
    List<Type> types = new List<Type>();
    foreach (ModContentPack modContentPack in LoadedModManager.RunningModsListForReading)
    {
      foreach (Assembly loadedAssembly in modContentPack.assemblies.loadedAssemblies)
      {
        try
        {
          Type[] types1 = loadedAssembly.GetTypes();
          types.AddRange((IEnumerable<Type>) types1);
        }
        catch (Exception ex) when (
        {
          // ISSUE: unable to correctly present filter
          bool flag;
          switch (ex)
          {
            case ReflectionTypeLoadException _:
            case TypeLoadException _:
              flag = true;
              break;
            default:
              flag = false;
              break;
          }
          if (flag)
          {
            SuccessfulFiltering;
          }
          else
            throw;
        }
        )
        {
          Log.Error($"Exception loading types from {loadedAssembly.FullName}. Mod compatible with RimWorld version: {GenText.ToStringYesNo(modContentPack.ModMetaData.VersionCompatible)}\n{ex}");
        }
      }
    }
    Parallel.ForEach<Tuple<int, int>>((Partitioner<Tuple<int, int>>) Partitioner.Create(0, types.Count), (Action<Tuple<int, int>, ParallelLoopState>) ((range, _) =>
    {
      for (int index = range.Item1; index < range.Item2; ++index)
        processor(types[index]);
    }));
  }

  public delegate void TypeProcessor(Type type);
}
