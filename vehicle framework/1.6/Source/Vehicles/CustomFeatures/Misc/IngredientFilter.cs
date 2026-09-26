// Decompiled with JetBrains decompiler
// Type: Vehicles.IngredientFilter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public class IngredientFilter : IExposable
{
  internal HashSet<ThingDef> stuffableDefs = new HashSet<ThingDef>();
  private HashSet<ThingDef> nonStuffableDefs = new HashSet<ThingDef>();
  public ThingFilter filter = new ThingFilter();
  public int count = 1;

  public bool IsFixedIngredient => this.filter.AllowedDefCount == 1;

  public ThingDef FixedIngredient
  {
    get
    {
      if (!this.IsFixedIngredient)
        Log.Error("Called for SingleIngredient on an IngredientCount that is not IsSingleIngredient: " + this?.ToString());
      return this.filter.AnyAllowedDef;
    }
  }

  public string Summary => $"{this.count.ToString()}x {this.filter.Summary}";

  public ThingDefCountClass CountClass
  {
    get
    {
      return this.IsFixedIngredient ? new ThingDefCountClass(this.FixedIngredient, this.count) : new ThingDefCountClass(this.filter.AllowedThingDefs.First<ThingDef>(), this.count);
    }
  }

  public bool StuffableDef(ThingDef def)
  {
    if (!this.stuffableDefs.Contains(def) && !this.nonStuffableDefs.Contains(def))
    {
      if (((List<StuffCategoryDef>) AccessTools.Field(typeof (ThingFilter), "stuffCategoriesToAllow").GetValue((object) this.filter)).NotNullAndAny<StuffCategoryDef>((Predicate<StuffCategoryDef>) (sc => ((BuildableDef) def).stuffCategories.Contains(sc))))
        this.stuffableDefs.Add(def);
      else
        this.nonStuffableDefs.Add(def);
    }
    return this.filter.Allows(def) && this.stuffableDefs.Contains(def);
  }

  public void ResolveReferences() => this.filter.ResolveReferences();

  public void ExposeData()
  {
    Scribe_Values.Look<int>(ref this.count, "count", 0, false);
    Scribe_Deep.Look<ThingFilter>(ref this.filter, "filter", Array.Empty<object>());
  }

  public override string ToString() => $"({this.Summary})";
}
