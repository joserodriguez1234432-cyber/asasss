// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_ExitMapBest
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class JobGiver_ExitMapBest : JobGiver_ExitMap
{
  protected override bool TryFindGoodExitDest(VehiclePawn vehicle, out IntVec3 cell)
  {
    return CellFinderExtended.TryFindBestExitSpot(vehicle, out cell, (TraverseMode) 0);
  }
}
