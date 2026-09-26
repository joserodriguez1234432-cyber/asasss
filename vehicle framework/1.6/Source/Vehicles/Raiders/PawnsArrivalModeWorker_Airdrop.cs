// Decompiled with JetBrains decompiler
// Type: Vehicles.Raiders.PawnsArrivalModeWorker_Airdrop
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.Raiders;

internal class PawnsArrivalModeWorker_Airdrop : PawnsArrivalModeWorker
{
  private const int DefaultDelayTicks = 360;
  private const int DefaultTicksToFlyOver = 720;

  public virtual bool CanUseOnMap(Map map) => map.CanAirdropInMap() && base.CanUseOnMap(map);

  public virtual void Arrive(List<Pawn> pawns, IncidentParms parms)
  {
    Map target = parms.target as Map;
    DropZone dropZone = DropZoneFinder.GetDropZone(target, parms.spawnRotation, pawns.Count);
    DropShip.Properties properties = new DropShip.Properties()
    {
      lifetime = 720,
      delayDropByTicks = 360,
      ticksBetweenDrops = 10
    };
    DropShip dropShip = new DropShip(target, dropZone, properties)
    {
      FlyoverSoundDef = SoundDefOf_Vehicles.AerialVehicle_Paratroopers_FlyOver,
      Faction = parms.faction
    };
    foreach (Pawn pawn in pawns)
      dropShip.Add((IAirDroppable) new Paratrooper(pawn));
    target.GetCachedMapComponent<AirdropManager>().Spawn(dropShip);
  }

  public virtual bool TryResolveRaidSpawnCenter(IncidentParms parms)
  {
    parms.spawnRotation = Rot4.Random;
    return true;
  }
}
