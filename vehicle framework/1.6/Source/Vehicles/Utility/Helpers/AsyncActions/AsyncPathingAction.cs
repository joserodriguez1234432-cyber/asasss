// Decompiled with JetBrains decompiler
// Type: Vehicles.AsyncPathingAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using Verse;

#nullable disable
namespace Vehicles;

public class AsyncPathingAction : AsyncAction
{
  private VehiclePathingSystem mapping;
  private IntVec3 position;

  public override bool IsValid
  {
    get
    {
      VehiclePathingSystem mapping = this.mapping;
      if (mapping == null)
        return false;
      int? index = mapping.map?.Index;
      int num = -1;
      return index.GetValueOrDefault() > num & index.HasValue;
    }
  }

  public void Set(VehiclePathingSystem mapping, IntVec3 position)
  {
    this.mapping = mapping;
    this.position = position;
  }

  public override void Invoke()
  {
    PathingHelper.RecalculatePerceivedPathCostAtFor(this.mapping, this.position);
  }

  public override void ReturnToPool()
  {
    this.mapping = (VehiclePathingSystem) null;
    AsyncPool<AsyncPathingAction>.Return(this);
  }
}
