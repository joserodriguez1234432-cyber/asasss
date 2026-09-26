// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompProperties_EngineLightOverlay
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

#nullable disable
namespace VehicleMapFramework;

public class CompProperties_EngineLightOverlay : CompProperties_OpacityOverlay
{
  public float engineOffOpacity;
  public float engineOnOpacity;
  public float ignitionDuration;
  public float inFlightOpacity;

  public CompProperties_EngineLightOverlay() => this.compClass = typeof (CompEngineLightOverlay);
}
