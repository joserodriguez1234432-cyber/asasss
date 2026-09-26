// Decompiled with JetBrains decompiler
// Type: Vehicles.ProjectileSkyfallerMaker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class ProjectileSkyfallerMaker
{
  public static ProjectileSkyfaller WrapProjectile(
    ThingDef skyfallerDef,
    ThingDef projectileDef,
    Thing caster,
    Vector3 origin,
    Vector3 destination,
    float speedTilesPerTick,
    bool reverseDraw = false)
  {
    try
    {
      ProjectileSkyfaller projectileSkyfaller = (ProjectileSkyfaller) ThingMaker.MakeThing(skyfallerDef, (ThingDef) null);
      projectileSkyfaller.caster = caster;
      projectileSkyfaller.origin = origin;
      projectileSkyfaller.destination = destination;
      projectileSkyfaller.projectileDef = projectileDef;
      projectileSkyfaller.speedTilesPerTick = speedTilesPerTick;
      projectileSkyfaller.reverseDraw = reverseDraw;
      return projectileSkyfaller;
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to generate StrafeProjectile with projectile <type>{projectileDef.thingClass}</type>. Exception=\"{ex}\"");
    }
    return (ProjectileSkyfaller) null;
  }
}
