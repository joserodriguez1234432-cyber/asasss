// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleNPCProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleNPCProperties
{
  public bool runDownTargets;
  public bool reverseWhileFleeing;
  public float targetPositionRadiusPercent = 0.8f;
  public float targetAcquireRadius = 65f;
  public float targetKeepRadius = 65f;
  public bool stopToShoot = true;
  public float distanceWeight = 2.5f;
  public VehicleRaidParamsDef raidParams;
  public VehicleStrategyDef strategy;
  public SimpleDictionary<TargetCategory, int> targets;
}
