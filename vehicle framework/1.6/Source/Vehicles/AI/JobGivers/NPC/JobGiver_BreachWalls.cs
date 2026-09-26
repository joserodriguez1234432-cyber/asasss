// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_BreachWalls
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Linq;
using System.Threading;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

[NoProfiling]
public class JobGiver_BreachWalls : JobGiver_RangedSupport
{
  protected override Job TryGiveJob(Pawn pawn)
  {
    VehiclePawn vehicle = pawn as VehiclePawn;
    VehicleDef vehicleDef = vehicle.VehicleDef;
    IntVec3 intVec3 = ((LocalTargetInfo) ref vehicle.mindState.duty.focus).Cell;
    if (((IntVec3) ref intVec3).IsValid && IntVec3Utility.DistanceToSquared(intVec3, ((Thing) vehicle).Position) < 25 && VehicleRegionAndRoomQuery.RoomAtFast(intVec3, ((Thing) vehicle).Map, vehicleDef) == VehicleRegionAndRoomQuery.RoomAtFast(((Thing) vehicle).Position, ((Thing) vehicle).Map, vehicleDef) && intVec3.WithinRegions(((Thing) vehicle).Position, ((Thing) vehicle).Map, vehicleDef, 9, TraverseParms.op_Implicit((TraverseMode) 2)))
    {
      LordUtility.GetLord((Pawn) vehicle).Notify_ReachedDutyLocation((Pawn) vehicle);
      return (Job) null;
    }
    if (!((IntVec3) ref intVec3).IsValid)
    {
      IAttackTarget iattackTarget;
      if (!GenCollection.TryRandomElement<IAttackTarget>(((Thing) pawn).Map.attackTargetsCache.GetPotentialTargetsFor((IAttackTargetSearcher) pawn).Where<IAttackTarget>((Func<IAttackTarget, bool>) (target => !target.ThreatDisabled((IAttackTargetSearcher) vehicle) && target.Thing.Faction == Faction.OfPlayer && vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(target.Thing.Position), (PathEndMode) 1, (Danger) 3, (TraverseMode) 3))), ref iattackTarget))
        return (Job) null;
      intVec3 = iattackTarget.Thing.Position;
    }
    if (!vehicle.CanReachVehicle(LocalTargetInfo.op_Implicit(intVec3), (PathEndMode) 1, (Danger) 3, (TraverseMode) 3))
      return (Job) null;
    using (VehiclePath path = MapComponentCache<VehiclePathingSystem>.GetComponent(((Thing) vehicle).Map)[vehicleDef].VehiclePathFinder.FindPath(((Thing) vehicle).Position, LocalTargetInfo.op_Implicit(intVec3), TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 3, false, false, false, true), CancellationToken.None, (PathEndMode) 1))
    {
      Thing thing = PathingHelper.FirstBlockingBuilding(vehicle, path);
      if (thing != null)
      {
        IntVec3 dest;
        if (this.TryFindCombatPosition(vehicle, out dest))
        {
          vehicle.mindState.breachingTarget = new BreachingTargetData(thing, dest);
          intVec3 = dest;
        }
      }
    }
    return JobMaker.MakeJob(JobDefOf.Goto, LocalTargetInfo.op_Implicit(intVec3), 500, true);
  }
}
