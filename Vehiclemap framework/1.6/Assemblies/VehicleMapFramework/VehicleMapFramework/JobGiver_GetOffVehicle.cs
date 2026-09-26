// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobGiver_GetOffVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class JobGiver_GetOffVehicle : ThinkNode_JobGiver
{
  public virtual float GetPriority(Pawn pawn) => 0.0f;

  protected virtual Job TryGiveJob(Pawn pawn)
  {
    Faction faction = ((Thing) pawn).Faction;
    if ((faction != null ? (faction.IsPlayer ? 1 : 0) : 0) != 0)
    {
      if (!VehicleMapFramework.VehicleMapFramework.settings.autoGetOffPlayer)
        return (Job) null;
    }
    else if (!VehicleMapFramework.VehicleMapFramework.settings.autoGetOffNonPlayer)
      return (Job) null;
    VehiclePawnWithMap vehicle;
    if (((Thing) pawn).IsOnVehicleMapOf(out vehicle) && ((Thing) vehicle).Spawned)
    {
      if (((Thing) pawn).Faction == ((Thing) vehicle).Faction)
        return (Job) null;
      CellRect cellRect = Ext_Vehicles.VehicleRect((VehiclePawn) vehicle, false);
      cellRect = ((CellRect) ref cellRect).ExpandedBy(1);
      IEnumerable<IntVec3> edgeCells = ((CellRect) ref cellRect).EdgeCells;
      TargetInfo exitSpot = TargetInfo.Invalid;
      Func<IntVec3, bool> predicate = (Func<IntVec3, bool>) (c => pawn.CanReach(LocalTargetInfo.op_Implicit(c), (PathEndMode) 1, (Danger) 3, false, false, (TraverseMode) 0, ((Thing) vehicle).Map, out exitSpot, out TargetInfo _, out List<TraverseSpots> _));
      if (edgeCells.Any<IntVec3>(predicate))
        return JobMaker.MakeJob(VMF_DefOf.VMF_GotoAcrossMaps).SetSpotsToJobAcrossMaps(pawn, new TargetInfo?(exitSpot));
    }
    return (Job) null;
  }
}
