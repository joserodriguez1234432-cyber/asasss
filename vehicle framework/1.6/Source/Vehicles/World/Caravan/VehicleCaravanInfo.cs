// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravanInfo
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles.World;

public class VehicleCaravanInfo
{
  public PlanetLayer layer = Find.WorldGrid.FirstLayerOfDef(PlanetLayerDefOf.Surface);
  public List<Pawn> pawns;
  public List<Pawn> vehiclesAndDismountedPawns;
  public float massUsage;
  public float massCapacity;
  public PlanetTile tile;
  public bool caravaning;

  public VehicleCaravanInfo(
    List<Pawn> pawns,
    float massUsage,
    float massCapacity,
    PlanetTile tile)
  {
    this.pawns = pawns;
    this.massUsage = massUsage;
    this.massCapacity = massCapacity;
    this.tile = tile;
  }

  public VehicleCaravanInfo(
    List<TransferableOneWay> transferables,
    float massUsage,
    float massCapacity,
    PlanetTile tile)
    : this(TransferableUtility.GetPawnsFromTransferables(transferables), massUsage, massCapacity, tile)
  {
    this.vehiclesAndDismountedPawns = this.pawns.Where<Pawn>((Func<Pawn, bool>) (pawn => pawn is VehiclePawn || !CaravanHelper.assignedSeats.IsAssigned(pawn))).ToList<Pawn>();
  }

  public VehicleCaravanInfo(Caravan caravan)
    : this(caravan.PawnsListForReading, caravan.MassUsage, caravan.MassCapacity, ((WorldObject) caravan).Tile)
  {
    this.caravaning = true;
    List<Pawn> pawnList;
    if (!(caravan is VehicleCaravan vehicleCaravan))
    {
      pawnList = (List<Pawn>) null;
    }
    else
    {
      pawnList = new List<Pawn>();
      pawnList.AddRange((IEnumerable<Pawn>) vehicleCaravan.Vehicles);
      pawnList.AddRange(vehicleCaravan.DismountedPawns);
    }
    this.vehiclesAndDismountedPawns = pawnList;
  }

  public VehicleCaravanInfo(Dialog_FormCaravan formCaravan)
    : this(formCaravan.transferables, formCaravan.MassUsage, formCaravan.MassCapacity, formCaravan.CurrentTile)
  {
  }
}
