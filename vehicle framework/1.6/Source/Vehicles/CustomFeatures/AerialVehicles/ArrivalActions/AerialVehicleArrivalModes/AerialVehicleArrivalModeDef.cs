// Decompiled with JetBrains decompiler
// Type: Vehicles.AerialVehicleArrivalModeDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using Verse;

#nullable disable
namespace Vehicles;

public class AerialVehicleArrivalModeDef : Def
{
  public System.Type workerClass = typeof (AerialVehicleArrivalModeWorker);
  public SimpleCurve selectionWeightCurve;
  public SimpleCurve pointsFactorCurve;
  public TechLevel minTechLevel;
  public bool forQuickMilitaryAid;
  public bool walkIn;
  [MustTranslate]
  public string textEnemy;
  [MustTranslate]
  public string textFriendly;
  [MustTranslate]
  public string textWillArrive;
  [Unsaved(false)]
  private AerialVehicleArrivalModeWorker workerInt;

  public AerialVehicleArrivalModeWorker Worker
  {
    get
    {
      if (this.workerInt == null)
      {
        this.workerInt = (AerialVehicleArrivalModeWorker) Activator.CreateInstance(this.workerClass);
        this.workerInt.def = this;
      }
      return this.workerInt;
    }
  }
}
