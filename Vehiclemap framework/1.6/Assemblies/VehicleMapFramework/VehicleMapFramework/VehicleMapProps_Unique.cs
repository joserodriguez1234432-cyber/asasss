// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapProps_Unique
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class VehicleMapProps_Unique : VehicleMapProps
{
  [Unsaved(false)]
  public VehicleDef baseDef;
  public int placeholderCount = 32 /*0x20*/;

  public virtual void ResolveReferences(Def parentDef)
  {
    base.ResolveReferences(parentDef);
    VehicleDef vehicleDef = parentDef as VehicleDef;
    if (vehicleDef == null)
      return;
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      List<VehicleDef> vehicleDefList;
      if (!UniqueVehicleManager.PlaceholderDefs.TryGetValue(vehicleDef, out vehicleDefList))
        UniqueVehicleManager.PlaceholderDefs[vehicleDef] = vehicleDefList = new List<VehicleDef>();
      vehicleDefList.Clear();
      for (int index = 0; index < this.placeholderCount; ++index)
      {
        VehicleDef uniqueVehicleDef = UniqueVehicleUtility.GenerateUniqueVehicleDef(vehicleDef, index);
        vehicleDefList.Add(uniqueVehicleDef);
      }
    }));
  }
}
