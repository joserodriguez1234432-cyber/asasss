// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Misc
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

internal class Patch_Misc : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Selector), "HandleMapClicks", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Misc), "MultiSelectFloatMenu", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MentalState_Manhunter), "ForceHostileTo", new System.Type[1]
    {
      typeof (Thing)
    }, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Misc), "ManhunterDontAttackVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (TickManager), "Paused"), postfix: new HarmonyMethod(typeof (Patch_Misc), "PausedFromVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (TickManager), "CurTimeSpeed"), postfix: new HarmonyMethod(typeof (Patch_Misc), "ForcePauseFromVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnCapacitiesHandler), "Notify_CapacityLevelsDirty", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Misc), "RecheckVehicleHandlerCapacities", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn), "Kill", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Misc), "MoveOnDeath", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnUtility), "ShouldSendNotificationAbout", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Misc), "SendNotificationsVehicle", (System.Type[]) null));
  }

  private static bool MultiSelectFloatMenu(List<object> ___selected)
  {
    return Event.current.type != null || Event.current.button != 1 || ___selected.Count <= 0 || ___selected.Count <= 1 || !SelectionHelper.MultiSelectClicker(___selected);
  }

  private static void ManhunterDontAttackVehicles(Thing t, ref bool __result)
  {
    if (!__result || !(t is VehiclePawn vehiclePawn) || SettingsCache.TryGetValue<bool>(vehiclePawn.VehicleDef, typeof (VehicleProperties), "manhunterTargetsVehicle", vehiclePawn.VehicleDef.properties.manhunterTargetsVehicle))
      return;
    __result = false;
  }

  private static void PausedFromVehicles(ref bool __result)
  {
    if (!LandingTargeter.Instance.ForcedTargeting && !StrafeTargeter.Instance.ForcedTargeting)
      return;
    __result = true;
  }

  private static void ForcePauseFromVehicles(ref TimeSpeed __result)
  {
    if (!LandingTargeter.Instance.ForcedTargeting && !StrafeTargeter.Instance.ForcedTargeting)
      return;
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    ^(sbyte&) ref __result = (sbyte) 0;
  }

  private static void RecheckVehicleHandlerCapacities(Pawn ___pawn)
  {
    ___pawn.GetVehicle()?.EventRegistry?[VehicleEventDefOf.PawnCapacitiesDirty].ExecuteEvents();
  }

  private static void MoveOnDeath(Pawn __instance)
  {
    if (!__instance.InVehicle())
      return;
    VehiclePawn vehicle = __instance.GetVehicle();
    vehicle.AddOrTransfer((Thing) __instance);
    if (Find.World.worldPawns.Contains(__instance))
      Find.WorldPawns.RemovePawn(__instance);
    vehicle.EventRegistry[VehicleEventDefOf.PawnKilled].ExecuteEvents();
  }

  private static void SendNotificationsVehicle(Pawn p, ref bool __result)
  {
    int num;
    if (!__result)
    {
      Faction faction = ((Thing) p).Faction;
      num = faction == null ? 0 : (faction.IsPlayer ? 1 : 0);
    }
    else
      num = 0;
    bool flag1 = num != 0;
    if (flag1)
    {
      bool flag2;
      if (p != null)
      {
        switch (((Thing) p).ParentHolder)
        {
          case VehicleRoleHandler _:
label_7:
            flag2 = true;
            goto label_9;
          case Pawn_InventoryTracker inventoryTracker:
            if (!(inventoryTracker.pawn is VehiclePawn))
              break;
            goto label_7;
        }
      }
      flag2 = false;
label_9:
      flag1 = flag2;
    }
    if (!flag1)
      return;
    __result = true;
  }
}
