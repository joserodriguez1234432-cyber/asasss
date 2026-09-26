// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_OfferGifts
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles.World;

public class ArrivalAction_OfferGifts : ArrivalAction_VisitSettlement
{
  public ArrivalAction_OfferGifts()
  {
  }

  public ArrivalAction_OfferGifts(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public override void Arrived(GlobalTargetInfo target)
  {
    base.Arrived(target);
    Settlement worldObject = ((GlobalTargetInfo) ref target).WorldObject as Settlement;
    if (ArrivalAction_Trade.GetValidNegotiator(this.vehicle, worldObject) == null)
    {
      Log.Warning($"No valid negotiator to trade with for {this.vehicle}.");
    }
    else
    {
      Find.WindowStack.Add((Window) new Dialog_Trade(this.vehicle.FindBestNegotiator(((WorldObject) worldObject).Faction, worldObject.TraderKind), (ITrader) worldObject, true));
      PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(worldObject.Goods.OfType<Pawn>(), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsTradingWithSettlement", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), LetterDefOf.NeutralEvent, false, true);
    }
  }

  public static FloatMenuAcceptanceReport CanOfferGiftsTo(
    VehiclePawn vehicle,
    Settlement settlement)
  {
    return FloatMenuAcceptanceReport.op_Implicit(ArrivalAction_Trade.ValidGiftOrTradePartner(settlement) && FactionUtility.HostileTo(((WorldObject) settlement).Faction, Faction.OfPlayer) && ArrivalAction_Trade.GetValidNegotiator(vehicle, settlement) != null);
  }
}
