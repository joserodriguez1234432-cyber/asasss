// Decompiled with JetBrains decompiler
// Type: Vehicles.World.StashedVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public class StashedVehicle : DynamicDrawnWorldObject, IThingHolder
{
  private ThingOwner<Thing> stash = new ThingOwner<Thing>();
  private static readonly StringBuilder InspectStringBuilder = new StringBuilder();
  private static readonly Dictionary<VehicleDef, int> VehicleCounts = new Dictionary<VehicleDef, int>();
  private Material cachedMaterial;

  public IEnumerable<VehiclePawn> Vehicles
  {
    get
    {
      foreach (Thing thing in this.stash.InnerListForReading)
      {
        if (thing is VehiclePawn vehicle)
          yield return vehicle;
      }
    }
  }

  public virtual Material Material
  {
    get
    {
      if (!Object.op_Implicit((Object) this.cachedMaterial))
      {
        Faction faction = this.Faction;
        Color color = faction != null ? faction.Color : Color.white;
        VehiclePawn vehiclePawn = GenCollection.MaxBy<VehiclePawn, float>(this.Vehicles, (Func<VehiclePawn, float>) (vehicle =>
        {
          IntVec2 size = ((BuildableDef) vehicle.VehicleDef).Size;
          return ((IntVec2) ref size).Magnitude;
        }));
        this.cachedMaterial = MaterialPool.MatFrom(GenCollection.TryGetValue<VehicleDef, string>((IReadOnlyDictionary<VehicleDef, string>) VehicleTex.CachedTextureIconPaths, vehiclePawn.VehicleDef, "UI/Icons/DefaultVehicleIcon"), ShaderDatabase.WorldOverlayTransparentLit, color, 3550);
      }
      return this.cachedMaterial;
    }
  }

  public virtual void Draw()
  {
    if (WorldObjectSelectionUtility.HiddenBehindTerrainNow((WorldObject) this))
      return;
    WorldHelper.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * Find.WorldGrid.AverageTileSize, 0.015f, base.Material);
  }

  public VehicleCaravan Notify_CaravanArrived(Caravan caravan)
  {
    if (caravan is VehicleCaravan vehicleCaravan1 && (vehicleCaravan1.AerialVehicle || this.Vehicles.Any<VehiclePawn>((Func<VehiclePawn, bool>) (vehicle => !vehicle.VehicleDef.canCaravan))))
    {
      Messages.Message("Unable to retrieve vehicle, aerial vehicles can't merge with other vehicle caravans.", MessageTypeDefOf.RejectInput, false);
      return (VehicleCaravan) null;
    }
    List<Pawn> list = caravan.pawns.InnerListForReading.ToList<Pawn>();
    caravan.RemoveAllPawns();
    List<VehiclePawn> vehicles = new List<VehiclePawn>();
    foreach (Thing thing in this.stash.InnerListForReading.ToList<Thing>())
    {
      if (thing is VehiclePawn vehiclePawn)
      {
        ((ThingOwner) this.stash).Remove(thing);
        vehicles.Add(vehiclePawn);
      }
    }
    RoleHelper.Distribute(in vehicles, list);
    list.AddRange((IEnumerable<Pawn>) vehicles);
    VehicleCaravan vehicleCaravan2 = CaravanHelper.MakeVehicleCaravan((IEnumerable<Pawn>) list, ((WorldObject) caravan).Faction, ((WorldObject) caravan).Tile, true);
    vehicleCaravan2.RecacheVehicles();
    for (int index = ((ThingOwner) this.stash).Count - 1; index >= 0; --index)
      vehicleCaravan2.AddPawnOrItem(this.stash[index], true);
    ((ThingOwner) this.stash).Clear();
    base.Destroy();
    ((WorldObject) caravan).Destroy();
    return vehicleCaravan2;
  }

  public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
  {
    StashedVehicle stashedVehicle = this;
    // ISSUE: reference to a compiler-generated method
    foreach (FloatMenuOption floatMenuOption in stashedVehicle.\u003C\u003En__0(caravan))
      yield return floatMenuOption;
    foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_StashedVehicle.GetFloatMenuOptions(caravan, stashedVehicle))
      yield return floatMenuOption;
  }

  public virtual string GetInspectString()
  {
    using (new ClearStringOnDispose(StashedVehicle.InspectStringBuilder))
    {
      using (new ClearOnDispose<KeyValuePair<VehicleDef, int>>((ICollection<KeyValuePair<VehicleDef, int>>) StashedVehicle.VehicleCounts))
      {
        VehicleDef vehicleDef1;
        int num1;
        foreach (VehiclePawn vehicle in this.Vehicles)
        {
          if (!StashedVehicle.VehicleCounts.TryAdd(vehicle.VehicleDef, 1))
          {
            Dictionary<VehicleDef, int> vehicleCounts = StashedVehicle.VehicleCounts;
            vehicleDef1 = vehicle.VehicleDef;
            num1 = vehicleCounts[vehicleDef1]++;
          }
        }
        foreach (KeyValuePair<VehicleDef, int> vehicleCount in StashedVehicle.VehicleCounts)
        {
          vehicleCount.Deconstruct(ref vehicleDef1, ref num1);
          VehicleDef vehicleDef2 = vehicleDef1;
          int num2 = num1;
          StashedVehicle.InspectStringBuilder.AppendLine($"{num2} {((Def) vehicleDef2).LabelCap}");
        }
        StashedVehicle.InspectStringBuilder.Append(base.GetInspectString());
        return StashedVehicle.InspectStringBuilder.ToString();
      }
    }
  }

  public virtual void Destroy()
  {
    base.Destroy();
    ((ThingOwner) this.stash).ClearAndDestroyContentsOrPassToWorld((DestroyMode) 0);
    foreach (Pawn vehicle in this.Vehicles)
      Find.WorldPawns.RemoveAndDiscardPawnViaGC(vehicle);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Deep.Look<ThingOwner<Thing>>(ref this.stash, "stash", new object[1]
    {
      (object) this
    });
  }

  public void GetChildHolders(List<IThingHolder> outChildren)
  {
    ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, (IList<Thing>) this.GetDirectlyHeldThings());
  }

  public ThingOwner GetDirectlyHeldThings() => (ThingOwner) this.stash;

  public static StashedVehicle Create(
    VehicleCaravan vehicleCaravan,
    out Caravan caravan,
    List<TransferableOneWay> transferables = null)
  {
    caravan = (Caravan) null;
    if (GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) vehicleCaravan.VehiclesListForReading))
    {
      Log.Error("No vehicles in vehicle caravan for stashed vehicle.");
      return (StashedVehicle) null;
    }
    StashedVehicle stashedVehicle = (StashedVehicle) WorldObjectMaker.MakeWorldObject(WorldObjectDefOfVehicles.StashedVehicle);
    stashedVehicle.Tile = ((WorldObject) vehicleCaravan).Tile;
    IntVec2 size1 = ((BuildableDef) GenCollection.MaxBy<VehiclePawn, float>((IEnumerable<VehiclePawn>) vehicleCaravan.VehiclesListForReading, (Func<VehiclePawn, float>) (vehicle =>
    {
      IntVec2 size2 = ((BuildableDef) vehicle.VehicleDef).Size;
      return ((IntVec2) ref size2).Magnitude;
    })).VehicleDef).Size;
    float num = Mathf.Lerp(15f, 30f, Ext_Math.ReverseInterpolate(((IntVec2) ref size1).Magnitude, 1f, 10f));
    stashedVehicle.GetComponent<TimeoutComp>().StartTimeout(Mathf.CeilToInt(num * 60000f));
    caravan = CaravanMaker.MakeCaravan((IEnumerable<Pawn>) Array.Empty<Pawn>(), ((WorldObject) vehicleCaravan).Faction, ((WorldObject) vehicleCaravan).Tile, true);
    List<Pawn> pawns = new List<Pawn>();
    foreach (Pawn pawn in vehicleCaravan.pawns)
    {
      if (!(pawn is VehiclePawn vehiclePawn))
      {
        pawns.Add(pawn);
      }
      else
      {
        foreach (VehicleRoleHandler handler in vehiclePawn.handlers)
        {
          for (int index = ((ThingOwner) handler.thingOwner).Count - 1; index >= 0; --index)
            pawns.Add(handler.thingOwner[index]);
        }
      }
    }
    using (new VehicleCaravan.RecacheDisabler(vehicleCaravan))
    {
      foreach (Pawn pawn in pawns)
      {
        pawn.GetVehicle()?.RemovePawn(pawn);
        vehicleCaravan.RemovePawn(pawn);
        caravan.AddPawn(pawn, true);
        if (!WorldPawnsUtility.IsWorldPawn(pawn))
          Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
      }
      foreach (VehiclePawn vehiclePawn in vehicleCaravan.VehiclesListForReading)
      {
        for (int index = ((ThingOwner) vehiclePawn.inventory.innerContainer).Count - 1; index >= 0; --index)
        {
          if (vehiclePawn.inventory.innerContainer[index] is Pawn pawn)
          {
            ((ThingOwner) vehiclePawn.inventory.innerContainer).TryTransferToContainer((Thing) pawn, (ThingOwner) caravan.pawns, true);
            Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
          }
        }
      }
      if (!GenList.NullOrEmpty<TransferableOneWay>((IList<TransferableOneWay>) transferables))
      {
        foreach (TransferableOneWay transferable in transferables)
          TransferableUtility.TransferNoSplit(transferable.things, ((Transferable) transferable).CountToTransfer, (Action<Thing, int>) ((thing, numToTake) =>
          {
            Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf((Caravan) vehicleCaravan, thing);
            if (ownerOf == null)
              Log.Error($"Error while stashing vehicle. {thing} has no owner.");
            else
              CaravanInventoryUtility.MoveInventoryToSomeoneElse(ownerOf, thing, pawns, vehicleCaravan.pawns.InnerListForReading, numToTake);
          }), true, true);
      }
      for (int index = ((ThingOwner) vehicleCaravan.pawns).Count - 1; index >= 0; --index)
      {
        Pawn pawn = vehicleCaravan.pawns[index];
        if (WorldPawnsUtility.IsWorldPawn(pawn))
          Find.WorldPawns.RemovePawn(pawn);
        if (!((ThingOwner) vehicleCaravan.pawns).TryTransferToContainer((Thing) pawn, (ThingOwner) stashedVehicle.stash, false))
        {
          Trace.Fail($"Unable to transfer {pawn} to stash. Moving to new caravan instead.");
          vehicleCaravan.RemovePawn(pawn);
          caravan.AddPawn(pawn, true);
        }
      }
    }
    Find.WorldObjects.Add((WorldObject) stashedVehicle);
    ((WorldObject) vehicleCaravan).Destroy();
    return stashedVehicle;
  }
}
