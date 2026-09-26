// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Components
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_Components : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Pawn), "CanTakeOrder"), postfix: new HarmonyMethod(typeof (Patch_Components), "CanVehicleTakeOrder", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (FloatMenuUtility), "GetMeleeAttackAction", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Components), "NoMeleeForVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnComponentsUtility), "CreateInitialComponents", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Components), "CreateInitialVehicleComponents", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnComponentsUtility), "AddAndRemoveDynamicComponents", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Components), "AddAndRemoveVehicleComponents", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_MeleeVerbs), "ChooseMeleeVerb", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Components), "VehiclesDontMeleeThings", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_InventoryTracker), "Notify_ItemRemoved", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Components), "RemovePawnFromInventory", (System.Type[]) null));
  }

  private static void CanVehicleTakeOrder(Pawn __instance, ref bool __result)
  {
    if (__result)
      return;
    __result = __instance is VehiclePawn;
  }

  private static bool NoMeleeForVehicles(Pawn pawn, LocalTargetInfo target, out string failStr)
  {
    if (pawn is VehiclePawn)
    {
      failStr = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IsIncapableOfRamming", NamedArgument.op_Implicit(((Entity) ((LocalTargetInfo) ref target).Thing).LabelShort)));
      return false;
    }
    failStr = string.Empty;
    return true;
  }

  private static void CreateInitialVehicleComponents(Pawn pawn)
  {
    if (!(pawn is VehiclePawn vehicle) || vehicle.vehiclePather != null)
      return;
    vehicle.vehiclePather = new VehiclePathFollower(vehicle);
    vehicle.vehicleAI = new VehicleAI(vehicle);
    vehicle.statHandler = new VehicleStatHandler(vehicle);
    vehicle.sharedJob = new SharedJob();
    PatternData graphicData;
    if (!VehicleMod.settings.vehicles.defaultGraphics.TryGetValue(((Def) vehicle.VehicleDef).defName, out graphicData))
      graphicData = vehicle.VehicleDef.graphicData != null ? new PatternData(vehicle.VehicleDef.graphicData) : new PatternData(Color.white, Color.white, Color.white, PatternDefOf.Default, Vector2.zero, 0.0f);
    vehicle.patternData = new PatternData((GraphicDataRGB) graphicData);
    if (((Thing) vehicle).Stuff == null)
      return;
    ((Thing) vehicle).DrawColor = ((BuildableDef) vehicle.VehicleDef).GetColorForStuff(((Thing) vehicle).Stuff);
  }

  private static void AddAndRemoveVehicleComponents(Pawn pawn, bool actAsIfSpawned = false)
  {
    if (!(pawn is VehiclePawn vehiclePawn1) || !(((Thing) vehiclePawn1).Spawned | actAsIfSpawned))
      return;
    VehiclePawn vehiclePawn2 = vehiclePawn1;
    if (vehiclePawn2.drafter == null)
      vehiclePawn2.drafter = new Pawn_DraftController((Pawn) vehiclePawn1);
    VehiclePawn vehiclePawn3 = vehiclePawn1;
    if (vehiclePawn3.story == null)
      vehiclePawn3.story = new Pawn_StoryTracker((Pawn) vehiclePawn1);
    VehiclePawn vehiclePawn4 = vehiclePawn1;
    if (vehiclePawn4.playerSettings == null)
      vehiclePawn4.playerSettings = new Pawn_PlayerSettings((Pawn) vehiclePawn1);
    vehiclePawn1.trader = (Pawn_TraderTracker) null;
    vehiclePawn1.training = (Pawn_TrainingTracker) null;
  }

  private static bool VehiclesDontMeleeThings(Pawn ___pawn) => !(___pawn is VehiclePawn);

  private static void RemovePawnFromInventory(Pawn ___pawn, Thing item)
  {
    if (!(___pawn is VehiclePawn pawn) || !(item is Pawn))
      return;
    pawn.GetVehicleCaravan()?.RecacheVehiclesOrConvertCaravan();
  }
}
