// Decompiled with JetBrains decompiler
// Type: Vehicles.Airdrop
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class Airdrop : Building, IThingHolder, IOpenable
{
  public ThingOwner<Thing> innerContainer = new ThingOwner<Thing>();

  bool IOpenable.CanOpen => true;

  int IOpenable.OpenTicks => 180;

  void IOpenable.Open() => ((Thing) this).Destroy((DestroyMode) 0);

  void IThingHolder.GetChildHolders(List<IThingHolder> outChildren)
  {
    ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, (IList<Thing>) this.innerContainer);
  }

  ThingOwner IThingHolder.GetDirectlyHeldThings() => (ThingOwner) this.innerContainer;

  private void DropAllContents()
  {
    if (GenList.NullOrEmpty<Thing>((IList<Thing>) this.innerContainer))
      return;
    for (int index = this.innerContainer.InnerListForReading.Count - 1; index >= 0; --index)
    {
      Thing thing;
      this.innerContainer.TryDrop(this.innerContainer[index], ((Thing) this).Position, ((Thing) this).Map, (ThingPlaceMode) 1, ref thing, new Action<Thing, int>(ItemDropped), (Predicate<IntVec3>) null);
    }

    static void ItemDropped(Thing droppedThing, int _)
    {
      if (!droppedThing.def.IsPleasureDrug)
        return;
      ForbidUtility.SetForbiddenIfOutsideHomeArea(droppedThing);
    }
  }

  public virtual void Destroy(DestroyMode mode = 0)
  {
    this.DropAllContents();
    base.Destroy(mode);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[1]
    {
      (object) this
    });
  }
}
