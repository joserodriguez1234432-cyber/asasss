// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleAssignment
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public sealed class VehicleAssignment
{
  private static readonly List<AssignedSeat> EmptyAssignments = new List<AssignedSeat>();
  private readonly Dictionary<VehiclePawn, List<AssignedSeat>> vehicleAssignments = new Dictionary<VehiclePawn, List<AssignedSeat>>();
  private readonly Dictionary<Pawn, AssignedSeat> pawnAssignment = new Dictionary<Pawn, AssignedSeat>();

  public Dictionary<Pawn, AssignedSeat> AllAssignments => this.pawnAssignment;

  public void Clear()
  {
    this.vehicleAssignments.Clear();
    this.pawnAssignment.Clear();
  }

  [Pure]
  public bool IsAssigned(Pawn pawn) => this.pawnAssignment.ContainsKey(pawn);

  [Pure]
  public AssignedSeat GetAssignment(Pawn pawn)
  {
    return GenCollection.TryGetValue<Pawn, AssignedSeat>((IReadOnlyDictionary<Pawn, AssignedSeat>) this.pawnAssignment, pawn, (AssignedSeat) null);
  }

  [Pure]
  public List<AssignedSeat> GetAssignments(VehiclePawn vehicle)
  {
    return GenCollection.TryGetValue<VehiclePawn, List<AssignedSeat>>((IReadOnlyDictionary<VehiclePawn, List<AssignedSeat>>) this.vehicleAssignments, vehicle, VehicleAssignment.EmptyAssignments);
  }

  public void RemoveAll(Predicate<Pawn> validator)
  {
    bool flag = false;
    foreach (Pawn key in this.pawnAssignment.Keys.ToList<Pawn>())
    {
      if (validator(key))
        flag |= this.pawnAssignment.Remove(key);
    }
    if (!flag)
      return;
    this.UpdateVehicleAssignments();
  }

  public void RemoveAssignment(Pawn pawn)
  {
    this.pawnAssignment.Remove(pawn);
    this.UpdateVehicleAssignments();
  }

  public void RemoveAssignments(VehiclePawn vehicle)
  {
    this.vehicleAssignments.Remove(vehicle);
    this.UpdatePawnAssignments();
  }

  public void SetAssignment(AssignedSeat assignment)
  {
    this.pawnAssignment.Remove(assignment.pawn);
    this.pawnAssignment[assignment.pawn] = assignment;
    this.UpdateVehicleAssignments();
  }

  public void SetAssignments(VehiclePawn vehicle, List<AssignedSeat> assignments)
  {
    this.vehicleAssignments[vehicle] = assignments;
    this.UpdatePawnAssignments();
  }

  private void UpdatePawnAssignments()
  {
    this.pawnAssignment.Clear();
    foreach (AssignedSeat assignedSeat in this.vehicleAssignments.SelectMany<KeyValuePair<VehiclePawn, List<AssignedSeat>>, AssignedSeat>((Func<KeyValuePair<VehiclePawn, List<AssignedSeat>>, IEnumerable<AssignedSeat>>) (kvp => (IEnumerable<AssignedSeat>) kvp.Value)))
      this.pawnAssignment[assignedSeat.pawn] = assignedSeat;
  }

  private void UpdateVehicleAssignments()
  {
    this.vehicleAssignments.Clear();
    foreach (AssignedSeat assignedSeat in this.pawnAssignment.Values)
      this.vehicleAssignments.AddOrAppend<VehiclePawn, List<AssignedSeat>, AssignedSeat>(assignedSeat.Vehicle, assignedSeat);
  }
}
