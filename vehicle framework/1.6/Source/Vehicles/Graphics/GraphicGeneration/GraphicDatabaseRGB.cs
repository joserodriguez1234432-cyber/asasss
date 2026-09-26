// Decompiled with JetBrains decompiler
// Type: Vehicles.GraphicDatabaseRGB
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class GraphicDatabaseRGB
{
  private static readonly Dictionary<IMaterialCacheTarget, Graphic_Rgb> allGraphics = new Dictionary<IMaterialCacheTarget, Graphic_Rgb>();

  public static Graphic_Rgb Get(
    IMaterialCacheTarget target,
    System.Type graphicClass,
    string path,
    Shader shader,
    Vector2 drawSize,
    Color color,
    Color colorTwo,
    Color colorThree,
    float tiles = 1f,
    float displacementX = 0.0f,
    float displacementY = 0.0f,
    GraphicDataRGB data = null,
    List<ShaderParameter> shaderParameters = null)
  {
    GraphicRequestRGB req = new GraphicRequestRGB(target, graphicClass, path, shader, drawSize, color, colorTwo, colorThree, tiles, new Vector2(displacementX, displacementY), data, 0, shaderParameters);
    try
    {
      if (req.graphicClass == typeof (Graphic_Vehicle))
        return (Graphic_Rgb) GraphicDatabaseRGB.GetInner<Graphic_Vehicle>(req);
      if (req.graphicClass == typeof (Graphic_Turret))
        return (Graphic_Rgb) GraphicDatabaseRGB.GetInner<Graphic_Turret>(req);
      return (Graphic_Rgb) GenGeneric.InvokeStaticGenericMethod(typeof (GraphicDatabaseRGB), req.graphicClass, "GetInner", new object[1]
      {
        (object) req
      });
    }
    catch (Exception ex)
    {
      Log.Error($"Exception getting {graphicClass} at {path}. Exception=\"{ex}\"");
    }
    return (Graphic_Rgb) null;
  }

  private static T GetInner<T>(GraphicRequestRGB req) where T : Graphic_Rgb, new()
  {
    Graphic_Rgb inner;
    if (!GraphicDatabaseRGB.allGraphics.TryGetValue(req.target, out inner))
    {
      inner = (Graphic_Rgb) new T();
      inner.Init(req);
      GraphicDatabaseRGB.allGraphics.Add(req.target, inner);
    }
    return (T) inner;
  }

  public static bool Remove(IMaterialCacheTarget target)
  {
    return GraphicDatabaseRGB.allGraphics.Remove(target);
  }

  public static void Clear() => GraphicDatabaseRGB.allGraphics.Clear();
}
