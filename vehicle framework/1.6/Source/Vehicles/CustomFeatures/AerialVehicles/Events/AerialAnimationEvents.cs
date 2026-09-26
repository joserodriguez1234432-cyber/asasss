// Decompiled with JetBrains decompiler
// Type: Vehicles.AerialAnimationEvents
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class AerialAnimationEvents
{
  public static void ShakeCamera(LaunchProtocol launchProtocol, float magnitude)
  {
    Find.CameraDriver.shaker.DoShake(magnitude);
  }

  public static void Explode(LaunchProtocol launchProtocol, float radius, bool ignoreVehicle = true)
  {
    IntVec3 position1 = launchProtocol.Position;
    if (!((IntVec3) ref position1).IsValid)
      return;
    List<Thing> thingList1;
    if (!ignoreVehicle)
    {
      thingList1 = (List<Thing>) null;
    }
    else
    {
      thingList1 = new List<Thing>();
      thingList1.Add((Thing) launchProtocol.Vehicle);
    }
    List<Thing> thingList2 = thingList1;
    IntVec3 position2 = launchProtocol.Position;
    Map map = launchProtocol.Map;
    double num = (double) radius;
    DamageDef bomb = DamageDefOf.Bomb;
    VehiclePawn vehicle = launchProtocol.Vehicle;
    List<Thing> thingList3 = thingList2;
    GasType? nullable1 = new GasType?();
    float? nullable2 = new float?();
    float? nullable3 = new float?();
    List<Thing> thingList4 = thingList3;
    FloatRange? nullable4 = new FloatRange?();
    GenExplosion.DoExplosion(position2, map, (float) num, bomb, (Thing) vehicle, -1, -1f, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Thing) null, (ThingDef) null, 0.0f, 1, nullable1, nullable2, (int) byte.MaxValue, false, (ThingDef) null, 0.0f, 1, 0.0f, false, nullable3, thingList4, nullable4, true, 1f, 0.0f, true, (ThingDef) null, 1f, (SimpleCurve) null, (List<IntVec3>) null, (ThingDef) null, (ThingDef) null);
  }
}
