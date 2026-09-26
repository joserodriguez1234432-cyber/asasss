// Decompiled with JetBrains decompiler
// Type: Vehicles.WorkGiver_PaintVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public class WorkGiver_PaintVehicle : VehicleWorkGiver
{
  public override JobDef JobDef => JobDefOf_Vehicles.PaintVehicle;

  public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn1)
  {
    return (IEnumerable<Thing>) ((Thing) pawn1).Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>) (pawn2 => pawn2 is VehiclePawn vehiclePawn && vehiclePawn.CanPaintNow));
  }

  public override bool CanBeWorkedOn(VehiclePawn vehicle) => vehicle.CanPaintNow;
}
