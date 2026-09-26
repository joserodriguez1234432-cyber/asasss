// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleStatDef : Def, IDefIndex<VehicleStatDef>
{
  public float defaultBaseValue;
  public float minValue = float.MinValue;
  public float maxValue = float.MaxValue;
  public float hideAtValue = float.NaN;
  public bool alwaysHide;
  public bool showIfUndefined;
  public bool neverDisabled;
  public bool showZeroBaseValue;
  public bool applyFactorsIfNegative = true;
  public List<VehicleStatDef> statFactors;
  public List<VehicleStatPart> parts;
  public SettingsValueInfo modSettingsInfo;
  public StatCategoryDef category;
  public List<string> showIfModsLoaded;
  public List<VehicleType> showOnVehicleTypes;
  public string formatString;
  public ToStringStyle toStringStyle;
  public ToStringStyle? toStringStyleUnfinalized;
  public ToStringNumberSense toStringNumberSense = (ToStringNumberSense) 1;
  public EfficiencyOperationType operationType;
  public UpgradeEffectType upgradeEffectType;
  public SimpleCurve postProcessCurve;
  public List<VehicleStatDef> postProcessStatFactors;
  public System.Type workerClass = typeof (VehicleStatWorker);
  public int displayPriorityInCategory = 1;
  [MustTranslate]
  public string formatStringUnfinalized;
  [Unsaved(false)]
  private VehicleStatWorker statWorker;

  public int DefIndex { get; set; }

  public VehicleStatWorker Worker
  {
    get
    {
      if (this.statWorker == null)
      {
        if (!GenList.NullOrEmpty<VehicleStatPart>((IList<VehicleStatPart>) this.parts))
        {
          foreach (VehicleStatPart part in this.parts)
            part.statDef = this;
        }
        this.statWorker = (VehicleStatWorker) Activator.CreateInstance(this.workerClass);
        this.statWorker.InitStatWorker(this);
      }
      return this.statWorker;
    }
  }

  public ToStringStyle ToStringStyleUnfinalized
  {
    get => this.toStringStyleUnfinalized ?? this.toStringStyle;
  }

  public virtual void PostLoad()
  {
    this.modSettingsInfo.minValue = this.minValue;
    this.modSettingsInfo.maxValue = this.maxValue;
  }

  public string ValueToString(float val, bool finalized = true, ToStringNumberSense numberSense = 1)
  {
    return this.Worker.ValueToString(val, finalized, numberSense);
  }

  public bool CanShowWithLoadedMods()
  {
    if (!GenList.NullOrEmpty<string>((IList<string>) this.showIfModsLoaded))
    {
      foreach (string packageId in this.showIfModsLoaded)
      {
        if (!Ext_Mods.HasActiveMod(packageId))
          return false;
      }
    }
    return true;
  }

  public bool CanShowWithVehicle(VehicleDef vehicleDef)
  {
    return GenList.NullOrEmpty<VehicleType>((IList<VehicleType>) this.showOnVehicleTypes) || this.showOnVehicleTypes.Contains(vehicleDef.type);
  }
}
