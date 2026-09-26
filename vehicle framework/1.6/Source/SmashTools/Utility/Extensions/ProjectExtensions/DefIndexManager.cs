// Decompiled with JetBrains decompiler
// Type: SmashTools.DefIndexManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
internal static class DefIndexManager
{
  static DefIndexManager()
  {
    foreach (Type type in GenTypes.AllSubclassesNonAbstract(typeof (Def)))
    {
      if (type.HasInterface(typeof (IDefIndex<>)))
        GenGeneric.InvokeStaticMethodOnGenericType(typeof (DefIndexManager.Indexer<>), type, "Init");
    }
  }

  public static class Indexer<T> where T : Def
  {
    private static int nextIndex;
    private static bool initialized;

    public static void Init()
    {
      if (DefIndexManager.Indexer<T>.initialized)
      {
        Log.Error("Attempting to reinit DefIndexManager.");
      }
      else
      {
        foreach (T obj in DefDatabase<T>.AllDefsListForReading)
        {
          if (obj is IDefIndex<T> defIndex)
          {
            int num = DefIndexManager.Indexer<T>.nextIndex++;
            defIndex.DefIndex = num;
          }
        }
        DefIndexManager.Indexer<T>.initialized = true;
      }
    }
  }
}
