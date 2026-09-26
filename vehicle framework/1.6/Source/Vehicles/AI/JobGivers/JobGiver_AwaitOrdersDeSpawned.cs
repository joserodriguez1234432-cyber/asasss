// Decompiled with JetBrains decompiler
// Type: Vehicles.JobGiver_AwaitOrdersDeSpawned
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class JobGiver_AwaitOrdersDeSpawned : ThinkNode_JobGiver
{
  protected virtual Job TryGiveJob(Pawn pawn)
  {
    return JobMaker.MakeJob(JobDefOf_Vehicles.IdleVehicleDeSpawned);
  }
}
