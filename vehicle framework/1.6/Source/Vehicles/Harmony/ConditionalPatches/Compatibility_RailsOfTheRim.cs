// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_RailsOfTheRim
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld.Planet;
using SmashTools.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_RailsOfTheRim : ConditionalVehiclePatch
{
  public override string PackageId => "Mlie.RailsAndRoadsOfTheRim";

  public override PatchSequence PatchAt => PatchSequence.Async;

  public override void PatchAll(ModMetaData mod)
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(AccessTools.TypeByName("RailsAndRoadsOfTheRim.Alert_CaravanIdle_GetReport"), "Postfix", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Compatibility_RailsOfTheRim), "GetAlertReportIdleConstructionVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(AccessTools.TypeByName("RailsAndRoadsOfTheRim.WorldObjectComp_Caravan"), "CaravanCurrentState", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Compatibility_RailsOfTheRim), "CaravanStateVehiclePather", (System.Type[]) null));
  }

  private static void CaravanStateVehiclePather(WorldObjectComp __instance, ref object __result)
  {
    if (!(__instance.parent is VehicleCaravan parent) || !parent.vehiclePather.MovingNow)
      return;
    __result = (object) (byte) 0;
  }

  private static IEnumerable<CodeInstruction> GetAlertReportIdleConstructionVehicle(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    FieldInfo patherField = AccessTools.Field(typeof (Caravan), "pather");
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction codeInstruction1 = instructionList[i];
      if (CodeInstructionExtensions.LoadsField(codeInstruction1, patherField, false))
      {
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Compatibility_RailsOfTheRim), "CaravanMovingNow", (System.Type[]) null, (System.Type[]) null));
        CodeInstruction codeInstruction2 = instructionList[++i];
        codeInstruction1 = instructionList[++i];
      }
      yield return codeInstruction1;
    }
  }

  private static bool CaravanMovingNow(Caravan caravan)
  {
    return caravan is VehicleCaravan vehicleCaravan ? vehicleCaravan.vehiclePather.MovingNow : caravan.pather.MovingNow;
  }
}
