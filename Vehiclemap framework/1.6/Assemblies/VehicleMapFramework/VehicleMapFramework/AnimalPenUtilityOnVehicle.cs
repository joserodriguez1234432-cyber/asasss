// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.AnimalPenUtilityOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class AnimalPenUtilityOnVehicle
{
  public static bool CanUseAndReach(
    Pawn animal,
    CompAnimalPenMarker penMarker,
    bool allowUnenclosedPens,
    Pawn roper = null)
  {
    bool flag = false;
    return AnimalPenUtilityOnVehicle.CheckUseAndReach(animal, penMarker, allowUnenclosedPens, roper, ref flag, ref flag, ref flag);
  }

  public static bool CheckUseAndReach(
    Pawn animal,
    CompAnimalPenMarker penMarker,
    bool allowUnenclosedPens,
    Pawn roper,
    ref bool foundEnclosed,
    ref bool foundUsable,
    ref bool foundReachable)
  {
    if (!allowUnenclosedPens && penMarker.PenState.Unenclosed)
      return false;
    foundEnclosed = true;
    if (!penMarker.AcceptsToPen(animal) || roper == null && ForbidUtility.IsForbidden((Thing) ((ThingComp) penMarker).parent, Faction.OfPlayer) || roper != null && ForbidUtility.IsForbidden((Thing) ((ThingComp) penMarker).parent, roper))
      return false;
    foundUsable = true;
    bool flag;
    if (roper == null)
    {
      TraverseParms traverseParms1 = TraverseParms.For((TraverseMode) 1, (Danger) 3, false, false, false, true, false);
      TraverseParms traverseParms2 = ((TraverseParms) ref traverseParms1).WithFenceblockedOf(animal);
      flag = CrossMapReachabilityUtility.CanReach(((Thing) animal).Map, ((Thing) animal).Position, LocalTargetInfo.op_Implicit((Thing) ((ThingComp) penMarker).parent), (PathEndMode) 2, traverseParms2, ((Thing) ((ThingComp) penMarker).parent).Map);
    }
    else
    {
      TraverseParms traverseParms3 = TraverseParms.For(roper, (Danger) 3, (TraverseMode) 0, false, false, false, true);
      TraverseParms traverseParms4 = ((TraverseParms) ref traverseParms3).WithFenceblockedOf(animal);
      flag = CrossMapReachabilityUtility.CanReach(((Thing) animal).Map, ((Thing) animal).Position, LocalTargetInfo.op_Implicit((Thing) ((ThingComp) penMarker).parent), (PathEndMode) 2, traverseParms4, ((Thing) ((ThingComp) penMarker).parent).Map);
    }
    if (!flag)
      return false;
    foundReachable = true;
    return true;
  }
}
