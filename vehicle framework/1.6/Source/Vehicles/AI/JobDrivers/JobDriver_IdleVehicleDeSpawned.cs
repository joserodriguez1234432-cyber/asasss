// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_IdleVehicleDeSpawned
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class JobDriver_IdleVehicleDeSpawned : JobDriver
{
  public virtual bool TryMakePreToilReservations(bool errorOnFailed) => true;

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    JobDriver_IdleVehicleDeSpawned vehicleDeSpawned = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated method
    vehicleDeSpawned.AddEndCondition(new Func<JobCondition>(vehicleDeSpawned.\u003CMakeNewToils\u003Eb__1_0));
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = JobDriver_IdleVehicleDeSpawned.IdleWhileDespawned();
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  private static Toil IdleWhileDespawned()
  {
    Toil toil = ToilMaker.MakeToil(nameof (IdleWhileDespawned));
    toil.defaultCompleteMode = (ToilCompleteMode) 5;
    return toil;
  }
}
