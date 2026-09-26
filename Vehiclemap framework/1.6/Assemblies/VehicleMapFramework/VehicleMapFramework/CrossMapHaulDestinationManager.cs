// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapHaulDestinationManager
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CrossMapHaulDestinationManager(Map map) : MapComponent(map)
{
  public List<IHaulDestination> AllHaulDestinationsListInPriorityOrder { get; } = new List<IHaulDestination>();

  public List<SlotGroup> AllGroupsListForReading { get; } = new List<SlotGroup>();

  public List<SlotGroup> AllGroupsListInPriorityOrder => this.AllGroupsListForReading;

  public virtual void MapComponentTick()
  {
    base.MapComponentTick();
    if (!Gen.IsHashIntervalTick(this.map, 60))
      return;
    Map map = this.map.BaseMap();
    if (this.map == map)
    {
      VehiclePawnWithMap vehicle;
      if (!this.map.IsVehicleMapOf(out vehicle))
        return;
      WorldObject orStashedVehicle = vehicle.VehicleCaravanOrStashedVehicle;
      CaravanHaulDestinationManager destinationManager;
      if (orStashedVehicle == null || !orStashedVehicle.TryGetComponent<CaravanHaulDestinationManager>(ref destinationManager))
        return;
      List<IHaulDestination> destinations = destinationManager.AllHaulDestinationsListInPriorityOrder;
      CollectionExtensions.Do<IHaulDestination>(this.AllHaulDestinationsListInPriorityOrder.Where<IHaulDestination>((Func<IHaulDestination, bool>) (h => !destinations.Contains(h))), new Action<IHaulDestination>(destinationManager.AddHaulDestination));
    }
    CrossMapHaulDestinationManager cachedMapComponent = ComponentCache.GetCachedMapComponent<CrossMapHaulDestinationManager>(map);
    List<IHaulDestination> baseMapDestinations = cachedMapComponent.AllHaulDestinationsListInPriorityOrder;
    CollectionExtensions.Do<IHaulDestination>(this.AllHaulDestinationsListInPriorityOrder.Where<IHaulDestination>((Func<IHaulDestination, bool>) (h => !baseMapDestinations.Contains(h))), new Action<IHaulDestination>(cachedMapComponent.AddHaulDestination));
  }

  public void AddHaulDestination(IHaulDestination haulDestination)
  {
    if (this.AllHaulDestinationsListInPriorityOrder.Contains(haulDestination))
      return;
    this.AllHaulDestinationsListInPriorityOrder.Add(haulDestination);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GenList.InsertionSort<IHaulDestination>((IList<IHaulDestination>) this.AllHaulDestinationsListInPriorityOrder, CrossMapHaulDestinationManager.\u003C\u003EO.\u003C0\u003E__CompareHaulDestinationPrioritiesDescending ?? (CrossMapHaulDestinationManager.\u003C\u003EO.\u003C0\u003E__CompareHaulDestinationPrioritiesDescending = new Comparison<IHaulDestination>(CrossMapHaulDestinationManager.CompareHaulDestinationPrioritiesDescending)));
    if (!(haulDestination is ISlotGroupParent islotGroupParent))
      return;
    SlotGroup slotGroup = islotGroupParent.GetSlotGroup();
    if (slotGroup == null)
    {
      VMF_Log.Error("ISlotGroupParent gave null slot group: " + Gen.ToStringSafe<ISlotGroupParent>(islotGroupParent));
    }
    else
    {
      this.AllGroupsListForReading.Add(slotGroup);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      GenList.InsertionSort<SlotGroup>((IList<SlotGroup>) this.AllGroupsListForReading, CrossMapHaulDestinationManager.\u003C\u003EO.\u003C1\u003E__CompareSlotGroupPrioritiesDescending ?? (CrossMapHaulDestinationManager.\u003C\u003EO.\u003C1\u003E__CompareSlotGroupPrioritiesDescending = new Comparison<SlotGroup>(CrossMapHaulDestinationManager.CompareSlotGroupPrioritiesDescending)));
    }
  }

  public void RemoveHaulDestination(IHaulDestination haulDestination)
  {
    this.AllHaulDestinationsListInPriorityOrder.Remove(haulDestination);
    if (!(haulDestination is ISlotGroupParent islotGroupParent))
      return;
    SlotGroup slotGroup = islotGroupParent.GetSlotGroup();
    if (slotGroup == null)
      VMF_Log.Error("ISlotGroupParent gave null slot group: " + Gen.ToStringSafe<ISlotGroupParent>(islotGroupParent));
    else
      this.AllGroupsListForReading.Remove(slotGroup);
  }

  public void Notify_HaulDestinationChangedPriority()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GenList.InsertionSort<IHaulDestination>((IList<IHaulDestination>) this.AllHaulDestinationsListInPriorityOrder, CrossMapHaulDestinationManager.\u003C\u003EO.\u003C0\u003E__CompareHaulDestinationPrioritiesDescending ?? (CrossMapHaulDestinationManager.\u003C\u003EO.\u003C0\u003E__CompareHaulDestinationPrioritiesDescending = new Comparison<IHaulDestination>(CrossMapHaulDestinationManager.CompareHaulDestinationPrioritiesDescending)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GenList.InsertionSort<SlotGroup>((IList<SlotGroup>) this.AllGroupsListForReading, CrossMapHaulDestinationManager.\u003C\u003EO.\u003C1\u003E__CompareSlotGroupPrioritiesDescending ?? (CrossMapHaulDestinationManager.\u003C\u003EO.\u003C1\u003E__CompareSlotGroupPrioritiesDescending = new Comparison<SlotGroup>(CrossMapHaulDestinationManager.CompareSlotGroupPrioritiesDescending)));
    Map map = this.map.BaseMap();
    if (this.map == map)
      return;
    ComponentCache.GetCachedMapComponent<CrossMapHaulDestinationManager>(map).Notify_HaulDestinationChangedPriority();
  }

  private static int CompareHaulDestinationPrioritiesDescending(
    IHaulDestination a,
    IHaulDestination b)
  {
    return ((int) ((IStoreSettingsParent) b).GetStoreSettings().Priority).CompareTo((int) ((IStoreSettingsParent) a).GetStoreSettings().Priority);
  }

  private static int CompareHaulSourcePrioritiesDescending(IHaulSource a, IHaulSource b)
  {
    return ((int) ((IStoreSettingsParent) b).GetStoreSettings().Priority).CompareTo((int) ((IStoreSettingsParent) a).GetStoreSettings().Priority);
  }

  private static int CompareSlotGroupPrioritiesDescending(SlotGroup a, SlotGroup b)
  {
    return ((int) b.Settings.Priority).CompareTo((int) a.Settings.Priority);
  }
}
