// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ExplosionAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[Obsolete("Changed to the patch for vanilla Explosion.")]
public class ExplosionAcrossMaps : Explosion
{
  private static readonly AccessTools.FieldRef<Explosion, List<IntVec3>> cellsToAffect = AccessTools.FieldRefAccess<Explosion, List<IntVec3>>(nameof (cellsToAffect));
  private static readonly FastInvokeHandler AddCellsNeighbors = MethodInvoker.GetHandler(AccessTools.Method(typeof (Explosion), nameof (AddCellsNeighbors), (Type[]) null, (Type[]) null), false);
  private static readonly FastInvokeHandler AffectCell = MethodInvoker.GetHandler(AccessTools.Method(typeof (Explosion), nameof (AffectCell), (Type[]) null, (Type[]) null), false);
  private static readonly FastInvokeHandler GetCellAffectTick = MethodInvoker.GetHandler(AccessTools.Method(typeof (Explosion), nameof (GetCellAffectTick), (Type[]) null, (Type[]) null), false);
  private Dictionary<VehiclePawnWithMap, List<IntVec3>> cellsToAffectOnVehicles = new Dictionary<VehiclePawnWithMap, List<IntVec3>>();

  public virtual void StartExplosion(SoundDef explosionSound, List<Thing> ignoredThings)
  {
    base.StartExplosion(explosionSound, ignoredThings);
    Room room = GridsUtility.GetRoom(((Thing) this).Position, ((Thing) this).Map);
    VehiclePawnWithMap[] array = room != null ? room.ContainedThings<VehiclePawnWithMap>().ToArray<VehiclePawnWithMap>() : (VehiclePawnWithMap[]) null;
    if (GenList.NullOrEmpty<VehiclePawnWithMap>((IList<VehiclePawnWithMap>) array))
      return;
    Map map = ((Thing) this).Map;
    IntVec3 position = ((Thing) this).Position;
    try
    {
      foreach (VehiclePawnWithMap vehiclePawnWithMap in array)
      {
        this.cellsToAffectOnVehicles[vehiclePawnWithMap] = SimplePool<List<IntVec3>>.Get();
        this.cellsToAffectOnVehicles[vehiclePawnWithMap].Clear();
        ((Thing) this).VirtualMapTransfer(vehiclePawnWithMap.VehicleMap, position.ToVehicleMapCoord(vehiclePawnWithMap));
        if (!GenList.NullOrEmpty<IntVec3>((IList<IntVec3>) this.overrideCells))
        {
          foreach (IntVec3 overrideCell in this.overrideCells)
            this.cellsToAffectOnVehicles[vehiclePawnWithMap].Add(overrideCell.ToVehicleMapCoord(vehiclePawnWithMap));
        }
        else
          this.cellsToAffectOnVehicles[vehiclePawnWithMap].AddRange(this.damType.Worker.ExplosionCellsToHit((Explosion) this));
        if (this.applyDamageToExplosionCellsNeighbors)
          ExplosionAcrossMaps.AddCellsNeighbors.Invoke((object) this, new object[1]
          {
            (object) this.cellsToAffectOnVehicles[vehiclePawnWithMap]
          });
        vehiclePawnWithMap.VehicleMap.listerThings.AllThings.ForEach((Action<Thing>) (t => t.Notify_Explosion((Explosion) this)));
      }
    }
    finally
    {
      ((Thing) this).VirtualMapTransfer(map, position);
    }
  }

  protected virtual void Tick()
  {
    int ticksGame = Find.TickManager.TicksGame;
    for (int index = ExplosionAcrossMaps.cellsToAffect.Invoke((Explosion) this).Count - 1; index >= 0; --index)
    {
      if (ticksGame >= (int) ExplosionAcrossMaps.GetCellAffectTick.Invoke((object) this, new object[1]
      {
        (object) ExplosionAcrossMaps.cellsToAffect.Invoke((Explosion) this)[index]
      }))
      {
        try
        {
          ExplosionAcrossMaps.AffectCell.Invoke((object) this, new object[1]
          {
            (object) ExplosionAcrossMaps.cellsToAffect.Invoke((Explosion) this)[index]
          });
        }
        catch (Exception ex)
        {
          Log.Error($"Explosion could not affect cell {(object) ExplosionAcrossMaps.cellsToAffect.Invoke((Explosion) this)[index]}: {(object) ex}");
        }
        ExplosionAcrossMaps.cellsToAffect.Invoke((Explosion) this).RemoveAt(index);
      }
      else
        break;
    }
    Map map = ((Thing) this).Map;
    IntVec3 position = ((Thing) this).Position;
    try
    {
      using (IEnumerator<VehiclePawnWithMap> enumerator = this.cellsToAffectOnVehicles.Keys.Where<VehiclePawnWithMap>((Func<VehiclePawnWithMap, bool>) (vehicle => vehicle != null && vehicle.VehicleMap != null && ((Thing) vehicle).Spawned)).GetEnumerator())
      {
label_16:
        while (enumerator.MoveNext())
        {
          VehiclePawnWithMap current = enumerator.Current;
          ((Thing) this).VirtualMapTransfer(current.VehicleMap, position.ToVehicleMapCoord(current));
          int index = this.cellsToAffectOnVehicles[current].Count - 1;
          while (true)
          {
            if (index >= 0)
            {
              if (ticksGame >= (int) ExplosionAcrossMaps.GetCellAffectTick.Invoke((object) this, new object[1]
              {
                (object) this.cellsToAffectOnVehicles[current][index]
              }))
              {
                if (!current.VehicleMap.Disposed)
                {
                  try
                  {
                    ExplosionAcrossMaps.AffectCell.Invoke((object) this, new object[1]
                    {
                      (object) this.cellsToAffectOnVehicles[current][index]
                    });
                  }
                  catch (Exception ex)
                  {
                    Log.Error($"Explosion could not affect cell {(object) this.cellsToAffectOnVehicles[current][index]}: {(object) ex}");
                  }
                  this.cellsToAffectOnVehicles[current].RemoveAt(index);
                  --index;
                }
                else
                  goto label_16;
              }
              else
                goto label_16;
            }
            else
              goto label_16;
          }
        }
      }
    }
    finally
    {
      ((Thing) this).VirtualMapTransfer(map, position);
      if (!GenCollection.Any<IntVec3>(ExplosionAcrossMaps.cellsToAffect.Invoke((Explosion) this)) && !this.cellsToAffectOnVehicles.Any<KeyValuePair<VehiclePawnWithMap, List<IntVec3>>>((Func<KeyValuePair<VehiclePawnWithMap, List<IntVec3>>, bool>) (v => GenCollection.Any<IntVec3>(v.Value))))
        ((Thing) this).Destroy((DestroyMode) 0);
    }
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    this.cellsToAffectOnVehicles = SimplePool<Dictionary<VehiclePawnWithMap, List<IntVec3>>>.Get();
    this.cellsToAffectOnVehicles.Clear();
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    base.DeSpawn(mode);
    for (int index = 0; index < this.cellsToAffectOnVehicles.Count; ++index)
    {
      VehiclePawnWithMap key = this.cellsToAffectOnVehicles.ElementAt<KeyValuePair<VehiclePawnWithMap, List<IntVec3>>>(index).Key;
      this.cellsToAffectOnVehicles[key].Clear();
      SimplePool<List<IntVec3>>.Return(this.cellsToAffectOnVehicles[key]);
      this.cellsToAffectOnVehicles[key] = (List<IntVec3>) null;
    }
    this.cellsToAffectOnVehicles.Clear();
    SimplePool<Dictionary<VehiclePawnWithMap, List<IntVec3>>>.Return(this.cellsToAffectOnVehicles);
    this.cellsToAffectOnVehicles = (Dictionary<VehiclePawnWithMap, List<IntVec3>>) null;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_NestedCollections.Look<VehiclePawnWithMap, IntVec3>(ref this.cellsToAffectOnVehicles, "cellsToAffectOnVehicles", (LookMode) 3, (LookMode) 1);
  }
}
