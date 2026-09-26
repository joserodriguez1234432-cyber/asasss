// Decompiled with JetBrains decompiler
// Type: Vehicles.UpgradeNode
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class UpgradeNode
{
  public string key;
  public string label;
  public string description;
  public string upgradeExplanation;
  public bool displayLabel;
  public string icon;
  public float work = 1f;
  public bool hidden;
  public SoundDef unlockSound;
  public SoundDef resetSound;
  public IntVec2 gridCoordinate;
  public Vector2 drawSize = new Vector2(40f, 40f);
  public Color? drawColorOne;
  public Color? drawColorTwo;
  public Color? drawColorThree;
  public List<Upgrade> upgrades;
  public List<string> replaces;
  public string disableIfUpgradeNodeEnabled;
  public List<string> disableIfUpgradeNodesEnabled;
  public List<ResearchProjectDef> researchPrerequisites = new List<ResearchProjectDef>();
  public List<string> prerequisiteNodes = new List<string>();
  [LoadAlias("costList")]
  public List<ThingDefCountClass> ingredients = new List<ThingDefCountClass>();
  public float refundFraction = 0.5f;
  public SimpleDictionary<ThingDef, float> refundLeavings = new SimpleDictionary<ThingDef, float>();
  public List<GraphicDataOverlay> graphicOverlays;

  public Texture2D Icon { get; private set; }

  public virtual IntVec2 GridCoordinate => this.gridCoordinate;

  public bool HasGraphics { get; private set; }

  public virtual Texture2D UpgradeImage
  {
    get
    {
      if (string.IsNullOrEmpty(this.icon))
        return BaseContent.BadTex;
      if (!Object.op_Implicit((Object) this.Icon))
        this.Icon = ContentFinder<Texture2D>.Get(this.icon, true);
      return this.Icon;
    }
  }

  public virtual void DrawExtraOnGUI(Rect rect)
  {
  }

  public void AddOverlays(VehiclePawn vehicle)
  {
    if (!UnityData.IsInMainThread)
      LongEventHandler.ExecuteWhenFinished((Action) (() => this.AddOverlaysInternal(vehicle)));
    else
      this.AddOverlaysInternal(vehicle);
  }

  private void AddOverlaysInternal(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<GraphicDataOverlay>((IList<GraphicDataOverlay>) this.graphicOverlays))
    {
      foreach (GraphicDataOverlay graphicOverlay1 in this.graphicOverlays)
      {
        GraphicOverlay graphicOverlay2 = GraphicOverlay.Create(graphicOverlay1, vehicle);
        vehicle.DrawTracker.overlayRenderer.AddOverlay(this.key, graphicOverlay2);
      }
    }
    bool flag = false;
    if (this.drawColorOne.HasValue)
    {
      ((Thing) vehicle).DrawColor = this.drawColorOne.Value;
      flag = true;
    }
    if (this.drawColorTwo.HasValue)
    {
      vehicle.DrawColorTwo = this.drawColorTwo.Value;
      flag = true;
    }
    if (this.drawColorThree.HasValue)
    {
      vehicle.DrawColorThree = this.drawColorThree.Value;
      flag = true;
    }
    if (!flag)
      return;
    ((Thing) vehicle).Notify_ColorChanged();
  }

  public void RemoveOverlays(VehiclePawn vehicle)
  {
    vehicle.DrawTracker.overlayRenderer.RemoveOverlays(this.key);
  }

  public override bool Equals(object obj)
  {
    UpgradeNode node = obj as UpgradeNode;
    return (object) node != null && this.Equals(node);
  }

  private bool Equals(UpgradeNode node) => this.key == node.key;

  public override int GetHashCode() => this.key.GetHashCode();

  public static bool operator ==(UpgradeNode lhs, UpgradeNode rhs) => lhs?.key == rhs?.key;

  public static bool operator !=(UpgradeNode lhs, UpgradeNode rhs) => lhs?.key != rhs?.key;

  public override string ToString() => $"{this.key}_{this.GetType()}";

  public bool IsIngredient(ThingDef def)
  {
    for (int index = 0; index < this.ingredients.Count; ++index)
    {
      if (this.ingredients[index].thingDef == def)
        return true;
    }
    return false;
  }

  public int MatchedItemCount(VehiclePawn vehicle, ThingDef def)
  {
    return GenCollection.Count<Thing>(vehicle.CompUpgradeTree.upgradeContainer.InnerListForReading, (Predicate<Thing>) (thing => thing.def == def));
  }

  public int TotalItemCountRequired(ThingDef def)
  {
    return GenCollection.FirstOrDefault<ThingDefCountClass>(this.ingredients, (Predicate<ThingDefCountClass>) (thingDefCount => thingDefCount.thingDef == def)).count;
  }

  public virtual bool AvailableSpace(VehiclePawn vehicle, Thing item)
  {
    return this.IsIngredient(item.def) && this.MatchedItemCount(vehicle, item.def) < this.TotalItemCountRequired(item.def);
  }

  public IEnumerable<ThingDefCountClass> MaterialsRequired(VehiclePawn vehicle)
  {
    foreach (ThingDefCountClass ingredient in this.ingredients)
    {
      if (!((ThingOwner) vehicle.CompUpgradeTree.upgradeContainer).Contains(ingredient.thingDef))
        yield return new ThingDefCountClass(ingredient.thingDef, ingredient.count);
      else if (((ThingOwner) vehicle.CompUpgradeTree.upgradeContainer).TotalStackCountOfDef(ingredient.thingDef) < ingredient.count)
        yield return new ThingDefCountClass(ingredient.thingDef, ingredient.count - ((ThingOwner) vehicle.CompUpgradeTree.upgradeContainer).TotalStackCountOfDef(ingredient.thingDef));
    }
  }

  public void ResolveReferences()
  {
    this.HasGraphics = !GenList.NullOrEmpty<GraphicDataOverlay>((IList<GraphicDataOverlay>) this.graphicOverlays);
    if (GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) this.upgrades))
      return;
    foreach (Upgrade upgrade in this.upgrades)
    {
      upgrade.Init(this);
      this.HasGraphics |= upgrade.HasGraphics;
    }
  }

  public void PostLoad()
  {
    if (GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) this.upgrades))
      return;
    foreach (Upgrade upgrade in this.upgrades)
      upgrade.PostLoad();
  }
}
