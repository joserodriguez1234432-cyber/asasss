// Decompiled with JetBrains decompiler
// Type: Vehicles.Area_RoadAvoidal
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Area_RoadAvoidal : Area
{
  public Area_RoadAvoidal()
  {
  }

  public Area_RoadAvoidal(AreaManager areaManager)
    : base(areaManager)
  {
  }

  public virtual string Label => TaggedString.op_Implicit(Translator.Translate("VF_RoadZone"));

  public virtual int ListPriority => 9000;

  public virtual Color Color
  {
    get
    {
      ColorInt colorInt = new ColorInt(125, 25, 25);
      return ((ColorInt) ref colorInt).ToColor;
    }
  }

  public virtual string GetUniqueLoadID() => $"Area_{this.ID}_VehicleRoadAvoidal";
}
