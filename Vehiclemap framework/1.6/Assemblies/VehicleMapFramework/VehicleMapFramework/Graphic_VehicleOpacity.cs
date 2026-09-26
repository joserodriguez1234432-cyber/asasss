// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Graphic_VehicleOpacity
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Graphic_VehicleOpacity : Graphic_Vehicle
{
  public static readonly int OpacityID = Shader.PropertyToID("_Opacity");
  private float opacityInt = 1f;

  public float Opacity
  {
    get => this.opacityInt;
    set
    {
      this.opacityInt = value;
      this.Notify_OpacityChanged();
    }
  }

  public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
  {
    Log.Warning($"Retrieving {((object) this).GetType()} Colored Graphic from vanilla GraphicDatabase which will result in redundant graphic creation.");
    return GraphicDatabase.Get<Graphic_VehicleOpacity>(((Graphic) this).path, newShader, ((Graphic) this).drawSize, newColor, newColorTwo, (GraphicData) ((Graphic_Rgb) this).DataRgb, (string) null);
  }

  private void Notify_OpacityChanged()
  {
    if (GenList.NullOrEmpty<Material>((IList<Material>) ((Graphic_Rgb) this).materials))
      return;
    foreach (Material material in ((Graphic_Rgb) this).materials)
      material?.SetFloat(Graphic_VehicleOpacity.OpacityID, this.opacityInt);
  }
}
