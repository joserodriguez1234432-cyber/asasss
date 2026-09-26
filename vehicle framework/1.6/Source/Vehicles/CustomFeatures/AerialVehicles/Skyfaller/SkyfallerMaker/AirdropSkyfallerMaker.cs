// Decompiled with JetBrains decompiler
// Type: Vehicles.AirdropSkyfallerMaker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public static class AirdropSkyfallerMaker
{
  public static Skyfaller MakeAirdrop(IAirDroppable airDroppable, float angle)
  {
    Skyfaller skyfaller = (Skyfaller) ThingMaker.MakeThing(airDroppable.SkyfallerDef, (ThingDef) null);
    skyfaller.innerContainer.TryAddOrTransfer(airDroppable.Thing, true);
    skyfaller.angle = angle;
    return skyfaller;
  }

  public static AirdropSkyfaller MakeAirdrop(
    AirdropDef airdropDef,
    [NotNull] Thing thing,
    in AirdropProperties props)
  {
    AirdropDef airdropDef1 = airdropDef;
    List<Thing> contents = new List<Thing>(1);
    contents.Add(thing);
    ref readonly AirdropProperties local = ref props;
    return AirdropSkyfallerMaker.MakeAirdrop(airdropDef1, contents, in local);
  }

  public static AirdropSkyfaller MakeAirdrop(
    AirdropDef airdropDef,
    [NotNull] List<Thing> contents,
    in AirdropProperties props)
  {
    AirdropSkyfaller airdropSkyfaller = (AirdropSkyfaller) ThingMaker.MakeThing((ThingDef) airdropDef, (ThingDef) null);
    if (contents.Count > 0 && !props.packIntoContainer)
    {
      Thing content = contents[0];
      if (content.Spawned)
        ((Entity) content).DeSpawn((DestroyMode) 0);
      airdropSkyfaller.innerContainer.TryAddOrTransfer(content, true);
    }
    else
    {
      Airdrop airdrop = (Airdrop) null;
      if (props.packIntoContainer)
        airdrop = (Airdrop) ThingMaker.MakeThing(ThingDefOf_Vehicles.Airdrop, (ThingDef) null);
      foreach (Thing content in contents)
        AirdropSkyfallerMaker.TryPackInto(content, props.packIntoContainer ? (ThingOwner) (object) airdrop.innerContainer : airdropSkyfaller.innerContainer);
      if (props.packIntoContainer)
        airdropSkyfaller.innerContainer.TryAdd((Thing) airdrop, true);
    }
    return airdropSkyfaller;
  }

  private static bool TryPackInto(Thing thing, ThingOwner container)
  {
    if (thing == null || container.TryAddOrTransfer(thing, true))
      return true;
    Log.Error($"Could not add {thing} to Airdrop.");
    thing.Destroy((DestroyMode) 0);
    return false;
  }
}
