// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CaravanHaulDestinationManager
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CaravanHaulDestinationManager : WorldObjectComp
{
  private List<VehiclePawn> Vehicles
  {
    get
    {
      return this.\u003CVehicles\u003Ek__BackingField ?? (this.\u003CVehicles\u003Ek__BackingField = VehicleCaravanHelper.get_Vehicles(this.parent).ToList<VehiclePawn>());
    }
  }

  public List<IHaulDestination> AllHaulDestinationsListInPriorityOrder { get; } = new List<IHaulDestination>();

  public List<SlotGroup> AllGroupsListInPriorityOrder { get; } = new List<SlotGroup>();

  public virtual void CompTick()
  {
    if (this.Vehicles.SequenceEqual<VehiclePawn>(VehicleCaravanHelper.get_Vehicles(this.parent)))
      return;
    this.Vehicles.Clear();
    this.Vehicles.AddRange(VehicleCaravanHelper.get_Vehicles(this.parent));
    this.AllHaulDestinationsListInPriorityOrder.Clear();
    this.AllGroupsListInPriorityOrder.Clear();
  }

  public void AddHaulDestination(IHaulDestination haulDestination)
  {
    if (this.AllHaulDestinationsListInPriorityOrder.Contains(haulDestination))
    {
      VMF_Log.Error("Double-added haul destination " + Gen.ToStringSafe<IHaulDestination>(haulDestination));
    }
    else
    {
      this.AllHaulDestinationsListInPriorityOrder.Add(haulDestination);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      GenList.InsertionSort<IHaulDestination>((IList<IHaulDestination>) this.AllHaulDestinationsListInPriorityOrder, CaravanHaulDestinationManager.\u003C\u003EO.\u003C0\u003E__CompareHaulDestinationPrioritiesDescending ?? (CaravanHaulDestinationManager.\u003C\u003EO.\u003C0\u003E__CompareHaulDestinationPrioritiesDescending = new Comparison<IHaulDestination>(CaravanHaulDestinationManager.CompareHaulDestinationPrioritiesDescending)));
      if (!(haulDestination is ISlotGroupParent islotGroupParent))
        return;
      SlotGroup slotGroup = islotGroupParent.GetSlotGroup();
      if (slotGroup == null)
      {
        VMF_Log.Error("ISlotGroupParent gave null slot group: " + Gen.ToStringSafe<ISlotGroupParent>(islotGroupParent));
      }
      else
      {
        this.AllGroupsListInPriorityOrder.Add(slotGroup);
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        GenList.InsertionSort<SlotGroup>((IList<SlotGroup>) this.AllGroupsListInPriorityOrder, CaravanHaulDestinationManager.\u003C\u003EO.\u003C1\u003E__CompareSlotGroupPrioritiesDescending ?? (CaravanHaulDestinationManager.\u003C\u003EO.\u003C1\u003E__CompareSlotGroupPrioritiesDescending = new Comparison<SlotGroup>(CaravanHaulDestinationManager.CompareSlotGroupPrioritiesDescending)));
      }
    }
  }

  private static int CompareHaulDestinationPrioritiesDescending(
    IHaulDestination a,
    IHaulDestination b)
  {
    return ((int) ((IStoreSettingsParent) b).GetStoreSettings().Priority).CompareTo((int) ((IStoreSettingsParent) a).GetStoreSettings().Priority);
  }

  private static int CompareSlotGroupPrioritiesDescending(SlotGroup a, SlotGroup b)
  {
    return ((int) b.Settings.Priority).CompareTo((int) a.Settings.Priority);
  }
}
