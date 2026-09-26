// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Enum
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class Ext_Enum
{
  public static T Min<T>(T a, T b) where T : Enum => Comparer<T>.Default.Compare(a, b) > 0 ? b : a;

  public static T Max<T>(T a, T b) where T : Enum => Comparer<T>.Default.Compare(a, b) < 0 ? b : a;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsAnyBitSet<T>(this T a, T b) where T : unmanaged, Enum
  {
    return (*(int*) &a & *(int*) &b) != 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool AreAllBitsSet<T>(this T a, T b) where T : unmanaged, Enum
  {
    int num = *(int*) &b;
    return (*(int*) &a & num) == num;
  }
}
