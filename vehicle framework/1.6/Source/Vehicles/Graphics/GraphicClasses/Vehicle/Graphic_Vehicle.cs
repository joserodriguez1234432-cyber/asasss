// Decompiled with JetBrains decompiler
// Type: Vehicles.Graphic_Vehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Graphic_Vehicle : Graphic_Rgb
{
  public override int MatCount => 8;

  public IEnumerable<Rot8> RotationsRenderableByUI
  {
    get
    {
      Graphic_Vehicle graphicVehicle = this;
      yield return Rot8.North;
      yield return Rot8.South;
      if (!graphicVehicle.eastFlipped)
        yield return Rot8.East;
      if (!graphicVehicle.westFlipped)
        yield return Rot8.West;
    }
  }

  public override void Init(GraphicRequestRGB req)
  {
    base.Init(req);
    this.materials = RGBMaterialPool.GetAll(req.target);
  }

  public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
  {
    Log.Warning($"Retrieving {((object) this).GetType()} Colored Graphic from vanilla GraphicDatabase which will result in redundant graphic creation.");
    return GraphicDatabase.Get<Graphic_Vehicle>(this.path, newShader, this.drawSize, newColor, newColorTwo, (GraphicData) this.DataRgb, (string) null);
  }
}
