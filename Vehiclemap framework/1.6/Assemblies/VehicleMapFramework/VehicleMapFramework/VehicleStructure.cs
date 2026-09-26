// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleStructure
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleStructure : Building
{
  public virtual void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) this).IsOnVehicleMapOf(out vehicle) && ((DamageInfo) ref dinfo).Def != DamageDefOf.Bomb)
      vehicle.TakeDamage(dinfo, ((Thing) this).Position.ToHitCell(vehicle));
    base.PreApplyDamage(ref dinfo, ref absorbed);
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    VehiclePawnWithMap vehicle;
    if (!((Thing) this).IsOnVehicleMapOf(out vehicle))
      return;
    vehicle.mapEdgeCellsDirty = true;
    vehicle.impassableCellsDirty = true;
    this.BackwardCompatibility();
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) this).IsOnVehicleMapOf(out vehicle))
    {
      vehicle.mapEdgeCellsDirty = true;
      vehicle.impassableCellsDirty = true;
    }
    base.DeSpawn(mode);
  }

  private void BackwardCompatibility()
  {
    if (((Thing) this).def != VMF_DefOf.VMF_VehicleStructureEmpty)
      return;
    ((Thing) this).Map.terrainGrid.SetTerrain(((Thing) this).Position, VMF_DefOf.VMF_ImpassableFloor);
    Thing.allowDestroyNonDestroyable = true;
    ((Thing) this).Destroy((DestroyMode) 0);
    Thing.allowDestroyNonDestroyable = false;
  }
}
