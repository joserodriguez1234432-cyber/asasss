// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleJobDriver
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public abstract class VehicleJobDriver : JobDriver
{
  protected virtual VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo targetA = this.TargetA;
      return ((LocalTargetInfo) ref targetA).Thing as VehiclePawn;
    }
  }

  public virtual IntVec3 JobCell
  {
    get
    {
      LocalTargetInfo targetB = this.TargetB;
      return ((LocalTargetInfo) ref targetB).Cell;
    }
  }

  protected abstract JobDef JobDef { get; }

  public virtual bool TryMakePreToilReservations(bool errorOnFailed)
  {
    IntVec3 jobCell = this.JobCell;
    return ((IntVec3) ref jobCell).IsValid && MapComponentCache<VehicleReservationManager>.GetComponent(((Thing) this.Vehicle).Map).Reserve<LocalTargetInfo, VehicleTargetReservation>(this.Vehicle, this.pawn, this.job, LocalTargetInfo.op_Implicit(this.JobCell));
  }
}
