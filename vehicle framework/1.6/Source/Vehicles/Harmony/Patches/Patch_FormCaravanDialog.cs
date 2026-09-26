// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_FormCaravanDialog
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
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal class Patch_FormCaravanDialog : IPatchCategory
{
  private const int TabVehicles = 10;
  private const string VehiclesTabLabelKey = "VF_Vehicles";
  private const string PawnsTabLabelKey = "PawnsTab";
  private const string ItemsTabLabelKey = "ItemsTab";
  private const string TravelSuppliesTabLabelKey = "TravelSupplies";
  private static readonly string[] TabKeys = new string[3]
  {
    "PawnsTab",
    "ItemsTab",
    "TravelSupplies"
  };
  private static readonly System.Type FormCaravanTabEnumType = GenTypes.GetTypeInAnyAssembly("Dialog_FormCaravan+Tab", "RimWorld");
  private static readonly System.Type SplitCaravanTabEnumType = GenTypes.GetTypeInAnyAssembly("Dialog_SplitCaravan+Tab", "RimWorld");
  private static readonly MethodInfo IgnoreInventoryModeProp = AccessTools.PropertyGetter(typeof (Dialog_FormCaravan), "IgnoreInventoryMode");
  private static System.Type displayClassType;
  private static System.Type gizmoStateMachineType;
  private static TransferableVehicleWidget vehiclesTransfer;
  private static int selectedTab;

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  private static bool HasVehiclesAvailable(Dialog_FormCaravan formCaravan)
  {
    foreach (TransferableOneWay transferable in formCaravan.transferables)
    {
      if (transferable != null && ((Transferable) transferable).AnyThing is VehiclePawn)
        return true;
    }
    return false;
  }

  private static bool VehiclesSelected(List<TransferableOneWay> transferables)
  {
    foreach (TransferableOneWay transferable in transferables)
    {
      if (transferable != null && ((Transferable) transferable).AnyThing is VehiclePawn && ((Transferable) transferable).CountToTransfer > 0)
        return true;
    }
    return false;
  }

  private static GlobalObjectPool.CollectionReceipt<List<VehiclePawn>, VehiclePawn> GetVehiclesToTransfer(
    List<TransferableOneWay> transferables,
    out List<VehiclePawn> vehicles)
  {
    GlobalObjectPool.CollectionReceipt<List<VehiclePawn>, VehiclePawn> vehiclesToTransfer = GlobalObjectPool.Get<VehiclePawn>(out vehicles);
    foreach (TransferableOneWay transferable in transferables)
    {
      if (transferable != null && ((Transferable) transferable).AnyThing is VehiclePawn && ((Transferable) transferable).CountToTransfer > 0)
        vehicles.Add(((Transferable) transferable).AnyThing as VehiclePawn);
    }
    return vehiclesToTransfer;
  }

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TransferableUIUtility), "DoCountAdjustInterfaceInternal", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "CanAdjustPawnTransferable", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TilesPerDayCalculator), "ApproxTilesPerDay", new System.Type[2]
    {
      typeof (Caravan),
      typeof (StringBuilder)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "ApproxTilesForVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (TilesPerDayCalculator), "ApproxTilesPerDay", new System.Type[7]
    {
      typeof (List<TransferableOneWay>),
      typeof (float),
      typeof (float),
      typeof (PlanetTile),
      typeof (PlanetTile),
      typeof (bool),
      typeof (StringBuilder)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "ApproxTilesForVehicleTransferables", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanUIUtility), "CreateCaravanTransferableWidgets", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "CreateTransferableVehicleWidget", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CaravanFormingUtility), "AllSendablePawns", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "AllSendablePawnsInVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_FormCaravan), "PostOpen", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "FormCaravanPostOpenTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_FormCaravan), "PostClose", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "FormCaravanPostClose", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Dialog_FormCaravan), "DaysWorthOfFood"), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "DaysOfWorthOfFoodWithVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Dialog_FormCaravan), "TicksToArrive"), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "TicksToArriveWithVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_FormCaravan), "DoBottomButtons", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "StartRoutePlanningForVehiclesTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_FormCaravan), "TrySend", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "TryAndSendWithVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_FormCaravan), "DebugTryFormCaravanInstantly", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "TryFormCaravanInstantly", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldGizmoUtility), "TryGetCaravanGizmo", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "TryGetCaravanForVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (FormCaravanComp), "CanReformNow", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "ReformWithVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_SplitCaravan), "PostOpen", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "SplitCaravanPostOpen", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Dialog_SplitCaravan), "DestDaysWorthOfFood"), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "SplitDaysOfWorthOfFoodWithVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Dialog_SplitCaravan), "TicksToArrive"), new HarmonyMethod(typeof (Patch_FormCaravanDialog), "SplitTicksToArriveWithVehicles", (System.Type[]) null));
    if (!Ext_Mods.HasActiveMod("Kopp.CaravanItemSelectionEnhanced"))
    {
      HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_FormCaravan), "DoWindowContents", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "FormCaravanTabsTranspiler", (System.Type[]) null));
      HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Dialog_SplitCaravan), "DoWindowContents", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "SplitCaravanTabsTranspiler", (System.Type[]) null));
    }
    System.Type[] nestedTypes = typeof (FormCaravanComp).GetNestedTypes(AccessTools.all);
    Patch_FormCaravanDialog.displayClassType = ((IEnumerable<System.Type>) nestedTypes).FirstOrDefault<System.Type>((Func<System.Type, bool>) (type => type.Name == "<>c__DisplayClass18_0"));
    Patch_FormCaravanDialog.gizmoStateMachineType = ((IEnumerable<System.Type>) nestedTypes).FirstOrDefault<System.Type>((Func<System.Type, bool>) (type => type.Name == "<GetGizmos>d__18"));
    HarmonyPatcher.Patch((MethodBase) GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(Patch_FormCaravanDialog.gizmoStateMachineType), (Predicate<MethodInfo>) (method => method.Name == "MoveNext")), transpiler: new HarmonyMethod(typeof (Patch_FormCaravanDialog), "ReformCaravanWithVehiclesGizmoTranspiler", (System.Type[]) null));
  }

  private static void CanAdjustPawnTransferable(Transferable trad, ref bool readOnly)
  {
    if (!(trad.AnyThing is Pawn anyThing))
      return;
    readOnly = CaravanHelper.assignedSeats.IsAssigned(anyThing) || anyThing.InVehicle();
  }

  private static bool ApproxTilesForVehicles(
    ref float __result,
    Caravan caravan,
    StringBuilder explanation = null)
  {
    if (!(caravan is VehicleCaravan caravan1))
      return true;
    __result = VehicleCaravanTicksPerMoveUtility.ApproxTilesPerDay(caravan1, explanation);
    return false;
  }

  private static bool ApproxTilesForVehicleTransferables(
    ref float __result,
    List<TransferableOneWay> transferables,
    float massUsage,
    float massCapacity,
    PlanetTile tile,
    PlanetTile nextTile,
    StringBuilder explanation = null)
  {
    if (!Patch_FormCaravanDialog.VehiclesSelected(transferables))
      return true;
    List<Pawn> list;
    using (GlobalObjectPool.Get<Pawn>(out list))
    {
      foreach (TransferableOneWay transferable in transferables)
      {
        if (transferable != null && ((Transferable) transferable).AnyThing is Pawn && ((Transferable) transferable).CountToTransfer > 0)
        {
          Pawn anyThing = ((Transferable) transferable).AnyThing as Pawn;
          if (anyThing is VehiclePawn || !CaravanHelper.assignedSeats.IsAssigned(anyThing))
            list.Add(anyThing);
        }
      }
      StringBuilder explanation1 = explanation != null ? new StringBuilder() : (StringBuilder) null;
      int ticksPerMove = VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(list, massUsage, massCapacity, explanation1);
      __result = VehicleCaravanTicksPerMoveUtility.ApproxTilesPerDay(list.UniqueVehicleDefsInList(), ticksPerMove, tile, nextTile, explanation, explanation1?.ToString());
      return false;
    }
  }

  private static void CreateTransferableVehicleWidget(
    List<TransferableOneWay> transferables,
    PlanetTile tile)
  {
    List<TransferableOneWay> vehicles = new List<TransferableOneWay>();
    List<TransferableOneWay> pawns = new List<TransferableOneWay>();
    foreach (TransferableOneWay transferable in transferables)
    {
      Thing anyThing = ((Transferable) transferable).AnyThing;
      if (!(anyThing is VehiclePawn))
      {
        if (anyThing is Pawn)
          pawns.Add(transferable);
      }
      else
        vehicles.Add(transferable);
    }
    Patch_FormCaravanDialog.vehiclesTransfer = new TransferableVehicleWidget(TaggedString.op_Implicit(Translator.Translate("VF_Vehicles")), vehicles, pawns, tile);
  }

  private static List<Pawn> AllSendablePawnsInVehicles(List<Pawn> __result, Map map)
  {
    foreach (VehiclePawn allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
    {
      if (allClaimant.AllPawnsAboard.Count > 0)
        __result.AddRange((IEnumerable<Pawn>) allClaimant.AllPawnsAboard);
    }
    return __result;
  }

  private static IEnumerable<CodeInstruction> FormCaravanPostOpenTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
    yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
    yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_FormCaravan), "map"));
    yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
    yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_FormCaravan), "tabsList"));
    yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
    yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_FormCaravan), "thisWindowInstanceEverOpened"));
    yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "CreateTabListPostOpen", (System.Type[]) null, (System.Type[]) null));
    MethodInfo worldRoutePlannerMethod = AccessTools.Method(typeof (WorldRoutePlanner), "Start", new System.Type[1]
    {
      typeof (Dialog_FormCaravan)
    }, (System.Type[]) null);
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (!instructionList.OutOfBounds<CodeInstruction>(i + 2) && CodeInstructionExtensions.Calls(instructionList[i + 2], worldRoutePlannerMethod))
      {
        i += 3;
        instruction = instructionList[i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "SetInitialTab", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static void CreateTabListPostOpen(
    Dialog_FormCaravan formCaravan,
    Map map,
    List<TabRecord> tabsList,
    bool thisWindowInstanceEverOpened)
  {
    if (thisWindowInstanceEverOpened)
      return;
    CaravanFormation.formation = new FormationInfo(formCaravan, map);
    tabsList.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate("VF_Vehicles")), (Action) (() => Patch_FormCaravanDialog.selectedTab = 10), (Func<bool>) (() => Patch_FormCaravanDialog.selectedTab == 10)));
    foreach (int num in Enum.GetValues(Patch_FormCaravanDialog.FormCaravanTabEnumType))
    {
      int value = num;
      string str = !((IList<string>) Patch_FormCaravanDialog.TabKeys).OutOfBounds<string>(value) ? Patch_FormCaravanDialog.TabKeys[value] : "Missing Label";
      tabsList.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate(str)), (Action) (() => Patch_FormCaravanDialog.selectedTab = value), (Func<bool>) (() => Patch_FormCaravanDialog.selectedTab == value)));
    }
  }

  private static void SetInitialTab(Dialog_FormCaravan formCaravan)
  {
    Patch_FormCaravanDialog.selectedTab = Patch_FormCaravanDialog.HasVehiclesAvailable(formCaravan) ? 10 : 0;
  }

  private static void FormCaravanPostClose(List<TabRecord> ___tabsList, bool ___choosingRoute)
  {
    if (___choosingRoute)
      return;
    CaravanFormation.formation = (FormationInfo) null;
    ___tabsList.Clear();
    Patch_FormCaravanDialog.selectedTab = 10;
    CaravanHelper.assignedSeats.Clear();
  }

  private static bool DaysOfWorthOfFoodWithVehicles(
    Dialog_FormCaravan __instance,
    ref (float days, float tillRot) ___cachedDaysWorthOfFood,
    ref bool ___daysWorthOfFoodDirty,
    PlanetTile ___destinationTile)
  {
    List<VehiclePawn> vehicles;
    using (Patch_FormCaravanDialog.GetVehiclesToTransfer(__instance.transferables, out vehicles))
    {
      if (!___daysWorthOfFoodDirty || vehicles.Count <= 0)
        return true;
      ___daysWorthOfFoodDirty = false;
      IgnorePawnsInventoryMode pawnsInventoryMode = (IgnorePawnsInventoryMode) Patch_FormCaravanDialog.IgnoreInventoryModeProp.Invoke((object) __instance, (object[]) null);
      float num1;
      float num2;
      if (((PlanetTile) ref ___destinationTile).Valid)
      {
        using (WorldPath path = Find.World.GetComponent<WorldVehiclePathfinder>().FindPath(__instance.CurrentTile, ___destinationTile, vehicles))
        {
          int ticksPerMove = VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(new VehicleCaravanInfo(__instance));
          num1 = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(__instance.transferables, __instance.CurrentTile, pawnsInventoryMode, Faction.OfPlayer, path, 0.0f, ticksPerMove);
          num2 = DaysUntilRotCalculator.ApproxDaysUntilRot(__instance.transferables, __instance.CurrentTile, pawnsInventoryMode, path, 0.0f, ticksPerMove);
        }
      }
      else
      {
        num1 = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(__instance.transferables, __instance.CurrentTile, pawnsInventoryMode, Faction.OfPlayer, (WorldPath) null, 0.0f, 3300);
        num2 = DaysUntilRotCalculator.ApproxDaysUntilRot(__instance.transferables, __instance.CurrentTile, pawnsInventoryMode, (WorldPath) null, 0.0f, 3300);
      }
      ___cachedDaysWorthOfFood = (num1, num2);
      return false;
    }
  }

  private static bool TicksToArriveWithVehicles(
    Dialog_FormCaravan __instance,
    ref int ___cachedTicksToArrive,
    ref bool ___ticksToArriveDirty,
    PlanetTile ___destinationTile)
  {
    if (!((PlanetTile) ref ___destinationTile).Valid)
      return true;
    List<VehiclePawn> vehicles;
    using (Patch_FormCaravanDialog.GetVehiclesToTransfer(__instance.transferables, out vehicles))
    {
      if (!___ticksToArriveDirty || vehicles.Count <= 0)
        return true;
      ___ticksToArriveDirty = false;
      using (WorldPath path = Find.World.GetComponent<WorldVehiclePathfinder>().FindPath(__instance.CurrentTile, ___destinationTile, vehicles))
      {
        VehicleCaravanInfo caravanInfo = new VehicleCaravanInfo(__instance);
        int ticksPerMove = VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(caravanInfo);
        ___cachedTicksToArrive = VehicleCaravanPathingHelper.EstimatedTicksToArrive(caravanInfo.vehiclesAndDismountedPawns.UniqueVehicleDefsInList(), __instance.CurrentTile, in ___destinationTile, path, 0.0f, ticksPerMove, Find.TickManager.TicksAbs);
        return false;
      }
    }
  }

  private static IEnumerable<CodeInstruction> FormCaravanTabsTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    FieldInfo tabListField = AccessTools.Field(typeof (Dialog_FormCaravan), "tabsList");
    FieldInfo tabField = AccessTools.Field(typeof (Dialog_FormCaravan), "tab");
    MethodInfo clearTabList = AccessTools.Method(typeof (List<TabRecord>), "Clear", (System.Type[]) null, (System.Type[]) null);
    bool tabClearing = false;
    bool switchBlockClearing = false;
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.LoadsField(instruction, tabListField, false) && CodeInstructionExtensions.Calls(instructionList[i + 1], clearTabList))
      {
        if (!tabClearing)
        {
          tabClearing = true;
        }
        else
        {
          tabClearing = false;
          instruction = instructionList[++i];
          instruction = instructionList[++i];
          yield return new CodeInstruction(OpCodes.Ldarga_S, (object) 1);
          yield return new CodeInstruction(OpCodes.Ldsfld, (object) tabListField);
          yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "DrawTabList", (System.Type[]) null, (System.Type[]) null));
        }
      }
      else if (!tabClearing && !switchBlockClearing && CodeInstructionExtensions.LoadsField(instruction, tabField, false))
      {
        switchBlockClearing = true;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) 3);
        yield return new CodeInstruction(OpCodes.Ldloca_S, (object) 4);
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_FormCaravan), "pawnsTransfer"));
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_FormCaravan), "itemsTransfer"));
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_FormCaravan), "travelSuppliesTransfer"));
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "DrawActiveTab", (System.Type[]) null, (System.Type[]) null));
      }
      if (switchBlockClearing && instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i].operand is LocalBuilder operand && operand.LocalIndex == 4)
        switchBlockClearing = false;
      if (!tabClearing && !switchBlockClearing)
        yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static void DrawTabList(ref Rect inRect, List<TabRecord> tabsList)
  {
    if (Ext_Mods.HasActiveMod("Kopp.CaravanItemSelectionEnhanced"))
      return;
    ref Rect local = ref inRect;
    ((Rect) ref local).yMin = ((Rect) ref local).yMin + 119f;
    Widgets.DrawMenuSection(inRect);
    TabDrawer.DrawTabs<TabRecord>(inRect, tabsList, 200f);
  }

  private static void DrawActiveTab(
    Dialog_FormCaravan __instance,
    Rect transferablesRect,
    out bool anythingChanged,
    TransferableOneWayWidget pawnsTransfer,
    TransferableOneWayWidget itemsTransfer,
    TransferableOneWayWidget travelSuppliesTransfer)
  {
    anythingChanged = false;
    if (Ext_Mods.HasActiveMod("Kopp.CaravanItemSelectionEnhanced"))
      return;
    switch (Patch_FormCaravanDialog.selectedTab)
    {
      case 0:
        pawnsTransfer.OnGUI(transferablesRect, ref anythingChanged);
        break;
      case 1:
        itemsTransfer.OnGUI(transferablesRect, ref anythingChanged);
        break;
      case 2:
        travelSuppliesTransfer.extraHeaderSpace = 35f;
        travelSuppliesTransfer.OnGUI(transferablesRect, ref anythingChanged);
        __instance?.DrawAutoSelectCheckbox(transferablesRect, ref anythingChanged);
        break;
      case 10:
        Patch_FormCaravanDialog.vehiclesTransfer.OnGUI(transferablesRect);
        break;
      default:
        Log.Error($"Unknown enum type {Patch_FormCaravanDialog.selectedTab} for patched FormCaravan dialog. Switching back to known tab");
        Patch_FormCaravanDialog.selectedTab = 0;
        break;
    }
  }

  private static IEnumerable<CodeInstruction> StartRoutePlanningForVehiclesTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo startPlanningMethod = AccessTools.Method(typeof (WorldRoutePlanner), "Start", new System.Type[1]
    {
      typeof (Dialog_FormCaravan)
    }, (System.Type[]) null);
    FieldInfo mapField = AccessTools.Field(typeof (Dialog_FormCaravan), "map");
    FieldInfo autoSelectTravelSuppliesField = AccessTools.Field(typeof (Dialog_FormCaravan), "autoSelectTravelSupplies");
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, startPlanningMethod))
      {
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) mapField);
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) autoSelectTravelSuppliesField);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "WorldRoutePannerReroute", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static void WorldRoutePannerReroute(
    WorldRoutePlanner routePlanner,
    Dialog_FormCaravan formCaravan,
    Map map,
    bool autoSelectTravelSupplies)
  {
    if (Patch_FormCaravanDialog.VehiclesSelected(formCaravan.transferables))
    {
      CaravanFormation.formation.ChoosingRoute = true;
      Find.WindowStack.TryRemove((Window) formCaravan, false);
      VehicleCaravanInfo caravanInfo = new VehicleCaravanInfo(formCaravan.transferables, formCaravan.MassUsage, formCaravan.MassCapacity, formCaravan.CurrentTile)
      {
        caravaning = true
      };
      Find.World.GetComponent<VehicleRoutePlanner>().Start(caravanInfo, (Action) (() =>
      {
        Find.WindowStack.Add((Window) formCaravan);
        formCaravan.Notify_NoLongerChoosingRoute();
      }), new Action<PlanetTile>(ChoseVehicleRoute));
    }
    else
      routePlanner.Start(formCaravan);

    void ChoseVehicleRoute(PlanetTile tile)
    {
      CaravanFormation.formation.DestinationTile = tile;
      List<VehicleDef> vehicleDefs = TransferableUtility.GetPawnsFromTransferables(formCaravan.transferables).UniqueVehicleDefsInList();
      CaravanFormation.formation.StartingTile = CaravanHelper.BestExitTileToGoTo(vehicleDefs, tile, map);
      CaravanFormation.formation.TicksToArriveDirty = true;
      CaravanFormation.formation.DaysWorthOfFoodDirty = true;
      SoundStarter.PlayOneShotOnCamera(((Window) formCaravan).soundAppear, (Map) null);
      if (!autoSelectTravelSupplies)
        return;
      CaravanFormation.formation.SelectApproximateBestTravelSupplies();
    }
  }

  private static bool TryAndSendWithVehicles(Dialog_FormCaravan __instance)
  {
    if (CaravanFormation.formation.Reform && CaravanFormation.TryShowConfirmLeaveVehiclesDialog(__instance))
      return false;
    if (!Patch_FormCaravanDialog.VehiclesSelected(__instance.transferables))
      return true;
    CaravanFormation.TrySendVehicleCaravan(__instance);
    return false;
  }

  private static bool TryFormCaravanInstantly(
    Dialog_FormCaravan __instance,
    Map ___map,
    PlanetTile ___startingTile,
    PlanetTile ___destinationTile)
  {
    if (CaravanFormation.formation == null)
      return true;
    CaravanFormation.formation.RecacheTransferables();
    if (GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) CaravanFormation.formation.vehicles))
      return true;
    if (CaravanFormation.formation.vehicles.Exists((Predicate<VehiclePawn>) (vehicle => !Find.World.GetComponent<WorldVehiclePathGrid>().PassableFast(___map.Tile, vehicle.VehicleDef))))
    {
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageNoValidExitTile")), MessageTypeDefOf.RejectInput, false);
      return false;
    }
    if (!GenCollection.Any<Pawn>(CaravanFormation.formation.AllPawnsAndVehicles, (Predicate<Pawn>) (pawn => CaravanUtility.IsOwner(pawn, Faction.OfPlayer))))
    {
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("CaravanMustHaveAtLeastOneColonist")), MessageTypeDefOf.RejectInput, false);
      return false;
    }
    CaravanHelper.BoardAllAssignedPawns();
    CaravanFormation.formation.AddItemsFromTransferablesToRandomInventories(CaravanFormation.formation.AllPawnsAndVehicles);
    PlanetTile directionTile = ___startingTile;
    if (!((PlanetTile) ref directionTile).Valid)
      directionTile = CaravanExitMapUtility.RandomBestExitTileFrom(___map);
    if (!((PlanetTile) ref directionTile).Valid)
      directionTile = __instance.CurrentTile;
    CaravanHelper.ExitMapAndCreateVehicleCaravan((IEnumerable<Pawn>) CaravanFormation.formation.AllPawnsAndVehicles, Faction.OfPlayer, __instance.CurrentTile, directionTile, ___destinationTile);
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
    ((Window) __instance).Close(false);
    return false;
  }

  private static IEnumerable<CodeInstruction> TryGetCaravanForVehicles(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    FieldInfo mapPawnsField = AccessTools.Field(typeof (Map), "mapPawns");
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.LoadsField(instruction, mapPawnsField, false))
      {
        instruction = instructionList[++i];
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "PawnsOrAutonomousVehicles", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static int PawnsOrAutonomousVehicles(Map map)
  {
    int colonistCount = map.mapPawns.ColonistCount;
    if (colonistCount > 0)
      return colonistCount;
    foreach (VehiclePawn allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
    {
      if (allClaimant.MovementPermissions == VehiclePermissions.Autonomous)
        ++colonistCount;
      colonistCount += allClaimant.AllPawnsAboard.Count;
    }
    return colonistCount;
  }

  private static bool ReformWithVehicles(
    out bool __result,
    FormCaravanComp __instance,
    WorldObject ___parent)
  {
    __result = false;
    if (!(___parent is MapParent mapParent) || !mapParent.HasMap || !__instance.Reform)
      return false;
    if (__instance.CanFormOrReformCaravanNow)
    {
      __result = true;
      return false;
    }
    foreach (VehiclePawn allClaimant in mapParent.Map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
    {
      if (allClaimant.AllPawnsAboard.Count > 0 || allClaimant.MovementPermissions == VehiclePermissions.Autonomous)
      {
        __result = true;
        break;
      }
    }
    return false;
  }

  private static void SplitCaravanPostOpen(
    Dialog_SplitCaravan __instance,
    List<TabRecord> ___tabsList,
    Caravan ___caravan)
  {
    Patch_FormCaravanDialog.selectedTab = 10;
    CaravanFormation.splitter = new SplitInfo(__instance, ___caravan);
    ___tabsList.Clear();
    ___tabsList.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate("VF_Vehicles")), (Action) (() => Patch_FormCaravanDialog.selectedTab = 10), (Func<bool>) (() => Patch_FormCaravanDialog.selectedTab == 10)));
    foreach (int num in Enum.GetValues(Patch_FormCaravanDialog.SplitCaravanTabEnumType))
    {
      int value = num;
      string str = !((IList<string>) Patch_FormCaravanDialog.TabKeys).OutOfBounds<string>(value) ? Patch_FormCaravanDialog.TabKeys[value] : "Missing Label";
      ___tabsList.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate(str)), (Action) (() => Patch_FormCaravanDialog.selectedTab = value), (Func<bool>) (() => Patch_FormCaravanDialog.selectedTab == value)));
    }
  }

  private static bool SplitDaysOfWorthOfFoodWithVehicles(
    List<TransferableOneWay> ___transferables,
    Caravan ___caravan,
    ref (float days, float tillRot) ___cachedDestDaysWorthOfFood,
    ref bool ___destDaysWorthOfFoodDirty)
  {
    if (!___destDaysWorthOfFoodDirty || !(___caravan is VehicleCaravan vehicleCaravan))
      return true;
    ___destDaysWorthOfFoodDirty = false;
    float num1;
    float num2;
    if (vehicleCaravan.vehiclePather.Moving)
    {
      num1 = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(___transferables, ((WorldObject) vehicleCaravan).Tile, (IgnorePawnsInventoryMode) 0, ((WorldObject) vehicleCaravan).Faction, vehicleCaravan.vehiclePather.curPath, vehicleCaravan.vehiclePather.nextTileCostLeft, vehicleCaravan.TicksPerMove);
      num2 = DaysUntilRotCalculator.ApproxDaysUntilRot(___transferables, ((WorldObject) vehicleCaravan).Tile, (IgnorePawnsInventoryMode) 0, vehicleCaravan.vehiclePather.curPath, vehicleCaravan.vehiclePather.nextTileCostLeft, vehicleCaravan.TicksPerMove);
    }
    else
    {
      num1 = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(___transferables, ((WorldObject) vehicleCaravan).Tile, (IgnorePawnsInventoryMode) 0, ((WorldObject) vehicleCaravan).Faction, (WorldPath) null, 0.0f, 3300);
      num2 = DaysUntilRotCalculator.ApproxDaysUntilRot(___transferables, ((WorldObject) vehicleCaravan).Tile, (IgnorePawnsInventoryMode) 0, (WorldPath) null, 0.0f, 3300);
    }
    ___cachedDestDaysWorthOfFood = (num1, num2);
    return false;
  }

  private static bool SplitTicksToArriveWithVehicles(
    Caravan ___caravan,
    ref int __result,
    ref int ___cachedTicksToArrive,
    ref bool ___ticksToArriveDirty)
  {
    if (!(___caravan is VehicleCaravan caravan))
      return true;
    if (!caravan.vehiclePather.Moving)
    {
      __result = 0;
      return false;
    }
    if (___ticksToArriveDirty)
    {
      ___ticksToArriveDirty = false;
      ___cachedTicksToArrive = VehicleCaravanPathingHelper.EstimatedTicksToArrive(caravan, false);
    }
    return false;
  }

  private static IEnumerable<CodeInstruction> SplitCaravanTabsTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    FieldInfo tabListField = AccessTools.Field(typeof (Dialog_SplitCaravan), "tabsList");
    FieldInfo tabField = AccessTools.Field(typeof (Dialog_SplitCaravan), "tab");
    MethodInfo clearTabList = AccessTools.Method(typeof (List<TabRecord>), "Clear", (System.Type[]) null, (System.Type[]) null);
    MethodInfo methodInfo1 = ((IEnumerable<MethodInfo>) typeof (TabDrawer).GetMethods(BindingFlags.Static | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (method => method.Name == "DrawTabs")).FirstOrDefault<MethodInfo>((Func<MethodInfo, bool>) (method => method.GetParameters().Length == 3));
    MethodInfo methodInfo2;
    if ((object) methodInfo1 == null)
      methodInfo2 = (MethodInfo) null;
    else
      methodInfo2 = methodInfo1.MakeGenericMethod(typeof (TabRecord));
    MethodInfo drawTabs = methodInfo2;
    bool tabClearing = false;
    bool switchBlockClearing = false;
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.LoadsField(instruction, tabListField, false) && CodeInstructionExtensions.Calls(instructionList[i + 1], clearTabList))
        tabClearing = true;
      else if (CodeInstructionExtensions.Calls(instruction, drawTabs))
      {
        tabClearing = false;
        instruction = instructionList[++i];
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarga_S, (object) 1);
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) tabListField);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "DrawTabList", (System.Type[]) null, (System.Type[]) null));
      }
      else if (!tabClearing && !switchBlockClearing && CodeInstructionExtensions.LoadsField(instruction, tabField, false))
      {
        switchBlockClearing = true;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Ldnull, (object) null);
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) 2);
        yield return new CodeInstruction(OpCodes.Ldloca_S, (object) 3);
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_SplitCaravan), "pawnsTransfer"));
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_SplitCaravan), "itemsTransfer"));
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (Dialog_SplitCaravan), "foodAndMedicineTransfer"));
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "DrawActiveTab", (System.Type[]) null, (System.Type[]) null));
      }
      if (switchBlockClearing && instructionList[i + 1].opcode == OpCodes.Ldloc_3)
      {
        switchBlockClearing = false;
        instruction = instructionList[++i];
      }
      if (!tabClearing && !switchBlockClearing)
        yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static IEnumerable<CodeInstruction> ReformCaravanWithVehiclesGizmoTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    FieldInfo mapPawnsField = AccessTools.Field(typeof (Map), "mapPawns");
    FieldInfo delContainer = AccessTools.Field(Patch_FormCaravanDialog.gizmoStateMachineType, "<>8__1");
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.LoadsField(instruction, mapPawnsField, false))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) delContainer);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(Patch_FormCaravanDialog.displayClassType, "mapParent"));
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_FormCaravanDialog), "AppendMapPawnsInVehicles", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static int AppendMapPawnsInVehicles(int count, MapParent mapParent)
  {
    if (count == 0)
      count = mapParent.Map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants.Sum<VehiclePawn>((Func<VehiclePawn, int>) (vehicle => vehicle.AllPawnsAboard.Count));
    return count;
  }
}
