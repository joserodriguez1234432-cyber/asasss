// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleSustainers
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class VehicleSustainers
{
  private VehiclePawn vehicle;
  private List<Sustainer> activeSustainers = new List<Sustainer>();

  public VehicleSustainers(VehiclePawn vehicle) => this.vehicle = vehicle;

  public void Spawn(ISustainerTarget sustainerTarget, SoundDef soundDef)
  {
    this.SpawnSustainer(sustainerTarget.Target, soundDef, sustainerTarget.MaintenanceType);
  }

  public void Spawn(VehiclePawn vehicle, SoundDef soundDef)
  {
    this.SpawnSustainer(TargetInfo.op_Implicit((Thing) vehicle), soundDef, (MaintenanceType) 1);
  }

  private void SpawnSustainer(
    TargetInfo target,
    SoundDef soundDef,
    MaintenanceType maintenanceType = 1)
  {
    SoundInfo soundInfo = SoundInfo.InMap(target, maintenanceType);
    this.activeSustainers.Add(SoundStarter.TrySpawnSustainer(soundDef, soundInfo));
  }

  public void EndAll()
  {
    for (int index = this.activeSustainers.Count - 1; index >= 0; --index)
      this.activeSustainers[index]?.End();
  }

  public void EndAll(SoundDef soundDef)
  {
    List<Sustainer> list = this.activeSustainers.Where<Sustainer>((Func<Sustainer, bool>) (sustainer => sustainer?.def == soundDef)).ToList<Sustainer>();
    for (int index = list.Count - 1; index >= 0; --index)
      list[index].End();
  }

  public void Tick()
  {
    for (int index = this.activeSustainers.Count - 1; index >= 0; --index)
    {
      Sustainer activeSustainer = this.activeSustainers[index];
      if (activeSustainer == null || activeSustainer.Ended)
        this.activeSustainers.Remove(activeSustainer);
      else
        activeSustainer.Maintain();
    }
  }
}
