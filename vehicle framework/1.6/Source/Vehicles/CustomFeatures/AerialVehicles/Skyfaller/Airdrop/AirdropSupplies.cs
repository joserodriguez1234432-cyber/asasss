// Decompiled with JetBrains decompiler
// Type: Vehicles.AirdropSupplies
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class AirdropSupplies : IAirDroppable, IExposable
{
  private Airdrop airdrop = (Airdrop) ThingMaker.MakeThing(ThingDefOf_Vehicles.Airdrop, (ThingDef) null);

  Thing IAirDroppable.Thing => (Thing) this.airdrop;

  ThingDef IAirDroppable.SkyfallerDef => (ThingDef) SkyfallerDefOf.AirdropPackage;

  public AirdropSupplies()
  {
  }

  public AirdropSupplies(IEnumerable<Thing> things)
  {
    foreach (Thing thing in things)
      this.Pack(thing);
  }

  public void Pack(Thing thing)
  {
    ((ThingOwner) this.airdrop.innerContainer).TryAddOrTransfer(thing, true);
  }

  void IExposable.ExposeData()
  {
    Scribe_Deep.Look<Airdrop>(ref this.airdrop, "airdrop", Array.Empty<object>());
  }

  void IAirDroppable.OnDropped(Map map, IntVec3 pos)
  {
  }

  void IAirDroppable.OnFailureToDrop(Map map, IntVec3 simPos)
  {
    IntVec3 intVec3 = DropCellFinder.TradeDropSpot(map);
    if (!GenGrid.InBounds(intVec3, map))
      return;
    GenSpawn.Spawn((Thing) AirdropSkyfallerMaker.MakeAirdrop((IAirDroppable) this, (float) Rand.Range(-15, 15)), intVec3, map, (WipeMode) 0);
  }
}
