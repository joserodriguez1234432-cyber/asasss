// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_HealthAndStats
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using SmashTools.Patching;
using System.Reflection;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_HealthAndStats : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn), "TicksPerMove", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehicleMoveSpeed", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (HealthUtility), "GetGeneralConditionLabel", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "ReplaceConditionLabel", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_HealthTracker), "ShouldBeDowned", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehicleShouldBeDowned", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_HealthTracker), "AddHediff", new System.Type[4]
    {
      typeof (Hediff),
      typeof (BodyPartRecord),
      typeof (DamageInfo?),
      typeof (DamageWorker.DamageResult)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesDontAddHediffs", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_HealthTracker), "MakeDowned", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesCantBeDowned", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MentalStateWorker), "StateCanOccur", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesCantEnterMentalState", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MentalBreakWorker), "BreakCanOccur", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesCantEnterMentalBreak", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (HediffUtility), "CanHealNaturally", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesDontHeal", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (HediffUtility), "CanHealFromTending", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesDontHealTended", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Verb_CastAbility), "CanHitTarget", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "VehiclesImmuneToPsycast", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (StatWorker), "IsDisabledFor", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "StatDisabledForVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (SchoolUtility), "CanTeachNow", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "CantTeachVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (StunHandler), "StunFor", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "StunVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (StaggerHandler), "StaggerFor", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_HealthAndStats), "StaggerVehicle", (System.Type[]) null));
  }

  public static bool VehicleMoveSpeed(bool diagonal, Pawn __instance, ref float __result)
  {
    if (!(__instance is VehiclePawn vehiclePawn))
      return true;
    float val = (float) (1.0 / ((double) vehiclePawn.GetStatValue(VehicleStatDefOf.MoveSpeed) / 60.0));
    if (((Thing) vehiclePawn).Spawned && !((Thing) vehiclePawn).Map.roofGrid.Roofed(((Thing) vehiclePawn).Position))
      val /= ((Thing) vehiclePawn).Map.weatherManager.CurMoveSpeedMultiplier;
    if (diagonal)
      val *= Ext_Math.Sqrt2;
    __result = val.Clamp(1f, 450f);
    return false;
  }

  public static bool ReplaceConditionLabel(ref string __result, Pawn pawn, bool shortVersion = false)
  {
    if (pawn == null || !(pawn is VehiclePawn vehiclePawn))
      return true;
    if (vehiclePawn.movementStatus == VehicleMovementStatus.Offline && !pawn.Dead)
    {
      __result = !((Thing) pawn).IsBoat() || !vehiclePawn.beached ? TaggedString.op_Implicit(Translator.Translate("VF_healthLabel_Immobile")) : TaggedString.op_Implicit(Translator.Translate("VF_healthLabel_Beached"));
      return false;
    }
    if (pawn.Dead)
    {
      __result = TaggedString.op_Implicit(Translator.Translate("VF_healthLabel_Dead"));
      return false;
    }
    if ((double) vehiclePawn.statHandler.HealthPercent < 0.949999988079071)
    {
      __result = TaggedString.op_Implicit(Translator.Translate("VF_healthLabel_Injured"));
      return false;
    }
    __result = TaggedString.op_Implicit(Translator.Translate("VF_healthLabel_Healthy"));
    return false;
  }

  public static bool VehiclesDontAddHediffs(Pawn ___pawn) => !(___pawn is VehiclePawn);

  public static bool VehiclesCantBeDowned(Pawn ___pawn) => !(___pawn is VehiclePawn);

  public static bool VehiclesCantEnterMentalState(Pawn pawn, ref bool __result)
  {
    if (!(pawn is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool VehiclesCantEnterMentalBreak(Pawn pawn, ref bool __result)
  {
    if (!(pawn is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool VehicleShouldBeDowned(ref bool __result, ref Pawn ___pawn)
  {
    if (___pawn == null || !(___pawn is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool VehiclesDontHeal(Hediff_Injury hd, ref bool __result)
  {
    if (!(((Hediff) hd).pawn is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool VehiclesDontHealTended(Hediff_Injury hd, ref bool __result)
  {
    if (!(((Hediff) hd).pawn is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool VehiclesImmuneToPsycast(LocalTargetInfo targ)
  {
    if (!(((LocalTargetInfo) ref targ).Pawn is VehiclePawn pawn))
      return true;
    Debug.Message($"Psycast blocked for {pawn}");
    return false;
  }

  public static bool StatDisabledForVehicle(Thing thing, ref bool __result)
  {
    if (!(thing is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool CantTeachVehicles(Pawn teacher, ref bool __result)
  {
    if (!(teacher is VehiclePawn))
      return true;
    __result = false;
    return false;
  }

  public static bool StunVehicle(int ticks, Thing instigator, Thing ___parent)
  {
    return !(___parent is VehiclePawn vehiclePawn) || vehiclePawn.statHandler.OverrideStunPatch;
  }

  public static bool StaggerVehicle(int ticks, Thing ___parent, ref bool __result)
  {
    if (!(___parent is VehiclePawn))
      return true;
    __result = false;
    return false;
  }
}
