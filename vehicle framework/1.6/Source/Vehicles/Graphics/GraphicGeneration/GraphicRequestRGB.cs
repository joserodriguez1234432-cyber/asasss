// Decompiled with JetBrains decompiler
// Type: Vehicles.GraphicRequestRGB
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public struct GraphicRequestRGB(
  IMaterialCacheTarget target,
  System.Type graphicClass,
  string path,
  Shader shader,
  Vector2 drawSize,
  Color color,
  Color colorTwo,
  Color colorThree,
  float tiles,
  Vector2 displacement,
  GraphicDataRGB graphicData,
  int renderQueue,
  List<ShaderParameter> shaderParameters) : IEquatable<GraphicRequestRGB>
{
  public IMaterialCacheTarget target = target;
  public System.Type graphicClass = graphicClass;
  public string path = path;
  public Shader shader = shader;
  public Vector2 drawSize = drawSize;
  public Color color = color;
  public Color colorTwo = colorTwo;
  public Color colorThree = colorThree;
  public float tiles = tiles;
  public Vector2 displacement = displacement;
  public GraphicDataRGB graphicData = graphicData;
  public int renderQueue = renderQueue;
  public List<ShaderParameter> shaderParameters = GenList.NullOrEmpty<ShaderParameter>((IList<ShaderParameter>) shaderParameters) ? (List<ShaderParameter>) null : shaderParameters;

  public string Summary
  {
    get
    {
      return $"Target: {this.target}\nType: {this.graphicClass}\nPath: {this.path}\nShader: {this.shader}\n" + $"DrawSize: {this.drawSize}\nColors: {this.color}|{this.colorTwo}|{this.colorThree}\nGraphicData: {this.graphicData}\n" + $"RenderQueue: {this.renderQueue}\nParams Count: {this.shaderParameters?.Count.ToString() ?? "Null"}";
    }
  }

  public override int GetHashCode()
  {
    if (this.path == null)
      this.path = BaseContent.BadTexPath;
    return this.target.GetHashCode();
  }

  public override bool Equals(object obj) => obj is GraphicRequestRGB other && this.Equals(other);

  public bool Equals(GraphicRequestRGB other) => other.target == this.target;

  public static bool operator ==(GraphicRequestRGB lhs, GraphicRequestRGB rhs) => lhs.Equals(rhs);

  public static bool operator !=(GraphicRequestRGB lhs, GraphicRequestRGB rhs) => !(lhs == rhs);

  public static implicit operator GraphicRequest(GraphicRequestRGB req)
  {
    return new GraphicRequest(req.graphicClass, req.path, req.shader, req.drawSize, req.color, req.colorTwo, (GraphicData) req.graphicData, req.renderQueue, req.shaderParameters, (string) null);
  }
}
