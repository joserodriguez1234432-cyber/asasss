// Decompiled with JetBrains decompiler
// Type: Vehicles.World.IncidentWorker_ShuttleDowned
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

#nullable disable
namespace Vehicles.World;

public class IncidentWorker_ShuttleDowned : IncidentWorker
{
  public static void Execute(
    AerialVehicleInFlight aerialVehicle,
    string[] reasons,
    WorldObject culprit = null,
    IntVec3? cell = null)
  {
    (VehicleIncidentDefOf.ShuttleCrashed.Worker as IncidentWorker_ShuttleDowned).TryExecuteEvent(aerialVehicle, reasons, culprit, cell);
  }

  protected virtual string GetLetterText(
    VehiclePawn vehicle,
    string[] reasons,
    WorldObject culprit)
  {
    StringBuilder stringBuilder = new StringBuilder();
    if (culprit != null)
    {
      stringBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IncidentCrashedSite_ShotDown", NamedArgument.op_Implicit((Thing) vehicle), NamedArgument.op_Implicit(culprit))));
    }
    else
    {
      stringBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IncidentCrashedSite_Crashing", NamedArgument.op_Implicit((Thing) vehicle))));
      if (!GenList.NullOrEmpty<string>((IList<string>) reasons))
        stringBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_IncidentReasonLister", NamedArgument.op_Implicit(string.Join(Environment.NewLine, reasons)))));
    }
    return stringBuilder.ToString();
  }

  protected virtual TaggedString GetLetterLabel(VehiclePawn vehicle, WorldObject culprit)
  {
    return culprit != null ? TranslatorFormattedStringExtensions.Translate("VF_IncidentCrashedSiteLabel_ShotDown", NamedArgument.op_Implicit((Thing) vehicle), NamedArgument.op_Implicit(culprit)) : TranslatorFormattedStringExtensions.Translate("VF_IncidentCrashedSiteLabel_Crashing", NamedArgument.op_Implicit((Thing) vehicle));
  }

  protected virtual bool TryExecuteEvent(
    AerialVehicleInFlight aerialVehicle,
    string[] reasons,
    WorldObject culprit = null,
    IntVec3? cell = null)
  {
    try
    {
      Map crashSiteMap;
      int andReinforcements = this.GenerateMapAndReinforcements(aerialVehicle, culprit, out crashSiteMap);
      IntVec3 landingCell = cell ?? this.RandomCrashingCell(aerialVehicle, crashSiteMap);
      if (IntVec3.op_Equality(landingCell, IntVec3.Invalid))
        return false;
      VehiclePawn vehicle = aerialVehicle.Vehicle;
      new ArrivalAction_CrashInMap(aerialVehicle.Vehicle, crashSiteMap.Parent, landingCell, Rot4.East).Arrived(GlobalTargetInfo.op_Implicit((WorldObject) crashSiteMap.Parent));
      string str1 = culprit?.Label ?? string.Empty;
      if (andReinforcements > 0)
      {
        string str2 = ((float) andReinforcements / 2500f).RoundTo(1f).ToString();
        this.SendCrashSiteLetter(culprit, this.GetLetterLabel(vehicle, culprit), TaggedString.op_Implicit(this.GetLetterText(vehicle, reasons, culprit)), this.def.letterDef, LookTargets.op_Implicit((WorldObject) crashSiteMap.Parent), NamedArgument.op_Implicit(((Entity) vehicle).Label), NamedArgument.op_Implicit(str1), NamedArgument.op_Implicit(str2));
      }
      else
        this.SendCrashSiteLetter(culprit, this.GetLetterLabel(vehicle, culprit), TaggedString.op_Implicit(this.GetLetterText(vehicle, reasons, culprit)), this.def.letterDef, LookTargets.op_Implicit((WorldObject) crashSiteMap.Parent), NamedArgument.op_Implicit(((Entity) vehicle).Label), NamedArgument.op_Implicit(str1));
      return true;
    }
    catch (Exception ex)
    {
      Log.Error($"Failed to execute incident {((object) this).GetType()}. Exception=\"{ex}\"");
      return false;
    }
  }

  protected virtual IntVec3 RandomCrashingCell(AerialVehicleInFlight aerialVehicle, Map crashSite)
  {
    IntVec3 intVec3;
    RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(new Predicate<IntVec3>(Validator), crashSite, ref intVec3);
    return intVec3;

    bool Validator(IntVec3 cell)
    {
      return !GridsUtility.Fogged(cell, crashSite) && GenGrid.InBounds(cell, crashSite) && ((IEnumerable<IntVec3>) (object) aerialVehicle.Vehicle.PawnOccupiedCells(cell, Rot4.East)).All<IntVec3>((Func<IntVec3, bool>) (hitboxCell => hitboxCell.Walkable(aerialVehicle.Vehicle.VehicleDef, crashSite.GetCachedMapComponent<VehiclePathingSystem>()) && !Ext_Vehicles.IsRoofed(hitboxCell, crashSite)));
    }
  }

  protected virtual int GenerateMapAndReinforcements(
    AerialVehicleInFlight aerialVehicle,
    WorldObject culprit,
    out Map crashSiteMap)
  {
    int andReinforcements = -1;
    MapParent mapParent = Find.WorldObjects.MapParentAt(aerialVehicle.Tile);
    if (mapParent != null && mapParent.Map != null)
    {
      crashSiteMap = mapParent.Map;
    }
    else
    {
      VehiclePawn vehicle = aerialVehicle.Vehicle;
      int incidentMapSize = CaravanIncidentUtility.CalculateIncidentMapSize(aerialVehicle.Vehicle.AllPawnsAboard, aerialVehicle.Vehicle.AllPawnsAboard);
      crashSiteMap = GetOrGenerateMapUtility.GetOrGenerateMap(aerialVehicle.Tile, new IntVec3(incidentMapSize, 1, incidentMapSize), WorldObjectDefOfVehicles.CrashedShipSite, (IEnumerable<GenStepWithParams>) null, false);
      CrashSite parent = crashSiteMap.Parent as CrashSite;
      if (culprit is Settlement reinforcementsFrom)
        andReinforcements = parent.InitiateReinforcementsRequest(reinforcementsFrom);
      MapHelper.UnfogMapFromEdge(crashSiteMap, aerialVehicle.Vehicle.VehicleDef);
    }
    return andReinforcements;
  }

  protected virtual void SendCrashSiteLetter(
    WorldObject shotDownBy,
    TaggedString baseLetterLabel,
    TaggedString baseLetterText,
    LetterDef letterDef,
    LookTargets lookTargets,
    params NamedArgument[] textArgs)
  {
    if (((TaggedString) ref baseLetterLabel).NullOrEmpty() || ((TaggedString) ref baseLetterText).NullOrEmpty())
      Log.Error("Sending standard incident letter with no label or text.");
    ChoiceLetter choiceLetter = LetterMaker.MakeLetter(GrammarResolverSimpleStringExtensions.Formatted(baseLetterLabel, textArgs), GrammarResolverSimpleStringExtensions.Formatted(baseLetterText, textArgs), letterDef, lookTargets, shotDownBy?.Faction, (Quest) null, (List<ThingDef>) null);
    List<HediffDef> hediffDefList = new List<HediffDef>();
    if (!GenList.NullOrEmpty<HediffDef>((IList<HediffDef>) this.def.letterHyperlinkHediffDefs))
      hediffDefList.AddRange((IEnumerable<HediffDef>) this.def.letterHyperlinkHediffDefs);
    choiceLetter.hyperlinkHediffDefs = hediffDefList;
    Find.LetterStack.ReceiveLetter((Letter) choiceLetter, (string) null, 0, true);
  }

  [NoProfiling]
  protected virtual bool TryExecuteWorker(IncidentParms parms)
  {
    throw new NotImplementedException("Shuttle downed event cannot be called through the Worker");
  }
}
