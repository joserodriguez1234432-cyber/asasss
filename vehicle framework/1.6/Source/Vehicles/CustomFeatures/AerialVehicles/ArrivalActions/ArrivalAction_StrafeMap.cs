// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AerialVehicleArrivalAction_StrafeMap
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class AerialVehicleArrivalAction_StrafeMap : VehicleArrivalAction
{
  public MapParent parent;

  public AerialVehicleArrivalAction_StrafeMap()
  {
  }

  public AerialVehicleArrivalAction_StrafeMap(VehiclePawn vehicle, MapParent parent)
    : base(vehicle)
  {
    this.parent = parent;
  }

  public override void Arrived(GlobalTargetInfo target)
  {
    LongEventHandler.QueueLongEvent((Action) (() =>
    {
      Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(((GlobalTargetInfo) ref target).Tile, (WorldObjectDef) null, (IEnumerable<GenStepWithParams>) null);
      TaggedString taggedString1 = Translator.Translate("LetterLabelCaravanEnteredEnemyBase");
      TaggedString taggedString2 = TranslatorFormattedStringExtensions.Translate("LetterTransportPodsLandedInEnemyBase", NamedArgument.op_Implicit(((WorldObject) this.parent).Label));
      TaggedString taggedString3 = ((TaggedString) ref taggedString2).CapitalizeFirst();
      if (this.parent is Settlement parent2)
        SettlementUtility.AffectRelationsOnAttacked((MapParent) parent2, ref taggedString3);
      if (!this.parent.HasMap)
      {
        Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter((IEnumerable<Pawn>) orGenerateMap.mapPawns.AllPawns, ref taggedString1, ref taggedString3, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsInMapWherePlayerLanded", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), true, true);
      }
      AerialVehicleInFlight aerialVehicle = this.vehicle.GetAerialVehicle();
      CameraJumper.TryJump(orGenerateMap.Center, orGenerateMap, (CameraJumper.MovementMode) 0);
      StrafeTargeter.Instance.BeginTargeting(this.vehicle, this.vehicle.CompVehicleLauncher.launchProtocol, (Action<IntVec3, IntVec3>) ((start, end) =>
      {
        VehicleSkyfaller_FlyOver skyfallerFlyOver = VehicleSkyfallerMaker.MakeSkyfallerFlyOver(this.vehicle.CompVehicleLauncher.Props.skyfallerStrafing, this.vehicle, start, end);
        skyfallerFlyOver.aerialVehicle = aerialVehicle;
        GenSpawn.Spawn((Thing) skyfallerFlyOver, start, this.parent.Map, (Rot4) Rot8.North, (WipeMode) 0, false, false);
      }), forcedTargeting: true);
      aerialVehicle.ClearAndDestroy();
    }), "GeneratingMap", false, (Action<Exception>) null, true, false, (Action) null);
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<MapParent>(ref this.parent, "parent", false);
  }

  public static FloatMenuAcceptanceReport CanAttack(VehiclePawn vehicle, MapParent parent)
  {
    if (parent == null || !WorldVehiclePathGrid.Instance.Passable(((WorldObject) parent).Tile, vehicle.VehicleDef))
      return FloatMenuAcceptanceReport.op_Implicit(false);
    return EnterCooldownCompUtility.EnterCooldownBlocksEntering(parent) ? FloatMenuAcceptanceReport.WithFailReasonAndMessage(TaggedString.op_Implicit(Translator.Translate("EnterCooldownBlocksEntering")), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageEnterCooldownBlocksEntering", NamedArgument.op_Implicit(GenDate.ToStringTicksToPeriod(EnterCooldownCompUtility.EnterCooldownTicksLeft(parent), true, false, true, true, false))))) : FloatMenuAcceptanceReport.op_Implicit(true);
  }
}
