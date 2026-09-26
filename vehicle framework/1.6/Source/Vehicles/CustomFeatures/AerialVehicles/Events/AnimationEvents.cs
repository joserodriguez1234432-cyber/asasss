// Decompiled with JetBrains decompiler
// Type: Vehicles.AnimationEvents
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Animations;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class AnimationEvents
{
  [AnimationEvent]
  private static void ShakeCamera(float magnitude) => Find.CameraDriver.shaker.DoShake(magnitude);

  [AnimationEvent]
  private static void Explode(
    IAnimator __animator,
    DamageDef damageDef,
    float radius,
    bool damageThing = false)
  {
    if (!(__animator is Thing thing1) || !thing1.Spawned)
      return;
    List<Thing> thingList1;
    if (!damageThing)
      thingList1 = new List<Thing>() { thing1 };
    else
      thingList1 = (List<Thing>) null;
    List<Thing> thingList2 = thingList1;
    IntVec3 position = thing1.Position;
    Map map = thing1.Map;
    double num = (double) radius;
    DamageDef damageDef1 = damageDef;
    Thing thing2 = thing1;
    List<Thing> thingList3 = thingList2;
    GasType? nullable1 = new GasType?();
    float? nullable2 = new float?();
    float? nullable3 = new float?();
    List<Thing> thingList4 = thingList3;
    FloatRange? nullable4 = new FloatRange?();
    GenExplosion.DoExplosion(position, map, (float) num, damageDef1, thing2, -1, -1f, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Thing) null, (ThingDef) null, 0.0f, 1, nullable1, nullable2, (int) byte.MaxValue, false, (ThingDef) null, 0.0f, 1, 0.0f, false, nullable3, thingList4, nullable4, true, 1f, 0.0f, true, (ThingDef) null, 1f, (SimpleCurve) null, (List<IntVec3>) null, (ThingDef) null, (ThingDef) null);
  }
}
