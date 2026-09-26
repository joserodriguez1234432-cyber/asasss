// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStrategyDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleStrategyDef : Def
{
  public System.Type strategyWorker;
  private StrategyWorker worker;

  public StrategyWorker Worker
  {
    get
    {
      if (this.worker == null)
        this.worker = (StrategyWorker) Activator.CreateInstance(this.strategyWorker);
      return this.worker;
    }
  }
}
