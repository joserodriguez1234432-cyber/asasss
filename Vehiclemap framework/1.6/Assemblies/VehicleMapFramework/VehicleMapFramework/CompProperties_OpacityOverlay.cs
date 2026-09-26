// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompProperties_OpacityOverlay
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class CompProperties_OpacityOverlay : CompProperties
{
  public string identifier = "";
  public string label;

  public CompProperties_OpacityOverlay() => this.compClass = typeof (CompOpacityOverlay);
}
