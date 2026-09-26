// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalAction_Trade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class ArrivalAction_Trade : ArrivalAction_VisitSettlement
{
  public ArrivalAction_Trade()
  {
  }

  public ArrivalAction_Trade(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public override void Arrived(GlobalTargetInfo target)
  {
    base.Arrived(target);
    Settlement worldObject = ((GlobalTargetInfo) ref target).WorldObject as Settlement;
    if (ArrivalAction_Trade.GetValidNegotiator(this.vehicle, worldObject) == null)
      return;
    Pawn bestNegotiator = this.vehicle.FindBestNegotiator(((WorldObject) worldObject).Faction, worldObject.TraderKind);
    if (bestNegotiator == null)
      return;
    Find.WindowStack.Add((Window) new Dialog_Trade(bestNegotiator, (ITrader) worldObject, false));
    PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(worldObject.Goods.OfType<Pawn>(), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsTradingWithSettlement", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), LetterDefOf.NeutralEvent, false, true);
  }

  public static bool ValidGiftOrTradePartner(Settlement settlement)
  {
    return settlement != null && ((WorldObject) settlement).Spawned && !((MapParent) settlement).HasMap && ((WorldObject) settlement).Faction != null && ((WorldObject) settlement).Faction != Faction.OfPlayer && !((WorldObject) settlement).Faction.def.permanentEnemy && settlement.CanTradeNow;
  }

  public static FloatMenuAcceptanceReport CanTradeWith(VehiclePawn vehicle, Settlement settlement)
  {
    return FloatMenuAcceptanceReport.op_Implicit(ArrivalAction_Trade.ValidGiftOrTradePartner(settlement) && !FactionUtility.HostileTo(((WorldObject) settlement).Faction, Faction.OfPlayer) && ArrivalAction_Trade.GetValidNegotiator(vehicle, settlement) != null);
  }

  public static Pawn GetValidNegotiator(VehiclePawn vehicle, Settlement settlement)
  {
    foreach (Pawn validNegotiator in vehicle.AllPawnsAboard)
    {
      if (!validNegotiator.Dead && !validNegotiator.Downed && !validNegotiator.InMentalState && !StatDefOf.TradePriceImprovement.Worker.IsDisabledFor((Thing) validNegotiator) && AcceptanceReport.op_Implicit(FactionUtility.CanTradeWith(validNegotiator, ((WorldObject) settlement).Faction, settlement.TraderKind)))
        return validNegotiator;
    }
    return (Pawn) null;
  }
}
