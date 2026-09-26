// Decompiled with JetBrains decompiler
// Type: Vehicles.AssignedSeat
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace Vehicles;

public class AssignedSeat : IExposable
{
  public Pawn pawn;
  public VehicleRoleHandler handler;

  public AssignedSeat()
  {
  }

  public AssignedSeat([NotNull] Pawn pawn, [NotNull] VehicleRoleHandler handler)
  {
    this.pawn = pawn;
    this.handler = handler;
  }

  public VehiclePawn Vehicle => this.handler?.vehicle;

  public static implicit operator (Pawn, VehicleRoleHandler)(AssignedSeat assignedSeat)
  {
    return (assignedSeat.pawn, assignedSeat.handler);
  }

  public void ExposeData()
  {
    Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
    Scribe_References.Look<VehicleRoleHandler>(ref this.handler, "handler", false);
  }
}
