// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenStep_VehicleInterior
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Verse;

#nullable disable
namespace VehicleMapFramework;

public class GenStep_VehicleInterior : GenStep
{
  public virtual int SeedPart => 6546854;

  public virtual void Generate(Map map, GenStepParams parms)
  {
    TerrainGrid terrainGrid = map.terrainGrid;
    foreach (IntVec3 allCell in map.AllCells)
    {
      if (GenGrid.InBounds(allCell, map))
        terrainGrid.SetTerrain(allCell, VMF_DefOf.VMF_VehicleFloor);
    }
  }
}
