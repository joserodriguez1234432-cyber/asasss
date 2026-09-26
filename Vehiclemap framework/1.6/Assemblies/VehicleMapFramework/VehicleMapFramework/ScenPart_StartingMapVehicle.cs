// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ScenPart_StartingMapVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class ScenPart_StartingMapVehicle : ScenPart_StartingVehicle
{
  private PrefabDef prefabDef;

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Defs.Look<PrefabDef>(ref this.prefabDef, "prefabDef");
  }

  public virtual void DoEditInterface(Listing_ScenEdit listing)
  {
    base.DoEditInterface(listing);
    Rect scenPartRect = listing.GetScenPartRect((ScenPart) this, ScenPart.RowHeight);
    string str = ((Def) this.prefabDef)?.defName;
    if (str == null)
    {
      TaggedString taggedString = Translator.Translate("VMF_SelectPrefab");
      str = TaggedString.op_Implicit(((TaggedString) ref taggedString).CapitalizeFirst());
    }
    TextAnchor? nullable = new TextAnchor?();
    if (!Widgets.ButtonText(scenPartRect, str, true, true, true, nullable))
      return;
    List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>()
    {
      new FloatMenuOption(" ", (Action) (() => this.prefabDef = (PrefabDef) null), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
    };
    foreach (PrefabDef prefabDef in DefDatabase<PrefabDef>.AllDefsListForReading)
    {
      PrefabDef prefab = prefabDef;
      floatMenuOptionList.Add(new FloatMenuOption(((Def) prefab).defName, (Action) (() => this.prefabDef = prefab), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
    }
    Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
  }

  public virtual void Randomize()
  {
    base.Randomize();
    this.prefabDef = (PrefabDef) null;
  }

  public virtual IEnumerable<Thing> PlayerStartingThings()
  {
    ScenPart_StartingMapVehicle startingMapVehicle1 = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Thing thing in startingMapVehicle1.\u003C\u003En__0())
    {
      ScenPart_StartingMapVehicle startingMapVehicle = startingMapVehicle1;
      if (startingMapVehicle1.prefabDef != null)
      {
        VehiclePawnWithMap vehicle = thing as VehiclePawnWithMap;
        if (vehicle != null)
        {
          Map vehicleMap = vehicle.VehicleMap;
          LongEventHandler.ExecuteWhenFinished((Action) (() => PrefabUtility.SpawnPrefab(startingMapVehicle.prefabDef, vehicle.VehicleMap, vehicle.VehicleMap.Center, Rot4.North, Faction.OfPlayer, (List<Thing>) null, (Func<PrefabThingData, Tuple<ThingDef, ThingDef>>) null, (Action<Thing>) null, false)));
        }
      }
      yield return thing;
    }
  }
}
