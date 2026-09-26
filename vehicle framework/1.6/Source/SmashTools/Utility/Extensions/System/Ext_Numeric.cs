// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Numeric
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Numeric
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Clamp(this float val, float min, float max) => Mathf.Clamp(val, min, max);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Clamp(this int val, int min, int max) => Mathf.Clamp(val, min, max);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float ClampAngle(this float theta) => Mathf.Repeat(theta, 360f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InRange(this FloatRange range, float value)
  {
    return (double) value >= (double) range.min && (double) value <= (double) range.max;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InRange(this IntRange range, int value)
  {
    return value >= range.min && value <= range.max;
  }
}
