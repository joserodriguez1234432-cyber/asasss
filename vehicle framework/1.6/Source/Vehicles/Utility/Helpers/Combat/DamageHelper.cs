// Decompiled with JetBrains decompiler
// Type: Vehicles.DamageHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class DamageHelper
{
  public static void Explode(Projectile proj)
  {
    Map map1 = ((Thing) proj).Map;
    ((Thing) proj).Destroy((DestroyMode) 0);
    if (((Thing) proj).def.projectile.explosionEffect != null)
    {
      Effecter effecter = ((Thing) proj).def.projectile.explosionEffect.Spawn();
      effecter.Trigger(new TargetInfo(((Thing) proj).Position, map1, false), new TargetInfo(((Thing) proj).Position, map1, false), -1);
      effecter.Cleanup();
    }
    IntVec3 position = ((Thing) proj).Position;
    Map map2 = map1;
    float num1 = map1.terrainGrid.TerrainAt(((Thing) proj).Position) == TerrainDefOf.WaterDeep || map1.terrainGrid.TerrainAt(((Thing) proj).Position) == TerrainDefOf.WaterMovingChestDeep || map1.terrainGrid.TerrainAt(((Thing) proj).Position) == TerrainDefOf.WaterOceanDeep ? 2.5f : 1.5f;
    float num2 = ((Thing) proj).def.projectile.explosionRadius / num1;
    if ((double) num2 < 1.0)
      num2 = 1f;
    DamageDef damageDef1 = ((Thing) proj).def.projectile.damageDef;
    Thing thing1 = (Thing) null;
    int damageAmount = proj.DamageAmount;
    float armorPenetration = proj.ArmorPenetration;
    SoundDef explodeBombWater = SoundDefOf_Vehicles.Explode_BombWater;
    ThingDef thingDef1 = (ThingDef) null;
    ThingDef def = ((Thing) proj).def;
    Thing thing2 = (Thing) null;
    ThingDef explosionSpawnThingDef1 = ((Thing) proj).def.projectile.postExplosionSpawnThingDef;
    float num3 = 0.0f;
    float num4 = ((Thing) proj).def.projectile.explosionChanceToStartFire * 0.0f;
    int explosionSpawnThingCount1 = ((Thing) proj).def.projectile.postExplosionSpawnThingCount;
    ThingDef explosionSpawnThingDef2 = ((Thing) proj).def.projectile.preExplosionSpawnThingDef;
    IntVec3 intVec3 = position;
    Map map3 = map2;
    double num5 = (double) num2;
    DamageDef damageDef2 = damageDef1;
    Thing thing3 = thing1;
    int num6 = damageAmount;
    double num7 = (double) armorPenetration;
    SoundDef soundDef = explodeBombWater;
    ThingDef thingDef2 = thingDef1;
    ThingDef thingDef3 = def;
    Thing thing4 = thing2;
    ThingDef thingDef4 = explosionSpawnThingDef1;
    double num8 = (double) num3;
    int num9 = explosionSpawnThingCount1;
    bool explosionCellsNeighbors = ((Thing) proj).def.projectile.applyDamageToExplosionCellsNeighbors;
    ThingDef thingDef5 = explosionSpawnThingDef2;
    float explosionSpawnChance = ((Thing) proj).def.projectile.preExplosionSpawnChance;
    int explosionSpawnThingCount2 = ((Thing) proj).def.projectile.preExplosionSpawnThingCount;
    float num10 = num4;
    bool explosionDamageFalloff = ((Thing) proj).def.projectile.explosionDamageFalloff;
    GasType? nullable1 = new GasType?();
    float? nullable2 = new float?();
    int num11 = explosionCellsNeighbors ? 1 : 0;
    ThingDef thingDef6 = thingDef5;
    double num12 = (double) explosionSpawnChance;
    int num13 = explosionSpawnThingCount2;
    double num14 = (double) num10;
    int num15 = explosionDamageFalloff ? 1 : 0;
    float? nullable3 = new float?();
    FloatRange? nullable4 = new FloatRange?();
    GenExplosion.DoExplosion(intVec3, map3, (float) num5, damageDef2, thing3, num6, (float) num7, soundDef, thingDef2, thingDef3, thing4, thingDef4, (float) num8, num9, nullable1, nullable2, (int) byte.MaxValue, num11 != 0, thingDef6, (float) num12, num13, (float) num14, num15 != 0, nullable3, (List<Thing>) null, nullable4, true, 1f, 0.0f, true, (ThingDef) null, 1f, (SimpleCurve) null, (List<IntVec3>) null, (ThingDef) null, (ThingDef) null);
  }

  public static float EMPChanceToStun(VehicleEMPSeverity severity)
  {
    float stun;
    switch (severity)
    {
      case VehicleEMPSeverity.Tiny:
        stun = 0.075f;
        break;
      case VehicleEMPSeverity.Minor:
        stun = 0.125f;
        break;
      case VehicleEMPSeverity.Moderate:
        stun = 0.15f;
        break;
      case VehicleEMPSeverity.Severe:
        stun = 0.25f;
        break;
      default:
        stun = 0.0f;
        break;
    }
    return stun;
  }

  public static int EMPStunLength(VehicleEMPSeverity severity, float damage)
  {
    int num;
    switch (severity)
    {
      case VehicleEMPSeverity.Tiny:
        num = Mathf.RoundToInt(damage * Rand.Range(0.5f, 1.5f));
        break;
      case VehicleEMPSeverity.Minor:
        num = Mathf.RoundToInt(damage * Rand.Range(2f, 3f));
        break;
      case VehicleEMPSeverity.Moderate:
        num = Mathf.RoundToInt(damage * Rand.Range(2f, 3f));
        break;
      case VehicleEMPSeverity.Severe:
        num = Mathf.RoundToInt(damage * Rand.Range(2.5f, 4f));
        break;
      default:
        num = 0;
        break;
    }
    return num;
  }

  public static float EMPStunDamage(VehicleEMPSeverity severity)
  {
    float num;
    switch (severity)
    {
      case VehicleEMPSeverity.Tiny:
        num = 0.0f;
        break;
      case VehicleEMPSeverity.Minor:
        num = 0.01f;
        break;
      case VehicleEMPSeverity.Moderate:
        num = 0.025f;
        break;
      case VehicleEMPSeverity.Severe:
        num = 0.075f;
        break;
      default:
        num = 0.0f;
        break;
    }
    return num;
  }
}
