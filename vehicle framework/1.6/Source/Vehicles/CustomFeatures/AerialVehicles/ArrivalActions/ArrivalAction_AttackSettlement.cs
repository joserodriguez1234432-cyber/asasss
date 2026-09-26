// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_AttackSettlement
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.World;

public class ArrivalAction_AttackSettlement : ArrivalAction_LoadMap
{
  public ArrivalAction_AttackSettlement()
  {
  }

  public ArrivalAction_AttackSettlement(
    VehiclePawn vehicle,
    AerialVehicleArrivalModeDef arrivalModeDef)
    : base(vehicle, arrivalModeDef)
  {
  }

  public override bool DestroyOnArrival => true;

  protected override void MapLoaded(Map map, bool generatedMap)
  {
    base.MapLoaded(map, generatedMap);
    TaggedString taggedString1 = Translator.Translate("LetterLabelCaravanEnteredEnemyBase");
    TaggedString taggedString2 = TranslatorFormattedStringExtensions.Translate("LetterTransportPodsLandedInEnemyBase", NamedArgument.op_Implicit(((WorldObject) map.Parent).Label));
    TaggedString taggedString3 = ((TaggedString) ref taggedString2).CapitalizeFirst();
    SettlementUtility.AffectRelationsOnAttacked(map.Parent, ref taggedString3);
    if (generatedMap)
    {
      Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
      PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter((IEnumerable<Pawn>) map.mapPawns.AllPawns, ref taggedString1, ref taggedString3, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsInMapWherePlayerLanded", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), true, true);
    }
    Find.LetterStack.ReceiveLetter(taggedString1, taggedString3, LetterDefOf.NeutralEvent, LookTargets.op_Implicit((Thing) this.vehicle), ((WorldObject) map.Parent).Faction, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
  }

  public static FloatMenuAcceptanceReport CanAttack(VehiclePawn vehicle, Settlement settlement)
  {
    if (settlement == null || !((WorldObject) settlement).Spawned || !settlement.Attackable)
      return FloatMenuAcceptanceReport.op_Implicit(false);
    if (!WorldVehiclePathGrid.Instance.Passable(((WorldObject) settlement).Tile, vehicle.VehicleDef))
      return FloatMenuAcceptanceReport.WithFailReason(TaggedString.op_Implicit(Translator.Translate("Impassable")));
    return EnterCooldownCompUtility.EnterCooldownBlocksEntering((MapParent) settlement) ? FloatMenuAcceptanceReport.WithFailReasonAndMessage(TaggedString.op_Implicit(Translator.Translate("EnterCooldownBlocksEntering")), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageEnterCooldownBlocksEntering", NamedArgument.op_Implicit(GenDate.ToStringTicksToPeriod(EnterCooldownCompUtility.EnterCooldownTicksLeft((MapParent) settlement), true, false, true, true, false))))) : FloatMenuAcceptanceReport.op_Implicit(true);
  }
}
