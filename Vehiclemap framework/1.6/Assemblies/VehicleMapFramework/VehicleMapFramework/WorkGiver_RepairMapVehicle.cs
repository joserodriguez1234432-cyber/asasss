// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.WorkGiver_RepairMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System.Collections.Generic;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class WorkGiver_RepairMapVehicle : WorkGiver_Scanner
{
  public virtual Danger MaxPathDanger(Pawn pawn) => (Danger) 2;

  public virtual PathEndMode PathEndMode => (PathEndMode) 3;

  public virtual bool ShouldSkip(Pawn pawn, bool forced = false)
  {
    VehiclePawnWithMap vehicle;
    return !(CrossMapReachabilityUtility.get_DepartMap(pawn) ?? ((Thing) pawn).Map).IsVehicleMapOf(out vehicle) || !vehicle.statHandler.NeedsRepairs;
  }

  public virtual IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) pawn).IsOnVehicleMapOf(out vehicle))
    {
      Map map = vehicle.VehicleMap;
      IntVec3 offset = VehicleMapUtility.HitboxToMapCell(vehicle);
      foreach (VehicleComponent vehicleComponent in vehicle.statHandler.ComponentsPrioritized)
      {
        if ((double) vehicleComponent.HealthPercent < 1.0)
        {
          List<IntVec2>.Enumerator enumerator = vehicleComponent.props.hitbox.Hitbox.GetEnumerator();
          while (enumerator.MoveNext())
          {
            IntVec2 current = enumerator.Current;
            IntVec3 intVec3 = IntVec3.op_Addition(((IntVec2) ref current).ToIntVec3, offset);
            if (GenGrid.InBounds(intVec3, map))
              yield return intVec3;
          }
          enumerator = new List<IntVec2>.Enumerator();
        }
      }
    }
  }

  public virtual bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
  {
    return ReservationUtility.CanReserveNew(pawn, LocalTargetInfo.op_Implicit(c));
  }

  public virtual Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
  {
    VehiclePawnWithMap vehicle;
    return !((Thing) pawn).IsOnVehicleMapOf(out vehicle) ? (Job) null : JobMaker.MakeJob(VMF_DefOf.VMF_RepairMapVehicle, LocalTargetInfo.op_Implicit((Thing) vehicle), LocalTargetInfo.op_Implicit(c));
  }
}
