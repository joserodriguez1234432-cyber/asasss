// Decompiled with JetBrains decompiler
// Type: Vehicles.GeneratorVehicleSkyfallerLeaving
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

internal class GeneratorVehicleSkyfallerLeaving : IVehicleDefGenerator<ThingDef>
{
  bool IVehicleDefGenerator<ThingDef>.TryGenerateImpliedDef(
    VehicleDef vehicleDef,
    out ThingDef skyfallerLeavingImpliedDef,
    bool hotReload)
  {
    skyfallerLeavingImpliedDef = (ThingDef) null;
    CompProperties_VehicleLauncher compProperties = vehicleDef.GetCompProperties<CompProperties_VehicleLauncher>();
    if (compProperties == null || compProperties.skyfallerLeaving != null)
      return false;
    string str = ((Def) vehicleDef).defName + "Leaving";
    skyfallerLeavingImpliedDef = !hotReload ? new ThingDef() : DefDatabase<ThingDef>.GetNamed(str, false) ?? new ThingDef();
    ((Def) skyfallerLeavingImpliedDef).defName = str;
    ((Def) skyfallerLeavingImpliedDef).modContentPack = ((Def) vehicleDef).modContentPack;
    ((Def) skyfallerLeavingImpliedDef).label = ((Def) vehicleDef).defName + "Leaving";
    skyfallerLeavingImpliedDef.thingClass = typeof (VehicleSkyfaller_Leaving);
    skyfallerLeavingImpliedDef.category = (ThingCategory) 10;
    skyfallerLeavingImpliedDef.useHitPoints = false;
    skyfallerLeavingImpliedDef.drawOffscreen = true;
    skyfallerLeavingImpliedDef.tickerType = (TickerType) 1;
    ((BuildableDef) skyfallerLeavingImpliedDef).altitudeLayer = (AltitudeLayer) 30;
    skyfallerLeavingImpliedDef.drawerType = (DrawerType) 1;
    ThingDef thingDef = skyfallerLeavingImpliedDef;
    SkyfallerProperties skyfallerProperties = new SkyfallerProperties();
    skyfallerProperties.shadow = "Things/Skyfaller/SkyfallerShadowDropPod";
    IntVec2 size = ((BuildableDef) vehicleDef).Size;
    skyfallerProperties.shadowSize = ((IntVec2) ref size).ToVector2();
    thingDef.skyfaller = skyfallerProperties;
    compProperties.skyfallerLeaving = skyfallerLeavingImpliedDef;
    return true;
  }
}
