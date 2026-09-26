// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.WorkGiver_LoadBuildableContainer
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

[Obsolete("Use instead vanilla logic.")]
public class WorkGiver_LoadBuildableContainer : WorkGiver_Scanner, IWorkGiverAcrossMaps
{
  public bool NeedVirtualMapTransfer => false;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
  {
    return (IEnumerable<Thing>) ((Thing) pawn).Map.BaseMapAndVehicleMaps().SelectMany<Map, Building>((Func<Map, IEnumerable<Building>>) (m => m.listerBuildings.allBuildingsColonist.Where<Building>((Func<Building, bool>) (b => ThingCompUtility.HasComp<CompTransporter>((Thing) b)))));
  }

  public virtual PathEndMode PathEndMode => (PathEndMode) 2;

  public virtual Danger MaxPathDanger(Pawn pawn) => (Danger) 3;

  public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    CompTransporter comp = ThingCompUtility.TryGetComp<CompTransporter>(t);
    return LoadTransportersJobOnVehicleUtility.HasJobOnTransporter(pawn, comp);
  }

  public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    CompTransporter comp = ThingCompUtility.TryGetComp<CompTransporter>(t);
    return LoadTransportersJobOnVehicleUtility.JobOnTransporter(pawn, comp);
  }
}
