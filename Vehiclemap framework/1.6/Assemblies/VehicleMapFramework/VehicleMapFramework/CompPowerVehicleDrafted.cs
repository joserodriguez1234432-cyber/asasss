// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompPowerVehicleDrafted
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompPowerVehicleDrafted : ThingComp
{
  protected CompPowerTrader PowerTrader
  {
    get
    {
      return this.\u003CPowerTrader\u003Ek__BackingField ?? (this.\u003CPowerTrader\u003Ek__BackingField = this.parent.GetComp<CompPowerTrader>());
    }
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
        return;
      CompPowerTrader powerTrader = this.PowerTrader;
      if (powerTrader != null)
        powerTrader.PowerOutput = vehicle.Drafted ? -((CompPower) this.PowerTrader).Props.PowerConsumption : 0.0f;
      Ext_EventManager.AddEvent<VehicleEventDef>((IEventManager<VehicleEventDef>) vehicle, VehicleEventDefOf.IgnitionOn, new Action(this.Activate), Array.Empty<Action>());
      Ext_EventManager.AddEvent<VehicleEventDef>((IEventManager<VehicleEventDef>) vehicle, VehicleEventDefOf.IgnitionOff, new Action(this.Inactivate), Array.Empty<Action>());
    }));
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return;
    Ext_EventManager.RemoveEvent<VehicleEventDef>((IEventManager<VehicleEventDef>) vehicle, VehicleEventDefOf.IgnitionOn, new Action(this.Activate));
    Ext_EventManager.RemoveEvent<VehicleEventDef>((IEventManager<VehicleEventDef>) vehicle, VehicleEventDefOf.IgnitionOff, new Action(this.Inactivate));
  }

  private void Activate()
  {
    CompPowerTrader powerTrader = this.PowerTrader;
    if (powerTrader == null)
      return;
    powerTrader.PowerOutput = -((CompPower) this.PowerTrader).Props.PowerConsumption;
  }

  private void Inactivate()
  {
    CompPowerTrader powerTrader = this.PowerTrader;
    if (powerTrader == null)
      return;
    powerTrader.PowerOutput = 0.0f;
  }
}
