// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompExtraPrint
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompExtraPrint : ThingComp
{
  public CompProperties_ExtraPrint Props => (CompProperties_ExtraPrint) this.props;

  public virtual void PostPrintOnto(SectionLayer layer)
  {
    if (this.Props.graphics == null)
      return;
    foreach (GraphicData graphic in this.Props.graphics)
      graphic.Graphic.Print(layer, (Thing) this.parent, VehicleMapUtility.PrintExtraRotation((Thing) this.parent));
  }
}
