// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompBuildableContainer
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework;

public class CompBuildableContainer : CompTransporter
{
  private readonly AccessTools.FieldRef<CompTransporter, bool> notifiedCantLoadMore = AccessTools.FieldRefAccess<CompTransporter, bool>(nameof (notifiedCantLoadMore));
  private bool gatherFromBaseMap;

  public VehiclePawnWithMap Vehicle
  {
    get
    {
      VehiclePawnWithMap vehicle;
      return !((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle) ? (VehiclePawnWithMap) null : vehicle;
    }
  }

  public bool AnyPawnCanLoadAnythingNow
  {
    get
    {
      if (!this.AnythingLeftToLoad || !((Thing) ((ThingComp) this).parent).Spawned)
        return false;
      IReadOnlyList<Pawn> allPawnsSpawned = ((Thing) ((ThingComp) this).parent).BaseMap().mapPawns.AllPawnsSpawned;
      for (int index = 0; index < allPawnsSpawned.Count; ++index)
      {
        if (allPawnsSpawned[index].CurJobDef == JobDefOf.HaulToTransporter)
        {
          CompTransporter transporter = ((JobDriver_HaulToTransporter) allPawnsSpawned[index].jobs.curDriver).Transporter;
          if (transporter != null && transporter.groupID == this.groupID)
            return true;
        }
        if (allPawnsSpawned[index].CurJobDef == JobDefOf.EnterTransporter)
        {
          CompTransporter transporter = ((JobDriver_EnterTransporter) allPawnsSpawned[index].jobs.curDriver).Transporter;
          if (transporter != null && transporter.groupID == this.groupID)
            return true;
        }
      }
      List<CompTransporter> compTransporterList = this.TransportersInGroup(((Thing) ((ThingComp) this).parent).Map);
      if (compTransporterList == null)
        return false;
      for (int index = 0; index < allPawnsSpawned.Count; ++index)
      {
        if (allPawnsSpawned[index].mindState.duty != null && allPawnsSpawned[index].mindState.duty.transportersGroup == this.groupID)
        {
          CompTransporter myTransporter = JobGiver_EnterTransporter.FindMyTransporter(compTransporterList, allPawnsSpawned[index]);
          if (myTransporter != null && allPawnsSpawned[index].CanReach(LocalTargetInfo.op_Implicit((Thing) ((ThingComp) myTransporter).parent), (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, myTransporter.Map))
            return true;
        }
      }
      for (int index1 = 0; index1 < allPawnsSpawned.Count; ++index1)
      {
        if (allPawnsSpawned[index1].IsColonist)
        {
          for (int index2 = 0; index2 < compTransporterList.Count; ++index2)
          {
            if (LoadTransportersJobUtility.HasJobOnTransporter(allPawnsSpawned[index1], compTransporterList[index2]))
              return true;
          }
        }
      }
      return false;
    }
  }

  public bool GatherFromBaseMap => this.gatherFromBaseMap;

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    Delay.AfterNTicks(1, (Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      if (((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle))
      {
        ThingOwner innerContainer = this.innerContainer;
        this.innerContainer = (ThingOwner) ((Pawn) vehicle).inventory.innerContainer;
        if (innerContainer != null)
          this.innerContainer.TryAddRangeOrTransfer((IEnumerable<Thing>) innerContainer, true, false);
        this.massCapacityOverride = vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
        vehicle.ContainerComps.Add(this);
      }
      else
      {
        this.innerContainer = (ThingOwner) new ThingOwner<Thing>((IThingHolder) this);
        this.massCapacityOverride = 0.0f;
      }
    }));
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    VehiclePawnWithMap vehicle;
    if (map.IsVehicleMapOf(out vehicle))
      vehicle.ContainerComps.Remove(this);
    if (!this.CancelLoad(map) || this.Shuttle != null)
      return;
    Messages.Message(TaggedString.op_Implicit(this.Props.max1PerGroup ? Translator.Translate("MessageTransporterSingleLoadCanceled_TransporterDestroyed") : Translator.Translate("MessageTransportersLoadCanceled_TransporterDestroyed")), MessageTypeDefOf.NegativeEvent, true);
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompBuildableContainer buildableContainer = this;
    if (buildableContainer.Vehicle != null)
    {
      if (!GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) buildableContainer.leftToLoad))
      {
        Command_Action commandAction = new Command_Action();
        ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("DesignatorCancel"));
        ((Command) commandAction).icon = (Texture) buildableContainer.Vehicle.VehicleDef.CancelCargoIcon;
        // ISSUE: reference to a compiler-generated method
        commandAction.action = new Action(buildableContainer.\u003CCompGetGizmosExtra\u003Eb__10_0);
        yield return (Gizmo) commandAction;
      }
      Command_Action commandAction1 = new Command_Action();
      ((Command) commandAction1).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_LoadCargo"));
      ((Command) commandAction1).icon = (Texture) buildableContainer.Vehicle.VehicleDef.LoadCargoIcon;
      // ISSUE: reference to a compiler-generated method
      commandAction1.action = new Action(buildableContainer.\u003CCompGetGizmosExtra\u003Eb__10_1);
      yield return (Gizmo) commandAction1;
      Command_Toggle commandToggle = new Command_Toggle();
      ((Command) commandToggle).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_GatherFromBaseMap"));
      ((Command) commandToggle).icon = (Texture) buildableContainer.Vehicle.VehicleDef.LoadCargoIcon;
      // ISSUE: reference to a compiler-generated method
      commandToggle.isActive = new Func<bool>(buildableContainer.\u003CCompGetGizmosExtra\u003Eb__10_2);
      // ISSUE: reference to a compiler-generated method
      commandToggle.toggleAction = new Action(buildableContainer.\u003CCompGetGizmosExtra\u003Eb__10_3);
      yield return (Gizmo) commandToggle;
    }
  }

  public void Notify_ThingAdded(Thing t)
  {
    this.SubtractFromToLoadList(t, t.stackCount, false);
    if (((Thing) ((ThingComp) this).parent).Spawned && this.Props.pawnLoadedSound != null && t is Pawn)
      SoundStarter.PlayOneShot(this.Props.pawnLoadedSound, SoundInfo.op_Implicit(new TargetInfo(((Thing) ((ThingComp) this).parent).Position, ((Thing) ((ThingComp) this).parent).Map, false)));
    QuestUtility.SendQuestTargetSignals(((Thing) ((ThingComp) this).parent).questTags, "ThingAdded", NamedArgumentUtility.Named((object) t, "SUBJECT"));
    if (!GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) this.leftToLoad))
      return;
    this.groupID = -1;
  }

  public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
  {
    this.SubtractFromToLoadList(t, mergedCount, false);
    if (!GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) this.leftToLoad))
      return;
    this.groupID = -1;
  }

  public virtual void PostExposeData()
  {
    Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
    VehiclePawnWithMap vehicle;
    if (!((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle) || this.innerContainer != ((Pawn) vehicle)?.inventory?.innerContainer)
      Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[1]
      {
        (object) this
      });
    Scribe_Collections.Look<TransferableOneWay>(ref this.leftToLoad, "leftToLoad", (LookMode) 2, Array.Empty<object>());
    Scribe_Values.Look<bool>(ref this.notifiedCantLoadMore.Invoke((CompTransporter) this), "notifiedCantLoadMore", false, false);
    Scribe_Values.Look<float>(ref this.massCapacityOverride, "massCapacityOverride", 0.0f, false);
    Scribe_Values.Look<bool>(ref this.gatherFromBaseMap, "gatherFromBaseMap", false, false);
  }
}
