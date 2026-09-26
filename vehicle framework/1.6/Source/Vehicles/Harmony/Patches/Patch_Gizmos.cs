// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Gizmos
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vehicles.Rendering;
using Vehicles.World;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal class Patch_Gizmos : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Settlement), "GetCaravanGizmos", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Gizmos), "NoAttackSettlementWhenDocked", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Settlement), "GetGizmos", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Gizmos), "AddVehicleCaravanGizmoPassthrough", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanFormingUtility), "GetGizmos", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Gizmos), "GizmosForVehicleCaravans", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Designator_Build), "GizmoOnGUI", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Gizmos), "VehicleMaterialOnBuildGizmo", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (BuildCopyCommandUtility), "BuildCopyCommand", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Gizmos), "VehicleMaterialOnCopyBuildGizmo", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Thing), "GetGizmos", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Gizmos), "ThingTransferToVehicleGizmo", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_InfoCard), "DoWindowContents", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Gizmos), "VehicleInfoCardOverride", (System.Type[]) null));
  }

  private static void NoAttackSettlementWhenDocked(
    Caravan caravan,
    ref IEnumerable<Gizmo> __result,
    Settlement __instance)
  {
    if (!(caravan is VehicleCaravan caravan1) || !caravan1.HasBoat() || caravan1.vehiclePather.Moving)
      return;
    List<Gizmo> list = __result.ToList<Gizmo>();
    if (caravan.PawnsListForReading.NotNullAndAny<Pawn>((Predicate<Pawn>) (p => !((Thing) p).IsBoat())))
    {
      int index = list.FindIndex((Predicate<Gizmo>) (x => Object.op_Equality((Object) ((Command) (x as Command_Action)).icon, (Object) Settlement.AttackCommand)));
      if (index >= 0 && index < list.Count)
        list[index].Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CommandAttackDockDisable", NamedArgument.op_Implicit(((WorldObject) __instance).LabelShort))));
    }
    else
    {
      int index1 = list.FindIndex((Predicate<Gizmo>) (x => Object.op_Equality((Object) ((Command) (x as Command_Action)).icon, (Object) ContentFinder<Texture2D>.Get("UI/Commands/Trade", false))));
      if (index1 >= 0 && index1 < list.Count)
        list[index1].Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CommandTradeDockDisable", NamedArgument.op_Implicit(((WorldObject) __instance).LabelShort))));
      int index2 = list.FindIndex((Predicate<Gizmo>) (x => Object.op_Equality((Object) ((Command) (x as Command_Action)).icon, (Object) ContentFinder<Texture2D>.Get("UI/Commands/OfferGifts", false))));
      if (index2 >= 0 && index2 < list.Count)
        list[index2].Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CommandTradeDockDisable", NamedArgument.op_Implicit(((WorldObject) __instance).LabelShort))));
    }
    __result = (IEnumerable<Gizmo>) list;
  }

  private static IEnumerable<Gizmo> AddVehicleCaravanGizmoPassthrough(
    IEnumerable<Gizmo> __result,
    Settlement __instance)
  {
    IEnumerator<Gizmo> enumerator = __result.GetEnumerator();
    if (((WorldObject) __instance).Faction != Faction.OfPlayer)
      ;
    while (enumerator.MoveNext())
      yield return enumerator.Current;
  }

  private static void GizmosForVehicleCaravans(
    ref IEnumerable<Gizmo> __result,
    Pawn pawn,
    Texture2D ___AddToCaravanCommand)
  {
    if (!((Thing) pawn).Spawned)
      return;
    bool flag = false;
    foreach (Lord lord in ((Thing) pawn).Map.lordManager.lords)
    {
      if (lord.faction == Faction.OfPlayer && lord.LordJob is LordJob_FormAndSendVehicles && !(lord.CurLordToil is LordToil_PrepareCaravan_LeaveWithVehicles) && !(lord.CurLordToil is LordToil_PrepareCaravan_BoardVehicles))
      {
        flag = true;
        break;
      }
    }
    if (!flag || !Dialog_FormCaravan.AllSendablePawns(((Thing) pawn).Map, false).Contains(pawn))
      return;
    Command_Action commandAction1 = new Command_Action();
    Command_Action commandAction2 = new Command_Action();
    ((Command) commandAction2).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandAddToCaravan"));
    ((Command) commandAction2).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandAddToCaravanDesc"));
    ((Command) commandAction2).icon = (Texture) ___AddToCaravanCommand;
    commandAction2.action = (Action) (() =>
    {
      List<Lord> lordList = new List<Lord>();
      foreach (Lord lord in ((Thing) pawn).Map.lordManager.lords)
      {
        if (lord.faction == Faction.OfPlayer && lord.LordJob is LordJob_FormAndSendVehicles)
          lordList.Add(lord);
      }
      if (lordList.Count <= 0)
        return;
      if (lordList.Count == 1)
      {
        AccessTools.Method(typeof (CaravanFormingUtility), "LateJoinFormingCaravan", (System.Type[]) null, (System.Type[]) null).Invoke((object) null, new object[2]
        {
          (object) pawn,
          (object) lordList[0]
        });
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
      }
      else
      {
        List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
        for (int index = 0; index < lordList.Count; ++index)
        {
          Lord caravanLocal = lordList[index];
          string str = TaggedString.op_Implicit(TaggedString.op_Addition(Translator.Translate("Caravan"), " ")) + (index + 1).ToString();
          floatMenuOptionList.Add(new FloatMenuOption(str, (Action) (() =>
          {
            if (!((Thing) pawn).Spawned || !((Thing) pawn).Map.lordManager.lords.Contains(caravanLocal) || !Dialog_FormCaravan.AllSendablePawns(((Thing) pawn).Map, false).Contains(pawn))
              return;
            AccessTools.Method(typeof (CaravanFormingUtility), "LateJoinFormingCaravan", (System.Type[]) null, (System.Type[]) null).Invoke((object) null, new object[2]
            {
              (object) pawn,
              (object) caravanLocal
            });
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
        }
        Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
      }
    });
    ((Command) commandAction2).hotKey = KeyBindingDefOf.Misc7;
    Command_Action commandAction3 = commandAction2;
    List<Gizmo> list = __result.ToList<Gizmo>();
    list.Add((Gizmo) commandAction3);
    __result = (IEnumerable<Gizmo>) list;
  }

  private static bool VehicleMaterialOnBuildGizmo(
    Vector2 topLeft,
    float maxWidth,
    BuildableDef ___entDef,
    ref GizmoResult __result,
    Designator_Build __instance,
    GizmoRenderParms parms)
  {
    if (!(___entDef is VehicleBuildDef buildDef))
      return true;
    float width = ((Gizmo) __instance).GetWidth(maxWidth);
    __result = VehicleGui.GizmoOnGUIWithMaterial((Command) __instance, new Rect(topLeft.x, topLeft.y, width, width), parms, buildDef);
    if (((BuildableDef) buildDef).MadeFromStuff)
      Designator_Dropdown.DrawExtraOptionsIcon(topLeft, ((Gizmo) __instance).GetWidth(maxWidth));
    return false;
  }

  private static bool VehicleMaterialOnCopyBuildGizmo(
    BuildableDef buildable,
    ThingDef stuff,
    ref Command __result)
  {
    if (!(buildable is VehicleBuildDef vehicleBuildDef))
      return true;
    Designator_Build designator = BuildCopyCommandUtility.FindAllowedDesignator(buildable, true);
    if (designator == null)
    {
      __result = (Command) null;
      return false;
    }
    if (buildable.MadeFromStuff && stuff == null)
      __result = (Command) designator;
    Command_ActionVehicleDrawn actionVehicleDrawn = new Command_ActionVehicleDrawn();
    actionVehicleDrawn.action = (Action) (() =>
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Tiny, (Map) null);
      Find.DesignatorManager.Select((Designator) designator);
      designator.SetStuffDef(stuff);
    });
    ((Command) actionVehicleDrawn).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandBuildCopy"));
    ((Command) actionVehicleDrawn).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandBuildCopyDesc"));
    ThingDef stuffDefRaw = designator.StuffDefRaw;
    designator.SetStuffDef(stuff);
    ((Command) actionVehicleDrawn).icon = designator.ResolvedIcon((ThingStyleDef) null);
    ((Command) actionVehicleDrawn).iconProportions = ((Command) designator).iconProportions;
    ((Command) actionVehicleDrawn).iconDrawScale = ((Command) designator).iconDrawScale;
    ((Command) actionVehicleDrawn).iconTexCoords = ((Command) designator).iconTexCoords;
    ((Command) actionVehicleDrawn).iconAngle = ((Command) designator).iconAngle;
    ((Command) actionVehicleDrawn).iconOffset = ((Command) designator).iconOffset;
    ((Gizmo) actionVehicleDrawn).Order = 10f;
    actionVehicleDrawn.buildDef = vehicleBuildDef;
    actionVehicleDrawn.SetColorOverride(((Command) designator).IconDrawColor);
    designator.SetStuffDef(stuffDefRaw);
    if (stuff != null)
      ((Command) actionVehicleDrawn).defaultIconColor = buildable.GetColorForStuff(stuff);
    else
      ((Command) actionVehicleDrawn).defaultIconColor = buildable.uiIconColor;
    ((Command) actionVehicleDrawn).hotKey = KeyBindingDefOf.Misc11;
    __result = (Command) actionVehicleDrawn;
    return false;
  }

  private static bool VehicleInfoCardOverride(
    Rect inRect,
    Dialog_InfoCard __instance,
    Thing ___thing,
    ThingDef ___def,
    Dialog_InfoCard.InfoCardTab ___tab)
  {
    if (___def is VehicleBuildDef vehicleBuildDef)
    {
      VehicleInfoCard.DrawFor(inRect, vehicleBuildDef.thingToSpawn, __instance, ___tab);
      return false;
    }
    switch (___thing)
    {
      case VehicleBuilding vehicleBuilding:
        VehicleInfoCard.DrawFor(inRect, vehicleBuilding.VehicleDef, __instance, ___tab);
        return false;
      case VehiclePawn vehicle:
        VehicleInfoCard.DrawFor(inRect, vehicle, __instance, ___tab);
        return false;
      default:
        return true;
    }
  }

  private static IEnumerable<Gizmo> ThingTransferToVehicleGizmo(
    IEnumerable<Gizmo> __result,
    Thing __instance)
  {
    foreach (Gizmo gizmo in __result)
      yield return gizmo;
    if (__instance.Spawned && !GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) __instance.Map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants) && __instance.CanBeHauledToVehicle())
    {
      if (__instance.IsOrderedToBeTransferredToAnyVehicle())
        yield return (Gizmo) Command_TransferToVehicle_Cancel.Command;
      else
        yield return (Gizmo) Command_TransferToVehicle_Order.Command;
    }
  }
}
