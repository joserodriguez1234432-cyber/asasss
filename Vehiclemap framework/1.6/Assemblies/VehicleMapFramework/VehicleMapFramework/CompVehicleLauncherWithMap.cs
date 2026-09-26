// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompVehicleLauncherWithMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompVehicleLauncherWithMap : CompVehicleLauncher
{
  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    foreach (Gizmo gizmo in base.CompGetGizmosExtra())
    {
      if (gizmo is Command_ActionHighlighter actionHighlighter)
      {
        ((Gizmo) actionHighlighter).Disabled = false;
        string disableReason;
        if (!this.CanLaunchWithCargoCapacityWithMap(out disableReason))
          ((Gizmo) actionHighlighter).Disable(disableReason);
      }
      yield return gizmo;
    }
  }

  public bool CanLaunchWithCargoCapacityWithMap(out string disableReason)
  {
    if (!this.CanLaunchWithCargoCapacity(ref disableReason))
      return false;
    if ((((VehicleComp) this).Vehicle.MovementPermissions & 1) == null && ((VehicleComp) this).Vehicle is VehiclePawnWithMap vehicle)
    {
      float statValue = ((VehicleComp) this).Vehicle.GetStatValue(VMF_DefOf.MaximumPayload);
      if ((double) CollectionsMassCalculator.MassUsage<Thing>(vehicle.VehicleMap.listerThings.AllThings, (IgnorePawnsInventoryMode) 3, true, false) > (double) statValue)
      {
        disableReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CannotLaunchOverEncumbered", NamedArgument.op_Implicit(((Entity) ((VehicleComp) this).Vehicle).LabelShort)));
        return false;
      }
    }
    return true;
  }
}
