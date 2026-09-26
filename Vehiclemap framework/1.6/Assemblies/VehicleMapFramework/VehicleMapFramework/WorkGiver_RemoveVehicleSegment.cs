// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.WorkGiver_RemoveVehicleSegment
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class WorkGiver_RemoveVehicleSegment : WorkGiver_RemoveBuilding
{
  protected virtual DesignationDef Designation => VMF_DefOf.VMF_RemoveSegment;

  protected virtual JobDef RemoveBuildingJob => VMF_DefOf.VMF_DeconstructVehicleSegment;

  public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
  {
    CompMapExpander compMapExpander;
    return ThingCompUtility.TryGetComp<CompMapExpander>(t, ref compMapExpander) && !compMapExpander.IsOnlyBridge && base.HasJobOnThing(pawn, t, forced);
  }
}
