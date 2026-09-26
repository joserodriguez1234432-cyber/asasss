// Decompiled with JetBrains decompiler
// Type: Vehicles.AsyncReachabilityCacheAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Performance;
using System.Collections.Generic;

#nullable disable
namespace Vehicles;

public class AsyncReachabilityCacheAction : AsyncAction
{
  private VehiclePathingSystem mapping;
  private List<VehicleDef> vehicleDefs;

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

  public void Set(VehiclePathingSystem mapping, List<VehicleDef> vehicleDefs)
  {
    this.mapping = mapping;
    this.vehicleDefs = vehicleDefs;
  }

  public override void Invoke()
  {
    foreach (VehicleDef vehicleDef in this.vehicleDefs)
    {
      if (this.mapping.GridOwners.IsOwner(vehicleDef))
        this.mapping[vehicleDef].VehicleReachability.ClearCache();
    }
  }

  public override void ReturnToPool()
  {
    this.mapping = (VehiclePathingSystem) null;
    this.vehicleDefs = (List<VehicleDef>) null;
    AsyncPool<AsyncReachabilityCacheAction>.Return(this);
  }
}
