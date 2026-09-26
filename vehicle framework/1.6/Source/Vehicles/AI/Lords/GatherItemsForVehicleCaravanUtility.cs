// Decompiled with JetBrains decompiler
// Type: Vehicles.GatherItemsForVehicleCaravanUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public static class GatherItemsForVehicleCaravanUtility
{
  public static List<TransferableOneWay> GetCaravanTransferables(Lord lord)
  {
    return (lord.LordJob as LordJob_FormAndSendVehicles).transferables;
  }

  public static bool IsUsableCarrier(Pawn carrier, Pawn forPawn, bool allowColonists = true)
  {
    return carrier is VehiclePawn pawn ? pawn.IsFormingVehicleCaravan() && !ThingUtility.DestroyedOrNull((Thing) pawn) && ((Thing) pawn).Spawned && ((Thing) pawn).Faction == ((Thing) forPawn).Faction && !FireUtility.IsBurning((Thing) pawn) && pawn.movementStatus != VehicleMovementStatus.Offline && !MassUtility.IsOverEncumbered((Pawn) pawn) : !CaravanHelper.assignedSeats.IsAssigned(carrier) && JobDriver_PrepareCaravan_GatherItems.IsUsableCarrier(carrier, forPawn, allowColonists);
  }
}
