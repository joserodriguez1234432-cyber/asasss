// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleRegion
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public sealed class VehicleRegion : IPoolable
{
  public const int ChunkSize = 12;
  public RegionType type = (RegionType) 2;
  private VehicleDef vehicleDef;
  private int referenceCount;
  private int cellCount;
  private VehicleRoom room;
  private Map map;
  private VehiclePathingSystem mapping;
  private VehicleRegionMaker regionMaker;
  private VehicleRegionGrid regionGrid;
  private readonly List<VehicleRegionLink> links = new List<VehicleRegionLink>();
  private readonly object linksLock = new object();
  public ThreadLocal<uint[]> closedIndex = new ThreadLocal<uint[]>((Func<uint[]>) (() => new uint[8]));
  public CellRect extentsClose;
  public CellRect extentsLimit;
  public bool touchesMapEdge;
  public bool valid = true;
  public uint reachedIndex;
  public int newRegionGroupIndex = -1;
  public int mark;
  private int debugMakeTick = -1000;

  public int Id { get; private set; }

  private bool DebugIsNew => this.debugMakeTick > Find.TickManager.TicksGame - 60;

  private int ReferenceCount => this.referenceCount;

  public bool InPool { get; set; }

  public int LinksCount => this.links.Count;

  public ListSnapshot<VehicleRegionLink> Links
  {
    get
    {
      lock (this.linksLock)
        return new ListSnapshot<VehicleRegionLink>(this.links);
    }
  }

  public Map Map
  {
    get => this.map;
    internal set
    {
      if (this.map == value)
        return;
      this.map = value;
      if (this.map == null)
      {
        this.mapping = (VehiclePathingSystem) null;
        this.regionMaker = (VehicleRegionMaker) null;
      }
      else
      {
        this.mapping = this.map.GetCachedMapComponent<VehiclePathingSystem>();
        this.regionMaker = this.mapping[this.vehicleDef].VehicleRegionMaker;
        this.regionGrid = this.mapping[this.vehicleDef].VehicleRegionGrid;
      }
    }
  }

  public int CellCount
  {
    get
    {
      if (this.cellCount < 0)
      {
        for (int minZ = this.extentsClose.minZ; minZ <= this.extentsClose.maxZ; ++minZ)
        {
          for (int minX = this.extentsClose.minX; minX <= this.extentsClose.maxX; ++minX)
          {
            IntVec3 cell;
            ((IntVec3) ref cell).\u002Ector(minX, 0, minZ);
            if (this.regionGrid.GetRegionAt(cell) == this)
              Interlocked.Increment(ref this.cellCount);
          }
        }
      }
      return this.cellCount;
    }
  }

  public IEnumerable<IntVec3> Cells
  {
    get
    {
      VehicleRegion vehicleRegion = this;
      if (!vehicleRegion.InPool)
      {
        VehicleRegionGrid regions = vehicleRegion.mapping[vehicleRegion.vehicleDef].VehicleRegionGrid;
        for (int z = vehicleRegion.extentsClose.minZ; z <= vehicleRegion.extentsClose.maxZ; ++z)
        {
          for (int x = vehicleRegion.extentsClose.minX; x <= vehicleRegion.extentsClose.maxX; ++x)
          {
            IntVec3 cell;
            ((IntVec3) ref cell).\u002Ector(x, 0, z);
            if (regions.GetRegionAt(cell) == vehicleRegion)
              yield return cell;
          }
        }
      }
    }
  }

  private IEnumerable<VehicleRegion> Neighbors
  {
    get
    {
      VehicleRegion vehicleRegion = this;
      lock (vehicleRegion.linksLock)
      {
        foreach (VehicleRegionLink link in vehicleRegion.links)
        {
          if (link.regionA != null && link.regionA != vehicleRegion && link.regionA.valid)
            yield return link.regionA;
          if (link.regionB != null && link.regionB != vehicleRegion && link.regionB.valid)
            yield return link.regionB;
        }
      }
    }
  }

  internal IEnumerable<VehicleRegion> NeighborsOfSameType
  {
    get
    {
      VehicleRegion vehicleRegion = this;
      lock (vehicleRegion.linksLock)
      {
        foreach (VehicleRegionLink link in vehicleRegion.links)
        {
          if (link.regionA != null && link.regionA != vehicleRegion && link.regionA.type == vehicleRegion.type && link.regionA.valid)
            yield return link.regionA;
          if (link.regionB != null && link.regionB != vehicleRegion && link.regionB.type == vehicleRegion.type && link.regionB.valid)
            yield return link.regionB;
        }
      }
    }
  }

  public VehicleRoom Room
  {
    get => this.room;
    set
    {
      if (value == this.room)
        return;
      this.room?.RemoveRegion(this);
      this.room = value;
      this.room?.AddRegion(this);
    }
  }

  public IntVec3 RandomCell
  {
    get
    {
      CellIndices cellIndices = this.Map.cellIndices;
      VehicleRegion[] directGrid = this.Map.GetCachedMapComponent<VehiclePathingSystem>()[this.vehicleDef].VehicleRegionGrid.DirectGrid;
      for (int index = 0; index < 1000; ++index)
      {
        IntVec3 randomCell = ((CellRect) ref this.extentsClose).RandomCell;
        if (directGrid[((CellIndices) ref cellIndices).CellToIndex(randomCell)] == this)
          return randomCell;
      }
      return this.AnyCell;
    }
  }

  public IntVec3 AnyCell
  {
    get
    {
      CellIndices cellIndices = this.Map.cellIndices;
      VehicleRegion[] directGrid = this.Map.GetCachedMapComponent<VehiclePathingSystem>()[this.vehicleDef].VehicleRegionGrid.DirectGrid;
      foreach (IntVec3 anyCell in this.extentsClose)
      {
        if (directGrid[((CellIndices) ref cellIndices).CellToIndex(anyCell)] == this)
          return anyCell;
      }
      Log.Error("Couldn't find any cell in region " + this.ToString());
      return ((CellRect) ref this.extentsClose).RandomCell;
    }
  }

  public void Init(VehicleDef vehicleDef, int id)
  {
    this.vehicleDef = vehicleDef;
    this.Id = id;
    this.cellCount = -1;
    this.debugMakeTick = Find.TickManager.TicksGame;
    this.type = (RegionType) 2;
    this.extentsClose = CellRect.Empty;
    this.extentsLimit = CellRect.Empty;
    this.touchesMapEdge = false;
    this.valid = true;
    this.reachedIndex = 0U;
    this.newRegionGroupIndex = -1;
  }

  public void IncrementRefCount() => Interlocked.Increment(ref this.referenceCount);

  public void DecrementRefCount()
  {
    Interlocked.Decrement(ref this.referenceCount);
    if (this.ReferenceCount != 0)
      return;
    this.regionMaker.Return(this);
  }

  public void AddLink(VehicleRegionLink regionLink)
  {
    lock (this.linksLock)
      this.links.Add(regionLink);
  }

  internal float WeightBetween(VehicleRegionLink linkA, VehicleRegionLink linkB)
  {
    Log.Error($"Unable to pull weight between {linkA.anchor} and {linkB.anchor}");
    return 0.0f;
  }

  public void Reset()
  {
    this.valid = false;
    this.Room = (VehicleRoom) null;
    this.cellCount = 0;
    this.referenceCount = 0;
    this.extentsClose = CellRect.Empty;
    this.extentsLimit = CellRect.Empty;
    this.ClearLinks();
  }

  private void ClearLinks()
  {
    lock (this.linksLock)
      this.links.Clear();
  }

  public static int EuclideanDistance(IntVec3 cell, VehicleRegionLink link)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(cell, link.anchor);
    return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow((float) intVec3.x, 2f) + Mathf.Pow((float) intVec3.z, 2f)));
  }

  public bool Allows(TraverseParms traverseParms)
  {
    bool flag;
    switch (traverseParms.mode - 3)
    {
      case 0:
        flag = true;
        break;
      case 1:
        flag = true;
        break;
      case 3:
        flag = true;
        break;
      default:
        flag = RegionTypeUtility.Passable(this.type);
        break;
    }
    return flag;
  }

  public override string ToString() => $"VehicleRegion_{this.Id}";

  public void DebugDraw()
  {
    GenDraw.DrawFieldEdges(this.Cells.ToList<IntVec3>(), new Color(0.0f, 0.0f, 1f, 0.5f), new float?(), (HashSet<IntVec3>) null, 2900);
  }

  public void DebugDraw(DebugRegionType debugRegionType)
  {
    Color color = this.valid ? (!this.DebugIsNew ? (RegionTypeUtility.Passable(this.type) ? Color.green : ColorLibrary.Orange) : Color.yellow) : Color.red;
    if ((debugRegionType & DebugRegionType.Regions) != DebugRegionType.None)
    {
      GenDraw.DrawFieldEdges(this.Cells.ToList<IntVec3>(), color, new float?(), (HashSet<IntVec3>) null, 2900);
      foreach (VehicleRegion neighbor in this.Neighbors)
        GenDraw.DrawFieldEdges(neighbor.Cells.ToList<IntVec3>(), Color.grey, new float?(), (HashSet<IntVec3>) null, 2900);
    }
    if ((debugRegionType & DebugRegionType.Links) == DebugRegionType.None)
      return;
    using (ListSnapshot<VehicleRegionLink> links = this.Links)
    {
      foreach (VehicleRegionLink vehicleRegionLink in links)
      {
        if (Mathf.RoundToInt(Time.realtimeSinceStartup * 2f) % 2 == 1)
        {
          Material material = DebugSolidColorMats.MaterialOf(Color.op_Multiply(Color.magenta, new Color(1f, 1f, 1f, 0.25f)));
          List<IntVec3> list = ((EdgeSpan) ref vehicleRegionLink.span).Cells.ToList<IntVec3>();
          foreach (IntVec3 intVec3 in list)
            CellRenderer.RenderCell(intVec3, material);
          GenDraw.DrawFieldEdges(list, Color.white, new float?(), (HashSet<IntVec3>) null, 2900);
        }
      }
    }
  }

  [Conditional("HIERARCHAL_PATHFINDING")]
  private void DrawWeights()
  {
    using (ListSnapshot<VehicleRegionLink> links = this.Links)
    {
      for (int index1 = 0; index1 < links.Count; ++index1)
      {
        VehicleRegionLink vehicleRegionLink1 = links.items[index1];
        for (int index2 = index1 + 1; index2 < links.Count; ++index2)
        {
          VehicleRegionLink vehicleRegionLink2 = links.items[index2];
          float weight = 1f;
          Vector3 vector3_1 = ((IntVec3) ref vehicleRegionLink1.anchor).ToVector3();
          vector3_1.y += Altitudes.AltitudeFor((AltitudeLayer) 38);
          Vector3 vector3_2 = ((IntVec3) ref vehicleRegionLink2.anchor).ToVector3();
          vector3_2.y += Altitudes.AltitudeFor((AltitudeLayer) 38);
          GenDraw.DrawLineBetween(vector3_1, vector3_2, VehicleRegionLink.WeightColor(weight), 0.2f);
        }
      }
    }
  }

  public void DebugOnGUIMouseover(DebugRegionType debugRegionType)
  {
    if ((debugRegionType & DebugRegionType.PathCosts) != DebugRegionType.None)
    {
      if (Find.CameraDriver.CurrentZoom > 1)
        return;
      foreach (IntVec3 cell in this.Cells)
      {
        Vector2 uiPosition = ((IntVec3) ref cell).ToUIPosition();
        Rect rect1;
        // ISSUE: explicit constructor call
        ((Rect) ref rect1).\u002Ector(uiPosition.x - 20f, uiPosition.y - 20f, 40f, 40f);
        Rect rect2 = new Rect(0.0f, 0.0f, (float) UI.screenWidth, (float) UI.screenHeight);
        if (((Rect) ref rect2).Overlaps(rect1))
          Widgets.Label(rect1, this.Map.GetCachedMapComponent<VehiclePathingSystem>()[DebugHelper.Local.VehicleDef].VehiclePathGrid.PerceivedPathCostAt(cell).ToString());
      }
    }
    else
    {
      if ((debugRegionType & DebugRegionType.References) == DebugRegionType.None || Find.CameraDriver.CurrentZoom > 1)
        return;
      IntVec3 intVec3;
      // ISSUE: explicit constructor call
      ((IntVec3) ref intVec3).\u002Ector(this.extentsClose.minX, 0, this.extentsClose.minZ);
      Vector2 uiPosition = ((IntVec3) ref intVec3).ToUIPosition();
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(uiPosition.x - 20f, uiPosition.y - 20f, 40f, 40f);
      Rect rect4 = new Rect(0.0f, 0.0f, (float) UI.screenWidth, (float) UI.screenHeight);
      if (!((Rect) ref rect4).Overlaps(rect3))
        return;
      Widgets.Label(rect3, this.ReferenceCount.ToString());
    }
  }

  public static CellRect ChunkAt(IntVec3 cell)
  {
    return new CellRect()
    {
      minX = cell.x - cell.x % 12,
      maxX = cell.x + 12 - (cell.x + 12) % 12 - 1,
      minZ = cell.z - cell.z % 12,
      maxZ = cell.z + 12 - (cell.z + 12) % 12 - 1
    };
  }
}
