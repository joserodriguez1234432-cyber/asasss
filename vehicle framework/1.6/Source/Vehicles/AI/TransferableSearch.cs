// Decompiled with JetBrains decompiler
// Type: Vehicles.TransferableSearch
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools.Performance;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public sealed class TransferableSearch : ISharedJobSearch, IPoolable
{
  private JobDef jobDef;
  private Lord lord;
  private TransferableOneWay transferable;

  bool IPoolable.InPool { get; set; }

  ThingDef ISharedJobSearch.ThingDef => ((Transferable) this.transferable).ThingDef;

  public void Init(JobDef jobDef, TransferableOneWay transferable)
  {
    this.jobDef = jobDef;
    this.transferable = transferable;
  }

  public void Init(JobDef jobDef, TransferableOneWay transferable, Lord lord)
  {
    this.Init(jobDef, transferable);
    this.lord = lord;
  }

  bool ISharedJobSearch.IsMatchingThing(Thing thing) => this.transferable.things.Contains(thing);

  bool ISharedJobSearch.ShouldConsiderPawn(Pawn otherPawn)
  {
    return (otherPawn.lord ?? otherPawn.CurJob?.lord) == this.lord && otherPawn.CurJobDef == this.jobDef;
  }

  void IPoolable.Reset()
  {
    this.jobDef = (JobDef) null;
    this.transferable = (TransferableOneWay) null;
    this.lord = (Lord) null;
  }
}
