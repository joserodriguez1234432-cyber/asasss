// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ShieldManagerMapComp_WillInterceptOrbitalStrike
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_EnergyShield")]
[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_ShieldManagerMapComp_WillInterceptOrbitalStrike
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo m_AllBuildingsColonistOfClass = AccessTools.Method(typeof (ListerBuildings), "AllBuildingsColonistOfClass", (Type[]) null, new Type[1]
    {
      ModCompat.EnergyShield.Building_Shield
    });
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (CodeInstructionExtensions.Calls(instruction, m_AllBuildingsColonistOfClass))
      {
        yield return CodeInstruction.LoadArgument(0, false);
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        yield return PatchHelper.get_CallInstruction((Patch_ShieldManagerMapComp_WillInterceptOrbitalStrike.\u003C\u003EO.\u003C0\u003E__AddBuildings ?? (Patch_ShieldManagerMapComp_WillInterceptOrbitalStrike.\u003C\u003EO.\u003C0\u003E__AddBuildings = new Func<IEnumerable<Building>, MapComponent, IEnumerable<Building>>(Patch_ShieldManagerMapComp_WillInterceptOrbitalStrike.AddBuildings))).Method);
      }
    }
  }

  private static IEnumerable<Building> AddBuildings(
    IEnumerable<Building> buildings,
    MapComponent component)
  {
    return buildings.Concat<Building>(VehiclePawnWithMapCache.AllVehiclesOn(component.map).SelectMany<VehiclePawnWithMap, Building>((Func<VehiclePawnWithMap, IEnumerable<Building>>) (v => v.VehicleMap.listerBuildings.allBuildingsColonist.Where<Building>((Func<Building, bool>) (b => GenTypes.SameOrSubclassOf(((Thing) b).def.thingClass, ModCompat.EnergyShield.Building_Shield))))));
  }
}
