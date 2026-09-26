// Decompiled with JetBrains decompiler
// Type: Vehicles.GeneratorVehicleSkyfallerIncoming
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

internal class GeneratorVehicleSkyfallerIncoming : IVehicleDefGenerator<ThingDef>
{
  bool IVehicleDefGenerator<ThingDef>.TryGenerateImpliedDef(
    VehicleDef vehicleDef,
    out ThingDef skyfallerIncomingImpliedDef,
    bool hotReload)
  {
    skyfallerIncomingImpliedDef = (ThingDef) null;
    CompProperties_VehicleLauncher compProperties = vehicleDef.GetCompProperties<CompProperties_VehicleLauncher>();
    if (compProperties == null || compProperties.skyfallerIncoming != null)
      return false;
    string str = ((Def) vehicleDef).defName + "Incoming";
    skyfallerIncomingImpliedDef = !hotReload ? new ThingDef() : DefDatabase<ThingDef>.GetNamed(str, false) ?? new ThingDef();
    ((Def) skyfallerIncomingImpliedDef).defName = str;
    ((Def) skyfallerIncomingImpliedDef).modContentPack = ((Def) vehicleDef).modContentPack;
    ((Def) skyfallerIncomingImpliedDef).label = ((Def) vehicleDef).defName + "Incoming";
    skyfallerIncomingImpliedDef.thingClass = typeof (VehicleSkyfaller_Arriving);
    skyfallerIncomingImpliedDef.category = (ThingCategory) 10;
    skyfallerIncomingImpliedDef.useHitPoints = false;
    skyfallerIncomingImpliedDef.drawOffscreen = true;
    skyfallerIncomingImpliedDef.tickerType = (TickerType) 1;
    ((BuildableDef) skyfallerIncomingImpliedDef).altitudeLayer = (AltitudeLayer) 30;
    skyfallerIncomingImpliedDef.drawerType = (DrawerType) 1;
    ThingDef thingDef = skyfallerIncomingImpliedDef;
    SkyfallerProperties skyfallerProperties = new SkyfallerProperties();
    skyfallerProperties.shadow = "Things/Skyfaller/SkyfallerShadowDropPod";
    IntVec2 size = ((BuildableDef) vehicleDef).Size;
    skyfallerProperties.shadowSize = ((IntVec2) ref size).ToVector2();
    thingDef.skyfaller = skyfallerProperties;
    compProperties.skyfallerIncoming = skyfallerIncomingImpliedDef;
    return true;
  }
}
