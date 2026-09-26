// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapParentsComponent
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld.Planet;
using System;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapParentsComponent : WorldComponent
{
  private static MapParent_Vehicle[] cachedMapParentVehicle = new MapParent_Vehicle[32 /*0x20*/];

  public VehicleMapParentsComponent(World world)
    : base(world)
  {
    Command_FocusVehicleMap.FocusLockedVehicle = (VehiclePawnWithMap) null;
    Command_FocusVehicleMap.FocusedVehicle = (VehiclePawnWithMap) null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static MapParent_Vehicle GetCachedVehicle(Map map)
  {
    if (map == null)
      return (MapParent_Vehicle) null;
    int uniqueId = map.uniqueID;
    return uniqueId >= 0 && uniqueId < VehicleMapParentsComponent.cachedMapParentVehicle.Length ? VehicleMapParentsComponent.cachedMapParentVehicle[uniqueId] : (MapParent_Vehicle) null;
  }

  public static void SetCachedVehicle(Map map, MapParent_Vehicle parent)
  {
    int uniqueId = map.uniqueID;
    if (uniqueId < 0)
      return;
    if (uniqueId >= VehicleMapParentsComponent.cachedMapParentVehicle.Length)
    {
      int length = VehicleMapParentsComponent.cachedMapParentVehicle.Length;
      while (uniqueId >= length)
        length *= 2;
      Array.Resize<MapParent_Vehicle>(ref VehicleMapParentsComponent.cachedMapParentVehicle, length);
    }
    VehicleMapParentsComponent.cachedMapParentVehicle[uniqueId] = parent;
  }

  public virtual void FinalizeInit(bool fromLoad)
  {
    Array.Clear((Array) VehicleMapParentsComponent.cachedMapParentVehicle, 0, VehicleMapParentsComponent.cachedMapParentVehicle.Length);
  }
}
