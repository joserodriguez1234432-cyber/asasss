// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.CheckEnablePipeConnector
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(100)]
public static class CheckEnablePipeConnector
{
  static CheckEnablePipeConnector()
  {
    if (CheckEnablePipeConnector.EnablePipeConnector())
      return;
    ((BuildableDef) DefDatabase<ThingDef>.GetNamed("VMF_PipeConnector", true)).designationCategory = (DesignationCategoryDef) null;
    ((Editable) DefDatabase<DesignationCategoryDef>.GetNamed("VF_Vehicles", true)).ResolveReferences();
  }

  private static bool EnablePipeConnector()
  {
    if (ModCompat.CompatBase<ModCompat.DubsBadHygiene>.Active && !ModCompat.DubsBadHygiene.LiteMode || ModCompat.CompatBase<ModCompat.Rimefeller>.Active)
      return true;
    if (ModCompat.CompatBase<ModCompat.VFECore>.Active)
    {
      if (((IEnumerable<object>) AccessTools.PropertyGetter(typeof (DefDatabase<>).MakeGenericType(ModCompat.VFECore.PipeNetDef), "AllDefs").Invoke((object) null, (object[]) null)).Count<object>() > 1)
        return true;
    }
    return false;
  }
}
