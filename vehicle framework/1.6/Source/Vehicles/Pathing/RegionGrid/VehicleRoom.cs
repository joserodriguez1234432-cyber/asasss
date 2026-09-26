// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRoom
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class VehicleRoom
{
  private static int nextRoomID;
  public sbyte mapIndex = -1;
  public int id = -1;
  private readonly VehicleDef vehicleDef;
  private int cellCount = -1;
  public int lastChangeTick = -1;
  private int numRegionsTouchingMapEdge;

  public VehicleRoom(VehicleDef vehicleDef) => this.vehicleDef = vehicleDef;

  public Map Map => this.mapIndex < (sbyte) 0 ? (Map) null : Find.Maps[(int) this.mapIndex];

  public RegionType RegionType
  {
    get
    {
      return !this.Regions.NullOrEmpty<VehicleRegion>() ? this.Regions.FirstOrDefault<KeyValuePair<VehicleRegion, byte>>().Key.type : (RegionType) 0;
    }
  }

  public ConcurrentSet<VehicleRegion> Regions { get; } = new ConcurrentSet<VehicleRegion>();

  public int RegionCount => this.Regions.Count;

  public bool TouchesMapEdge => this.numRegionsTouchingMapEdge > 0;

  private IEnumerable<IntVec3> Cells
  {
    get
    {
      foreach (VehicleRegion key in (IEnumerable<VehicleRegion>) this.Regions.Keys)
      {
        IEnumerator<IntVec3> enumerator = key.Cells.GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
        enumerator = (IEnumerator<IntVec3>) null;
      }
    }
  }

  public int CellCount
  {
    get
    {
      if (this.cellCount < 0)
      {
        this.cellCount = 0;
        foreach (VehicleRegion key in (IEnumerable<VehicleRegion>) this.Regions.Keys)
          this.cellCount += key.CellCount;
      }
      return this.cellCount;
    }
  }

  public static VehicleRoom MakeNew(Map map, VehicleDef vehicleDef)
  {
    int num = Interlocked.CompareExchange(ref VehicleRoom.nextRoomID, 0, 0);
    VehicleRoom vehicleRoom = new VehicleRoom(vehicleDef)
    {
      mapIndex = (sbyte) map.Index,
      id = num
    };
    Interlocked.Increment(ref VehicleRoom.nextRoomID);
    return vehicleRoom;
  }

  public void AddRegion(VehicleRegion region)
  {
    if (this.Regions.ContainsKey(region))
    {
      Log.Error($"Tried to add the same region twice to Room. region={region} room={this}");
    }
    else
    {
      this.Regions.Add(region);
      this.cellCount = -1;
      if (region.touchesMapEdge)
        ++this.numRegionsTouchingMapEdge;
      if (this.Regions.Count != 1)
        return;
      this.Map.GetCachedMapComponent<VehiclePathingSystem>()[this.vehicleDef].VehicleRegionGrid.allRooms.Add(this);
    }
  }

  public void RemoveRegion(VehicleRegion region)
  {
    if (!this.Regions.ContainsKey(region))
    {
      Log.Warning($"Tried to remove region from Room but this region is not here. region={region} room={this}");
    }
    else
    {
      this.Regions.Remove(region);
      this.cellCount = -1;
      if (region.touchesMapEdge)
        --this.numRegionsTouchingMapEdge;
      if (this.Regions.Count != 0)
        return;
      MapComponentCache<VehiclePathingSystem>.GetComponent(this.Map)?[this.vehicleDef].VehicleRegionGrid?.allRooms.Remove(this);
    }
  }

  internal void DebugDraw(DebugRegionType debugRegionType)
  {
    if ((debugRegionType & DebugRegionType.Rooms) == DebugRegionType.None)
      return;
    float num = Rand.ValueSeeded(this.GetHashCode());
    foreach (IntVec3 cell in this.Cells)
      CellRenderer.RenderCell(cell, num);
  }

  public override int GetHashCode()
  {
    return Gen.HashCombineInt(this.id, ((object) this.vehicleDef).GetHashCode());
  }
}
