// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_LoadMap
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Vehicles.Compatibility;
using Verse;

#nullable disable
namespace Vehicles.World;

public class ArrivalAction_LoadMap : VehicleArrivalAction
{
  public AerialVehicleArrivalModeDef arrivalModeDef;

  public ArrivalAction_LoadMap()
  {
  }

  public ArrivalAction_LoadMap(VehiclePawn vehicle, AerialVehicleArrivalModeDef arrivalModeDef)
    : base(vehicle)
  {
    this.arrivalModeDef = arrivalModeDef;
  }

  public override bool DestroyOnArrival => true;

  public override void Arrived(GlobalTargetInfo target)
  {
    base.Arrived(target);
    LongEventHandler.QueueLongEvent((Action) (() =>
    {
      MapParent mapParent = Find.WorldObjects.MapParentAt(((GlobalTargetInfo) ref target).Tile);
      if (mapParent == null)
      {
        Log.Error("Trying to arrive at map with null MapParent.");
      }
      else
      {
        bool hasMap = !mapParent.HasMap;
        if (AerialVehicleCompatibility.ShouldClaimOnArrival(mapParent))
          ((WorldObject) mapParent).SetFaction(Faction.OfPlayer);
        Site site = Find.WorldObjects.WorldObjectAt<Site>(((GlobalTargetInfo) ref target).Tile);
        Map map = site != null ? GetOrGenerateMapUtility.GetOrGenerateMap(((GlobalTargetInfo) ref target).Tile, site.PreferredMapSize, (WorldObjectDef) null, (IEnumerable<GenStepWithParams>) null, false) : GetOrGenerateMapUtility.GetOrGenerateMap(((GlobalTargetInfo) ref target).Tile, (WorldObjectDef) null, (IEnumerable<GenStepWithParams>) null);
        if (hasMap)
        {
          MapHelper.UnfogMapFromEdge(map, this.vehicle.VehicleDef);
          if (mapParent is EscapeShip)
          {
            Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            Find.LetterStack.ReceiveLetter(Translator.Translate("EscapeShipFoundLabel"), !Find.Storyteller.difficulty.allowBigThreats ? Translator.Translate("EscapeShipFoundPeaceful") : Translator.Translate("EscapeShipFound"), LetterDefOf.PositiveEvent, LookTargets.op_Implicit(new GlobalTargetInfo(map.Center, map, false)), (Faction) null, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
          }
        }
        this.MapLoaded(map, hasMap);
        this.ExecuteEvents();
        this.arrivalModeDef.Worker.VehicleArrived(this.vehicle, this.vehicle.CompVehicleLauncher.launchProtocol, map);
      }
    }), "GeneratingMap", false, (Action<Exception>) null, true, false, (Action) null);
  }

  protected virtual void MapLoaded(Map map, bool hasMap)
  {
  }

  protected virtual void ExecuteEvents()
  {
    this.vehicle.EventRegistry[VehicleEventDefOf.AerialVehicleLanding].ExecuteEvents();
  }

  [Obsolete("Deprecated since 1.6 refactor. Currently unused and will be removed.")]
  public static FloatMenuAcceptanceReport CanLand(VehiclePawn vehicle, MapParent mapParent)
  {
    if (mapParent == null || !((WorldObject) mapParent).Spawned)
      return FloatMenuAcceptanceReport.op_Implicit(false);
    if (!WorldVehiclePathGrid.Instance.Passable(((WorldObject) mapParent).Tile, vehicle.VehicleDef))
      return FloatMenuAcceptanceReport.WithFailReason(TaggedString.op_Implicit(Translator.Translate("Impassable")));
    return EnterCooldownCompUtility.EnterCooldownBlocksEntering(mapParent) ? FloatMenuAcceptanceReport.WithFailReasonAndMessage(TaggedString.op_Implicit(Translator.Translate("EnterCooldownBlocksEntering")), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageEnterCooldownBlocksEntering", NamedArgument.op_Implicit(GenDate.ToStringTicksToPeriod(EnterCooldownCompUtility.EnterCooldownTicksLeft(mapParent), true, false, true, true, false))))) : FloatMenuAcceptanceReport.op_Implicit(true);
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Defs.Look<AerialVehicleArrivalModeDef>(ref this.arrivalModeDef, "arrivalModeDef");
  }
}
