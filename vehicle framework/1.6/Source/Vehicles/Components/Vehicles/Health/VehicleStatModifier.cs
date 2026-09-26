// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatModifier
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Xml;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleStatModifier : ICustomSettingsDrawer
{
  public VehicleStatDef statDef;
  public float value;
  public List<VehicleStatPart> parts;

  public void LoadDataFromXmlCustom(XmlNode xmlNode)
  {
    string name = xmlNode.Name;
    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef((object) this, "statDef", name, (string) null, (string) null, (System.Type) null);
    if (xmlNode.FirstChild is XmlText)
    {
      this.value = ParseHelper.FromString<float>(xmlNode.InnerText);
    }
    else
    {
      ClassLoader.Distribute<VehicleStatModifier>(xmlNode, this);
      if (GenList.NullOrEmpty<VehicleStatPart>((IList<VehicleStatPart>) this.parts))
        return;
      foreach (object part in this.parts)
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(part, "statDef", name, (string) null, (string) null, (System.Type) null);
    }
  }

  public void DrawSetting(
    Listing_Settings lister,
    VehicleDef vehicleDef,
    FieldInfo field,
    string label,
    string tooltip,
    string disabledTooltip,
    bool locked,
    bool translate)
  {
    if (!this.statDef.modSettingsInfo.IsValid)
      return;
    PostToSettingsAttribute.DrawSetting(lister, vehicleDef, field, this.statDef.modSettingsInfo, TaggedString.op_Implicit(this.statDef.LabelCap), this.statDef.description, disabledTooltip, locked, false);
  }

  public override string ToString() => $"{this.statDef?.defName ?? "[Null Stat]"} - {this.value}";
}
