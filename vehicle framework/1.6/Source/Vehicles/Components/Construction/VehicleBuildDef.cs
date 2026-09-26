// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleBuildDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleBuildDef : ThingDef
{
  public VehicleDef thingToSpawn;
  public SoundDef soundBuilt;
  public SimpleCurve shakeAmountPerAreaCurve;

  public virtual IEnumerable<VehicleStatDrawEntry> SpecialDisplayStats()
  {
    VehicleBuildDef vehicleBuildDef = this;
    if (((BuildableDef) vehicleBuildDef).BuildableByPlayer)
    {
      List<TerrainAffordanceDef> source = new List<TerrainAffordanceDef>();
      if (((BuildableDef) vehicleBuildDef).PlaceWorkers != null)
        source.AddRange(((BuildableDef) vehicleBuildDef).PlaceWorkers.SelectMany<PlaceWorker, TerrainAffordanceDef>((Func<PlaceWorker, IEnumerable<TerrainAffordanceDef>>) (pw => pw.DisplayAffordances())));
      TerrainAffordanceDef terrainAffordanceNeed = ThingUtility.GetTerrainAffordanceNeed((BuildableDef) vehicleBuildDef, (ThingDef) null);
      if (terrainAffordanceNeed != null)
        source.Add(terrainAffordanceNeed);
      if (source.Count > 0)
        yield return new VehicleStatDrawEntry(StatCategoryDefOf.Building, TaggedString.op_Implicit(Translator.Translate("TerrainRequirement")), GenText.CapitalizeFirst(GenText.ToCommaList(source.Distinct<TerrainAffordanceDef>().OrderBy<TerrainAffordanceDef, int>((Func<TerrainAffordanceDef, int>) (ta => ta.order)).Select<TerrainAffordanceDef, string>((Func<TerrainAffordanceDef, string>) (ta => ((Def) ta).label)), false, false)), TaggedString.op_Implicit(Translator.Translate("Stat_Thing_TerrainRequirement_Desc")), 1101);
      BuildableDef.tmpCostList.Clear();
      BuildableDef.tmpHyperlinks.Clear();
      if (((BuildableDef) vehicleBuildDef).MadeFromStuff && ((BuildableDef) vehicleBuildDef).costStuffCount > 0)
      {
        BuildableDef.tmpCostList.Add($"{((BuildableDef) vehicleBuildDef).costStuffCount}x {GenText.ToCommaListOr(((BuildableDef) vehicleBuildDef).stuffCategories.Select<StuffCategoryDef, string>((Func<StuffCategoryDef, string>) (sc => ((Def) sc).label)), false)}");
        foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
        {
          if (allDef.IsStuff && !GenList.NullOrEmpty<StuffCategoryDef>((IList<StuffCategoryDef>) allDef.stuffProps.categories) && GenCollection.Any<StuffCategoryDef>(allDef.stuffProps.categories, new Predicate<StuffCategoryDef>(((BuildableDef) vehicleBuildDef).stuffCategories.Contains)))
            BuildableDef.tmpHyperlinks.Add(new Dialog_InfoCard.Hyperlink((Def) allDef, -1));
        }
      }
      if (!GenList.NullOrEmpty<ThingDefCountClass>((IList<ThingDefCountClass>) ((BuildableDef) vehicleBuildDef).CostList))
      {
        foreach (ThingDefCountClass cost in ((BuildableDef) vehicleBuildDef).CostList)
        {
          ThingDefCountClass countClass = cost;
          BuildableDef.tmpCostList.Add(countClass.Summary);
          if (!GenCollection.Any<Dialog_InfoCard.Hyperlink>(BuildableDef.tmpHyperlinks, (Predicate<Dialog_InfoCard.Hyperlink>) (hyperlink => hyperlink.def == countClass.thingDef)))
            BuildableDef.tmpHyperlinks.Add(new Dialog_InfoCard.Hyperlink((Def) countClass.thingDef, -1));
        }
      }
      if (GenCollection.Any<string>(BuildableDef.tmpCostList))
        yield return new VehicleStatDrawEntry(StatCategoryDefOf.Building, TaggedString.op_Implicit(Translator.Translate("Stat_Building_ResourcesToMake")), GenText.CapitalizeFirst(GenText.ToCommaList((IEnumerable<string>) BuildableDef.tmpCostList, false, false)), TaggedString.op_Implicit(Translator.Translate("Stat_Building_ResourcesToMakeDesc")), 4405, hyperlinks: (IEnumerable<Dialog_InfoCard.Hyperlink>) BuildableDef.tmpHyperlinks);
      float statValueAbstract = StatExtension.GetStatValueAbstract((BuildableDef) vehicleBuildDef, StatDefOf.WorkToBuild, (ThingDef) null);
      yield return new VehicleStatDrawEntry(StatCategoryDefOf.Building, TaggedString.op_Implicit(((Def) StatDefOf.WorkToBuild).LabelCap), statValueAbstract.ToString(), ((Def) StatDefOf.WorkToBuild).description, 4404);
    }
    if (((BuildableDef) vehicleBuildDef).constructionSkillPrerequisite > 0)
      yield return new VehicleStatDrawEntry(StatCategoryDefOf.Building, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("SkillRequiredToBuild", NamedArgument.op_Implicit(((Def) SkillDefOf.Construction).LabelCap))), ((BuildableDef) vehicleBuildDef).constructionSkillPrerequisite.ToString(), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("SkillRequiredToBuildExplanation", NamedArgument.op_Implicit(((Def) SkillDefOf.Construction).LabelCap))), 1100);
    if (((BuildableDef) vehicleBuildDef).artisticSkillPrerequisite > 0)
      yield return new VehicleStatDrawEntry(StatCategoryDefOf.Building, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("SkillRequiredToBuild", NamedArgument.op_Implicit(((Def) SkillDefOf.Artistic).LabelCap))), ((BuildableDef) vehicleBuildDef).artisticSkillPrerequisite.ToString(), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("SkillRequiredToBuildExplanation", NamedArgument.op_Implicit(((Def) SkillDefOf.Artistic).LabelCap))), 1100);
  }

  public VehicleBuildDef()
  {
    SimpleCurve simpleCurve = new SimpleCurve();
    simpleCurve.Add(new CurvePoint(1f, 0.07f), true);
    simpleCurve.Add(new CurvePoint(2f, 0.07f), true);
    simpleCurve.Add(new CurvePoint(4f, 0.1f), true);
    simpleCurve.Add(new CurvePoint(9f, 0.2f), true);
    simpleCurve.Add(new CurvePoint(16f, 0.5f), true);
    this.shakeAmountPerAreaCurve = simpleCurve;
    // ISSUE: explicit constructor call
    base.\u002Ector();
  }
}
