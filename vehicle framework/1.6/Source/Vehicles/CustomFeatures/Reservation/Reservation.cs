// Decompiled with JetBrains decompiler
// Type: Vehicles.Reservation`1
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Text;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public abstract class Reservation<T> : ReservationBase
{
  public Reservation()
  {
  }

  public Reservation(VehiclePawn vehicle, Job job, int maxClaimants)
    : base(vehicle, job, maxClaimants)
  {
  }

  public abstract bool AddClaimant(Pawn pawn, T target);

  public abstract bool CanReserve(Pawn pawn, T target, StringBuilder stringBuilder = null);

  public abstract bool ReservedBy(Pawn pawn, T target);
}
