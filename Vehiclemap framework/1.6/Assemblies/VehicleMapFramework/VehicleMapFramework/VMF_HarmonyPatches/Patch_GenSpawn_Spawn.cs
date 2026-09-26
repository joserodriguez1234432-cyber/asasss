// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenSpawn_Spawn
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenSpawn), "Spawn", new Type[] {typeof (Thing), typeof (IntVec3), typeof (Map), typeof (Rot4), typeof (WipeMode), typeof (bool), typeof (bool)})]
[PatchLevel(Level.Safe)]
public static class Patch_GenSpawn_Spawn
{
  public static void Prefix(Thing newThing, ref Map map, IntVec3 loc)
  {
    if (map == null)
      return;
    switch (newThing)
    {
      case Projectile projectile:
        if (projectile is Spark)
          return;
        break;
      case Mote _:
        if (GenGrid.InBounds(loc, map))
          return;
        break;
      case PawnFlyer pawnFlyer:
        Map map1;
        if (!((Thing) pawnFlyer.FlyingPawn).TryGetTargetMap(out map1))
          return;
        map = map1;
        ((Thing) pawnFlyer.FlyingPawn).RemoveTargetInfo();
        return;
      default:
        return;
    }
    map = map.BaseMap();
  }
}
