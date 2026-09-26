// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_LaunchProtocol_GetArrivalOptions
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using SmashTools.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (LaunchProtocol), "GetArrivalOptions")]
[PatchLevel(Level.Safe)]
public static class Patch_LaunchProtocol_GetArrivalOptions
{
  public static IEnumerable<ArrivalOption> Postfix(
    IEnumerable<ArrivalOption> values,
    GlobalTargetInfo target,
    LaunchProtocol __instance)
  {
    foreach (ArrivalOption arrivalOption in values)
      yield return arrivalOption;
    if (!(__instance.Vehicle is VehiclePawnWithMap))
    {
      MapParent_Vehicle mapParent = Find.World.pocketMaps.Where<PocketMapParent>((Func<PocketMapParent, bool>) (p => PlanetTile.op_Equality(((WorldObject) p).Tile, ((GlobalTargetInfo) ref target).Tile))).OfType<MapParent_Vehicle>().FirstOrDefault<MapParent_Vehicle>((Func<MapParent_Vehicle, bool>) (m =>
      {
        VehiclePawnWithMap vehicle = m.vehicle;
        return vehicle != null && !((Thing) vehicle).Spawned;
      }));
      if (mapParent != null)
      {
        VehiclePawn vehicle = __instance.Vehicle;
        if (((MapParent) mapParent).HasMap && !EnterCooldownCompUtility.EnterCooldownBlocksEntering((MapParent) mapParent))
          yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("LandInExistingMap", NamedArgument.op_Implicit(((WorldObject) mapParent).Label)), (Action<TargetData<GlobalTargetInfo>>) (targetData =>
          {
            Current.Game.CurrentMap = ((MapParent) mapParent).Map;
            CameraJumper.TryHideWorld();
            LandingTargeter.Instance.BeginTargeting(vehicle, ((MapParent) mapParent).Map, (Action<LocalTargetInfo, Rot4>) ((landingCell, rot) => LaunchProtocol.StartTargetingLocalMap(vehicle, targetData, Patch_LaunchProtocol_GetArrivalOptions.VehicleMapParentOrMe((MapParent) mapParent), landingCell, rot)), (Func<LocalTargetInfo, bool>) (targetInfo =>
            {
              MapParent mapParent2 = Patch_LaunchProtocol_GetArrivalOptions.VehicleMapParentOrMe((MapParent) mapParent);
              return GenGrid.InBounds(((LocalTargetInfo) ref targetInfo).Cell, mapParent2.Map) && !Ext_Vehicles.IsRoofRestricted(vehicle.VehicleDef, ((LocalTargetInfo) ref targetInfo).Cell, mapParent2.Map);
            }), (Action) null, (Action) null, (Texture2D) null, ((ThingDef) vehicle.VehicleDef).rotatable, false);
          }));
      }
    }
  }

  public static MapParent VehicleMapParentOrMe(MapParent mapParent)
  {
    return Command_FocusVehicleMap.FocusedVehicle == null ? mapParent : Command_FocusVehicleMap.FocusedVehicle.VehicleMap.Parent;
  }
}
