// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TargetingParametersForSpecificThingDef
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class TargetingParametersForSpecificThingDef : TargetingParameters
{
  public ThingDef thingDef;

  public void PostLoad()
  {
    this.validator = (Predicate<TargetInfo>) (targetInfo => ((TargetInfo) ref targetInfo).Thing?.def == this.thingDef);
  }
}
