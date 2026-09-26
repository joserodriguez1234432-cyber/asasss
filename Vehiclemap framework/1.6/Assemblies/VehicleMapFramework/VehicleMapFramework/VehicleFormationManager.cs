// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleFormationManager
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleFormationManager(World world) : WorldComponent(world)
{
  private List<VehicleFormationManager.FormationPreset> formationPresets = new List<VehicleFormationManager.FormationPreset>();

  public List<VehicleFormationManager.FormationPreset> FormationPresets => this.formationPresets;

  public virtual void ExposeData()
  {
    Scribe_Collections.Look<VehicleFormationManager.FormationPreset>(ref this.formationPresets, "formationPresets", (LookMode) 2, Array.Empty<object>());
  }

  public class FormationPreset : IExposable, IRenameable
  {
    public Dictionary<VehiclePawn, VehicleFormationComp.DrawData> drawPositions;
    private string labelInt;
    private List<VehiclePawn> keysWorkingList;
    private List<VehicleFormationComp.DrawData> valuesWorkingList;

    public string BaseLabel => "Formation";

    public string RenamableLabel
    {
      get => this.labelInt ?? this.BaseLabel;
      set => this.labelInt = value;
    }

    public string InspectLabel => this.RenamableLabel;

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<string>(ref this.labelInt, "labelInt", (string) null, false);
      Scribe_Collections.Look<VehiclePawn, VehicleFormationComp.DrawData>(ref this.drawPositions, "drawPositions", (LookMode) 3, (LookMode) 2, ref this.keysWorkingList, ref this.valuesWorkingList, true, false, false);
    }

    public class Dialog_RenameFormationPreset(VehicleFormationManager.FormationPreset preset) : 
      Dialog_Rename<VehicleFormationManager.FormationPreset>(preset)
    {
    }
  }
}
