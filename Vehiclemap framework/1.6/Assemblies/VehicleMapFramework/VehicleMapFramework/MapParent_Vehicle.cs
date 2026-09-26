// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.MapParent_Vehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class MapParent_Vehicle : PocketMapParent
{
  public VehiclePawnWithMap vehicle;

  public virtual string Label
  {
    get => $"{((Entity) this.vehicle).Label}{Translator.Translate("VMF_VehicleMap")}";
  }

  public virtual Material Material => BaseContent.ClearMat;

  public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
  {
    return !GenCollection.Any<Pawn>(caravan.PawnsListForReading, (Predicate<Pawn>) (p => p is VehiclePawnWithMap)) ? ((MapParent) this).GetFloatMenuOptions(caravan) : (IEnumerable<FloatMenuOption>) Array.Empty<FloatMenuOption>();
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<VehiclePawnWithMap>(ref this.vehicle, "vehicle", false);
  }
}
