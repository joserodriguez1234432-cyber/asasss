// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_PawnAi
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools.Patching;
using System.Reflection;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_PawnAi : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn), "ThreatDisabled", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_PawnAi), "VehicleThreatDisabled", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MentalStateHandler), "TryStartMentalState", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_PawnAi), "EjectPawnForMentalState", (System.Type[]) null));
  }

  private static void VehicleThreatDisabled(
    Pawn __instance,
    IAttackTargetSearcher disabledFor,
    ref bool __result)
  {
    if (__result || !(__instance is VehiclePawn vehiclePawn))
      return;
    __result = !vehiclePawn.IsThreatToAttackTargetSearcher(disabledFor);
  }

  private static void EjectPawnForMentalState(MentalStateDef stateDef, Pawn ___pawn)
  {
    if (!(((Thing) ___pawn).ParentHolder is VehicleRoleHandler parentHolder))
      return;
    if (CaravanUtility.IsCaravanMember(___pawn))
    {
      if (!parentHolder.RequiredForMovement)
        return;
      Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleCaravanMentalBreakMovementRole", NamedArgument.op_Implicit((Thing) ___pawn))), MessageTypeDefOf.NegativeEvent, true);
    }
    else
    {
      if (parentHolder.vehicle.vehiclePather.Moving)
        return;
      parentHolder.vehicle.DisembarkPawn(___pawn);
    }
  }
}
