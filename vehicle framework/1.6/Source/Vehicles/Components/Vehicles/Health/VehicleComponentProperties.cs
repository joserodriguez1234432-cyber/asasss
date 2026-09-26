// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleComponentProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleComponentProperties
{
  [NoTranslate]
  public string key;
  public string label;
  public System.Type compClass;
  public int health;
  public VehicleComponent.VehiclePartDepth depth;
  public float efficiencyWeight = 1f;
  public float hitWeight = 1f;
  public List<StatModifier> armor;
  public VehicleEMPSeverity empSeverity;
  public bool priorityStatEfficiency;
  public ComponentHitbox hitbox = new ComponentHitbox();
  public List<VehicleStatDef> categories;
  public LinearCurve efficiency;
  [TweakField]
  public List<Reactor> reactors;
  public List<string> tags;

  public virtual T GetReactor<T>() where T : Reactor
  {
    List<Reactor> reactors = this.reactors;
    return (reactors != null ? GenCollection.FirstOrDefault<Reactor>(reactors, (Predicate<Reactor>) (reactor => reactor is T)) : (Reactor) null) as T;
  }

  public virtual bool HasReactor<T>() where T : Reactor => (object) this.GetReactor<T>() != null;

  public virtual void ResolveReferences(VehicleDef def)
  {
    if (this.efficiency == null)
      this.efficiency = new LinearCurve()
      {
        new CurvePoint(0.0f, 0.0f),
        new CurvePoint(0.25f, 0.0f),
        new CurvePoint(0.4f, 0.4f),
        new CurvePoint(0.7f, 0.7f),
        new CurvePoint(0.85f, 1f),
        new CurvePoint(1f, 1f)
      };
    if (this.categories == null)
      this.categories = new List<VehicleStatDef>();
    if ((object) this.compClass == null)
      this.compClass = typeof (VehicleComponent);
    this.hitbox.Initialize(def);
  }

  public virtual IEnumerable<string> ConfigErrors()
  {
    if (GenText.NullOrEmpty(this.key))
      yield return (this.key + ": <field>key</field> field must be implemented.").ConvertRichText();
    if (this.health <= 0)
      yield return (this.key + ": <field>health</field> must be greater than 0.").ConvertRichText();
    LinearCurve efficiency = this.efficiency;
    if (efficiency != null && efficiency.PointsCount < 5)
      yield return (this.key + ": <field>efficiency</field> must include at least 5 points for proper color gradient construction.").ConvertRichText();
    if (this.hitbox == null)
      yield return (this.key + ": <field>hitbox</field> must be specified even if it occupies no cells.").ConvertRichText();
    if ((double) this.efficiencyWeight == 0.0)
      yield return this.key + ": <field>efficiencyWeight</field> cannot = 0. If average weight = 0, resulting damage will be NaN, causing an instant-kill on the vehicle.";
  }
}
