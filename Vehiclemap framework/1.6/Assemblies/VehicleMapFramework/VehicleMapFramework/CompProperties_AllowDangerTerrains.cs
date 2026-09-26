// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompProperties_AllowDangerTerrains
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class CompProperties_AllowDangerTerrains : CompProperties
{
  public List<TerrainDef> allowedDangerTerrains;

  public CompProperties_AllowDangerTerrains() => this.compClass = typeof (CompAllowDangerTerrains);

  public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
  {
    if (GenList.NullOrEmpty<TerrainDef>((IList<TerrainDef>) this.allowedDangerTerrains))
    {
      yield return "allowedDangerTerrains is null or empty";
    }
    else
    {
      foreach (TerrainDef allowedDangerTerrain in this.allowedDangerTerrains)
      {
        if (!allowedDangerTerrain.dangerous)
          yield return $"terrain {((Def) allowedDangerTerrain).defName} is not dangerous";
      }
    }
  }
}
