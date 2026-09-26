// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Building_TurretGunForcedTargetOnly
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Building_TurretGunForcedTargetOnly : Building_TurretGun
{
  private bool canSetForcedTargetThisTick;

  protected virtual bool CanSetForcedTarget
  {
    get
    {
      if (this.canSetForcedTargetThisTick)
        return true;
      return !((LocalTargetInfo) ref ((Building_Turret) this).forcedTarget).IsValid && !(this.interactableComp is CompInteractableRocketswarmLauncher);
    }
  }

  public LocalTargetInfo ForcedTarget
  {
    get => ((Building_Turret) this).forcedTarget;
    set => ((Building_Turret) this).forcedTarget = value;
  }

  protected virtual void Tick()
  {
    if (((Building_Turret) this).AttackVerb is Verb_LaunchZipline attackVerb)
    {
      Thing ziplineEnd = attackVerb.ziplineEnd;
      if (ziplineEnd != null && ziplineEnd.Spawned)
      {
        float num = Vector3Utility.AngleFlat(Vector3.op_Subtraction(ziplineEnd.DrawPos, ((Thing) this).DrawPos));
        VehiclePawnWithMap vehicle;
        if (((Thing) this).IsOnNonFocusedVehicleMapOf(out vehicle))
        {
          LocalTargetInfo currentTarget = ((Building_Turret) this).CurrentTarget;
          if (!((LocalTargetInfo) ref currentTarget).IsValid)
            num = Ext_Math.RotateAngle(num, -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
        }
        this.top.CurRotation = num;
        return;
      }
    }
    this.canSetForcedTargetThisTick = true;
    base.Tick();
    this.canSetForcedTargetThisTick = false;
  }

  protected virtual void TickInterval(int delta)
  {
    ((ThingWithComps) this).TickInterval(delta);
    if (!(((Building_Turret) this).AttackVerb is Verb_LaunchZipline attackVerb))
      return;
    Thing ziplineEnd = attackVerb.ziplineEnd;
    if (ziplineEnd != null && ziplineEnd.Spawned)
    {
      if (!(ziplineEnd is ZiplineEnd) || !LocalTargetInfo.op_Inequality(((Building_Turret) this).forcedTarget, LocalTargetInfo.op_Implicit(ziplineEnd)) && ((Verb) attackVerb).TryFindShootLineFromToOnVehicle(VehicleMapUtility.get_PositionOnBaseMap((Thing) this), LocalTargetInfo.op_Implicit(VehicleMapUtility.get_PositionOnBaseMap(ziplineEnd)), out ShootLine _))
        return;
      ziplineEnd.Destroy((DestroyMode) 0);
    }
    else
    {
      if (((Thing) this).Faction == Faction.OfPlayer || ((Building_Turret) this).IsStunned || this.burstCooldownTicksLeft > 0)
        return;
      this.TryActivateBurst();
    }
  }

  public virtual LocalTargetInfo TryFindNewTarget()
  {
    if (((Thing) this).Faction == Faction.OfPlayer)
      return LocalTargetInfo.Invalid;
    Verb attackVerb = ((Building_Turret) this).AttackVerb;
    TargetingParameters targetParams = attackVerb.targetParams;
    bool canTargetLocations = targetParams.canTargetLocations;
    bool flag = targetParams.canTargetPawns || targetParams.canTargetBuildings || targetParams.canTargetItems || targetParams.canTargetPlants || targetParams.canTargetSelf || targetParams.canTargetFires;
    Map map = ((Thing) this).Map;
    Map groundMap = VehicleMapUtility.get_GroundMap((Thing) this);
    VehicleMapGrid cachedMapComponent = ComponentCache.GetCachedMapComponent<VehicleMapGrid>(groundMap);
    int num1 = GenRadial.NumCellsInRadius(attackVerb.verbProps.EffectiveMinRange(true));
    int num2 = GenRadial.NumCellsInRadius(attackVerb.EffectiveRange);
    IntVec3 onBaseMapSpawned = VehicleMapUtility.get_PositionOnBaseMapSpawned((Thing) this);
    for (int index = num1; index < num2; ++index)
    {
      IntVec3 intVec3 = IntVec3.op_Addition(onBaseMapSpawned, GenRadial.RadialPattern[index]);
      if (GenGrid.InBounds(intVec3, groundMap))
      {
        VehiclePawnWithMap vehicle = cachedMapComponent.VehicleAt(intVec3);
        Map vehicleMap = vehicle?.VehicleMap;
        if (vehicleMap != null && map != vehicleMap && ((Thing) vehicle).Faction != ((Thing) this).Faction)
        {
          IntVec3 vehicleMapCoord = intVec3.ToVehicleMapCoord(vehicle);
          if (GenGrid.InBounds(vehicleMapCoord, vehicleMap))
          {
            TargetMapUtility.set_TargetMap((Thing) this, vehicleMap);
            if (canTargetLocations && attackVerb.ValidateTarget(LocalTargetInfo.op_Implicit(vehicleMapCoord), false) && attackVerb.CanHitTarget(LocalTargetInfo.op_Implicit(vehicleMapCoord)))
              return LocalTargetInfo.op_Implicit(vehicleMapCoord);
            ((Thing) this).RemoveTargetInfo();
            if (flag)
            {
              foreach (Thing thing in vehicleMap.thingGrid.ThingsListAtFast(vehicleMapCoord))
              {
                if (targetParams.CanTarget(TargetInfo.op_Implicit(thing), (ITargetingSource) null) && attackVerb.ValidateTarget(LocalTargetInfo.op_Implicit(thing), false) && attackVerb.CanHitTarget(LocalTargetInfo.op_Implicit(thing)))
                  return LocalTargetInfo.op_Implicit(thing);
              }
            }
          }
        }
      }
    }
    return LocalTargetInfo.Invalid;
  }
}
