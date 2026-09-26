// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.LoadTransportersJobOnVehicleUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

[Obsolete]
public static class LoadTransportersJobOnVehicleUtility
{
  public static ThingCount FindThingToLoad(
    Pawn p,
    CompTransporter transporter,
    bool gatherFromBaseMap)
  {
    return !gatherFromBaseMap ? LoadTransportersJobUtility.FindThingToLoad(p, transporter) : Patch_LoadTransportersJobUtility_FindThingToLoad.FindThingToLoad(p, transporter);
  }

  public static Job JobOnTransporter(Pawn p, CompTransporter transporter)
  {
    return LoadTransportersJobUtility.JobOnTransporter(p, transporter);
  }

  public static bool HasJobOnTransporter(Pawn pawn, CompTransporter transporter)
  {
    if (ForbidUtility.IsForbidden((Thing) ((ThingComp) transporter).parent, pawn) || !transporter.AnythingLeftToLoad || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !pawn.CanReach(LocalTargetInfo.op_Implicit((Thing) ((ThingComp) transporter).parent), (PathEndMode) 2, DangerUtility.NormalMaxDanger(pawn), false, false, (TraverseMode) 0, ((Thing) ((ThingComp) transporter).parent).Map))
      return false;
    ThingCount thingToLoad = LoadTransportersJobOnVehicleUtility.FindThingToLoad(pawn, transporter, !(transporter is CompBuildableContainer buildableContainer) || buildableContainer.GatherFromBaseMap);
    return ((ThingCount) ref thingToLoad).Thing != null;
  }
}
