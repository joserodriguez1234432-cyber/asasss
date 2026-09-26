// Decompiled with JetBrains decompiler
// Type: Vehicles.ThreadDisabler
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class ThreadDisabler : IDisposable
{
  private readonly Dictionary<Map, bool> threadStates = new Dictionary<Map, bool>();

  public ThreadDisabler()
  {
    foreach (Map map in Find.Maps)
    {
      VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
      if (cachedMapComponent.ThreadAlive)
      {
        this.threadStates[map] = !cachedMapComponent.dedicatedThread.IsSuspended;
        cachedMapComponent.dedicatedThread.Suspend();
      }
    }
  }

  public void Dispose()
  {
    foreach (Map map in Find.Maps)
    {
      VehiclePathingSystem cachedMapComponent = map.GetCachedMapComponent<VehiclePathingSystem>();
      bool flag;
      if (((!cachedMapComponent.ThreadAlive ? 0 : (this.threadStates.TryGetValue(map, out flag) ? 1 : 0)) & (flag ? 1 : 0)) != 0)
        cachedMapComponent.dedicatedThread.Unsuspend();
    }
    GC.SuppressFinalize((object) this);
  }
}
