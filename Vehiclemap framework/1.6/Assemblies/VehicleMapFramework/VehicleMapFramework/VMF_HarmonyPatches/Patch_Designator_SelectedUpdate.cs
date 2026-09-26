// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Designator_SelectedUpdate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (DesignatorManager), "DesignatorManagerUpdate")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Designator_SelectedUpdate
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo m_SelectedUpdate = AccessTools.Method(typeof (Designator), "SelectedUpdate", (Type[]) null, (Type[]) null);
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (CodeInstructionExtensions.Calls(instruction, m_SelectedUpdate))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        yield return CodeInstruction.LoadField(typeof (DesignatorManager), "selectedDesignator", false);
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_Designator_SelectedUpdate.\u003C\u003EO.\u003C0\u003E__SelectedUpdatePostfix ?? (Patch_Designator_SelectedUpdate.\u003C\u003EO.\u003C0\u003E__SelectedUpdatePostfix = new Action<Designator>(Patch_Designator_SelectedUpdate.SelectedUpdatePostfix))).Method);
      }
    }
  }

  public static void SelectedUpdatePostfix(Designator ___selectedDesignator)
  {
    if (Command_FocusVehicleMap.FocusLockedVehicle != null)
      return;
    Command_FocusVehicleMap.FocusedVehicle = (VehiclePawnWithMap) null;
    Vector3 original = UI.MouseMapPosition();
    VehicleMapFlag flag = VehicleMapFlag.None;
    if (___selectedDesignator is Designator_Build designatorBuild && ((Designator_Place) designatorBuild).PlacingDef is ThingDef placingDef)
    {
      if (placingDef is VehicleBuildDef vehicleBuildDef)
      {
        VehicleDef thingToSpawn = vehicleBuildDef.thingToSpawn;
        if (thingToSpawn != null)
        {
          Type thingClass = ((ThingDef) thingToSpawn).thingClass;
          if ((object) thingClass != null && GenTypes.SameOrSubclassOf(thingClass, typeof (VehiclePawnWithMap)))
            return;
        }
      }
      if (placingDef.HasComp<CompMapExpander>())
        flag |= VehicleMapFlag.ExpandableCells;
    }
    VehiclePawnWithMap vehicle;
    if (original.TryGetVehicleMap(Find.CurrentMap, out vehicle, flag))
      Command_FocusVehicleMap.FocusedVehicle = vehicle;
    if (!(___selectedDesignator is Designator_AreaAllowed))
      return;
    Area selArea = Designator_AreaAllowed.selectedArea;
    if (selArea == null || selArea.Map == ___selectedDesignator.Map)
      return;
    Designator_AreaAllowed.selectedArea = GenCollection.FirstOrDefault<Area>(___selectedDesignator.Map.areaManager.AllAreas, (Predicate<Area>) (a => a.AssignableAsAllowed() && a.InspectLabel == selArea.InspectLabel));
    if (Designator_AreaAllowed.selectedArea != null)
      return;
    Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VMF_AreaDeselect", NamedArgument.op_Implicit(selArea.InspectLabel))), MessageTypeDefOf.RejectInput, false);
    Find.DesignatorManager.Deselect();
  }
}
