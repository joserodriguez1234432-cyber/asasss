// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Conversion
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Conversion
{
  public static Pair<T1, T2> ToPair<T1, T2>(this Tuple<T1, T2> tuple)
  {
    return new Pair<T1, T2>(tuple.Item1, tuple.Item2);
  }

  public static Tuple<T1, T2> ToTuple<T1, T2>(this Pair<T1, T2> pair)
  {
    return new Tuple<T1, T2>(pair.First, pair.Second);
  }
}
