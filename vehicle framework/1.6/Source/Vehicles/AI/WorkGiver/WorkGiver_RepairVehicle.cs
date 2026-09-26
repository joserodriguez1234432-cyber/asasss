// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_RepairVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class WorkGiver_RepairVehicle : VehicleWorkGiver
{
  public override JobDef JobDef => JobDefOf_Vehicles.RepairVehicle;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.GetCachedMapComponent<ListerVehiclesRepairable>().RepairsForFaction(((Thing) pawn).Faction);
  }

  public virtual Danger MaxPathDanger(Pawn pawn) => (Danger) 3;

  public override bool CanBeWorkedOn(VehiclePawn vehicle) => vehicle.statHandler.NeedsRepairs;
}
