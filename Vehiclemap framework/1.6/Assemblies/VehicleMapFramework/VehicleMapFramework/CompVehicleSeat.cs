// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompVehicleSeat
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class CompVehicleSeat : CompBuildableUpgrades, IAttackTarget, ILoadReferenceable
{
  private readonly List<(VehicleRoleHandler, VehicleUpgrade.RoleUpgrade)> handlers = new List<(VehicleRoleHandler, VehicleUpgrade.RoleUpgrade)>();

  Thing IAttackTarget.Thing => (Thing) this.parent;

  LocalTargetInfo IAttackTarget.TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

  float IAttackTarget.TargetPriorityFactor => 1f;

  bool IAttackTarget.ThreatDisabled(IAttackTargetSearcher _)
  {
    return !this.handlers.SelectMany<(VehicleRoleHandler, VehicleUpgrade.RoleUpgrade), Pawn>((Func<(VehicleRoleHandler, VehicleUpgrade.RoleUpgrade), IEnumerable<Pawn>>) (h => (IEnumerable<Pawn>) h.Item1.thingOwner.InnerListForReading)).Any<Pawn>();
  }

  string ILoadReferenceable.GetUniqueLoadID()
  {
    return ((Thing) this.parent).GetUniqueLoadID() + "_CompVehicleSeat";
  }

  public virtual IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
  {
    CompVehicleSeat compVehicleSeat = this;
    VehiclePawnWithMap vehicle;
    TargetInfo exitSpot;
    TargetInfo enterSpot;
    List<TraverseSpots> spotsQueue;
    if (((Thing) compVehicleSeat.parent).IsOnVehicleMapOf(out vehicle) && selPawn.CanReach(LocalTargetInfo.op_Implicit((Thing) compVehicleSeat.parent), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, ((Thing) compVehicleSeat.parent).Map, out exitSpot, out enterSpot, out spotsQueue))
    {
      foreach (FloatMenuOption floatMenuOption in vehicle.handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => handler.AreSlotsAvailableAndReservable && GenCollection.Any<UpgradeID>(this.handlerUniqueIDs, (Predicate<UpgradeID>) (h => h.id == handler.uniqueID)))).Select(handler =>
      {
        VehicleRoleHandler vehicleRoleHandler = handler;
        Map map = ((Thing) vehicle).Map;
        VehicleReservationManager cachedMapComponent = map != null ? ComponentCache.GetCachedMapComponent<VehicleReservationManager>(map) : (VehicleReservationManager) null;
        return new
        {
          handler = vehicleRoleHandler,
          reservationManager = cachedMapComponent
        };
      }).Select(_param1 => new
      {
        \u003C\u003Eh__TransparentIdentifier0 = _param1,
        canOperate = _param1.handler.CanOperateRole(selPawn)
      }).Select(_param1 => new
      {
        \u003C\u003Eh__TransparentIdentifier1 = _param1,
        reservedCount = _param1.\u003C\u003Eh__TransparentIdentifier0.reservationManager?.GetReservation<VehicleHandlerReservation>((VehiclePawn) vehicle)?.ClaimantsOnHandler(_param1.\u003C\u003Eh__TransparentIdentifier0.handler).GetValueOrDefault()
      }).Select(_param1 => new
      {
        \u003C\u003Eh__TransparentIdentifier2 = _param1,
        label = _param1.\u003C\u003Eh__TransparentIdentifier1.canOperate ? TranslatorFormattedStringExtensions.Translate("VF_BoardVehicle", NamedArgument.op_Implicit(_param1.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.handler.role.label), NamedArgument.op_Implicit((_param1.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.handler.role.Slots - (((ThingOwner) _param1.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.handler.thingOwner).Count + _param1.reservedCount)).ToString())) : TranslatorFormattedStringExtensions.Translate("VF_BoardVehicleGroupFail", NamedArgument.op_Implicit(_param1.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.handler.role.label), NamedArgument.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_BoardFailureNonCombatant", NamedArgument.op_Implicit(((Entity) selPawn).LabelShort))))
      }).Select(_param1 => new FloatMenuOption(TaggedString.op_Implicit(_param1.label), (Action) (() =>
      {
        Job jobAcrossMaps = JobMaker.MakeJob(VMF_DefOf.VMF_BoardAcrossMaps, LocalTargetInfo.op_Implicit((Thing) this.parent)).SetSpotsToJobAcrossMaps(selPawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue);
        vehicle.GiveLoadJob(selPawn, _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.handler);
        selPawn.jobs.TryTakeOrderedJob(jobAcrossMaps, new JobTag?((JobTag) 6), false);
        if (!((Thing) selPawn).Spawned)
          return;
        _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.reservationManager?.Reserve<VehicleRoleHandler, VehicleHandlerReservation>((VehiclePawn) vehicle, selPawn, selPawn.CurJob, _param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.\u003C\u003Eh__TransparentIdentifier0.handler);
      }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
      {
        Disabled = !_param1.\u003C\u003Eh__TransparentIdentifier2.\u003C\u003Eh__TransparentIdentifier1.canOperate
      }))
        yield return floatMenuOption;
    }
  }

  public override IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompVehicleSeat compVehicleSeat = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in compVehicleSeat.\u003C\u003En__0())
      yield return gizmo;
    VehiclePawnWithMap vehicle;
    if (((Thing) compVehicleSeat.parent).IsOnVehicleMapOf(out vehicle))
    {
      CellRect cellRect = GenAdj.OccupiedRect((Thing) compVehicleSeat.parent);
      cellRect = ((CellRect) ref cellRect).ExpandedBy(1);
      bool exitBlocked = !Ext_IEnumerable.NotNullAndAny<IntVec3>(((CellRect) ref cellRect).EdgeCells, (Predicate<IntVec3>) (cell => GenGrid.Walkable(cell, ((Thing) this.parent).Map)));
      foreach (Command_ActionPawnDrawer actionPawnDrawer in compVehicleSeat.handlerUniqueIDs.Select<UpgradeID, VehicleRoleHandler>((Func<UpgradeID, VehicleRoleHandler>) (keyIDPair => GenCollection.FirstOrDefault<VehicleRoleHandler>(vehicle.handlers, (Predicate<VehicleRoleHandler>) (h => h.uniqueID == keyIDPair.id)))).Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => handler != null)).SelectMany((Func<VehicleRoleHandler, IEnumerable<Pawn>>) (handler => ((IEnumerable) handler.thingOwner).Cast<Pawn>()), (handler, pawn) => new
      {
        handler = handler,
        pawn = pawn
      }).Where(_param1 => !vehicle.Drafted || !((Enum) (object) _param1.handler.role.HandlingTypes).HasFlag((Enum) (object) (HandlingType) 1) || !((Thing) vehicle).Spawned).Select(_param1 =>
      {
        Command_ActionPawnDrawer gizmosExtra = new Command_ActionPawnDrawer();
        ((Command) gizmosExtra).defaultLabel = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisembarkSinglePawn", NamedArgument.op_Implicit(((Entity) _param1.pawn).LabelShort)));
        ((Command) gizmosExtra).groupable = false;
        gizmosExtra.pawn = _param1.pawn;
        ((Command_Action) gizmosExtra).action = (Action) (() =>
        {
          CaravanUtility.GetCaravan((Thing) _param1.pawn)?.RemovePawn(_param1.pawn);
          if (Find.WorldPawns.Contains(_param1.pawn))
            Find.WorldPawns.RemovePawn(_param1.pawn);
          vehicle.DisembarkPawn(_param1.pawn);
        });
        return gizmosExtra;
      }))
      {
        if (exitBlocked)
          ((Gizmo) actionPawnDrawer).Disable(TaggedString.op_Implicit(Translator.Translate("VF_DisembarkNoExit")));
        yield return (Gizmo) actionPawnDrawer;
      }
      foreach (Gizmo gizmo in ((ThingWithComps) vehicle).AllComps.OfType<CompOpacityOverlay>().SelectMany<CompOpacityOverlay, Gizmo>((Func<CompOpacityOverlay, IEnumerable<Gizmo>>) (c => ((ThingComp) c).CompGetGizmosExtra())))
        yield return gizmo;
    }
  }

  public override void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
        return;
      vehicle.CompVehicleTurrets?.RecacheTurretPermissions();
      vehicle.RecachePawnCount();
      this.handlers.AddRange(vehicle.handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (h => GenCollection.Any<UpgradeID>(this.handlerUniqueIDs, (Predicate<UpgradeID>) (i => h.uniqueID == i.id)))).Select<VehicleRoleHandler, (VehicleRoleHandler, VehicleUpgrade.RoleUpgrade)>((Func<VehicleRoleHandler, (VehicleRoleHandler, VehicleUpgrade.RoleUpgrade)>) (h => (h, this.Props.upgrades.OfType<VehicleUpgrade>().SelectMany<VehicleUpgrade, VehicleUpgrade.RoleUpgrade>((Func<VehicleUpgrade, IEnumerable<VehicleUpgrade.RoleUpgrade>>) (u => (IEnumerable<VehicleUpgrade.RoleUpgrade>) u.roles)).FirstOrDefault<VehicleUpgrade.RoleUpgrade>((Func<VehicleUpgrade.RoleUpgrade, bool>) (r => r?.key == h.role.key))))));
    }));
  }

  public override void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    base.PostDeSpawn(map, mode);
    this.handlers.Clear();
  }

  public virtual void PostDraw()
  {
    base.PostDraw();
    VehiclePawnWithMap vehicle;
    if (VehicleMapFramework.VehicleMapFramework.settings.drawPlanet || !((Thing) this.parent).IsOnVehicleMapOf(out vehicle) || ((Thing) vehicle).Spawned || GenList.NullOrEmpty<(VehicleRoleHandler, VehicleUpgrade.RoleUpgrade)>((IList<(VehicleRoleHandler, VehicleUpgrade.RoleUpgrade)>) this.handlers))
      return;
    foreach ((VehicleRoleHandler, VehicleUpgrade.RoleUpgrade) handler in this.handlers)
    {
      if (handler.Item1.role.PawnRenderer != null)
      {
        foreach (Pawn pawn in handler.Item1.thingOwner)
          pawn.Drawer.renderer.RenderPawnAt(Vector3.op_Addition(((Thing) this.parent).DrawPos, handler.Item2.pawnRenderer.DrawOffsetFor(Rot8.op_Implicit(((Thing) this.parent).BaseRotation()))), new Rot4?(handler.Item1.role.PawnRenderer.RotFor(Rot8.op_Implicit(((Thing) this.parent).BaseRotation()))), false);
      }
    }
  }

  public virtual string CompInspectStringExtra()
  {
    if ((double) VehicleMapFramework.VehicleMapFramework.settings.weightFactor == 0.0)
      return (string) null;
    VehiclePawnWithMap vehicle;
    if (!((Thing) this.parent).IsOnVehicleMapOf(out vehicle))
      return (string) null;
    string str = base.CompInspectStringExtra();
    float statValue = vehicle.GetStatValue(VMF_DefOf.MaximumPayload);
    return $"{str}{$"{((Def) VMF_DefOf.MaximumPayload).LabelCap}:"} {GenText.ToStringEnsureThreshold(VehicleMapUtility.VehicleMapMass(vehicle) * VehicleMapFramework.VehicleMapFramework.settings.weightFactor, 2f, 0)} /{$" {GenText.ToStringEnsureThreshold(statValue, 2f, 0)} {Translator.Translate("kg")}"}";
  }
}
