// Decompiled with JetBrains decompiler
// Type: Vehicles.PatternData
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class PatternData : IExposable
{
  public Color color = Color.white;
  public Color colorTwo = Color.white;
  public Color colorThree = Color.white;
  public float tiles = 1f;
  public Vector2 displacement = Vector2.zero;
  public PatternDef patternDef;
  private string patternId = "Default";

  public PatternData()
  {
  }

  public PatternData(VehiclePawn vehicle)
    : this(((Thing) vehicle).DrawColor, vehicle.DrawColorTwo, vehicle.DrawColorThree, vehicle.Pattern, vehicle.Displacement, vehicle.Tiles)
  {
  }

  public PatternData(GraphicDataRGB graphicData)
    : this(graphicData.color, graphicData.colorTwo, graphicData.colorThree, graphicData.pattern, graphicData.displacement, graphicData.tiles)
  {
  }

  public PatternData(
    Color color,
    Color colorTwo,
    Color colorThree,
    PatternDef patternDef,
    Vector2 displacement,
    float tiles)
  {
    this.color = color;
    this.colorTwo = colorTwo;
    this.colorThree = colorThree;
    this.patternDef = patternDef;
    this.displacement = displacement;
    this.tiles = tiles;
  }

  public void Copy(PatternData reference)
  {
    this.color = reference.color;
    this.colorTwo = reference.colorTwo;
    this.colorThree = reference.colorThree;
    this.patternDef = reference.patternDef;
    this.displacement = reference.displacement;
    this.tiles = reference.tiles;
  }

  public static implicit operator GraphicDataRGB(PatternData patternData)
  {
    GraphicDataRGB graphicDataRgb = new GraphicDataRGB();
    graphicDataRgb.color = patternData.color;
    graphicDataRgb.colorTwo = patternData.colorTwo;
    graphicDataRgb.colorThree = patternData.colorThree;
    graphicDataRgb.tiles = patternData.tiles;
    graphicDataRgb.displacement = patternData.displacement;
    graphicDataRgb.pattern = patternData.patternDef;
    return graphicDataRgb;
  }

  public static implicit operator PatternData(GraphicDataRGB graphicDataRGB)
  {
    return new PatternData()
    {
      color = graphicDataRGB.color,
      colorTwo = graphicDataRGB.colorTwo,
      colorThree = graphicDataRGB.colorThree,
      tiles = graphicDataRGB.tiles,
      displacement = graphicDataRGB.displacement,
      patternDef = graphicDataRGB.pattern
    };
  }

  public virtual void ExposeDataPostDefDatabase()
  {
    if (GenText.NullOrEmpty(this.patternId) || this.patternDef != null)
      return;
    this.patternDef = DefDatabase<PatternDef>.GetNamed(this.patternId, true);
    if (this.patternDef != null)
      return;
    this.patternDef = PatternDefOf.Default;
  }

  public override string ToString()
  {
    return $"Pattern Data: Color={this.color} ColorTwo={this.colorTwo} ColorThree={this.colorThree} Pattern={this.patternDef} Displacement={this.displacement} Tiles={this.tiles}";
  }

  public void ExposeData()
  {
    if (Scribe.mode == 1)
      this.patternId = this.patternDef?.defName ?? "Default";
    Scribe_Values.Look<float>(ref this.tiles, "tiles", 1f, false);
    Scribe_Values.Look<Vector2>(ref this.displacement, "displacement", Vector2.zero, false);
    Scribe_Values.Look<Color>(ref this.color, "color", Color.white, false);
    Scribe_Values.Look<Color>(ref this.colorTwo, "colorTwo", Color.white, false);
    Scribe_Values.Look<Color>(ref this.colorThree, "colorThree", Color.white, false);
    Scribe_Values.Look<string>(ref this.patternId, "patternId", (string) null, false);
    if (Scribe.mode != 4)
      return;
    this.patternDef = !GenText.NullOrEmpty(this.patternId) ? DefDatabase<PatternDef>.GetNamedSilentFail(this.patternId) : PatternDefOf.Default;
  }
}
