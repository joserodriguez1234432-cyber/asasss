// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WITab_AerialVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.World;

public abstract class WITab_AerialVehicle : WITab
{
  protected AerialVehicleInFlight SelAerialVehicle => this.SelObject as AerialVehicleInFlight;

  protected List<Pawn> Pawns => this.SelAerialVehicle.Vehicle.AllPawnsAboard;
}
