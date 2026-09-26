// Decompiled with JetBrains decompiler
// Type: Vehicles.Paratrooper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public class Paratrooper : IAirDroppable, IExposable
{
  public Pawn pawn;

  Thing IAirDroppable.Thing => (Thing) this.pawn;

  ThingDef IAirDroppable.SkyfallerDef => (ThingDef) SkyfallerDefOf.AirdropParatrooper;

  public Paratrooper(Pawn pawn) => this.pawn = pawn;

  void IExposable.ExposeData()
  {
    Scribe_Deep.Look<Pawn>(ref this.pawn, "pawn", Array.Empty<object>());
  }

  void IAirDroppable.OnDropped(Map map, IntVec3 pos)
  {
  }

  void IAirDroppable.OnFailureToDrop(Map map, IntVec3 simPos)
  {
    new Debouncer((Action) (() =>
    {
      IntVec3 intVec3 = CellFinder.RandomClosewalkCellNear(simPos, map, 12, (Predicate<IntVec3>) null);
      if (((IntVec3) ref intVec3).IsValid && GenGrid.InBounds(intVec3, map))
        GenSpawn.Spawn((Thing) this.pawn, intVec3, map, Rot4.Random, (WipeMode) 0, false, false);
      else
        ThingUtility.DestroyOrPassToWorld((Thing) this.pawn, (DestroyMode) 0);
    }), 4000).Invoke();
  }
}
