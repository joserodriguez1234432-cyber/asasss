// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompRelatedBuildCommands
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompRelatedBuildCommands : VehicleComp
{
  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    foreach (IGrouping<DesignatorDropdownGroupDef, Designator_Build> source in BuildRelatedCommandUtility.RelatedBuildCommands((BuildableDef) this.Vehicle.VehicleDef.buildDef).OfType<Designator_Build>().GroupBy<Designator_Build, DesignatorDropdownGroupDef>((Func<Designator_Build, DesignatorDropdownGroupDef>) (des => ((Designator_Place) des).PlacingDef?.designatorDropdown)))
    {
      if (source.Key == null)
      {
        foreach (Gizmo gizmo in (IEnumerable<Designator_Build>) source)
          yield return gizmo;
      }
      else
      {
        foreach (IGrouping<DesignationCategoryDef, Designator_Build> grouping in source.GroupBy<Designator_Build, DesignationCategoryDef>((Func<Designator_Build, DesignationCategoryDef>) (des => ((Designator_Place) des).PlacingDef?.designationCategory)))
        {
          IGrouping<DesignationCategoryDef, Designator_Build> categoryGroup = grouping;
          DesignationCategoryDef key = categoryGroup.Key;
          Designator designator = key != null ? key.ResolvedAllowedDesignators.FirstOrDefault<Designator>((Func<Designator, bool>) (des => des is Designator_Dropdown designatorDropdown && GenCollection.Any<Designator>(designatorDropdown.Elements, (Predicate<Designator>) (des2 => ((IEnumerable<Designator>) categoryGroup).Contains<Designator>(des2))))) : (Designator) null;
          if (designator != null)
            yield return (Gizmo) designator;
        }
      }
    }
  }
}
