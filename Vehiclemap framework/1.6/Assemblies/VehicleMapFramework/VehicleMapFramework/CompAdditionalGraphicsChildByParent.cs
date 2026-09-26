// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompAdditionalGraphicsChildByParent
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompAdditionalGraphicsChildByParent : CompAdditionalGraphicsChild
{
  public CompProperties_AdditionalGraphicsChildByParent Props
  {
    get => (CompProperties_AdditionalGraphicsChildByParent) this.props;
  }

  public override List<GraphicData> Graphics
  {
    get
    {
      List<GraphicData> graphicDataList;
      return !this.Props.graphicsByParent.TryGetValue(((Thing) this.parentThing).def, out graphicDataList) ? new List<GraphicData>() : graphicDataList;
    }
  }
}
