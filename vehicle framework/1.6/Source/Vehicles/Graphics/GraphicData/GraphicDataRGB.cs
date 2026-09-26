// Decompiled with JetBrains decompiler
// Type: Vehicles.GraphicDataRGB
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class GraphicDataRGB : GraphicDataLayered
{
  public Color colorThree = Color.white;
  public float tiles = 1f;
  public Vector2 displacement = Vector2.zero;
  public PatternDef pattern;
  private Graphic_Rgb cachedRGBGraphic;

  public Graphic_Rgb Graphic
  {
    get
    {
      if (this.cachedRGBGraphic == null && !this.shaderType.Shader.SupportsRGBMaskTex())
        this.cachedRGBGraphic = base.Graphic as Graphic_Rgb;
      return this.cachedRGBGraphic;
    }
  }

  public virtual void CopyDrawData(GraphicDataRGB graphicData)
  {
    this.color = graphicData.color;
    this.colorTwo = graphicData.colorTwo;
    this.colorThree = graphicData.colorThree;
    this.tiles = graphicData.tiles;
    this.displacement = graphicData.displacement;
    this.pattern = graphicData.pattern ?? PatternDefOf.Default;
  }

  public override void CopyFrom(GraphicDataLayered graphicData)
  {
    base.CopyFrom(graphicData);
    if (!(graphicData is GraphicDataRGB graphicDataRgb))
      return;
    this.colorThree = graphicDataRgb.colorThree;
    this.pattern = graphicDataRgb.pattern ?? PatternDefOf.Default;
  }

  public virtual void CopyFrom(
    GraphicDataLayered graphicData,
    PatternDef pattern,
    Color colorThree)
  {
    this.CopyFrom(graphicData);
    if (!(graphicData is GraphicDataRGB))
      return;
    this.colorThree = colorThree;
    this.pattern = pattern ?? PatternDefOf.Default;
  }

  public override void Init(IMaterialCacheTarget target)
  {
    base.Init(target);
    if ((object) this.graphicClass == null)
    {
      this.cachedRGBGraphic = (Graphic_Rgb) null;
    }
    else
    {
      if (this.pattern == null)
        this.pattern = PatternDefOf.Default;
      ShaderTypeDef shaderTypeDef = this.pattern is SkinDef ? VehicleShaderTypeDefOf.CutoutComplexSkin : this.shaderType;
      if (shaderTypeDef == null)
      {
        this.color = Color.white;
        this.colorTwo = Color.white;
        this.colorThree = Color.white;
        shaderTypeDef = ShaderTypeDefOf.Cutout;
      }
      if (!VehicleMod.settings.main.useCustomShaders)
        shaderTypeDef = shaderTypeDef.Shader.SupportsRGBMaskTex(true) ? ShaderTypeDefOf.CutoutComplex : ShaderTypeDefOf.Cutout;
      Shader shader = shaderTypeDef.Shader;
      this.cachedRGBGraphic = GraphicDatabaseRGB.Get(target, this.graphicClass, this.texPath, shader, this.drawSize, this.color, this.colorTwo, this.colorThree, this.tiles, this.displacement.x, this.displacement.y, this, this.shaderParameters);
      AccessTools.Field(typeof (GraphicData), "cachedGraphic").SetValue((object) this, (object) this.cachedRGBGraphic);
    }
  }

  public virtual string ToString()
  {
    return $"({this.texPath}, {this.color}, {this.colorTwo}, {this.colorThree}, {this.pattern}, {this.tiles}, {this.displacement})";
  }
}
