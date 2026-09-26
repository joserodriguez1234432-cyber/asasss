// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.WorkGiver_RefuelVehicleTank
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class WorkGiver_RefuelVehicleTank : WorkGiver_RefuelVehicle
{
  public virtual JobDef JobStandard => VMF_DefOf.VMF_RefuelVehicleTank;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) pawn).IsOnVehicleMapOf(out vehicle) && (!((Thing) vehicle).Spawned || ComponentCache.GetCachedMapComponent<VehicleReservationManager>(((Thing) vehicle).Map).VehicleListers("Refuel").Contains<VehiclePawn>((VehiclePawn) vehicle)))
    {
      foreach (ThingComp fuelTankComp in vehicle.FuelTankComps)
        yield return (Thing) fuelTankComp.parent;
    }
  }

  public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    VehiclePawnWithMap vehicle;
    return WorkGiver_RefuelVehicleTank.CanRefuelTank(pawn, t, forced) && t.IsOnVehicleMapOf(out vehicle) && vehicle.CompFueledTravel != null && WorkGiver_RefuelVehicleTank.CanRefuelVehicle(pawn, (VehiclePawn) vehicle, forced);
  }

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    VehiclePawnWithMap vehicle;
    if (!t.IsOnVehicleMapOf(out vehicle))
      return (Job) null;
    Thing thing = vehicle.CompFueledTravel.ClosestFuelAvailable(pawn);
    return thing != null ? JobMaker.MakeJob(VMF_DefOf.VMF_RefuelVehicleTank, LocalTargetInfo.op_Implicit(t), LocalTargetInfo.op_Implicit(thing)) : (Job) null;
  }

  public static bool CanRefuelVehicle(Pawn pawn, VehiclePawn vehicle, bool forced)
  {
    CompFueledTravel compFueledTravel = vehicle?.CompFueledTravel;
    if (compFueledTravel == null || compFueledTravel.FullTank || compFueledTravel.FuelLeaking || !forced && !ShouldAutoRefuelNow())
      return false;
    if (compFueledTravel.ClosestFuelAvailable(pawn) != null)
      return ((Thing) vehicle).Faction == ((Thing) pawn).Faction;
    JobFailReason.Is(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("NoFuelToRefuel", NamedArgument.op_Implicit((Def) compFueledTravel.Props.fuelType))), (string) null);
    return false;

    bool ShouldAutoRefuelNow()
    {
      return (double) FuelPercentOfTarget() <= (double) compFueledTravel.Props.autoRefuelPercent && !compFueledTravel.FullTank && (double) compFueledTravel.TargetFuelLevel > 0.0 && ShouldAutoRefuelNowIgnoringFuelPct();
    }

    bool ShouldAutoRefuelNowIgnoringFuelPct()
    {
      if (!compFueledTravel.allowAutoRefuel)
        return false;
      if (!((Thing) vehicle).Spawned)
        return true;
      return !FireUtility.IsBurning((Thing) vehicle) && ((Thing) vehicle).Map.designationManager.DesignationOn((Thing) vehicle, DesignationDefOf_Vehicles.DisassembleVehicle) == null;
    }

    float FuelPercentOfTarget()
    {
      return (double) compFueledTravel.TargetFuelLevel != 0.0 ? compFueledTravel.Fuel / compFueledTravel.TargetFuelLevel : 0.0f;
    }
  }

  public static bool CanRefuelTank(Pawn pawn, Thing t, bool forced = false)
  {
    return !ForbidUtility.IsForbidden(t, pawn) && pawn.CanReserve(LocalTargetInfo.op_Implicit(t), 1, -1, (ReservationLayerDef) null, forced, t.MapHeld) && t.Faction == ((Thing) pawn).Faction;
  }
}
