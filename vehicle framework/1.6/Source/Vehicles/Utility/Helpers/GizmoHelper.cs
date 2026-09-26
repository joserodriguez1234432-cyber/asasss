// Decompiled with JetBrains decompiler
// Type: Vehicles.GizmoHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

public static class GizmoHelper
{
  public static Command_Action AerialVehicleTradeCommand(
    this AerialVehicleInFlight aerialVehicle,
    Faction faction = null,
    TraderKindDef trader = null)
  {
    Pawn bestNegotiator = aerialVehicle.Vehicle.FindBestNegotiator(faction, trader);
    Command_Action commandAction1 = new Command_Action();
    ((Command) commandAction1).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandTrade"));
    ((Command) commandAction1).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandTradeDesc"));
    ((Command) commandAction1).icon = (Texture) TexData.TradeCommandTex;
    commandAction1.action = (Action) (() =>
    {
      Settlement settlement = Find.WorldObjects.SettlementAt(aerialVehicle.Tile);
      if (settlement == null || !settlement.CanTradeNow)
        return;
      Find.WindowStack.Add((Window) new Dialog_Trade(bestNegotiator, (ITrader) settlement, false));
      PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(settlement.Goods.OfType<Pawn>(), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsTradingWithSettlement", NamedArgument.op_Implicit(Faction.OfPlayer.def.pawnsPlural))), LetterDefOf.NeutralEvent, false, true);
    });
    Command_Action commandAction2 = commandAction1;
    if (bestNegotiator == null)
    {
      if (trader?.permitRequiredForTrading != null && !GenCollection.Any<Pawn>(aerialVehicle.Vehicle.AllPawnsAboard, (Predicate<Pawn>) (pawn => pawn.royalty != null && pawn.royalty.HasPermit(trader.permitRequiredForTrading, faction))))
        ((Gizmo) commandAction2).Disable(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CommandTradeFailNeedPermit", NamedArgument.op_Implicit(((Def) trader.permitRequiredForTrading).LabelCap))));
      else
        ((Gizmo) commandAction2).Disable(TaggedString.op_Implicit(Translator.Translate("CommandTradeFailNoNegotiator")));
    }
    if (bestNegotiator != null)
    {
      SkillRecord skill = bestNegotiator.skills?.GetSkill(SkillDefOf.Social);
      if (skill != null && skill.TotallyDisabled)
        ((Gizmo) commandAction2).Disable(TaggedString.op_Implicit(Translator.Translate("CommandTradeFailSocialDisabled")));
    }
    return commandAction2;
  }

  public static void DesignatorsChanged(DesignationCategoryDef designationCategoryDef)
  {
    AccessTools.Method(typeof (DesignationCategoryDef), "ResolveDesignators", (System.Type[]) null, (System.Type[]) null).Invoke((object) designationCategoryDef, Array.Empty<object>());
  }

  public static void ResetDesignatorStatuses()
  {
    foreach (VehicleDef def in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      bool flag1;
      switch (SettingsCache.TryGetValue<VehicleEnabled.For>(def, typeof (VehicleDef), "enabled", def.enabled))
      {
        case VehicleEnabled.For.Player:
        case VehicleEnabled.For.Everyone:
          flag1 = true;
          break;
        default:
          flag1 = false;
          break;
      }
      bool flag2 = flag1;
      Current.Game.Rules.SetAllowBuilding((ThingDef) def.buildDef, flag2);
    }
  }
}
