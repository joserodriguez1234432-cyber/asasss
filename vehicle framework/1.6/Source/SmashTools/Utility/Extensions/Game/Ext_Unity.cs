// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Unity
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Unity
{
  public static Pair<float, float> ToPair(this Vector2 vector)
  {
    return new Pair<float, float>(vector.x, vector.y);
  }

  public static Pair<float, float> ToPair(this Vector3 vector)
  {
    return new Pair<float, float>(vector.x, vector.z);
  }

  public static Vector3 MirrorHorizontal(this Vector3 vector)
  {
    return new Vector3(-vector.x, vector.y, vector.z);
  }

  public static Vector3 MirrorVertical(this Vector3 vector)
  {
    return new Vector3(vector.x, vector.y, -vector.z);
  }
}
