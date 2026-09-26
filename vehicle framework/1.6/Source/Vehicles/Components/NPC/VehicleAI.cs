// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleAI
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class VehicleAI : IExposable
{
  private VehiclePawn vehicle;

  public VehicleAI(VehiclePawn vehicle) => this.vehicle = vehicle;

  public void AITick()
  {
    foreach (ThingComp allComp in ((ThingWithComps) this.vehicle).AllComps)
    {
      if (allComp is VehicleAIComp vehicleAiComp)
        vehicleAiComp.AITick();
    }
  }

  public void ExposeData()
  {
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", true);
  }
}
