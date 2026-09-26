// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompProperties_NpcVehicleMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class CompProperties_NpcVehicleMap : VehicleCompProperties
{
  public List<CompProperties_NpcVehicleMap.VehicleMapParams> mapParams;
  public float pawnCountWeight = 1f;

  public CompProperties_NpcVehicleMap()
  {
    ((CompProperties) this).compClass = typeof (CompNpcVehicleMap);
  }

  public class VehicleMapParams : IExposable
  {
    public IntRange pawnCountRange;
    public PrefabDef prefabDef;
    public Rot8 preferredDir;

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<IntRange>(ref this.pawnCountRange, "pawnCountRange", new IntRange(), false);
      Scribe_Defs.Look<PrefabDef>(ref this.prefabDef, "prefabDef");
      Scribe_Values.Look<Rot8>(ref this.preferredDir, "preferredDir", new Rot8(), false);
    }
  }
}
