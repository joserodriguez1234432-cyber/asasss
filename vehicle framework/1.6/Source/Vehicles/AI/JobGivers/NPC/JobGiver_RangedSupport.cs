// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_RangedSupport
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class JobGiver_RangedSupport : JobGiver_CombatFormation
{
  protected override bool TryFindCombatPosition(VehiclePawn vehicle, out IntVec3 dest)
  {
    return CombatPositionFinder.TryFindCastPosition(CastPositionRequest.For(vehicle, vehicle.mindState.enemyTarget), out dest);
  }

  protected override void UpdateEnemyTarget(VehiclePawn vehicle)
  {
    Thing thing = vehicle.mindState.enemyTarget;
    if (thing != null && this.ShouldLoseTarget(vehicle))
      thing = (Thing) null;
    if (thing == null)
    {
      thing = CombatTargetFinder.FindAttackTarget(vehicle, (TargetScanFlags) 297, (Func<Thing, bool>) (target => this.ExtraTargetValidator(vehicle, target)), vehicle.CompVehicleTurrets.MinRange, vehicle.CompVehicleTurrets.MaxRange, onlyRanged: true);
      if (thing != null)
      {
        JobGiver_CombatFormation.Notify_EngagedTarget(vehicle.mindState);
        LordUtility.GetLord((Pawn) vehicle)?.Notify_PawnAcquiredTarget((Pawn) vehicle, thing);
      }
    }
    vehicle.mindState.enemyTarget = thing;
    if (!(thing is Pawn) || thing.Faction != Faction.OfPlayer)
      return;
    IntVec3 position = ((Thing) vehicle).Position;
    if (!((IntVec3) ref position).InHorDistOf(thing.Position, 60f))
      return;
    Find.TickManager.slower.SignalForceNormalSpeed();
  }
}
