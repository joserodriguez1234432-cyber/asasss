// Decompiled with JetBrains decompiler
// Type: Vehicles.ArrivalAction_VisitSettlement
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class ArrivalAction_VisitSettlement : ArrivalAction_LandToCaravan
{
  public ArrivalAction_VisitSettlement()
  {
  }

  public ArrivalAction_VisitSettlement(VehiclePawn vehicle)
    : base(vehicle)
  {
  }

  public static FloatMenuAcceptanceReport CanVisit(VehiclePawn vehicle, Settlement settlement)
  {
    if (settlement == null || !((WorldObject) settlement).Spawned || !settlement.Visitable)
      return FloatMenuAcceptanceReport.op_Implicit(false);
    return !WorldVehiclePathGrid.Instance.Passable(((WorldObject) settlement).Tile, vehicle.VehicleDef) ? FloatMenuAcceptanceReport.WithFailReason(TaggedString.op_Implicit(Translator.Translate("Impassable"))) : FloatMenuAcceptanceReport.op_Implicit(true);
  }
}
