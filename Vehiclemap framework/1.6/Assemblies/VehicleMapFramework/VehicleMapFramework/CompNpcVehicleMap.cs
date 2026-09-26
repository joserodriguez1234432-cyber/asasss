// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompNpcVehicleMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompNpcVehicleMap : VehicleComp
{
  public CompProperties_NpcVehicleMap Props
  {
    get => (CompProperties_NpcVehicleMap) ((ThingComp) this).props;
  }

  public CompProperties_NpcVehicleMap.VehicleMapParams Params { get; private set; }

  public void SetParams(int pawnCount)
  {
    if (this.Params != null)
    {
      VMF_Log.Warning("CompNpcVehicleMap: Params already set.");
    }
    else
    {
      CompProperties_NpcVehicleMap.VehicleMapParams vehicleMapParams;
      if (GenCollection.TryRandomElement<CompProperties_NpcVehicleMap.VehicleMapParams>(this.Props.mapParams.Where<CompProperties_NpcVehicleMap.VehicleMapParams>((Func<CompProperties_NpcVehicleMap.VehicleMapParams, bool>) (mapParams => Ext_Numeric.InRange(mapParams.pawnCountRange, pawnCount))), ref vehicleMapParams))
      {
        this.Params = vehicleMapParams;
      }
      else
      {
        VMF_Log.Warning($"CompNpcVehicleMap: No mapParams found for pawnCount {pawnCount}. Using first mapParams.");
        this.Params = this.Props.mapParams.FirstOrDefault<CompProperties_NpcVehicleMap.VehicleMapParams>();
      }
    }
  }

  public virtual void PostExposeData()
  {
    ((ThingComp) this).PostExposeData();
    CompProperties_NpcVehicleMap.VehicleMapParams vehicleMapParams = this.Params;
    Scribe_Deep.Look<CompProperties_NpcVehicleMap.VehicleMapParams>(ref vehicleMapParams, "params", Array.Empty<object>());
    this.Params = vehicleMapParams;
  }
}
