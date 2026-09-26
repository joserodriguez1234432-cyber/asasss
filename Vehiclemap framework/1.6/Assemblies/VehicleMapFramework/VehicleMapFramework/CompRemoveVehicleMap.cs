// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompRemoveVehicleMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld.Planet;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompRemoveVehicleMap : ThingComp
{
  public virtual void Notify_PassedToWorld()
  {
    if (!(this.parent is VehiclePawnWithMap parent))
      return;
    FrameDelay.DelayOne<VehiclePawnWithMap>((Action<VehiclePawnWithMap>) (v =>
    {
      if (!WorldPawnsUtility.IsWorldPawn((Pawn) v) || ((Thing) v).ParentHolder != null)
        return;
      v.RemoveVehicleMap();
    }), parent);
  }
}
