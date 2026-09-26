// Decompiled with JetBrains decompiler
// Type: Vehicles.ScenPart_StartingVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class ScenPart_StartingVehicle : ScenPart
{
  private VehicleDef vehicleDef;

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Defs.Look<VehicleDef>(ref this.vehicleDef, "vehicleDef");
  }

  public virtual void Randomize()
  {
    this.vehicleDef = GenCollection.RandomElement<VehicleDef>((IEnumerable<VehicleDef>) DefDatabase<VehicleDef>.AllDefsListForReading);
  }

  public virtual void DoEditInterface(Listing_ScenEdit listing)
  {
    Rect scenPartRect = listing.GetScenPartRect((ScenPart) this, (float) (2.0 * (double) ScenPart.RowHeight + 4.0));
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref scenPartRect).xMin, ((Rect) ref scenPartRect).yMin, ((Rect) ref scenPartRect).width, ScenPart.RowHeight);
    VehicleDef vehicleDef1 = this.vehicleDef;
    string str = TaggedString.op_Implicit(vehicleDef1 != null ? ((Def) vehicleDef1).LabelCap : Translator.Translate("VF_RandomVehicle"));
    if (!Widgets.ButtonText(rect, str, true, true, true, new TextAnchor?()))
      return;
    List<FloatMenuOption> floatMenuOptionList1 = new List<FloatMenuOption>();
    List<FloatMenuOption> floatMenuOptionList2 = floatMenuOptionList1;
    TaggedString taggedString = Translator.Translate("VF_RandomVehicle");
    FloatMenuOption floatMenuOption = new FloatMenuOption(TaggedString.op_Implicit(((TaggedString) ref taggedString).CapitalizeFirst()), (Action) (() => this.vehicleDef = (VehicleDef) null), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
    floatMenuOptionList2.Add(floatMenuOption);
    foreach (VehicleDef vehicleDef2 in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      VehicleDef vehicleDefOpt = vehicleDef2;
      floatMenuOptionList1.Add(new FloatMenuOption(TaggedString.op_Implicit(((Def) vehicleDefOpt).LabelCap), (Action) (() => this.vehicleDef = vehicleDefOpt), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
    }
    Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList1));
  }

  public virtual string Summary(Scenario scen)
  {
    return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
  }

  public virtual IEnumerable<string> GetSummaryListEntries(string tag)
  {
    if (tag == "PlayerStartsWith")
      yield return TaggedString.op_Implicit(((Def) this.vehicleDef).LabelCap);
  }

  public virtual IEnumerable<Thing> PlayerStartingThings()
  {
    ScenPart_StartingVehicle partStartingVehicle = this;
    if (partStartingVehicle.vehicleDef == null)
      ((ScenPart) partStartingVehicle).Randomize();
    yield return (Thing) VehicleSpawner.GenerateVehicle(partStartingVehicle.vehicleDef, Faction.OfPlayer);
  }

  public virtual int GetHashCode()
  {
    int hashCode1 = base.GetHashCode();
    VehicleDef vehicleDef = this.vehicleDef;
    int hashCode2 = vehicleDef != null ? ((object) vehicleDef).GetHashCode() : 0;
    return hashCode1 ^ hashCode2;
  }
}
