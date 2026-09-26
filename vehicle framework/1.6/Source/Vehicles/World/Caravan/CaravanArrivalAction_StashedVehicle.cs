// Decompiled with JetBrains decompiler
// Type: Vehicles.World.CaravanArrivalAction_StashedVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class CaravanArrivalAction_StashedVehicle : CaravanArrivalAction
{
  private StashedVehicle stashedVehicle;

  public CaravanArrivalAction_StashedVehicle()
  {
  }

  public CaravanArrivalAction_StashedVehicle(StashedVehicle stashedVehicle)
  {
    this.stashedVehicle = stashedVehicle;
  }

  public virtual string Label
  {
    get => TaggedString.op_Implicit(Translator.Translate("VF_RecoverVehicles"));
  }

  public virtual string ReportString
  {
    get
    {
      return TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanVisiting", NamedArgument.op_Implicit(this.stashedVehicle.Label)));
    }
  }

  public virtual FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
  {
    FloatMenuAcceptanceReport acceptanceReport = base.StillValid(caravan, destinationTile);
    if (!FloatMenuAcceptanceReport.op_Implicit(acceptanceReport))
      return acceptanceReport;
    return this.stashedVehicle != null && PlanetTile.op_Inequality(this.stashedVehicle.Tile, destinationTile) ? FloatMenuAcceptanceReport.op_Implicit(false) : CaravanArrivalAction_StashedVehicle.CanVisit(caravan, this.stashedVehicle);
  }

  public virtual void Arrived(Caravan caravan)
  {
    this.stashedVehicle.Notify_CaravanArrived(caravan);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<StashedVehicle>(ref this.stashedVehicle, "stashedVehicle", false);
  }

  public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, StashedVehicle stashedVehicle)
  {
    return FloatMenuAcceptanceReport.op_Implicit(stashedVehicle != null && stashedVehicle.Spawned);
  }

  public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(
    Caravan caravan,
    StashedVehicle stashedVehicle)
  {
    return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_StashedVehicle>((Func<FloatMenuAcceptanceReport>) (() => CaravanArrivalAction_StashedVehicle.CanVisit(caravan, stashedVehicle)), (Func<CaravanArrivalAction_StashedVehicle>) (() => new CaravanArrivalAction_StashedVehicle(stashedVehicle)), TaggedString.op_Implicit(Translator.Translate("VF_RecoverVehicles")), caravan, stashedVehicle.Tile, (WorldObject) stashedVehicle, (Action<Action>) null);
  }
}
