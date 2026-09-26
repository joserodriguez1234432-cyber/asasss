// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompProperties_BuildableUpgrades
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class CompProperties_BuildableUpgrades : CompProperties
{
  public List<Upgrade> upgrades;
  public bool syncWithPowerCondition;

  public CompProperties_BuildableUpgrades() => this.compClass = typeof (CompBuildableUpgrades);
}
