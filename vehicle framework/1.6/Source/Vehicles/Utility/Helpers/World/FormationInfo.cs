// Decompiled with JetBrains decompiler
// Type: Vehicles.World.FormationInfo
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class FormationInfo : ICaravanInfo
{
  private static readonly MethodInfo MustChooseRouteGetter = AccessTools.PropertyGetter(typeof (Dialog_FormCaravan), nameof (MustChooseRoute));
  private static readonly MethodInfo DaysWorthOfFoodGetter = AccessTools.PropertyGetter(typeof (Dialog_FormCaravan), nameof (DaysWorthOfFood));
  private static readonly MethodInfo MostFoodWillRotSoonGetter = AccessTools.PropertyGetter(typeof (Dialog_FormCaravan), nameof (MostFoodWillRotSoon));
  private static readonly MethodInfo FlashMassMethod = AccessTools.Method(typeof (Dialog_FormCaravan), "FlashMass", (System.Type[]) null, (System.Type[]) null);
  private static readonly MethodInfo AddItemsFromTransferablesToRandomInventoriesMethod;
  private static readonly MethodInfo ShouldShowWarningForUndesirableFoodMethod = AccessTools.Method(typeof (Dialog_FormCaravan), "ShouldShowWarningForUndesirableFood", (System.Type[]) null, (System.Type[]) null);
  private static readonly MethodInfo ShouldShowWarningForMechWithoutMechanitorMethod = AccessTools.Method(typeof (Dialog_FormCaravan), "ShouldShowWarningForMechWithoutMechanitor", (System.Type[]) null, (System.Type[]) null);
  private static readonly MethodInfo Notify_TransferablesChangedMethod;
  private static readonly MethodInfo SelectApproximateBestTravelSuppliesMethod;
  private static readonly AccessTools.FieldRef<Dialog_FormCaravan, bool> ChoosingRouteFieldRef;
  private static readonly AccessTools.FieldRef<Dialog_FormCaravan, bool> ReformFieldRef;
  private static readonly AccessTools.FieldRef<Dialog_FormCaravan, PlanetTile> StartingTileFieldRef;
  private static readonly AccessTools.FieldRef<Dialog_FormCaravan, PlanetTile> DestinationTileFieldRef;
  private static readonly AccessTools.FieldRef<Dialog_FormCaravan, bool> TicksToArriveDirtyFieldRef;
  private static readonly AccessTools.FieldRef<Dialog_FormCaravan, bool> DaysWorthOfFoodDirtyFieldRef;
  private readonly Func<bool> mustChooseRoute;
  private readonly Func<(float, float)> daysWorthOfFood;
  private readonly Func<bool> mostFoodWillRotSoon;
  private readonly Action flashMass;
  private readonly Func<bool> shouldShowWarningForUndesirableFood;
  private readonly Func<bool> shouldShowWarningForMechWithoutMechanitor;
  private readonly Action<List<Pawn>> addItemsFromTransferablesToRandomInventories;
  private readonly Action notifyTransferablesChanged;
  private readonly Action selectApproximateBestTravelSupplies;
  private VehiclePawn leadVehicle;
  public readonly List<Pawn> pawns = new List<Pawn>();
  public readonly List<VehiclePawn> vehicles = new List<VehiclePawn>();
  public readonly List<Thing> things = new List<Thing>();
  public readonly List<VehiclePawn> unselectedVehicles = new List<VehiclePawn>();
  private readonly Map map;
  private readonly Dialog_FormCaravan formCaravan;

  static FormationInfo()
  {
    FormationInfo.AddItemsFromTransferablesToRandomInventoriesMethod = AccessTools.Method(typeof (Dialog_FormCaravan), "AddItemsFromTransferablesToRandomInventories", (System.Type[]) null, (System.Type[]) null);
    FormationInfo.Notify_TransferablesChangedMethod = AccessTools.Method(typeof (Dialog_FormCaravan), "Notify_TransferablesChanged", (System.Type[]) null, (System.Type[]) null);
    FormationInfo.SelectApproximateBestTravelSuppliesMethod = AccessTools.Method(typeof (Dialog_FormCaravan), "SelectApproximateBestTravelSupplies", (System.Type[]) null, (System.Type[]) null);
    FormationInfo.ChoosingRouteFieldRef = (AccessTools.FieldRef<Dialog_FormCaravan, bool>) AccessTools.FieldRefAccess<bool>(typeof (Dialog_FormCaravan), "choosingRoute");
    FormationInfo.ReformFieldRef = (AccessTools.FieldRef<Dialog_FormCaravan, bool>) AccessTools.FieldRefAccess<bool>(typeof (Dialog_FormCaravan), "reform");
    FormationInfo.StartingTileFieldRef = (AccessTools.FieldRef<Dialog_FormCaravan, PlanetTile>) AccessTools.FieldRefAccess<PlanetTile>(typeof (Dialog_FormCaravan), "startingTile");
    FormationInfo.DestinationTileFieldRef = (AccessTools.FieldRef<Dialog_FormCaravan, PlanetTile>) AccessTools.FieldRefAccess<PlanetTile>(typeof (Dialog_FormCaravan), "destinationTile");
    FormationInfo.TicksToArriveDirtyFieldRef = (AccessTools.FieldRef<Dialog_FormCaravan, bool>) AccessTools.FieldRefAccess<bool>(typeof (Dialog_FormCaravan), "ticksToArriveDirty");
    FormationInfo.DaysWorthOfFoodDirtyFieldRef = (AccessTools.FieldRef<Dialog_FormCaravan, bool>) AccessTools.FieldRefAccess<bool>(typeof (Dialog_FormCaravan), "daysWorthOfFoodDirty");
  }

  public FormationInfo(Dialog_FormCaravan formCaravan, Map map)
  {
    this.formCaravan = formCaravan;
    this.map = map;
    this.mustChooseRoute = (Func<bool>) Delegate.CreateDelegate(typeof (Func<bool>), (object) formCaravan, FormationInfo.MustChooseRouteGetter);
    this.daysWorthOfFood = (Func<(float, float)>) Delegate.CreateDelegate(typeof (Func<(float, float)>), (object) formCaravan, FormationInfo.DaysWorthOfFoodGetter);
    this.mostFoodWillRotSoon = (Func<bool>) Delegate.CreateDelegate(typeof (Func<bool>), (object) formCaravan, FormationInfo.MostFoodWillRotSoonGetter);
    this.flashMass = (Action) Delegate.CreateDelegate(typeof (Action), (object) formCaravan, FormationInfo.FlashMassMethod);
    this.shouldShowWarningForUndesirableFood = (Func<bool>) Delegate.CreateDelegate(typeof (Func<bool>), (object) formCaravan, FormationInfo.ShouldShowWarningForUndesirableFoodMethod);
    this.shouldShowWarningForMechWithoutMechanitor = (Func<bool>) Delegate.CreateDelegate(typeof (Func<bool>), (object) formCaravan, FormationInfo.ShouldShowWarningForMechWithoutMechanitorMethod);
    this.addItemsFromTransferablesToRandomInventories = (Action<List<Pawn>>) Delegate.CreateDelegate(typeof (Action<List<Pawn>>), (object) formCaravan, FormationInfo.AddItemsFromTransferablesToRandomInventoriesMethod);
    this.notifyTransferablesChanged = (Action) Delegate.CreateDelegate(typeof (Action), (object) formCaravan, FormationInfo.Notify_TransferablesChangedMethod);
    this.selectApproximateBestTravelSupplies = (Action) Delegate.CreateDelegate(typeof (Action), (object) formCaravan, FormationInfo.SelectApproximateBestTravelSuppliesMethod);
  }

  public Dialog_FormCaravan Dialog => this.formCaravan;

  public Map Map => this.map;

  public VehiclePawn LeadVehicle => this.leadVehicle;

  public List<Pawn> AllPawnsAndVehicles
  {
    get => this.pawns.Concat<Pawn>((IEnumerable<Pawn>) this.vehicles).ToList<Pawn>();
  }

  public bool Reform => FormationInfo.ReformFieldRef.Invoke(this.formCaravan);

  bool ICaravanInfo.AllowSelectionOfAllVehicles => this.Reform;

  public bool ChoosingRoute
  {
    get => FormationInfo.ChoosingRouteFieldRef.Invoke(this.formCaravan);
    set => FormationInfo.ChoosingRouteFieldRef.Invoke(this.formCaravan) = value;
  }

  public PlanetTile StartingTile
  {
    get => FormationInfo.StartingTileFieldRef.Invoke(this.formCaravan);
    set => FormationInfo.StartingTileFieldRef.Invoke(this.formCaravan) = value;
  }

  public PlanetTile DestinationTile
  {
    get => FormationInfo.DestinationTileFieldRef.Invoke(this.formCaravan);
    set => FormationInfo.DestinationTileFieldRef.Invoke(this.formCaravan) = value;
  }

  public bool TicksToArriveDirty
  {
    get => FormationInfo.TicksToArriveDirtyFieldRef.Invoke(this.formCaravan);
    set => FormationInfo.TicksToArriveDirtyFieldRef.Invoke(this.formCaravan) = value;
  }

  public bool DaysWorthOfFoodDirty
  {
    get => FormationInfo.DaysWorthOfFoodDirtyFieldRef.Invoke(this.formCaravan);
    set => FormationInfo.DaysWorthOfFoodDirtyFieldRef.Invoke(this.formCaravan) = value;
  }

  public bool MustChooseRoute => this.mustChooseRoute();

  public (float days, float tillRot) DaysWorthOfFood => this.daysWorthOfFood();

  public bool MostFoodWillRotSoon => this.mostFoodWillRotSoon();

  public void FlashMass() => this.flashMass();

  public bool ShouldShowWarningForUndesirableFood() => this.shouldShowWarningForUndesirableFood();

  public bool ShouldShowWarningForMechWithoutMechanitor()
  {
    return this.shouldShowWarningForMechWithoutMechanitor();
  }

  public void AddItemsFromTransferablesToRandomInventories(List<Pawn> pawns)
  {
    this.addItemsFromTransferablesToRandomInventories(pawns);
  }

  public void SelectApproximateBestTravelSupplies() => this.selectApproximateBestTravelSupplies();

  public void NotifyTransferablesChanged()
  {
    this.notifyTransferablesChanged();
    PlanetTile destinationTile = this.DestinationTile;
    if (!((PlanetTile) ref destinationTile).Valid)
      return;
    PlanetTile currentTile = this.formCaravan.CurrentTile;
    using (WorldPath path = ((PlanetTile) ref currentTile).Layer.Pather.FindPath(this.formCaravan.CurrentTile, this.DestinationTile, (Caravan) null, (Func<float, bool>) null))
    {
      if (path.Found)
        return;
      this.DestinationTile = PlanetTile.Invalid;
    }
  }

  internal void RecacheTransferables()
  {
    this.leadVehicle = (VehiclePawn) null;
    this.pawns.Clear();
    this.vehicles.Clear();
    this.things.Clear();
    this.unselectedVehicles.Clear();
    int num = -1;
    foreach (TransferableOneWay transferable in this.formCaravan.transferables)
    {
      if (((Transferable) transferable).AnyThing != null)
      {
        if (((Transferable) transferable).CountToTransfer == 0)
        {
          if (((Transferable) transferable).AnyThing is VehiclePawn anyThing)
            this.unselectedVehicles.Add(anyThing);
        }
        else
        {
          foreach (Thing thing in transferable.things)
          {
            switch (thing)
            {
              case VehiclePawn vehiclePawn:
                if ((double) ((IntVec2) ref ((Thing) vehiclePawn).def.size).Magnitude > (double) num)
                {
                  this.leadVehicle = vehiclePawn;
                  num = ((IntVec2) ref ((Thing) vehiclePawn).def.size).MagnitudeManhattan;
                }
                this.vehicles.Add(vehiclePawn);
                continue;
              case Pawn pawn:
                this.pawns.Add(pawn);
                continue;
              default:
                this.things.Add(thing);
                continue;
            }
          }
        }
      }
    }
  }
}
