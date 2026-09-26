// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VirtualTeleporter
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public readonly struct VirtualTeleporter : IDisposable
{
  public static readonly AccessTools.FieldRef<Thing, sbyte> mapIndexOrState = AccessTools.FieldRefAccess<Thing, sbyte>(nameof (mapIndexOrState));
  private readonly Thing _thing;
  private readonly Map _map;
  private readonly IntVec3 _pos;
  private readonly bool _setDepartMap;
  private readonly bool _departMapIsNull;
  private readonly bool _departPositionIsNull;

  public VirtualTeleporter(Thing thing, Map map, IntVec3? c = null, bool setDepartMap = false)
  {
    this._map = (Map) null;
    this._setDepartMap = false;
    this._departMapIsNull = false;
    this._departPositionIsNull = false;
    this._pos = IntVec3.Invalid;
    this._thing = thing;
    if (thing == null || !thing.Spawned)
      return;
    this._map = thing.Map;
    this._pos = thing.Position;
    if (map != null)
      VirtualTeleporter.mapIndexOrState.Invoke(thing) = (sbyte) map.Index;
    if (c.HasValue)
      thing.SetPositionDirect(c.Value);
    this._setDepartMap = setDepartMap;
    if (!this._setDepartMap || !(thing is Pawn pawn))
      return;
    if (CrossMapReachabilityUtility.get_DepartMap(pawn) == null)
    {
      CrossMapReachabilityUtility.set_DepartMap(pawn, this._map);
      this._departMapIsNull = true;
    }
    if (!c.HasValue || CrossMapReachabilityUtility.get_DepartPosition(pawn).HasValue)
      return;
    CrossMapReachabilityUtility.set_DepartPosition(pawn, new IntVec3?(this._pos));
    this._departPositionIsNull = true;
  }

  public void Dispose()
  {
    if (this._thing == null)
      return;
    if (this._map != null)
      VirtualTeleporter.mapIndexOrState.Invoke(this._thing) = (sbyte) this._map.Index;
    IntVec3 pos = this._pos;
    if (((IntVec3) ref pos).IsValid)
      this._thing.SetPositionDirect(this._pos);
    if (!this._setDepartMap || !(this._thing is Pawn thing))
      return;
    if (this._departMapIsNull)
      CrossMapReachabilityUtility.set_DepartMap(thing, (Map) null);
    if (!this._departPositionIsNull)
      return;
    CrossMapReachabilityUtility.set_DepartPosition(thing, new IntVec3?());
  }
}
