// Decompiled with JetBrains decompiler
// Type: Vehicles.ThinkNode_ExecuteAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class ThinkNode_ExecuteAction : ThinkNode
{
  private DynamicDelegate<VehiclePawn> action;

  public virtual ThinkNode DeepCopy(bool resolve = true)
  {
    ThinkNode_ExecuteAction nodeExecuteAction = (ThinkNode_ExecuteAction) base.DeepCopy(resolve);
    nodeExecuteAction.action = this.action;
    return (ThinkNode) nodeExecuteAction;
  }

  public virtual ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
  {
    if (!((Thing) pawn).Spawned)
      return ThinkResult.NoJob;
    if (!(pawn is VehiclePawn vehiclePawn))
    {
      Log.Error($"Trying to assign vehicle job to non-vehicle pawn {pawn}.");
      return ThinkResult.NoJob;
    }
    this.action.Invoke((object) null, vehiclePawn);
    return ThinkResult.NoJob;
  }
}
