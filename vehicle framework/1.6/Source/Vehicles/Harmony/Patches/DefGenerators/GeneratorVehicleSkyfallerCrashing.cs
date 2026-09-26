// Decompiled with JetBrains decompiler
// Type: Vehicles.GeneratorVehicleSkyfallerCrashing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

internal class GeneratorVehicleSkyfallerCrashing : IVehicleDefGenerator<ThingDef>
{
  bool IVehicleDefGenerator<ThingDef>.TryGenerateImpliedDef(
    VehicleDef vehicleDef,
    out ThingDef skyfallerCrashingImpliedDef,
    bool hotReload)
  {
    skyfallerCrashingImpliedDef = (ThingDef) null;
    CompProperties_VehicleLauncher compProperties = vehicleDef.GetCompProperties<CompProperties_VehicleLauncher>();
    if (compProperties == null || compProperties.skyfallerCrashing != null)
      return false;
    string str = ((Def) vehicleDef).defName + "Crashing";
    skyfallerCrashingImpliedDef = !hotReload ? new ThingDef() : DefDatabase<ThingDef>.GetNamed(str, false) ?? new ThingDef();
    ((Def) skyfallerCrashingImpliedDef).defName = str;
    ((Def) skyfallerCrashingImpliedDef).modContentPack = ((Def) vehicleDef).modContentPack;
    ((Def) skyfallerCrashingImpliedDef).label = ((Def) vehicleDef).defName + "Crashing";
    skyfallerCrashingImpliedDef.thingClass = typeof (VehicleSkyfaller_Crashing);
    skyfallerCrashingImpliedDef.category = (ThingCategory) 10;
    skyfallerCrashingImpliedDef.useHitPoints = false;
    skyfallerCrashingImpliedDef.drawOffscreen = true;
    skyfallerCrashingImpliedDef.tickerType = (TickerType) 1;
    ((BuildableDef) skyfallerCrashingImpliedDef).altitudeLayer = (AltitudeLayer) 30;
    skyfallerCrashingImpliedDef.drawerType = (DrawerType) 1;
    ThingDef thingDef = skyfallerCrashingImpliedDef;
    SkyfallerProperties skyfallerProperties = new SkyfallerProperties();
    skyfallerProperties.shadow = "Things/Skyfaller/SkyfallerShadowDropPod";
    IntVec2 size = ((BuildableDef) vehicleDef).Size;
    skyfallerProperties.shadowSize = ((IntVec2) ref size).ToVector2();
    skyfallerProperties.movementType = (SkyfallerMovementType) 1;
    skyfallerProperties.explosionRadius = (float) Mathf.Max(((BuildableDef) vehicleDef).Size.x, ((BuildableDef) vehicleDef).Size.z) * 1.5f;
    skyfallerProperties.explosionDamage = DamageDefOf.Bomb;
    skyfallerProperties.rotateGraphicTowardsDirection = vehicleDef.rotatable;
    skyfallerProperties.speed = 2f;
    skyfallerProperties.ticksToImpactRange = new IntRange(300, 350);
    thingDef.skyfaller = skyfallerProperties;
    compProperties.skyfallerCrashing = skyfallerCrashingImpliedDef;
    return true;
  }
}
