// Decompiled with JetBrains decompiler
// Type: Vehicles.World.WITab_AerialVehicle_Items
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class WITab_AerialVehicle_Items : WITab_AerialVehicle
{
  private const float SortersSpace = 25f;
  private const float AssignDrugPoliciesButtonHeight = 27f;
  private Vector2 scrollPosition;
  private float scrollViewHeight;
  private TransferableSorterDef sorter1;
  private TransferableSorterDef sorter2;
  private List<TransferableImmutable> cachedItems = new List<TransferableImmutable>();
  private int cachedItemsHash;
  private int cachedItemsCount;

  public WITab_AerialVehicle_Items() => ((InspectTabBase) this).labelKey = "TabCaravanItems";

  protected virtual void FillTab()
  {
    this.CheckCreateSorters();
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 0.0f, ((InspectTabBase) this).size.x, ((InspectTabBase) this).size.y);
    Rect rect2 = GenUI.ContractedBy(rect1, 10f);
    float num = MassUtility.GearAndInventoryMass((Pawn) this.SelAerialVehicle.Vehicle);
    float statValue = this.SelAerialVehicle.Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
    Widgets.Label(rect2, TranslatorFormattedStringExtensions.Translate("MassCarried", NamedArgument.op_Implicit(num.ToString("0.##")), NamedArgument.op_Implicit(statValue.ToString("0.##"))));
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).yMin = ((Rect) ref local1).yMin + 37f;
    Widgets.BeginGroup(GenUI.ContractedBy(rect1, 10f));
    TransferableUIUtility.DoTransferableSorters(this.sorter1, this.sorter2, (Action<TransferableSorterDef>) (sorter =>
    {
      this.sorter1 = sorter;
      this.CacheItems();
    }), (Action<TransferableSorterDef>) (sorter =>
    {
      this.sorter2 = sorter;
      this.CacheItems();
    }));
    Widgets.EndGroup();
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).yMin = ((Rect) ref local2).yMin + 25f;
    Widgets.BeginGroup(rect1);
    this.CheckCacheItems();
    AerialVehicleTabHelper.DoRows(((Rect) ref rect1).size, this.cachedItems, this.SelAerialVehicle, ref this.scrollPosition, ref this.scrollViewHeight);
    Widgets.EndGroup();
  }

  protected virtual void UpdateSize()
  {
    ((InspectTabBase) this).UpdateSize();
    this.CheckCacheItems();
    ((InspectTabBase) this).size = CaravanItemsTabUtility.GetSize(this.cachedItems, ((InspectTabBase) this).PaneTopY, true);
  }

  private void CheckCacheItems()
  {
    List<Thing> thingList = WorldHelper.AllInventoryItems(this.SelAerialVehicle);
    if (thingList.Count != this.cachedItemsCount)
    {
      this.CacheItems();
    }
    else
    {
      int num = 0;
      for (int index = 0; index < thingList.Count; ++index)
        num = Gen.HashCombineInt(num, thingList[index].GetHashCode());
      if (num == this.cachedItemsHash)
        return;
      this.CacheItems();
    }
  }

  private void CacheItems()
  {
    this.CheckCreateSorters();
    this.cachedItems.Clear();
    List<Thing> thingList = WorldHelper.AllInventoryItems(this.SelAerialVehicle);
    int num = 0;
    for (int index = 0; index < thingList.Count; ++index)
    {
      TransferableImmutable transferableImmutable = TransferableUtility.TransferableMatching<TransferableImmutable>(thingList[index], this.cachedItems, (TransferAsOneMode) 0);
      if (transferableImmutable == null)
      {
        transferableImmutable = new TransferableImmutable();
        this.cachedItems.Add(transferableImmutable);
      }
      transferableImmutable.things.Add(thingList[index]);
      num = Gen.HashCombineInt(num, thingList[index].GetHashCode());
    }
    this.cachedItems = this.cachedItems.OrderBy<TransferableImmutable, Transferable>((Func<TransferableImmutable, Transferable>) (tr => (Transferable) tr), (IComparer<Transferable>) this.sorter1.Comparer).ThenBy<TransferableImmutable, Transferable>((Func<TransferableImmutable, Transferable>) (tr => (Transferable) tr), (IComparer<Transferable>) this.sorter2.Comparer).ThenBy<TransferableImmutable, float>((Func<TransferableImmutable, float>) (tr => TransferableUIUtility.DefaultListOrderPriority((Transferable) tr))).ToList<TransferableImmutable>();
    this.cachedItemsCount = thingList.Count;
    this.cachedItemsHash = num;
  }

  private void CheckCreateSorters()
  {
    if (this.sorter1 == null)
      this.sorter1 = TransferableSorterDefOf.Category;
    if (this.sorter2 != null)
      return;
    this.sorter2 = TransferableSorterDefOf.MarketValue;
  }
}
