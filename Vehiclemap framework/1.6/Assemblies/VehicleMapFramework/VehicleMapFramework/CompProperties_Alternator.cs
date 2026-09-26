// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompProperties_Alternator
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompProperties_Alternator : CompProperties_Power
{
  public List<CompProperties_Alternator.FuelProperties> fuelConsumptionRates;

  public CompProperties_Alternator() => ((CompProperties) this).compClass = typeof (CompAlternator);

  public class FuelProperties
  {
    public float fuelConsumptionRate = 1f;
    public ThingDef fuelDef;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
      DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef((object) this, "fuelDef", xmlRoot.Name, (string) null, (string) null, (Type) null);
      this.fuelConsumptionRate = ParseHelper.FromString<float>(xmlRoot.InnerText);
    }
  }
}
