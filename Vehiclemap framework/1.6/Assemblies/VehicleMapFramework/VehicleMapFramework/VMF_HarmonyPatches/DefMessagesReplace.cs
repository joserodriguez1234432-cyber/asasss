// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.DefMessagesReplace
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(100)]
public static class DefMessagesReplace
{
  public const string prefix = "VMF_";
  public const string suffix = "AcrossMaps";

  static DefMessagesReplace()
  {
    WorkGiverDef namedSilentFail1 = DefDatabase<WorkGiverDef>.GetNamedSilentFail("VMF_RefuelVehicleTank");
    WorkGiverDef namedSilentFail2 = DefDatabase<WorkGiverDef>.GetNamedSilentFail("RefuelVehicle");
    if (namedSilentFail1 != null && namedSilentFail2 != null)
    {
      ((Def) namedSilentFail1).label = ((Def) namedSilentFail2).label;
      namedSilentFail1.verb = namedSilentFail2.verb;
      namedSilentFail1.gerund = namedSilentFail2.gerund;
    }
    WorkGiverDef namedSilentFail3 = DefDatabase<WorkGiverDef>.GetNamedSilentFail("VMF_RepairMapVehicle");
    WorkGiverDef namedSilentFail4 = DefDatabase<WorkGiverDef>.GetNamedSilentFail("RepairVehicle");
    if (namedSilentFail3 != null && namedSilentFail4 != null)
    {
      ((Def) namedSilentFail3).label = ((Def) namedSilentFail4).label;
      namedSilentFail3.verb = namedSilentFail4.verb;
      namedSilentFail3.gerund = namedSilentFail4.gerund;
    }
    WorkGiverDef removeVehicleSegment = VMF_DefOf.VMF_RemoveVehicleSegment;
    WorkGiverDef namedSilentFail5 = DefDatabase<WorkGiverDef>.GetNamedSilentFail("Deconstruct");
    if (removeVehicleSegment != null && namedSilentFail5 != null)
    {
      ((Def) removeVehicleSegment).label = ((Def) namedSilentFail5).label;
      removeVehicleSegment.verb = namedSilentFail5.verb;
      removeVehicleSegment.gerund = namedSilentFail5.gerund;
    }
    foreach (JobDef jobDef in DefDatabase<JobDef>.AllDefs.Where<JobDef>((Func<JobDef, bool>) (d => ((Def) d).defName.StartsWith("VMF_") && ((Def) d).defName.EndsWith("AcrossMaps"))))
    {
      JobDef namedSilentFail6 = DefDatabase<JobDef>.GetNamedSilentFail(((Def) jobDef).defName.Replace("VMF_", "").Replace("AcrossMaps", ""));
      if (namedSilentFail6 != null)
      {
        ((Def) jobDef).label = ((Def) namedSilentFail6).label;
        jobDef.reportString = namedSilentFail6.reportString;
      }
    }
    JobDef refuelVehicleTank = VMF_DefOf.VMF_RefuelVehicleTank;
    JobDef refuelVehicle = JobDefOf_Vehicles.RefuelVehicle;
    if (refuelVehicleTank != null && refuelVehicle != null)
    {
      ((Def) refuelVehicleTank).label = ((Def) refuelVehicle).label;
      refuelVehicleTank.reportString = refuelVehicle.reportString;
    }
    VMF_DefOf.VMF_RepairMapVehicle.reportString = JobDefOf_Vehicles.RepairVehicle.reportString;
    JobDef deconstructVehicleSegment = VMF_DefOf.VMF_DeconstructVehicleSegment;
    if (deconstructVehicleSegment == null)
      return;
    deconstructVehicleSegment.reportString = JobDefOf.Deconstruct.reportString;
  }
}
