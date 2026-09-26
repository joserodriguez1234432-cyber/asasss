// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_WorldHandling
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Patching;
using System.Reflection;
using Vehicles.World;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

internal class Patch_WorldHandling : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldPawns), "GetSituation", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "SituationBoardedVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GameEnder), "CheckOrUpdateGameOver", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "GameEnderWithVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldObjectsHolder), "AddToCache", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "AddVehicleObjectToCache", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldObjectsHolder), "RemoveFromCache", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "RemoveVehicleObjectToCache", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldObjectsHolder), "Recache", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_WorldHandling), "RecacheVehicleObjectCache", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnUtility), "IsTravelingInTransportPodWorldObject", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "AerialVehiclesDontRandomizePrisoners", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CameraJumper), "TryShowWorld", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_WorldHandling), "ForcedTargetingDontShowWorld", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MainButtonWorker_ToggleWorld), "Activate", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_WorldHandling), "ForcedTargetingDontToggleWorld", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldTargeter), "TargeterUpdate", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "WorldTargeterUpdate", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldTargeter), "TargeterOnGUI", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "WorldTargeterOnGUI", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WorldTargeter), "ProcessInputEvents", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_WorldHandling), "WorldTargeterProcessInputEvents", (System.Type[]) null));
  }

  private static void SituationBoardedVehicle(Pawn p, ref WorldPawnSituation __result)
  {
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    if (^(int&) ref __result != 1 || ((Thing) p).Faction == null || ((Thing) p).Faction != Faction.OfPlayerSilentFail)
      return;
    if (p is VehiclePawn)
    {
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref __result = 6;
    }
    else
    {
      if (((Thing) p).ParentHolder?.ParentHolder is VehiclePawn)
      {
        // ISSUE: cast to a reference type
        // ISSUE: explicit reference operation
        ^(int&) ref __result = 6;
      }
      if (p.GetAerialVehicle() == null)
        return;
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref __result = 7;
    }
  }

  private static void GameEnderWithVehicles(GameEnder __instance, ref int ___ticksToGameOver)
  {
    if (!__instance.gameEnding)
      return;
    if (LandingTargeter.Instance.IsTargeting)
    {
      __instance.gameEnding = false;
      ___ticksToGameOver = -1;
    }
    else
    {
      foreach (Map map in Find.Maps)
      {
        foreach (VehiclePawn allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
        {
          foreach (Pawn pawn in allClaimant.AllPawnsAboard)
          {
            if (pawn.IsFreeColonist)
            {
              __instance.gameEnding = false;
              ___ticksToGameOver = -1;
              return;
            }
          }
        }
      }
      foreach (AerialVehicleInFlight aerialVehicle in Find.World.GetComponent<VehicleWorldObjectsHolder>().AerialVehicles)
      {
        foreach (Pawn pawn in aerialVehicle.Vehicle.AllPawnsAboard)
        {
          if (pawn.IsFreeColonist)
          {
            __instance.gameEnding = false;
            ___ticksToGameOver = -1;
            return;
          }
        }
      }
    }
  }

  private static void AddVehicleObjectToCache(WorldObject o)
  {
    Find.World.GetComponent<VehicleWorldObjectsHolder>().AddToCache(o);
  }

  private static void RemoveVehicleObjectToCache(WorldObject o)
  {
    Find.World.GetComponent<VehicleWorldObjectsHolder>().RemoveFromCache(o);
  }

  private static void RecacheVehicleObjectCache()
  {
    Find.World.GetComponent<VehicleWorldObjectsHolder>().Recache();
  }

  private static bool ForcedTargetingDontShowWorld(ref bool __result)
  {
    if (!LandingTargeter.Instance.ForcedTargeting)
      return true;
    __result = false;
    return false;
  }

  private static bool ForcedTargetingDontToggleWorld()
  {
    if (!LandingTargeter.Instance.ForcedTargeting)
      return true;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_MustTargetLanding")), MessageTypeDefOf.RejectInput, true);
    return false;
  }

  private static void AerialVehiclesDontRandomizePrisoners(Pawn pawn, ref bool __result)
  {
    if (!ThingOwnerUtility.AnyParentIs<VehiclePawn>((Thing) pawn) && !ThingOwnerUtility.AnyParentIs<AerialVehicleInFlight>((Thing) pawn))
      return;
    __result = true;
  }

  private static void WorldTargeterUpdate() => Targeters.UpdateWorldTargeter();

  private static void WorldTargeterOnGUI() => Targeters.OnGUIWorldTargeter();

  private static void WorldTargeterProcessInputEvents()
  {
    Targeters.ProcessWorldTargeterInputEvent();
  }
}
