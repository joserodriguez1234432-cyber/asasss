// Decompiled with JetBrains decompiler
// Type: Vehicles.Job_Vehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class Job_Vehicle : Job
{
  public VehicleRoleHandler handler;

  public Job_Vehicle()
  {
  }

  public Job_Vehicle(JobDef def)
    : base(def)
  {
  }

  public Job_Vehicle(JobDef def, LocalTargetInfo targetA)
    : base(def, targetA, LocalTargetInfo.op_Implicit((Thing) null))
  {
  }

  public Job_Vehicle(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB)
    : base(def, targetA, targetB)
  {
  }

  public Job_Vehicle(
    JobDef def,
    LocalTargetInfo targetA,
    LocalTargetInfo targetB,
    LocalTargetInfo targetC)
    : base(def, targetA, targetB, targetC)
  {
  }

  public Job_Vehicle(
    JobDef def,
    LocalTargetInfo targetA,
    int expiryInterval,
    bool checkOverrideOnExpiry = false)
    : base(def, targetA, expiryInterval, checkOverrideOnExpiry)
  {
  }

  public Job_Vehicle(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
    : base(def, expiryInterval, checkOverrideOnExpiry)
  {
  }
}
