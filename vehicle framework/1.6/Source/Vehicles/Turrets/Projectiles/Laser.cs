// Decompiled with JetBrains decompiler
// Type: Vehicles.Laser
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

public class Laser : Bullet
{
  private SpawnerPropertiesDefModExtension modExtension;

  public SpawnerPropertiesDefModExtension SpawnProps
  {
    get
    {
      if (this.modExtension == null)
      {
        this.modExtension = ((Def) ((Thing) this).def).GetModExtension<SpawnerPropertiesDefModExtension>();
        if (this.modExtension == null)
          Log.Error("Must include <type>SpawnerPropertiesDefModExtension</type> for projectile of type <type>Laser</type>");
      }
      return this.modExtension;
    }
  }

  protected virtual void Impact(Thing hitThing, bool blockedByShield = false)
  {
    Map map = ((Thing) this).Map;
    IntVec3 position = ((Thing) this).Position;
    base.Impact(hitThing, false);
    if ((double) Rand.Range(0, 1) > (double) this.SpawnProps.chanceToSpawnThing)
      return;
    if (((Def) this.SpawnProps.thingToSpawn).defName == "Fire")
    {
      float num = Rand.Range(0.25f, 0.925f);
      if (FireUtility.TryStartFireIn(position, map, num, (Thing) this, (SimpleCurve) null))
        return;
      foreach (Thing thing in map.thingGrid.ThingsAt(position))
        FireUtility.TryAttachFire(thing, num, (Thing) this);
    }
    else
      GenSpawn.Spawn(this.SpawnProps.thingToSpawn, position, map, (WipeMode) 0);
  }
}
