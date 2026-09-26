// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRaidParamsDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleRaidParamsDef : Def
{
  public List<FactionDef> factions;
  public List<PawnInventoryOption> inventory;
  public List<PawnsArrivalModeDef> arrivalModes;

  public bool Allows(Faction faction, PawnsArrivalModeDef arrivalModeDef)
  {
    return !GenList.NullOrEmpty<FactionDef>((IList<FactionDef>) this.factions) && this.factions.Contains(faction.def) && (arrivalModeDef == null || GenList.NullOrEmpty<PawnsArrivalModeDef>((IList<PawnsArrivalModeDef>) this.arrivalModes) || this.arrivalModes.Contains(arrivalModeDef));
  }
}
