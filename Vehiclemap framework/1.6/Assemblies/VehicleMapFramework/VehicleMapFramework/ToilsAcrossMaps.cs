// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ToilsAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class ToilsAcrossMaps
{
  public static IEnumerable<Toil> GotoTargetMap(JobDriverAcrossMaps driver, TraverseSpots spots)
  {
    TargetInfo exitSpot = spots.exitSpot;
    TargetInfo enterSpot = spots.enterSpot;
    Pawn pawn = driver.pawn;
    Toil afterEnterMap = Toils_General.Label();
    Toil afterConsumeSpots = Toils_General.Label();
    yield return Toils_Jump.JumpIf(afterConsumeSpots, (Func<bool>) (() => driver.Consumed(spots)));
    if (((TargetInfo) ref exitSpot).IsValid)
    {
      Thing thing = ((TargetInfo) ref exitSpot).Thing;
      CompVehicleEnterSpot comp = thing != null ? ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(thing) : (CompVehicleEnterSpot) null;
      Pawn_AbilityTracker abilities = pawn.abilities;
      Ability ability = abilities != null ? GenCollection.FirstOrDefault<Ability>(abilities.AllAbilitiesForReading, (Predicate<Ability>) (a => a is Ability_MapTraverse)) : (Ability) null;
      Toil afterExitMap = Toils_General.Label();
      yield return Toils_Jump.JumpIf(afterExitMap, (Func<bool>) (() => ((TargetInfo) ref exitSpot).Map == null || ((Thing) pawn).Map != ((TargetInfo) ref exitSpot).Map));
      VehiclePawn vehiclePawn = pawn as VehiclePawn;
      Toil jumpTarget = Toils_General.Label();
      yield return Toils_Jump.JumpIf(jumpTarget, (Func<bool>) (() =>
      {
        VehiclePawn vehiclePawn1 = vehiclePawn;
        if (vehiclePawn1 == null)
          return false;
        CellRect cellRect = Ext_Vehicles.VehicleRect(vehiclePawn1, true);
        return ((CellRect) ref cellRect).Contains(ExitCell(pawn, exitSpot));
      }));
      Toil toil = ToilMaker.MakeToil(nameof (GotoTargetMap));
      toil.initAction = (Action) (() =>
      {
        IntVec3 intVec3 = ExitCell(pawn, exitSpot);
        if (IntVec3.op_Equality(((Thing) pawn).Position, intVec3))
          driver.ReadyForNextToil();
        else
          pawn.pather.StartPath(LocalTargetInfo.op_Implicit(intVec3), (PathEndMode) 1);
      });
      toil.defaultCompleteMode = (ToilCompleteMode) 2;
      yield return toil;
      yield return jumpTarget;
      yield return ToilsAcrossMaps.OpenDoor((JobDriver) driver, exitSpot);
      Toil abilityToil = Toils_General.Do((Action) (() =>
      {
        if (ability == null)
          return;
        AcceptanceReport canCast = ability.CanCast;
        if (!((AcceptanceReport) ref canCast).Accepted)
          return;
        IntVec3 cell = ((TargetInfo) ref enterSpot).Cell;
        if (!((IntVec3) ref cell).IsValid || ((TargetInfo) ref enterSpot).Map == null)
          return;
        if (TargetMapUtility.get_TargetMap((Thing) pawn) != ((TargetInfo) ref enterSpot).Map)
          TargetMapUtility.set_TargetMap((Thing) pawn, ((TargetInfo) ref enterSpot).Map);
        ability.verb.TryStartCastOn(LocalTargetInfo.op_Implicit(((TargetInfo) ref enterSpot).Cell), false, true, false, false);
        driver.JumpToToil(afterEnterMap);
      }));
      abilityToil.handlingFacing = true;
      yield return Toils_Jump.JumpIf(abilityToil, (Func<bool>) (() =>
      {
        if (comp != null)
          return false;
        IntVec3 intVec3 = CrossMapReachabilityUtility.EnterVehiclePosition(exitSpot, vehiclePawn);
        Map groundMap = VehicleMapUtility.get_GroundMap(((TargetInfo) ref exitSpot).Map);
        if (!((IntVec3) ref intVec3).IsValid || !GenGrid.WalkableBy(intVec3, groundMap, pawn))
          return true;
        TerrainDef terrain = GridsUtility.GetTerrain(intVec3, groundMap);
        return terrain != null && terrain.dangerous;
      }));
      yield return ToilsAcrossMaps.TraverseAnimationToil(driver, exitSpot, comp);
      Toil mapCheck = Toils_Jump.JumpIf(afterExitMap, (Func<bool>) (() => ((Thing) pawn).MapHeld != ((TargetInfo) ref exitSpot).Map));
      yield return Toils_Jump.Jump(mapCheck);
      yield return abilityToil;
      yield return mapCheck;
      yield return ToilsAcrossMaps.TraverseToil(exitSpot, comp);
      yield return afterExitMap;
      afterExitMap = (Toil) null;
      jumpTarget = (Toil) null;
      abilityToil = (Toil) null;
      mapCheck = (Toil) null;
    }
    if (((TargetInfo) ref enterSpot).IsValid)
    {
      Thing thing = ((TargetInfo) ref enterSpot).Thing;
      CompVehicleEnterSpot comp = thing != null ? ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(thing) : (CompVehicleEnterSpot) null;
      yield return Toils_Jump.JumpIf(afterEnterMap, (Func<bool>) (() => ((TargetInfo) ref enterSpot).Map == null || ((Thing) pawn).MapHeld != VehicleMapUtility.get_GroundMap(((TargetInfo) ref enterSpot).Map) || ((Thing) pawn).MapHeld == ((TargetInfo) ref enterSpot).Map));
      yield return ToilsAcrossMaps.GotoVehicleEnterSpot(enterSpot);
      yield return ToilsAcrossMaps.OpenDoor((JobDriver) driver, enterSpot);
      yield return ToilsAcrossMaps.TraverseAnimationToil(driver, enterSpot, comp);
      yield return ToilsAcrossMaps.TraverseToil(enterSpot, comp);
      comp = (CompVehicleEnterSpot) null;
    }
    yield return afterEnterMap;
    yield return Toils_General.Do((Action) (() => driver.ConsumeSpots(spots)));
    yield return afterConsumeSpots;

    static IntVec3 ExitCell(Pawn pawn, TargetInfo exitSpot)
    {
      VehiclePawnWithMap vehicle1;
      if (!((TargetInfo) ref exitSpot).Map.IsVehicleMapOf(out vehicle1))
        return IntVec3.Invalid;
      IntVec3 intVec3 = ((TargetInfo) ref exitSpot).Cell;
      if (pawn is VehiclePawn vehicle)
      {
        Rot4 insideMap = ((TargetInfo) ref exitSpot).Cell.DirectionToInsideMap(vehicle1);
        intVec3 = IntVec3.op_Addition(intVec3, IntVec3.op_Multiply(vehicle.HalfLength(), ((Rot4) ref insideMap).FacingCell));
      }
      return intVec3;
    }
  }

  private static Toil GotoVehicleEnterSpot(TargetInfo enterSpot)
  {
    Toil toil = ToilMaker.MakeToil(nameof (GotoVehicleEnterSpot));
    toil.defaultCompleteMode = (ToilCompleteMode) 2;
    Thing thing = ((TargetInfo) ref enterSpot).Thing;
    CompVehicleEnterSpot comp = thing != null ? ThingCompUtility.TryGetComp<CompVehicleEnterSpot>(thing) : (CompVehicleEnterSpot) null;
    LocalTargetInfo dest = new LocalTargetInfo();
    toil.initAction = (Action) (() =>
    {
      dest = GetDest(comp, enterSpot, toil.actor);
      if (LocalTargetInfo.op_Equality(LocalTargetInfo.op_Implicit(((Thing) toil.actor).Position), dest))
        toil.actor.jobs.curDriver.ReadyForNextToil();
      else
        toil.actor.pather.StartPath(dest, (PathEndMode) 1);
    });
    toil.tickIntervalAction = (Action<int>) (_ =>
    {
      LocalTargetInfo dest1 = GetDest(comp, enterSpot, toil.actor);
      if (!LocalTargetInfo.op_Inequality(dest, dest1))
        return;
      dest = dest1;
      toil.actor.pather.StartPath(dest, (PathEndMode) 1);
    });
    ToilFailConditions.FailOn<Toil>(toil, (Func<bool>) (() => !((LocalTargetInfo) ref dest).IsValid));
    ToilFailConditions.FailOn<Toil>(toil, (Func<bool>) (() => !((TargetInfo) ref enterSpot).IsValid || VehicleMapUtility.get_BaseMapOrCaravan(((TargetInfo) ref enterSpot).Map) != VehicleMapUtility.get_BaseMapOrCaravan((Thing) toil.actor)));
    return toil;

    static LocalTargetInfo GetDest(CompVehicleEnterSpot comp, TargetInfo spot, Pawn actor)
    {
      IntVec3 intVec3;
      if (comp == null)
      {
        intVec3 = CrossMapReachabilityUtility.EnterVehiclePosition(spot, actor as VehiclePawn);
      }
      else
      {
        TargetInfo availableAccessSpot = comp.AvailableAccessSpot;
        intVec3 = ((TargetInfo) ref availableAccessSpot).Cell;
      }
      return LocalTargetInfo.op_Implicit(intVec3);
    }
  }

  private static Toil OpenDoor(JobDriver driver, TargetInfo target)
  {
    Toil waitOpen = Toils_General.Wait(0, (TargetIndex) 0);
    waitOpen.handlingFacing = true;
    waitOpen.initAction += (Action) (() =>
    {
      Building_Door door = GridsUtility.GetDoor(((TargetInfo) ref target).Cell, ((TargetInfo) ref target).Map);
      Building_VehicleRamp firstThing = GridsUtility.GetFirstThing<Building_VehicleRamp>(((TargetInfo) ref target).Cell, ((TargetInfo) ref target).Map);
      waitOpen.defaultDuration = Math.Max(door != null ? door.TicksToOpenNow : 0, firstThing != null ? firstThing.TicksToOpenNow : 0);
      driver.ticksLeftThisToil = waitOpen.defaultDuration;
      door?.StartManualOpenBy(waitOpen.actor);
      firstThing?.StartManualOpenBy(waitOpen.actor);
    });
    return waitOpen;
  }

  private static Toil TraverseAnimationToil(
    JobDriverAcrossMaps driver,
    TargetInfo enterSpot,
    [CanBeNull] CompVehicleEnterSpot comp)
  {
    Toil toil = ToilMaker.MakeToil(nameof (TraverseAnimationToil));
    toil.handlingFacing = true;
    int initTick = 0;
    toil.initAction = (Action) (() => initTick = GenTicks.TicksGame);
    toil.tickAction = (Action) (() =>
    {
      Vector3 vector3_1 = Vector3.zero;
      Vector3 vector3_2 = VehicleMapUtility.get_CenterVector3OnGroundMap(enterSpot);
      Vector3 vector3_3;
      if (comp == null)
      {
        IntVec3 intVec3 = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot);
        vector3_3 = ((IntVec3) ref intVec3).ToVector3Shifted();
      }
      else
        vector3_3 = VehicleMapUtility.get_CenterVector3OnGroundMap(comp.AvailableAccessSpot);
      Vector3 vector3_4 = vector3_3;
      Vector3 drawPos = ((Thing) toil.actor).DrawPos;
      if (((TargetInfo) ref enterSpot).Map != ((Thing) toil.actor).Map)
      {
        Vector3 vector3_5 = vector3_4;
        vector3_4 = vector3_2;
        vector3_2 = vector3_5;
        driver.drawOffset = Vector3.zero;
        vector3_1 = Vector3Utility.Yto0(Vector3.op_Subtraction(vector3_2, ((Thing) toil.actor).DrawPos));
      }
      Vector3 vector3_6 = NormalizeFlat(Vector3.op_Subtraction(vector3_4, vector3_2));
      Rot4 rot4 = Pawn_RotationTracker.RotFromAngleBiased(Vector3Utility.AngleFlat(vector3_6));
      VehiclePawnWithMap vehicle;
      if (((Thing) toil.actor).IsOnNonFocusedVehicleMapOf(out vehicle))
      {
        vector3_6 = Vector3Utility.RotatedBy(vector3_6, -VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle));
        if (!toil.actor.Drafted)
        {
          ref Rot4 local = ref rot4;
          int asInt1 = ((Rot4) ref local).AsInt;
          Rot4 rotation = ((Thing) vehicle).Rotation;
          int asInt2 = ((Rot4) ref rotation).AsInt;
          ((Rot4) ref local).AsInt = asInt1 - asInt2;
        }
      }
      ((Thing) toil.actor).Rotation = rot4;
      CompVehicleEnterSpot vehicleEnterSpot = comp;
      float num1 = vehicleEnterSpot != null ? vehicleEnterSpot.MovePerTick(toil.actor) : 0.15f / toil.actor.TicksPerMoveCardinal;
      float num2 = GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(vector3_4, drawPos));
      float num3 = (double) num2 < (double) num1 * (double) num1 ? Mathf.Sqrt(num2) : num1;
      driver.drawOffset = Vector3.op_Addition(vector3_1, Vector3.op_Multiply(Vector3.op_Multiply(vector3_6, num3), (float) (GenTicks.TicksGame - initTick)));
      driver.drawOffset.y = Mathf.Max(Mathf.Max(vector3_2.y, vector3_4.y) - drawPos.y, 0.0f) + 0.5f;
      Building_Door door = GridsUtility.GetDoor(((TargetInfo) ref enterSpot).Cell, ((TargetInfo) ref enterSpot).Map);
      Building_VehicleRamp firstThing = GridsUtility.GetFirstThing<Building_VehicleRamp>(((TargetInfo) ref enterSpot).Cell, ((TargetInfo) ref enterSpot).Map);
      door?.StartManualOpenBy(toil.actor);
      firstThing?.StartManualOpenBy(toil.actor);
      Rect rect1 = Rect.MinMaxRect(vector3_2.x, vector3_2.z, vector3_4.x, vector3_4.z);
      if ((double) ((Rect) ref rect1).xMin > (double) ((Rect) ref rect1).xMax)
      {
        ref Rect local1 = ref rect1;
        ref Rect local2 = ref rect1;
        float xMax = ((Rect) ref rect1).xMax;
        float xMin = ((Rect) ref rect1).xMin;
        double num4 = (double) xMax;
        ((Rect) ref local1).xMin = (float) num4;
        ((Rect) ref local2).xMax = xMin;
      }
      if ((double) ((Rect) ref rect1).yMin > (double) ((Rect) ref rect1).yMax)
      {
        ref Rect local3 = ref rect1;
        ref Rect local4 = ref rect1;
        float yMax = ((Rect) ref rect1).yMax;
        float yMin = ((Rect) ref rect1).yMin;
        double num5 = (double) yMax;
        ((Rect) ref local3).yMin = (float) num5;
        ((Rect) ref local4).yMax = yMin;
      }
      if ((double) num2 >= 0.05000000074505806)
      {
        Rect rect2 = GenUI.ExpandedBy(rect1, 1f);
        if (((Rect) ref rect2).Contains(Vector2Utility.ToVector2(((Thing) toil.actor).DrawPos)))
          return;
      }
      driver.ReadyForNextToil();
    });
    toil.AddFinishAction((Action) (() => driver.drawOffset = Vector3.zero));
    ToilFailConditions.FailOn<Toil>(toil, (Func<bool>) (() =>
    {
      bool flag;
      if (comp != null)
      {
        ThingWithComps parent = comp.parent;
        if (parent == null || ((Thing) parent).Spawned)
        {
          TargetInfo availableAccessSpot = comp.AvailableAccessSpot;
          if (((TargetInfo) ref availableAccessSpot).IsValid)
            goto label_4;
        }
        flag = true;
        goto label_5;
      }
label_4:
      flag = false;
label_5:
      return flag;
    }));
    toil.defaultCompleteMode = (ToilCompleteMode) 5;
    return toil;

    static Vector3 NormalizeFlat(Vector3 vec)
    {
      float num = GenGeo.MagnitudeHorizontal(vec);
      return new Vector3(vec.x / num, 0.0f, vec.z / num);
    }
  }

  private static Toil TraverseToil(TargetInfo enterSpot, CompVehicleEnterSpot comp)
  {
    Toil toil = ToilMaker.MakeToil(nameof (TraverseToil));
    toil.defaultCompleteMode = (ToilCompleteMode) 1;
    toil.initAction = (Action) (() =>
    {
      Rot4 rotation = ((Thing) toil.actor).Rotation;
      VehiclePawn actor = toil.actor as VehiclePawn;
      IntVec3 intVec3;
      Map map;
      if (((Thing) toil.actor).Map != ((TargetInfo) ref enterSpot).Map)
      {
        intVec3 = ((TargetInfo) ref enterSpot).Cell;
        map = ((TargetInfo) ref enterSpot).Map;
      }
      else if (comp != null)
      {
        TargetInfo availableAccessSpot = comp.AvailableAccessSpot;
        if (!((TargetInfo) ref availableAccessSpot).IsValid)
          return;
        intVec3 = ((TargetInfo) ref availableAccessSpot).Cell;
        map = ((TargetInfo) ref availableAccessSpot).Map;
      }
      else
      {
        intVec3 = CrossMapReachabilityUtility.EnterVehiclePosition(enterSpot, actor);
        map = VehicleMapUtility.get_GroundMap(((TargetInfo) ref enterSpot).Map);
      }
      if (!((IntVec3) ref intVec3).IsValid)
        return;
      if (actor != null)
      {
        actor.DeSpawnWithoutJobClearVehicle((DestroyMode) 0);
        GenSpawn.Spawn((Thing) toil.actor, IntVec3.op_Addition(intVec3, IntVec3.op_Multiply(((Rot4) ref rotation).FacingCell, actor.HalfLength())), map, rotation, (WipeMode) 0, false, false);
      }
      else
      {
        toil.actor.DeSpawnWithoutJobClear((DestroyMode) 0);
        if (toil.actor.roping != null)
        {
          foreach (Pawn ropee in toil.actor.roping.Ropees)
            ropee.DeSpawnWithoutJobClear((DestroyMode) 0);
        }
        GenSpawn.Spawn((Thing) toil.actor, intVec3, map, rotation, (WipeMode) 2, false, false);
        if (toil.actor.roping == null)
          return;
        foreach (Thing ropee in toil.actor.roping.Ropees)
          GenSpawn.Spawn(ropee, intVec3, map, rotation, (WipeMode) 2, false, false);
      }
    });
    return ToilFailConditions.FailOn<Toil>(toil, (Func<bool>) (() =>
    {
      IntVec3 cell = ((TargetInfo) ref enterSpot).Cell;
      int num;
      if (((IntVec3) ref cell).IsValid)
      {
        Map map = ((TargetInfo) ref enterSpot).Map;
        if (map != null)
        {
          num = !map.Disposed ? 1 : 0;
          goto label_4;
        }
      }
      num = 0;
label_4:
      return num == 0;
    }));
  }
}
