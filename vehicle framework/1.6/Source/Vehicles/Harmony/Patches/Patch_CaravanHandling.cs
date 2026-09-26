// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_CaravanHandling
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Patching;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Vehicles.World;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

internal class Patch_CaravanHandling : IPatchCategory
{
  private static readonly List<Thing> TmpAerialVehicleThingsWillToBuy = new List<Thing>();
  private static readonly StaticFuncPtr<Pawn, Pawn, Caravan, bool> IsValidDoctorFor = new StaticFuncPtr<Pawn, Pawn, Caravan, bool>(AccessTools.Method(typeof (CaravanTendUtility), nameof (IsValidDoctorFor), (System.Type[]) null, (System.Type[]) null));
  private static readonly FastInvokeHandler GetUsableBeds = MethodInvoker.GetHandler(AccessTools.Method(typeof (Caravan_BedsTracker), nameof (GetUsableBeds), (System.Type[]) null, (System.Type[]) null), false);
  private static readonly FastInvokeHandler GetAndRemoveFirstAvailableBedFor = MethodInvoker.GetHandler(AccessTools.Method(typeof (Caravan_BedsTracker), nameof (GetAndRemoveFirstAvailableBedFor), (System.Type[]) null, (System.Type[]) null), false);

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanVisibilityCalculator), "Visibility", new System.Type[3]
    {
      typeof (List<Pawn>),
      typeof (bool),
      typeof (StringBuilder)
    }, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleVisibilityInCaravanTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MassUtility), "Capacity", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "CapacityOfVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MassUtility), "CanEverCarryAnything", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "CanCarryIfVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CollectionsMassCalculator), "Capacity", new System.Type[2]
    {
      typeof (List<ThingCount>),
      typeof (StringBuilder)
    }, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_CaravanHandling), "PawnCapacityInVehicleTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CollectionsMassCalculator), "MassUsage", new System.Type[4]
    {
      typeof (List<ThingCount>),
      typeof (IgnorePawnsInventoryMode),
      typeof (bool),
      typeof (bool)
    }, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_CaravanHandling), "IgnorePawnGearAndInventoryMassTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (InventoryCalculatorsUtility), "ShouldIgnoreInventoryOf", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "ShouldIgnoreInventoryPawnInVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MassUtility), "CanEverCarryAnything", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "CanCarryIfVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "FillTab", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "FillTabVehicleCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "DoPeopleAndAnimals", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "DoPeopleAnimalsAndVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Alert_CaravanIdle), "IdleCaravans"), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "IdleVehicleCaravans", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanEnterMapUtility), "Enter", new System.Type[6]
    {
      typeof (Caravan),
      typeof (Map),
      typeof (CaravanEnterMode),
      typeof (CaravanDropInventoryMode),
      typeof (bool),
      typeof (Predicate<IntVec3>)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "EnterMapVehiclesCatchAll1", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanEnterMapUtility), "Enter", new System.Type[5]
    {
      typeof (Caravan),
      typeof (Map),
      typeof (Func<Pawn, IntVec3>),
      typeof (CaravanDropInventoryMode),
      typeof (bool)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "EnterMapVehiclesCatchAll2", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan), "AllOwnersDowned"), new HarmonyMethod(typeof (Patch_CaravanHandling), "AllOwnersDownedVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan), "AllOwnersHaveMentalBreak"), new HarmonyMethod(typeof (Patch_CaravanHandling), "AllOwnersMentalBreakVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan), "NightResting"), new HarmonyMethod(typeof (Patch_CaravanHandling), "NoRestForVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan), "PawnsListForReading"), new HarmonyMethod(typeof (Patch_CaravanHandling), "AllPawnsAndVehiclePassengers", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan), "TicksPerMove"), new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleCaravanTicksPerMove", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan), "TicksPerMoveExplanation"), new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleCaravanTicksPerMoveExplanation", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ForagedFoodPerDayCalculator), "GetBaseForagedNutritionPerDay", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "GetBaseForagedNutritionPerDayInVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan), "ContainsPawn", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "ContainsPawnInVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan), "AddPawn", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "AddPawnInVehicleCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan), "Notify_PawnAdded", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "NotifyVehicleCaravanPawnAdded", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan), "Notify_PawnRemoved", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "NotifyVehicleCaravanPawnRemoved", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan), "IsOwner", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "IsOwnerOfVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan_PathFollower), "Moving"), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleCaravanMoving", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanTweenerUtility), "PatherTweenedPosRoot", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleCaravanTweenedPosRoot", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Caravan_PathFollower), "MovingNow"), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleCaravanMovingNow", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan_Tweener), "TweenerTickInterval", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleCaravanTweenerTick", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Caravan_BedsTracker), "RecalculateUsedBeds", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "RecalculateUsedBedsInVehicleCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnUtility), "GainComfortFromThingIfPossible", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "GainComfortFromVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (SettlementDefeatUtility), "CheckDefeated", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_CaravanHandling), "CheckDefeatedWithVehiclesTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Tale_DoublePawn), "Concerns", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "ConcernNullThing", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Settlement_TraderTracker), "ColonyThingsWillingToBuy", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "AerialVehicleInventoryItems", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Tradeable), "Interactive"), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "AerialVehicleSlaveTradeRoomCheck", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_Trade), "CountToTransferChanged", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "AerialVehicleCountPawnsToTransfer", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanInventoryUtility), "FindPawnToMoveInventoryTo", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "FindVehicleToMoveInventoryTo", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Property(typeof (WITab_Caravan_Health), "Pawns").GetGetMethod(true), new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleHealthTabPawns", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Property(typeof (WITab_Caravan_Social), "Pawns").GetGetMethod(true), new HarmonyMethod(typeof (Patch_CaravanHandling), "VehicleSocialTabPawns", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanTendUtility), "CheckTend", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "CheckTendInVehicleCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanNeedsTabUtility), "DoRows", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "NoVehiclesNeedNeeds", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanNeedsTabUtility), "GetSize", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "NoVehiclesNeedNeeds", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (BestCaravanPawnUtility), "FindBestNegotiator", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "FindBestNegotiatorInVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn), "PreTraded", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "RemoveSoldPawnFromVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Settlement_TraderTracker), "GiveSoldThingToPlayer", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_CaravanHandling), "GiveSoldThingToVehicleTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanUtility), "GetCaravan", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "GetParentCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanUtility), "RandomOwner", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "RandomVehicleOwner", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanMergeUtility), "MergeCaravans", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "MergeCaravansWithVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanMergeUtility), "MergeCommand", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "DisableMergeForAerialVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanMaker), "MakeCaravan", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_CaravanHandling), "MakeVehicleCaravan", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanArrivalAction_Trade), "CanTradeWith", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "NoTradingUndocked", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TradeDeal), "InSellablePosition", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_CaravanHandling), "NegotiatorInVehicle", (System.Type[]) null));
  }

  private static IEnumerable<CodeInstruction> VehicleVisibilityInCaravanTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo bodySizeProperty = AccessTools.PropertyGetter(typeof (Pawn), "BodySize");
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, bodySizeProperty))
      {
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_CaravanHandling), "VehicleVisibilityWeight", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static float VehicleVisibilityWeight(Pawn pawn)
  {
    if (pawn is VehiclePawn vehiclePawn)
      return Mathf.Clamp(vehiclePawn.VehicleDef.properties.visibilityWeight, 0.0f, 40f);
    return !pawn.InVehicle() && !CaravanHelper.assignedSeats.IsAssigned(pawn) ? pawn.BodySize : 0.0f;
  }

  private static bool CapacityOfVehicle(Pawn p, ref float __result, StringBuilder explanation = null)
  {
    if (!(p is VehiclePawn vehiclePawn))
      return true;
    __result = vehiclePawn.GetStatValue(VehicleStatDefOf.CargoCapacity);
    if (explanation != null)
    {
      if (explanation.Length > 0)
        explanation.AppendLine();
      explanation.Append($"  - {((Entity) vehiclePawn).LabelShortCap}: {GenText.ToStringMassOffset(__result)}");
    }
    return false;
  }

  private static bool CanCarryIfVehicle(Pawn p, out bool __result)
  {
    __result = p is VehiclePawn;
    return !__result;
  }

  private static IEnumerable<CodeInstruction> PawnCapacityInVehicleTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo capacityMethod = AccessTools.Method(typeof (MassUtility), "Capacity", (System.Type[]) null, (System.Type[]) null);
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(codeInstruction, capacityMethod))
      {
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_CaravanHandling), "PawnCapacityInVehicle", (System.Type[]) null, (System.Type[]) null));
        codeInstruction = instructionList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static IEnumerable<CodeInstruction> IgnorePawnGearAndInventoryMassTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo capacityMethod = AccessTools.Method(typeof (MassUtility), "GearAndInventoryMass", (System.Type[]) null, (System.Type[]) null);
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, capacityMethod))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) 4);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_CaravanHandling), "PawnMassUsageInVehicle", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static float PawnMassUsageInVehicle(float massUsage, Pawn pawn)
  {
    return pawn.InVehicle() || CaravanHelper.assignedSeats.IsAssigned(pawn) ? 0.0f : massUsage;
  }

  private static void ShouldIgnoreInventoryPawnInVehicle(ref bool __result, Pawn pawn)
  {
    if (!__result)
      return;
    __result = !pawn.InVehicle() && !CaravanHelper.assignedSeats.IsAssigned(pawn);
  }

  private static float PawnCapacityInVehicle(Pawn pawn, StringBuilder explanation)
  {
    return pawn.InVehicle() || CaravanHelper.assignedSeats.IsAssigned(pawn) ? 0.0f : MassUtility.Capacity(pawn, explanation);
  }

  private static bool FillTabVehicleCaravan(
    ITab_Pawn_FormingCaravan __instance,
    ref List<Thing> ___thingsToSelect,
    Vector2 ___size,
    ref float ___lastDrawnHeight,
    ref Vector2 ___scrollPosition,
    ref List<Thing> ___tmpSingleThing)
  {
    if (!(LordUtility.GetLord(Find.Selector.SingleSelectedThing as Pawn).LordJob is LordJob_FormAndSendVehicles))
      return true;
    ___thingsToSelect.Clear();
    Rect rect = GenUI.ContractedBy(new Rect(new Vector2(), ___size), 10f);
    ref Rect local = ref rect;
    ((Rect) ref local).yMin = ((Rect) ref local).yMin + 20f;
    Rect inRect;
    // ISSUE: explicit constructor call
    ((Rect) ref inRect).\u002Ector(0.0f, 0.0f, ((Rect) ref rect).width - 16f, Mathf.Max(___lastDrawnHeight, ((Rect) ref rect).height));
    Widgets.BeginScrollView(rect, ref ___scrollPosition, inRect, true);
    float num1 = 0.0f;
    string status = ((LordJob_FormAndSendCaravan) LordUtility.GetLord(Find.Selector.SingleSelectedThing as Pawn).LordJob).Status;
    Widgets.Label(new Rect(0.0f, num1, ((Rect) ref inRect).width, 100f), status);
    float num2 = num1 + 22f + 4f;
    object[] parameters = new object[2]
    {
      (object) inRect,
      (object) num2
    };
    AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "DoPeopleAndAnimals", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, parameters);
    float curY = (float) parameters[1] + 4f;
    CaravanHelper.DoItemsListForVehicle(inRect, ref curY, ref ___tmpSingleThing, __instance);
    ___lastDrawnHeight = curY;
    Widgets.EndScrollView();
    if (GenCollection.Any<Thing>(___thingsToSelect))
    {
      ITab_Pawn_FormingCaravan.SelectNow(___thingsToSelect);
      ___thingsToSelect.Clear();
    }
    return false;
  }

  private static bool DoPeopleAnimalsAndVehicle(
    Rect inRect,
    ref float curY,
    ITab_Pawn_FormingCaravan __instance,
    ref List<Thing> ___tmpPawns)
  {
    if (!(LordUtility.GetLord(Find.Selector.SingleSelectedThing as Pawn).LordJob is LordJob_FormAndSendVehicles))
      return true;
    Widgets.ListSeparator(ref curY, ((Rect) ref inRect).width, TaggedString.op_Implicit(Translator.Translate("CaravanMembers")));
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    int num4 = 0;
    int num5 = 0;
    int num6 = 0;
    int num7 = 0;
    int num8 = 0;
    Lord lord = LordUtility.GetLord(Find.Selector.SingleSelectedThing as Pawn);
    foreach (Pawn ownedPawn in lord.ownedPawns)
    {
      if (ownedPawn.IsFreeColonist)
      {
        ++num1;
        if (ownedPawn.InMentalState)
          ++num2;
      }
      if (ownedPawn is VehiclePawn vehiclePawn)
      {
        if (vehiclePawn.AllPawnsAboard.NotNullAndAny<Pawn>())
        {
          num1 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.IsFreeColonist)).Count;
          num2 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.IsFreeColonist && x.InMentalState)).Count;
          num3 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.IsPrisoner)).Count;
          num4 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.IsPrisoner && x.InMentalState)).Count;
          num5 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.RaceProps.Animal)).Count;
          num6 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.RaceProps.Animal && x.InMentalState)).Count;
          num7 += vehiclePawn.AllPawnsAboard.FindAll((Predicate<Pawn>) (x => x.RaceProps.Animal && x.RaceProps.packAnimal)).Count;
        }
        if (!vehiclePawn.beached)
          ++num8;
      }
      else if (ownedPawn.IsPrisoner)
      {
        ++num3;
        if (ownedPawn.InMentalState)
          ++num4;
      }
      else if (ownedPawn.RaceProps.Animal)
      {
        ++num5;
        if (ownedPawn.InMentalState)
          ++num6;
        if (ownedPawn.RaceProps.packAnimal)
          ++num7;
      }
    }
    MethodInfo methodInfo1 = AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "GetPawnsCountLabel", (System.Type[]) null, (System.Type[]) null);
    string str1 = (string) methodInfo1.Invoke((object) __instance, new object[3]
    {
      (object) num1,
      (object) num2,
      (object) -1
    });
    string str2 = (string) methodInfo1.Invoke((object) __instance, new object[3]
    {
      (object) num3,
      (object) num4,
      (object) -1
    });
    string str3 = (string) methodInfo1.Invoke((object) __instance, new object[3]
    {
      (object) num5,
      (object) num6,
      (object) num7
    });
    string str4 = (string) methodInfo1.Invoke((object) __instance, new object[3]
    {
      (object) num8,
      (object) -1,
      (object) -1
    });
    MethodInfo methodInfo2 = AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "DoPeopleAndAnimalsEntry", (System.Type[]) null, (System.Type[]) null);
    float num9 = curY;
    object[] parameters1 = new object[5]
    {
      (object) inRect,
      (object) GenText.CapitalizeFirst(Faction.OfPlayer.def.pawnsPlural),
      (object) str1,
      (object) curY,
      null
    };
    methodInfo2.Invoke((object) __instance, parameters1);
    curY = (float) parameters1[3];
    float num10 = (float) parameters1[4];
    float num11 = curY;
    object[] parameters2 = new object[5]
    {
      (object) inRect,
      (object) Gen.ToStringSafe<TaggedString>(Translator.Translate("VF_Vehicles")),
      (object) str4,
      (object) curY,
      null
    };
    methodInfo2.Invoke((object) __instance, parameters2);
    curY = (float) parameters2[3];
    float num12 = (float) parameters2[4];
    float num13 = curY;
    object[] parameters3 = new object[5]
    {
      (object) inRect,
      (object) Gen.ToStringSafe<TaggedString>(Translator.Translate("CaravanPrisoners")),
      (object) str2,
      (object) curY,
      null
    };
    methodInfo2.Invoke((object) __instance, parameters3);
    curY = (float) parameters3[3];
    float num14 = (float) parameters3[4];
    float num15 = curY;
    object[] parameters4 = new object[5]
    {
      (object) inRect,
      (object) Gen.ToStringSafe<TaggedString>(Translator.Translate("CaravanAnimals")),
      (object) str3,
      (object) curY,
      null
    };
    methodInfo2.Invoke((object) __instance, parameters4);
    curY = (float) parameters4[3];
    float num16 = (float) parameters4[4];
    float num17 = Mathf.Max(new float[4]
    {
      num10,
      num12,
      num14,
      num16
    }) + 2f;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, num9, num17, 22f);
    if (Mouse.IsOver(rect1))
    {
      Widgets.DrawHighlight(rect1);
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "HighlightColonists", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, (object[]) null);
    }
    if (Widgets.ButtonInvisible(rect1, false))
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "SelectColonistsLater", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, (object[]) null);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(0.0f, num11, num17, 22f);
    if (Mouse.IsOver(rect2))
    {
      Widgets.DrawHighlight(rect2);
      foreach (Pawn ownedPawn in lord.ownedPawns)
      {
        if (ownedPawn is VehiclePawn)
          TargetHighlighter.Highlight(GlobalTargetInfo.op_Implicit((Thing) ownedPawn), true, true, false);
      }
    }
    if (Widgets.ButtonInvisible(rect2, false))
    {
      ___tmpPawns.Clear();
      foreach (Pawn ownedPawn in lord.ownedPawns)
      {
        if (ownedPawn is VehiclePawn)
          ___tmpPawns.Add((Thing) ownedPawn);
      }
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "SelectLater", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, new object[1]
      {
        (object) ___tmpPawns
      });
      ___tmpPawns.Clear();
    }
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(0.0f, num13, num17, 22f);
    if (Mouse.IsOver(rect3))
    {
      Widgets.DrawHighlight(rect3);
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "HighlightPrisoners", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, (object[]) null);
    }
    if (Widgets.ButtonInvisible(rect3, false))
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "SelectPrisonersLater", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, (object[]) null);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(0.0f, num15, num17, 22f);
    if (Mouse.IsOver(rect4))
    {
      Widgets.DrawHighlight(rect4);
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "HighlightAnimals", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, (object[]) null);
    }
    if (Widgets.ButtonInvisible(rect4, false))
      AccessTools.Method(typeof (ITab_Pawn_FormingCaravan), "SelectAnimalsLater", (System.Type[]) null, (System.Type[]) null).Invoke((object) __instance, (object[]) null);
    return false;
  }

  private static void IdleVehicleCaravans(ref List<Caravan> __result)
  {
    if (GenList.NullOrEmpty<Caravan>((IList<Caravan>) __result))
      return;
    __result.RemoveAll((Predicate<Caravan>) (caravan => caravan is VehicleCaravan vehicleCaravan && vehicleCaravan.vehiclePather.MovingNow));
  }

  private static bool EnterMapVehiclesCatchAll1(
    Caravan caravan,
    Map map,
    CaravanEnterMode enterMode,
    CaravanDropInventoryMode dropInventoryMode = 0,
    bool draftColonists = false,
    Predicate<IntVec3> extraCellValidator = null)
  {
    if (!(caravan is VehicleCaravan caravan1))
      return true;
    EnterMapUtilityVehicles.SpawnParams spawnParams = new EnterMapUtilityVehicles.SpawnParams(enterMode)
    {
      dropInventoryMode = dropInventoryMode,
      draftColonists = draftColonists
    };
    EnterMapUtilityVehicles.EnterMap(caravan1, map, in spawnParams);
    return false;
  }

  private static bool EnterMapVehiclesCatchAll2(
    Caravan caravan,
    Map map,
    CaravanDropInventoryMode dropInventoryMode = 0,
    bool draftColonists = false)
  {
    if (!(caravan is VehicleCaravan caravan1))
      return true;
    EnterMapUtilityVehicles.SpawnParams spawnParams = new EnterMapUtilityVehicles.SpawnParams((CaravanEnterMode) 1)
    {
      dropInventoryMode = dropInventoryMode,
      draftColonists = draftColonists
    };
    EnterMapUtilityVehicles.EnterMap(caravan1, map, in spawnParams);
    return false;
  }

  private static bool AllOwnersDownedVehicle(Caravan __instance, ref bool __result)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return true;
    foreach (Pawn pawn1 in vehicleCaravan.pawns)
    {
      if (vehicleCaravan.IsOwner(pawn1) && !pawn1.Downed)
      {
        __result = false;
        return false;
      }
      if (pawn1 is VehiclePawn vehiclePawn)
      {
        foreach (Pawn pawn2 in vehiclePawn.AllPawnsAboard)
        {
          if (__instance.IsOwner(pawn2) && !pawn2.Downed)
          {
            __result = false;
            return false;
          }
        }
      }
    }
    __result = true;
    return false;
  }

  private static bool AllOwnersMentalBreakVehicle(Caravan __instance, ref bool __result)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return true;
    foreach (Pawn pawn1 in vehicleCaravan.pawns)
    {
      if (vehicleCaravan.IsOwner(pawn1) && !pawn1.InMentalState)
      {
        __result = false;
        return false;
      }
      if (pawn1 is VehiclePawn vehiclePawn)
      {
        foreach (Pawn pawn2 in vehiclePawn.AllPawnsAboard)
        {
          if (__instance.IsOwner(pawn2) && !pawn2.InMentalState)
          {
            __result = false;
            return false;
          }
        }
      }
    }
    __result = true;
    return false;
  }

  private static bool NoRestForVehicles(Caravan __instance, ref bool __result)
  {
    if (!(__instance is VehicleCaravan caravan))
      return true;
    __result = VehicleCaravanPathingHelper.ShouldRestAt(caravan, ((WorldObject) caravan).Tile);
    return false;
  }

  private static bool AllPawnsAndVehiclePassengers(ref List<Pawn> __result, Caravan __instance)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return true;
    __result = vehicleCaravan.AllPawnsAndVehiclePassengers;
    return false;
  }

  private static bool VehicleCaravanTicksPerMove(ref int __result, Caravan __instance)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return true;
    __result = vehicleCaravan.TicksPerMove;
    return false;
  }

  private static bool VehicleCaravanTicksPerMoveExplanation(ref string __result, Caravan __instance)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return true;
    __result = vehicleCaravan.TicksPerMoveExplanation;
    return false;
  }

  private static bool GetBaseForagedNutritionPerDayInVehicle(
    Pawn p,
    out bool skip,
    ref float __result)
  {
    skip = false;
    if (!p.InVehicle() && !CaravanHelper.assignedSeats.IsAssigned(p))
      return true;
    skip = true;
    __result = 0.0f;
    return false;
  }

  private static IEnumerable<CodeInstruction> CheckDefeatedWithVehiclesTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(codeInstruction, AccessTools.Property(typeof (MapPawns), "FreeColonists").GetGetMethod()))
      {
        yield return new CodeInstruction(OpCodes.Callvirt, (object) AccessTools.Property(typeof (MapPawns), "AllPawnsSpawned").GetGetMethod());
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (CaravanHelper), "GrabPawnsFromMapPawnsInVehicle", (System.Type[]) null, (System.Type[]) null));
        codeInstruction = instructionList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static bool ConcernNullThing(Thing th, Tale_DoublePawn __instance, ref bool __result)
  {
    if (th != null && __instance != null && __instance.secondPawnData != null && __instance.firstPawnData != null)
      return true;
    __result = false;
    return false;
  }

  private static bool AerialVehicleInventoryItems(
    Pawn playerNegotiator,
    ref IEnumerable<Thing> __result)
  {
    AerialVehicleInFlight aerialVehicle = playerNegotiator.GetAerialVehicle();
    if (aerialVehicle == null)
      return true;
    Patch_CaravanHandling.TmpAerialVehicleThingsWillToBuy.Clear();
    foreach (Thing thing in aerialVehicle.Vehicle.inventory.innerContainer)
      Patch_CaravanHandling.TmpAerialVehicleThingsWillToBuy.Add(thing);
    foreach (Pawn pawn in aerialVehicle.Vehicle.AllPawnsAboard)
    {
      if (!CaravanUtility.IsOwner(pawn, aerialVehicle.Faction))
        Patch_CaravanHandling.TmpAerialVehicleThingsWillToBuy.Add((Thing) pawn);
    }
    __result = (IEnumerable<Thing>) Patch_CaravanHandling.TmpAerialVehicleThingsWillToBuy;
    return false;
  }

  private static void AerialVehicleSlaveTradeRoomCheck(ref bool __result, Tradeable __instance)
  {
    if (!(((Transferable) __instance).AnyThing is Pawn anyThing) || !anyThing.RaceProps.Humanlike || ((Transferable) __instance).CountToTransfer != 0)
      return;
    AerialVehicleInFlight aerialVehicle = TradeSession.playerNegotiator.GetAerialVehicle();
    if (aerialVehicle == null)
      return;
    __result &= CaravanHelper.CanFitInVehicle(aerialVehicle);
  }

  private static void AerialVehicleCountPawnsToTransfer(List<Tradeable> ___cachedTradeables)
  {
    CaravanHelper.CountPawnsBeingTraded(___cachedTradeables);
  }

  private static bool FindVehicleToMoveInventoryTo(
    ref Pawn __result,
    Thing item,
    List<Pawn> candidates,
    List<Pawn> ignoreCandidates,
    Pawn currentItemOwner = null)
  {
    return !candidates.HasVehicle() || !GenCollection.TryRandomElement<Pawn>(candidates.Where<Pawn>((Func<Pawn, bool>) (pawn => pawn is VehiclePawn && (ignoreCandidates == null || !ignoreCandidates.Contains(pawn)) && currentItemOwner != pawn && !MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, item.stackCount))), ref __result);
  }

  private static bool VehicleHealthTabPawns(ref List<Pawn> __result)
  {
    if (!(Find.WorldSelector.SingleSelectedObject is Caravan singleSelectedObject) || !singleSelectedObject.HasVehicle())
      return true;
    List<Pawn> pawnList = new List<Pawn>();
    foreach (Pawn pawn in singleSelectedObject.PawnsListForReading)
    {
      if (!(pawn is VehiclePawn))
        pawnList.Add(pawn);
    }
    __result = pawnList;
    return false;
  }

  private static bool VehicleSocialTabPawns(ref List<Pawn> __result)
  {
    if (!(Find.WorldSelector.SingleSelectedObject is VehicleCaravan singleSelectedObject) || !singleSelectedObject.HasVehicle())
      return true;
    List<Pawn> pawnList = new List<Pawn>();
    foreach (Pawn pawn in singleSelectedObject.PawnsListForReading)
    {
      if (!(pawn is VehiclePawn))
        pawnList.Add(pawn);
    }
    __result = pawnList;
    return false;
  }

  private static bool CheckTendInVehicleCaravan(Caravan caravan, int delta)
  {
    if (!(caravan is VehicleCaravan vehicleCaravan))
      return true;
    foreach (Pawn vehiclePassenger in vehicleCaravan.AllPawnsAndVehiclePassengers)
    {
      if (Patch_CaravanHandling.IsValidDoctorFor.Invoke(vehiclePassenger, (Pawn) null, (Caravan) vehicleCaravan) && Gen.IsHashIntervalTick((Thing) vehiclePassenger, 1250, delta))
        CaravanTendUtility.TryTendToAnyPawn((Caravan) vehicleCaravan);
    }
    return false;
  }

  private static void NoVehiclesNeedNeeds(ref List<Pawn> pawns)
  {
    pawns.RemoveAll((Predicate<Pawn>) (pawn => pawn is VehiclePawn));
  }

  private static bool FindBestNegotiatorInVehicle(
    Caravan caravan,
    ref Pawn __result,
    Faction negotiatingWith = null,
    TraderKindDef trader = null)
  {
    if (!(caravan is VehicleCaravan caravan1))
      return true;
    __result = WorldHelper.FindBestNegotiator(caravan1, negotiatingWith, trader);
    return false;
  }

  private static void ContainsPawnInVehicle(Pawn p, Caravan __instance, ref bool __result)
  {
    if (__result)
      return;
    VehiclePawn vehicle = p.GetVehicle();
    if (vehicle == null)
      return;
    __result = __instance.ContainsPawn((Pawn) vehicle);
  }

  private static void AddPawnInVehicleCaravan(Caravan __instance, Pawn p)
  {
    if (__instance is VehicleCaravan vehicleCaravan)
    {
      vehicleCaravan.RecacheVehiclesOrConvertCaravan();
    }
    else
    {
      if (!(p is VehiclePawn))
        return;
      CaravanHelper.SwapToVehicleCaravan(__instance);
    }
  }

  private static void NotifyVehicleCaravanPawnAdded(Caravan __instance, Pawn p)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return;
    vehicleCaravan.Notify_PawnAdded(p);
  }

  private static void NotifyVehicleCaravanPawnRemoved(Caravan __instance, Pawn p)
  {
    if (!(__instance is VehicleCaravan vehicleCaravan))
      return;
    vehicleCaravan.Notify_PawnRemoved(p);
  }

  private static void IsOwnerOfVehicle(Pawn p, Caravan __instance, ref bool __result)
  {
    if (__result)
      return;
    VehiclePawn vehicle = p.GetVehicle();
    __result = vehicle != null && ((ThingOwner) __instance.pawns).Contains((Thing) vehicle) && CaravanUtility.IsOwner(p, ((WorldObject) __instance).Faction);
  }

  private static void VehicleCaravanMoving(ref bool __result, Caravan ___caravan)
  {
    if (!(___caravan is VehicleCaravan vehicleCaravan))
      return;
    __result = vehicleCaravan.vehiclePather.Moving;
  }

  private static bool VehicleCaravanTweenedPosRoot(Caravan caravan, ref Vector3 __result)
  {
    if (!(caravan is VehicleCaravan))
      return true;
    __result = Find.WorldGrid.GetTileCenter(((WorldObject) caravan).Tile);
    return false;
  }

  private static void VehicleCaravanMovingNow(ref bool __result, Caravan ___caravan)
  {
    if (!(___caravan is VehicleCaravan vehicleCaravan))
      return;
    __result = vehicleCaravan.vehiclePather.MovingNow;
  }

  private static bool VehicleCaravanTweenerTick(Caravan ___caravan)
  {
    if (!(___caravan is VehicleCaravan vehicleCaravan))
      return true;
    vehicleCaravan.vehicleTweener.TweenerTick();
    return false;
  }

  private static bool RecalculateUsedBedsInVehicleCaravan(
    Caravan_BedsTracker __instance,
    Dictionary<Pawn, Building_Bed> ___usedBeds)
  {
    if (!(__instance.caravan is VehicleCaravan caravan))
      return true;
    ___usedBeds.Clear();
    if (!((WorldObject) caravan).Spawned)
      return false;
    List<Building_Bed> list;
    using (GlobalObjectPool.Get<Building_Bed>(out list))
    {
      Patch_CaravanHandling.GetUsableBeds.Invoke((object) __instance, new object[1]
      {
        (object) list
      });
      if (!caravan.vehiclePather.MovingNow)
      {
        GenCollection.SortByDescending<Building_Bed, float>(list, (Func<Building_Bed, float>) (bed => StatExtension.GetStatValue((Thing) bed, StatDefOf.BedRestEffectiveness, true, -1)));
        foreach (Pawn pawn in caravan.DismountedPawnsListForReading)
          TryAssignUsableBed(pawn, __instance, ___usedBeds, list);
        foreach (VehiclePawn vehiclePawn in caravan.VehiclesListForReading)
        {
          foreach (Pawn pawn in vehiclePawn.AllPawnsAboard)
            TryAssignUsableBed(pawn, __instance, ___usedBeds, list);
        }
      }
      else
      {
        GenCollection.SortByDescending<Building_Bed, float>(list, (Func<Building_Bed, float>) (bed => StatExtension.GetStatValue((Thing) bed, StatDefOf.ImmunityGainSpeedFactor, true, -1)));
        foreach (Pawn pawn in caravan.DismountedPawnsListForReading)
          TryAssignToSickPawn(pawn, caravan, __instance, ___usedBeds, list);
        foreach (VehiclePawn vehiclePawn in caravan.VehiclesListForReading)
        {
          foreach (Pawn pawn in vehiclePawn.AllPawnsAboard)
            TryAssignToSickPawn(pawn, caravan, __instance, ___usedBeds, list);
        }
      }
      return false;
    }

    static void TryAssignUsableBed(
      Pawn pawn,
      Caravan_BedsTracker instance,
      Dictionary<Pawn, Building_Bed> usedBeds,
      List<Building_Bed> usableBeds)
    {
      if (pawn.needs?.rest == null)
        return;
      Building_Bed buildingBed = (Building_Bed) Patch_CaravanHandling.GetAndRemoveFirstAvailableBedFor.Invoke((object) instance, new object[2]
      {
        (object) pawn,
        (object) usableBeds
      });
      if (buildingBed == null)
        return;
      usedBeds.Add(pawn, buildingBed);
    }

    static void TryAssignToSickPawn(
      Pawn pawn,
      VehicleCaravan caravan,
      Caravan_BedsTracker instance,
      Dictionary<Pawn, Building_Bed> usedBeds,
      List<Building_Bed> usableBeds)
    {
      if (pawn.needs?.rest == null && !CaravanBedUtility.WouldBenefitFromRestingInBed(pawn) || caravan.vehiclePather.MovingNow && !CaravanCarryUtility.CarriedByCaravan(pawn))
        return;
      Building_Bed buildingBed = (Building_Bed) Patch_CaravanHandling.GetAndRemoveFirstAvailableBedFor.Invoke((object) pawn, new object[1]
      {
        (object) usableBeds
      });
      if (buildingBed == null)
        return;
      usedBeds.Add(pawn, buildingBed);
    }
  }

  private static bool GainComfortFromVehicle(Pawn p, Thing from, int delta)
  {
    if (!(from is VehiclePawn))
      return true;
    if (p.needs?.comfort == null || !Gen.IsHashIntervalTick((Thing) p, 15, delta))
      return false;
    if (((Thing) p).ParentHolder is VehicleRoleHandler parentHolder2)
    {
      float comfort = parentHolder2.role.Comfort;
      if ((double) comfort >= 0.0 && (double) comfort >= (double) ((Need) p.needs.comfort).CurInstantLevel)
        p.needs.comfort.ComfortUsed(comfort);
    }
    else if (0.15000000596046448 >= (double) ((Need) p.needs.comfort).CurInstantLevel && ((Thing) p).ParentHolder is Pawn_InventoryTracker parentHolder1 && parentHolder1.pawn is VehiclePawn)
      p.needs.comfort.ComfortUsed(0.15f);
    return false;
  }

  private static void RemoveSoldPawnFromVehicle(Pawn __instance, TradeAction action)
  {
    if (action == 1 || action != 2)
      return;
    __instance.GetVehicle()?.DisembarkPawn(__instance);
  }

  private static IEnumerable<CodeInstruction> GiveSoldThingToVehicleTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionList[i];
      if (codeInstruction.opcode == OpCodes.Ldnull && instructionList[i + 1].opcode == OpCodes.Ldnull)
      {
        yield return new CodeInstruction(OpCodes.Ldarg_3, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (CaravanUtility), "GetCaravan", (System.Type[]) null, (System.Type[]) null));
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Ext_Caravan), "GrabPawnsFromVehicleCaravanSilentFail", (System.Type[]) null, (System.Type[]) null));
        codeInstruction = instructionList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static void GetParentCaravan(Thing thing, ref Caravan __result)
  {
    if (__result != null || !(thing is Pawn pawn))
      return;
    __result = (Caravan) pawn.GetVehicleCaravan();
  }

  private static bool RandomVehicleOwner(Caravan caravan, ref Pawn __result)
  {
    if (!caravan.HasVehicle())
      return true;
    __result = GenCollection.RandomElement<Pawn>(caravan.GrabPawnsFromVehicleCaravanSilentFail().Where<Pawn>(new Func<Pawn, bool>(caravan.IsOwner)));
    return false;
  }

  private static void MergeCaravansWithVehicle(List<Caravan> caravans)
  {
    Caravan caravan = caravans.First<Caravan>(new Func<Caravan, bool>(NotDestroyed));
    if (!caravan.pawns.InnerListForReading.Exists(new Predicate<Pawn>(IsVehicle)) || caravan is VehicleCaravan)
      return;
    CaravanHelper.SwapToVehicleCaravan(caravan);

    static bool NotDestroyed(Caravan caravan) => !((WorldObject) caravan).Destroyed;

    static bool IsVehicle(Pawn pawn) => pawn is VehiclePawn;
  }

  private static void DisableMergeForAerialVehicles(ref Command __result, Caravan caravan)
  {
    if (__result == null || !(caravan is VehicleCaravan vehicleCaravan1))
      return;
    foreach (WorldObject selectedObject in Find.WorldSelector.SelectedObjects)
    {
      if (selectedObject is VehicleCaravan vehicleCaravan2 && (vehicleCaravan2.AerialVehicle || vehicleCaravan1.AerialVehicle))
        ((Gizmo) __result).Disable(TaggedString.op_Implicit(Translator.Translate("VF_CantMergeAerialVehicle")));
    }
  }

  private static bool MakeVehicleCaravan(
    ref Caravan __result,
    IEnumerable<Pawn> pawns,
    Faction faction,
    PlanetTile startingTile,
    bool addToWorldPawnsIfNotAlready)
  {
    if (!pawns.Any<Pawn>((Func<Pawn, bool>) (pawn => pawn is VehiclePawn)))
      return true;
    __result = (Caravan) CaravanHelper.MakeVehicleCaravan(pawns, faction, startingTile, addToWorldPawnsIfNotAlready);
    return false;
  }

  private static void NoTradingUndocked(Caravan caravan, ref FloatMenuAcceptanceReport __result)
  {
    if (!((FloatMenuAcceptanceReport) ref __result).Accepted || !caravan.HasBoat() || caravan.PawnsListForReading.NotNullAndAny<Pawn>((Predicate<Pawn>) (p => !((Thing) p).IsBoat())))
      return;
    __result = FloatMenuAcceptanceReport.op_Implicit(false);
  }

  private static void NegotiatorInVehicle(ref bool __result)
  {
    if (__result)
      return;
    ref bool local = ref __result;
    VehiclePawn vehicle = TradeSession.playerNegotiator.GetVehicle();
    int num = vehicle == null ? 0 : (vehicle.InVehicleCaravan() ? 1 : 0);
    local = num != 0;
  }
}
