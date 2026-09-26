// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TraverseSpotsSaveLoader
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class TraverseSpotsSaveLoader(TraverseSpots spots) : IExposable
{
  public TraverseSpots spots = spots;

  [UsedImplicitly]
  public TraverseSpotsSaveLoader()
    : this(new TraverseSpots(TargetInfo.Invalid, TargetInfo.Invalid))
  {
  }

  public void ExposeData()
  {
    Scribe_TargetInfo.Look(ref this.spots.exitSpot, "exitSpot");
    Scribe_TargetInfo.Look(ref this.spots.enterSpot, "enterSpot");
  }
}
