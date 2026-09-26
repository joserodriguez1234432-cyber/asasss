// Decompiled with JetBrains decompiler
// Type: Vehicles.MaterialRequestRGB
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public struct MaterialRequestRGB
{
  public Shader shader;
  public Texture2D mainTex;
  public PatternProperties properties;
  public Texture2D maskTex;
  public Texture2D patternTex;
  public int renderQueue;
  public List<ShaderParameter> shaderParameters;
  public Color color;
  public Color colorTwo;
  public Color colorThree;
  public float tiles;
  public Vector2 displacement;

  public MaterialRequestRGB(Texture2D tex)
  {
    this.shader = ShaderDatabase.Cutout;
    this.mainTex = tex;
    this.properties = new PatternProperties();
    this.color = Color.white;
    this.colorTwo = Color.white;
    this.colorThree = Color.white;
    this.tiles = 1f;
    this.displacement = Vector2.zero;
    this.maskTex = (Texture2D) null;
    this.patternTex = (Texture2D) null;
    this.renderQueue = 0;
    this.shaderParameters = (List<ShaderParameter>) null;
  }

  public MaterialRequestRGB(Texture2D tex, Shader shader)
  {
    this.shader = shader;
    this.mainTex = tex;
    this.maskTex = (Texture2D) null;
    this.properties = new PatternProperties();
    this.color = Color.white;
    this.colorTwo = Color.white;
    this.colorThree = Color.white;
    this.tiles = 1f;
    this.displacement = Vector2.zero;
    this.patternTex = (Texture2D) null;
    this.renderQueue = 0;
    this.shaderParameters = (List<ShaderParameter>) null;
  }

  public MaterialRequestRGB(Texture2D tex, Shader shader, PatternProperties properties)
  {
    this.shader = shader;
    this.mainTex = tex;
    this.maskTex = (Texture2D) null;
    this.properties = properties;
    Color? nullable = properties.colorOne;
    this.color = nullable ?? Color.white;
    nullable = properties.colorTwo;
    this.colorTwo = nullable ?? Color.white;
    nullable = properties.colorThree;
    this.colorThree = nullable ?? Color.white;
    this.tiles = GenCollection.TryGetValue<string, float>((IReadOnlyDictionary<string, float>) properties.tiles, "All", 1f);
    this.displacement = Vector2.zero;
    this.patternTex = (Texture2D) null;
    this.renderQueue = 0;
    this.shaderParameters = (List<ShaderParameter>) null;
  }

  public MaterialRequestRGB(
    MaterialRequest req,
    Texture2D patternTex,
    PatternProperties properties)
  {
    this.shader = req.shader;
    this.mainTex = req.mainTex as Texture2D;
    this.maskTex = req.maskTex;
    this.properties = properties;
    Color? nullable = properties.colorOne;
    this.color = nullable ?? Color.white;
    nullable = properties.colorTwo;
    this.colorTwo = nullable ?? Color.white;
    nullable = properties.colorThree;
    this.colorThree = nullable ?? Color.white;
    this.tiles = GenCollection.TryGetValue<string, float>((IReadOnlyDictionary<string, float>) properties.tiles, "All", 1f);
    this.displacement = Vector2.zero;
    this.patternTex = patternTex;
    this.renderQueue = req.renderQueue;
    this.shaderParameters = req.shaderParameters;
  }

  public string BaseTexPath
  {
    set => this.mainTex = ContentFinder<Texture2D>.Get(value, true);
  }

  public override readonly int GetHashCode()
  {
    return Gen.HashCombine<List<ShaderParameter>>(Gen.HashCombineInt(Gen.HashCombine<Texture2D>(Gen.HashCombine<Texture2D>(Gen.HashCombine<Color>(Gen.HashCombine<Color>(Gen.HashCombine<Color>(Gen.HashCombine<float>(Gen.HashCombine<Vector2>(0, this.displacement), this.tiles), this.color), this.colorTwo), this.colorThree), this.mainTex), this.maskTex), this.renderQueue), this.shaderParameters);
  }

  public override bool Equals(object obj) => obj is MaterialRequestRGB other && this.Equals(other);

  public bool Equals(MaterialRequestRGB other)
  {
    if (Object.op_Equality((Object) other.shader, (Object) this.shader) && Object.op_Equality((Object) other.mainTex, (Object) this.mainTex))
    {
      Color? nullable1 = other.properties.colorOne;
      Color? colorOne = this.properties.colorOne;
      if ((nullable1.HasValue == colorOne.HasValue ? (nullable1.HasValue ? (Color.op_Equality(nullable1.GetValueOrDefault(), colorOne.GetValueOrDefault()) ? 1 : 0) : 1) : 0) != 0)
      {
        Color? nullable2 = other.properties.colorTwo;
        nullable1 = this.properties.colorTwo;
        if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (Color.op_Equality(nullable2.GetValueOrDefault(), nullable1.GetValueOrDefault()) ? 1 : 0) : 1) : 0) != 0)
        {
          nullable1 = other.properties.colorThree;
          nullable2 = this.properties.colorThree;
          if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (Color.op_Equality(nullable1.GetValueOrDefault(), nullable2.GetValueOrDefault()) ? 1 : 0) : 1) : 0) != 0 && Object.op_Equality((Object) other.maskTex, (Object) this.maskTex) && Object.op_Equality((Object) other.patternTex, (Object) this.patternTex) && other.renderQueue == this.renderQueue && other.shaderParameters == this.shaderParameters && (double) other.tiles == (double) this.tiles)
            return Vector2.op_Equality(other.displacement, this.displacement);
        }
      }
    }
    return false;
  }

  public static bool operator ==(MaterialRequestRGB lhs, MaterialRequestRGB rhs) => lhs.Equals(rhs);

  public static bool operator !=(MaterialRequestRGB lhs, MaterialRequestRGB rhs) => !(lhs == rhs);

  public override string ToString()
  {
    return $"MaterialRGBRequest({((Object) this.shader).name}, {((Object) this.mainTex).name}, {this.properties}, {this.maskTex}, {this.patternTex}, " + $"{this.renderQueue})";
  }
}
