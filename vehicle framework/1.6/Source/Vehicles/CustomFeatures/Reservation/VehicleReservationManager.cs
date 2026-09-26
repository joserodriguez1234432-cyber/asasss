// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleReservationManager
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

#nullable enable
namespace Vehicles;

[PublicAPI]
public class VehicleReservationManager(Map map) : MapComponent(map)
{
  private const int ReservationVerificationInterval = 120;
  private Dictionary<VehiclePawn, VehicleReservationManager.VehicleReservationCollection> reservations = new Dictionary<VehiclePawn, VehicleReservationManager.VehicleReservationCollection>();
  private Dictionary<VehiclePawn, VehicleReservationManager.VehicleRequestCollection> vehicleListers = new Dictionary<VehiclePawn, VehicleReservationManager.VehicleRequestCollection>();
  private List<VehiclePawn> vehiclesReserving_tmp = new List<VehiclePawn>();
  private List<VehicleReservationManager.VehicleReservationCollection> vehicleReservations_tmp = new List<VehicleReservationManager.VehicleReservationCollection>();
  private List<VehiclePawn> vehicleListerPawns_tmp = new List<VehiclePawn>();
  private List<VehicleReservationManager.VehicleRequestCollection> vehicleListerRequests_tmp = new List<VehicleReservationManager.VehicleRequestCollection>();

  public bool Reserve<T1, T2>(VehiclePawn vehicle, Pawn pawn, Job job, T1 target) where T2 : Reservation<T1>
  {
    try
    {
      this.ReleaseAllClaimedBy(pawn);
      T2 reservation1 = this.GetReservation<T2>(vehicle);
      if ((object) reservation1 != null)
        return reservation1.CanReserve(pawn, target, (StringBuilder) null) && reservation1.AddClaimant(pawn, target);
      int num = vehicle.TotalAllowedFor(job.def);
      VehicleReservationManager.VehicleReservationCollection reservationCollection;
      if (!this.reservations.TryGetValue(vehicle, out reservationCollection))
      {
        reservationCollection = new VehicleReservationManager.VehicleReservationCollection();
        this.reservations[vehicle] = reservationCollection;
      }
      reservationCollection.Add<ReservationBase>((ReservationBase) Activator.CreateInstance(typeof (T2), (object) vehicle, (object) job, (object) num));
      T2 reservation2 = this.GetReservation<T2>(vehicle);
      if ((object) reservation2 == null)
      {
        Log.Error($"Unable to retrieve reservation for {pawn} performing Job={job} from new reservation.");
        return false;
      }
      reservation2.AddClaimant(pawn, target);
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while attempting to reserve Vehicle based job. {ex}");
      return false;
    }
    return true;
  }

  public T? GetReservation<T>(VehiclePawn vehicle) where T : ReservationBase
  {
    VehicleReservationManager.VehicleReservationCollection reservationCollection;
    if (!this.reservations.TryGetValue(vehicle, out reservationCollection))
      return default (T);
    foreach (ReservationBase reservationBase in reservationCollection.List)
    {
      if (reservationBase is T reservation)
        return reservation;
    }
    return default (T);
  }

  public void ReleaseAllClaims()
  {
    foreach (VehiclePawn vehicle in this.reservations.Keys.ToList<VehiclePawn>())
      this.ClearReservedFor(vehicle);
  }

  public void ReleaseAllClaimedBy(Pawn pawn)
  {
    foreach (VehicleReservationManager.VehicleReservationCollection reservationCollection in this.reservations.Values)
    {
      foreach (ReservationBase reservationBase in reservationCollection.List)
        reservationBase.ReleaseReservationBy(pawn);
    }
  }

  public void ClearReservedFor(VehiclePawn vehicle)
  {
    if (!this.reservations.ContainsKey(vehicle))
      return;
    this.reservations[vehicle].List.ForEach((Action<ReservationBase>) (reservation => reservation.ReleaseAllReservations()));
    this.reservations.Remove(vehicle);
  }

  public bool CanReserve(
    VehiclePawn vehicle,
    Pawn pawn,
    JobDef jobDef,
    StringBuilder? stringBuilder = null)
  {
    stringBuilder?.AppendLine("Starting Reservation check.");
    VehicleReservationManager.VehicleReservationCollection reservationCollection;
    if (this.reservations.TryGetValue(vehicle, out reservationCollection))
    {
      foreach (ReservationBase reservationBase in reservationCollection.List)
      {
        if (reservationBase.JobDef == jobDef)
        {
          stringBuilder?.AppendLine($"Reservation cached. Claimants = {vehicle.TotalAllowedFor(jobDef)}/{reservationBase.TotalClaimants}.");
          return vehicle.TotalAllowedFor(jobDef) > reservationBase.TotalClaimants;
        }
      }
    }
    stringBuilder?.AppendLine("Reservation not cached. Can automatically reserve");
    return true;
  }

  public bool CanReserve<T1, T2>(
    VehiclePawn vehicle,
    Pawn pawn,
    T1 target,
    StringBuilder? stringBuilder = null)
    where T2 : Reservation<T1>
  {
    VehicleReservationManager.VehicleReservationCollection reservationCollection;
    if (this.reservations.TryGetValue(vehicle, out reservationCollection))
    {
      foreach (ReservationBase reservationBase in reservationCollection.List)
      {
        if (reservationBase is T2 obj)
        {
          bool flag = obj.CanReserve(pawn, target, stringBuilder);
          stringBuilder?.AppendLine($"Reservation cached. Type={typeof (T2)} Summary={stringBuilder}");
          return flag;
        }
      }
    }
    stringBuilder?.AppendLine("Reservation not cached. Can automatically reserve");
    return true;
  }

  public bool ReservedBy<T1, T2>(
    VehiclePawn vehicle,
    Pawn pawn,
    T1 target,
    StringBuilder? stringBuilder = null)
    where T2 : Reservation<T1>
  {
    VehicleReservationManager.VehicleReservationCollection reservationCollection;
    if (this.reservations.TryGetValue(vehicle, out reservationCollection))
    {
      foreach (ReservationBase reservationBase in reservationCollection.List)
      {
        if (reservationBase is T2 obj)
        {
          bool flag = obj.ReservedBy(pawn, target);
          stringBuilder?.AppendLine($"Reserved={flag}");
          return flag;
        }
      }
    }
    stringBuilder?.AppendLine("Reservation not cached.");
    return true;
  }

  public int TotalReserving(VehiclePawn vehicle)
  {
    VehicleReservationManager.VehicleReservationCollection reservationCollection;
    if (!this.reservations.TryGetValue(vehicle, out reservationCollection))
      return 0;
    int num = 0;
    foreach (ReservationBase reservationBase in reservationCollection.List)
      num += reservationBase.TotalClaimants;
    return num;
  }

  public virtual void MapComponentTick()
  {
    if (Find.TickManager.TicksGame % 120 != 0)
      return;
    List<KeyValuePair<VehiclePawn, VehicleReservationManager.VehicleReservationCollection>> list = this.reservations.ToList<KeyValuePair<VehiclePawn, VehicleReservationManager.VehicleReservationCollection>>();
    for (int index1 = list.Count - 1; index1 >= 0; --index1)
    {
      VehiclePawn vehiclePawn;
      VehicleReservationManager.VehicleReservationCollection reservationCollection1;
      list[index1].Deconstruct(ref vehiclePawn, ref reservationCollection1);
      VehiclePawn key = vehiclePawn;
      VehicleReservationManager.VehicleReservationCollection reservationCollection2 = reservationCollection1;
      for (int index2 = reservationCollection2.Count - 1; index2 >= 0; --index2)
      {
        ReservationBase reservation = reservationCollection2[index2];
        reservation.VerifyAndValidateClaimants();
        if (reservation.RemoveNow)
          this.reservations[key].Remove<ReservationBase>(reservation);
      }
      if (this.reservations[key].NullOrEmpty())
        this.reservations.Remove(key);
    }
  }

  public virtual void FinalizeInit()
  {
    base.FinalizeInit();
    VehicleReservationManager.VerifyCollection<VehiclePawn>(ref this.vehiclesReserving_tmp);
    VehicleReservationManager.VerifyCollection<VehicleReservationManager.VehicleReservationCollection>(ref this.vehicleReservations_tmp);
    VehicleReservationManager.VerifyCollection<VehiclePawn>(ref this.vehicleListerPawns_tmp);
    VehicleReservationManager.VerifyCollection<VehicleReservationManager.VehicleRequestCollection>(ref this.vehicleListerRequests_tmp);
  }

  private static void VerifyCollection<T>(ref List<T?> list)
  {
    if (list == null)
      list = new List<T>();
    for (int index = list.Count - 1; index >= 0; ++index)
    {
      if ((object) list[index] == null)
        list.RemoveAt(index);
    }
  }

  public bool VehicleListed(VehiclePawn vehicle, string request)
  {
    VehicleReservationManager.VehicleRequestCollection requestCollection;
    return this.vehicleListers.TryGetValue(vehicle, out requestCollection) && requestCollection.requests.Contains(request);
  }

  public IEnumerable<VehiclePawn> VehicleListers(string request)
  {
    foreach (KeyValuePair<VehiclePawn, VehicleReservationManager.VehicleRequestCollection> vehicleLister in this.vehicleListers)
    {
      VehiclePawn vehiclePawn1;
      VehicleReservationManager.VehicleRequestCollection requestCollection;
      vehicleLister.Deconstruct(ref vehiclePawn1, ref requestCollection);
      VehiclePawn vehiclePawn2 = vehiclePawn1;
      if (requestCollection.requests.Contains(request))
        yield return vehiclePawn2;
    }
  }

  public bool RegisterLister(VehiclePawn vehicle, string request)
  {
    VehicleReservationManager.VehicleRequestCollection requestCollection;
    if (this.vehicleListers.TryGetValue(vehicle, out requestCollection))
    {
      if (requestCollection.requests.NotNullAndAny<string>())
        return requestCollection.requests.Add(request);
      this.vehicleListers[vehicle] = new VehicleReservationManager.VehicleRequestCollection(request);
      return true;
    }
    this.vehicleListers.Add(vehicle, new VehicleReservationManager.VehicleRequestCollection(request));
    return true;
  }

  public bool RemoveLister(VehiclePawn vehicle, string request)
  {
    VehicleReservationManager.VehicleRequestCollection requestCollection;
    if (!this.vehicleListers.TryGetValue(vehicle, out requestCollection))
      return false;
    return !GenCollection.NullOrEmpty<string>(requestCollection.requests) ? requestCollection.requests.Remove(request) : this.vehicleListers.Remove(vehicle);
  }

  public bool RemoveAllListerFor(VehiclePawn vehicle) => this.vehicleListers.Remove(vehicle);

  public static VehiclePawn? VehicleInhabitingCells(CellRect cellRect, Map map)
  {
    foreach (Pawn pawn in (IEnumerable<Pawn>) map.mapPawns.AllPawnsSpawned)
    {
      if (pawn is VehiclePawn vehiclePawn)
      {
        CellRect cellRect1 = GenAdj.OccupiedRect((Thing) vehiclePawn);
        if (((CellRect) ref cellRect1).Overlaps(cellRect))
          return vehiclePawn;
      }
    }
    return (VehiclePawn) null;
  }

  public static bool AnyVehicleInhabitingCells(CellRect cellRect, Map map)
  {
    return VehicleReservationManager.VehicleInhabitingCells(cellRect, map) != null;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Collections.Look<VehiclePawn, VehicleReservationManager.VehicleReservationCollection>(ref this.reservations, "reservations", (LookMode) 3, (LookMode) 2, ref this.vehiclesReserving_tmp, ref this.vehicleReservations_tmp, true, false, false);
    Scribe_Collections.Look<VehiclePawn, VehicleReservationManager.VehicleRequestCollection>(ref this.vehicleListers, "vehicleListers", (LookMode) 3, (LookMode) 2, ref this.vehicleListerPawns_tmp, ref this.vehicleListerRequests_tmp, true, false, false);
    if (this.reservations == null)
      this.reservations = new Dictionary<VehiclePawn, VehicleReservationManager.VehicleReservationCollection>();
    if (this.vehicleListers != null)
      return;
    this.vehicleListers = new Dictionary<VehiclePawn, VehicleReservationManager.VehicleRequestCollection>();
  }

  public class VehicleReservationCollection : IExposable
  {
    public List<ReservationBase> reservations = new List<ReservationBase>();

    public bool NullOrEmpty()
    {
      return GenList.NullOrEmpty<ReservationBase>((IList<ReservationBase>) this.reservations);
    }

    public void Add<T>(T reservation) where T : ReservationBase
    {
      this.reservations.Add((ReservationBase) reservation);
    }

    public bool Remove<T>(T reservation) where T : ReservationBase
    {
      return this.reservations.Remove((ReservationBase) reservation);
    }

    public List<ReservationBase> List => this.reservations;

    public int Count => this.reservations.Count;

    public ReservationBase this[int index] => this.reservations[index];

    public void ExposeData()
    {
      Scribe_Collections.Look<ReservationBase>(ref this.reservations, "reservations", (LookMode) 2, Array.Empty<object>());
    }
  }

  public class VehicleRequestCollection : IExposable
  {
    public HashSet<string> requests = new HashSet<string>();

    public VehicleRequestCollection() => this.requests = new HashSet<string>();

    public VehicleRequestCollection(string req)
    {
      this.requests = new HashSet<string>() { req };
    }

    public void ExposeData()
    {
      Scribe_Collections.Look<string>(ref this.requests, "requests", (LookMode) 1);
    }
  }
}
