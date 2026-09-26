// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Building_Hatch
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Building_Hatch : 
  Building_Bed,
  ISlotGroupParent,
  IStoreSettingsParent,
  IHaulDestination,
  IStorageGroupMember,
  IHaulEnroute,
  ILoadReferenceable
{
  public StorageSettings settings;
  public StorageGroup storageGroup;
  public string label;
  public readonly SlotGroup slotGroup;
  private List<IntVec3> cachedOccupiedCells;
  public static readonly BedInteractionCellSearchPattern customBedInteractionCellsOrder = (BedInteractionCellSearchPattern) new BedInteractionCellSearchPattern3xN();
  private static readonly StringBuilder sb = new StringBuilder();

  public Building_Hatch() => this.slotGroup = new SlotGroup((ISlotGroupParent) this);

  StorageGroup IStorageGroupMember.Group
  {
    get => this.storageGroup;
    set => this.storageGroup = value;
  }

  bool IStorageGroupMember.DrawConnectionOverlay => ((Thing) this).Spawned;

  Map IStorageGroupMember.Map => ((Thing) this).MapHeld;

  string IStorageGroupMember.StorageGroupTag => ((Thing) this).def.building.storageGroupTag;

  StorageSettings IStorageGroupMember.StoreSettings => this.GetStoreSettings();

  StorageSettings IStorageGroupMember.ParentStoreSettings => this.GetParentStoreSettings();

  StorageSettings IStorageGroupMember.ThingStoreSettings => this.settings;

  bool IStorageGroupMember.DrawStorageTab => true;

  bool IStorageGroupMember.ShowRenameButton => ((Thing) this).Faction == Faction.OfPlayer;

  public bool StorageTabVisible => true;

  public bool IgnoreStoredThingsBeauty => ((Thing) this).def.building.ignoreStoredThingsBeauty;

  public SlotGroup GetSlotGroup() => this.slotGroup;

  public virtual void Notify_ReceivedThing(Thing newItem)
  {
    if (((Thing) this).Faction != Faction.OfPlayer || newItem.def.storedConceptLearnOpportunity == null)
      return;
    LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, (OpportunityType) 0);
  }

  public virtual void Notify_LostThing(Thing newItem)
  {
  }

  public virtual IEnumerable<IntVec3> AllSlotCells()
  {
    Building_Hatch buildingHatch = this;
    if (((Thing) buildingHatch).Spawned)
    {
      foreach (IntVec3 intVec3 in GenAdj.CellsOccupiedBy((Thing) buildingHatch))
        yield return intVec3;
    }
  }

  public List<IntVec3> AllSlotCellsList()
  {
    return this.cachedOccupiedCells ?? (this.cachedOccupiedCells = this.AllSlotCells().ToList<IntVec3>());
  }

  public StorageSettings GetStoreSettings()
  {
    return this.storageGroup?.GetStoreSettings() ?? this.settings;
  }

  public StorageSettings GetParentStoreSettings()
  {
    return ((Thing) this).def.building.fixedStorageSettings ?? StorageSettings.EverStorableFixedSettings();
  }

  public void Notify_SettingsChanged()
  {
    if (!((Thing) this).Spawned || this.slotGroup == null)
      return;
    ((Thing) this).Map.listerHaulables.Notify_SlotGroupChanged(this.slotGroup);
  }

  public string SlotYielderLabel() => ((Entity) this).LabelCap;

  public string GroupingLabel => ((Thing) this).def.building.groupingLabel;

  public int GroupingOrder => ((Thing) this).def.building.groupingOrder;

  public bool HaulDestinationEnabled => true;

  public bool Accepts(Thing t) => this.GetStoreSettings().AllowedToAccept(t);

  public int SpaceRemainingFor(ThingDef _)
  {
    int heldThingsCount = this.slotGroup.HeldThingsCount;
    int maxItemsInCell = ((Thing) this).def.building.maxItemsInCell;
    IntVec2 size = ((BuildableDef) ((Thing) this).def).Size;
    int area = ((IntVec2) ref size).Area;
    int num = maxItemsInCell * area;
    return heldThingsCount - num;
  }

  public virtual void PostMake()
  {
    ((ThingWithComps) this).PostMake();
    this.settings = new StorageSettings((IStoreSettingsParent) this);
    if (((Thing) this).def.building.defaultStorageSettings == null)
      return;
    this.settings.CopyFrom(((Thing) this).def.building.defaultStorageSettings);
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    this.cachedOccupiedCells = (List<IntVec3>) null;
    base.SpawnSetup(map, respawningAfterLoad);
    if (this.storageGroup == null || map == this.storageGroup.Map)
      return;
    StorageSettings storeSettings = this.storageGroup.GetStoreSettings();
    this.storageGroup.RemoveMember((IStorageGroupMember) this, true);
    this.storageGroup = (StorageGroup) null;
    this.settings.CopyFrom(storeSettings);
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    base.DeSpawn(mode);
    this.cachedOccupiedCells = (List<IntVec3>) null;
  }

  public virtual void Destroy(DestroyMode mode = 0)
  {
    ((Building) this).Destroy(mode);
    if (this.storageGroup != null)
    {
      this.storageGroup?.RemoveMember((IStorageGroupMember) this, true);
      this.storageGroup = (StorageGroup) null;
    }
    BillUtility.Notify_ISlotGroupRemoved((ISlotGroup) this.slotGroup);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Deep.Look<StorageSettings>(ref this.settings, "settings", new object[1]
    {
      (object) this
    });
    Scribe_References.Look<StorageGroup>(ref this.storageGroup, "storageGroup", false);
    Scribe_Values.Look<string>(ref this.label, "label", (string) null, false);
  }

  public virtual void DrawExtraSelectionOverlays()
  {
    base.DrawExtraSelectionOverlays();
    StorageGroupUtility.DrawSelectionOverlaysFor((IStorageGroupMember) this);
  }

  public virtual string GetInspectString()
  {
    Building_Hatch.sb.Clear();
    Building_Hatch.sb.Append(base.GetInspectString());
    if (((Thing) this).Spawned)
    {
      if (this.storageGroup != null)
      {
        GenString.AppendLineIfNotEmpty(Building_Hatch.sb);
        Building_Hatch.sb.Append($"{Translator.Translate("StorageGroupLabel")}: {GenText.CapitalizeFirst(this.storageGroup.RenamableLabel)} ");
        Building_Hatch.sb.Append(this.storageGroup.MemberCount > 1 ? $"({TranslatorFormattedStringExtensions.Translate("NumBuildings", NamedArgument.op_Implicit(this.storageGroup.MemberCount))})" : $"({Translator.Translate("OneBuilding")})");
      }
      if (this.slotGroup.HeldThings.Any<Thing>())
      {
        GenString.AppendLineIfNotEmpty(Building_Hatch.sb);
        Building_Hatch.sb.Append(TaggedString.op_Implicit(Translator.Translate("StoresThings")));
        Building_Hatch.sb.Append(": ");
        Building_Hatch.sb.Append(GenText.ToCommaList(this.slotGroup.HeldThings.Select<Thing, string>((Func<Thing, string>) (x => ((Entity) x).LabelShortCap)).Distinct<string>(), false, false));
        Building_Hatch.sb.Append(".");
      }
    }
    return Building_Hatch.sb.ToString();
  }

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    Building_Hatch buildingHatch = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in buildingHatch.\u003C\u003En__0())
      yield return gizmo;
    // ISSUE: explicit non-virtual call
    foreach (Gizmo gizmo in StorageSettingsClipboard.CopyPasteGizmosFor(__nonvirtual (buildingHatch.GetStoreSettings())))
      yield return gizmo;
    // ISSUE: explicit non-virtual call
    if (__nonvirtual (buildingHatch.StorageTabVisible) && ((Thing) buildingHatch).MapHeld != null)
    {
      foreach (Gizmo groupMemberGizmo in StorageGroupUtility.StorageGroupMemberGizmos((IStorageGroupMember) buildingHatch))
        yield return groupMemberGizmo;
      if (Find.Selector.NumSelected == 1)
      {
        foreach (Thing heldThing in buildingHatch.slotGroup.HeldThings)
        {
          string str1 = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CommandSelectStoredThing", NamedArgument.op_Implicit(heldThing)));
          TaggedString taggedString = TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition(Translator.Translate("CommandSelectStoredThingDesc"), "\n\n"), ColoredText.Colorize(((Entity) heldThing).LabelCap, ColoredText.TipSectionTitleColor)), "\n\n"), heldThing.GetInspectString());
          string str2 = ((TaggedString) ref taggedString).Resolve();
          Thing thing1 = heldThing;
          Thing thing2 = heldThing;
          yield return ContainingSelectionUtility.CreateSelectStorageGizmo(str1, str2, thing1, thing2, false);
        }
      }
    }
  }
}
