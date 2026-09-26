// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ReservationUtility_CanReserveSittableOrSpot
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ReservationUtility), "CanReserveSittableOrSpot", new Type[] {typeof (Pawn), typeof (IntVec3), typeof (Thing), typeof (bool)})]
[PatchLevel(Level.Safe)]
public static class Patch_ReservationUtility_CanReserveSittableOrSpot
{
  public static bool Prefix(
    Pawn pawn,
    IntVec3 exactSittingPos,
    Thing ignoreThing,
    ref Map __state)
  {
    if (((Thing) pawn)?.Map == null)
      return false;
    Map map = ignoreThing?.Map ?? TargetMapUtility.get_TargetMapOrThingMap((Thing) pawn);
    if (map == null)
      return true;
    if (((Thing) pawn).Map != map && VehicleMapUtility.get_GroundMap((Thing) pawn) == VehicleMapUtility.get_GroundMap(map))
    {
      __state = ((Thing) pawn).Map;
      ((Thing) pawn).VirtualMapTransfer(map);
    }
    return GenGrid.InBounds(exactSittingPos, map);
  }

  public static void Finalizer(Pawn pawn, Map __state)
  {
    if (__state == null)
      return;
    ((Thing) pawn).VirtualMapTransfer(__state);
  }
}
