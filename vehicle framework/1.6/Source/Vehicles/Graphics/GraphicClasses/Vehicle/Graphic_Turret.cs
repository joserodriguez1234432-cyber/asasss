// Decompiled with JetBrains decompiler
// Type: Vehicles.Graphic_Turret
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Graphic_Turret : Graphic_Rgb
{
  public static string TurretMaskSuffix = "_m";
  protected int matCount;
  protected string graphicPath;
  protected Material material;

  public override Material MatSingle => this.materials[0];

  public override Material MatNorth => base.MatSingle;

  public override Material MatEast => base.MatSingle;

  public override Material MatSouth => base.MatSingle;

  public override Material MatWest => base.MatSingle;

  public override int MatCount => 1;

  public override void Init(GraphicRequestRGB req)
  {
    base.Init(req);
    if (GenText.NullOrEmpty(req.path))
      throw new ArgumentNullException("path");
    this.materials = !Object.op_Equality((Object) req.shader, (Object) null) ? RGBMaterialPool.GetAll(req.target) : throw new ArgumentNullException("shader");
  }

  protected override void GetMasks(string path, Shader shader)
  {
    this.patternPointers = new int[8]
    {
      0,
      1,
      2,
      3,
      4,
      5,
      6,
      7
    };
    this.masks = Enumerable.Repeat<Texture2D>(ContentFinder<Texture2D>.Get(path + Graphic_Turret.TurretMaskSuffix, false), this.MatCount).ToArray<Texture2D>();
  }

  public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
  {
    return GraphicDatabase.Get<Graphic_Turret>(this.path, newShader, this.drawSize, newColor, newColorTwo, (GraphicData) this.DataRgb, (string) null);
  }
}
