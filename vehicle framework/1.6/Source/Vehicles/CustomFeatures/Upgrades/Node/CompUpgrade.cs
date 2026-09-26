// Decompiled with JetBrains decompiler
// Type: Vehicles.CompUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class CompUpgrade : Upgrade
{
  public List<CompProperties> activate;
  public List<CompProperties> deactivate;

  public override bool UnlockOnLoad => false;

  public override void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    if (!GenList.NullOrEmpty<CompProperties>((IList<CompProperties>) this.activate))
    {
      foreach (CompProperties compProperties in this.activate)
        CompUpgrade.ActivateComp(vehicle, compProperties);
    }
    if (GenList.NullOrEmpty<CompProperties>((IList<CompProperties>) this.deactivate))
      return;
    foreach (CompProperties compProperties in this.deactivate)
      CompUpgrade.DeactivateComp(vehicle, compProperties);
  }

  public override void Refund(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<CompProperties>((IList<CompProperties>) this.activate))
    {
      foreach (CompProperties compProperties in this.activate)
        CompUpgrade.DeactivateComp(vehicle, compProperties);
    }
    if (GenList.NullOrEmpty<CompProperties>((IList<CompProperties>) this.deactivate))
      return;
    foreach (CompProperties compProperties in this.deactivate)
      CompUpgrade.ActivateComp(vehicle, compProperties);
  }

  private static void ActivateComp(VehiclePawn vehicle, CompProperties compProperties)
  {
    ThingComp thingComp = vehicle.GetDeactivatedComp(compProperties.compClass) ?? vehicle.GetComp(compProperties.compClass);
    if (thingComp == null)
    {
      thingComp = CompUpgrade.CreateComp((ThingWithComps) vehicle, compProperties);
      vehicle.AddComp(thingComp);
      thingComp.Initialize(compProperties);
    }
    vehicle.ActivateComp(thingComp);
  }

  private static void DeactivateComp(VehiclePawn vehicle, CompProperties compProperties)
  {
    System.Type compClass = compProperties.compClass;
    ThingComp comp = vehicle.GetComp(compClass);
    vehicle.DeactivateComp(comp);
  }

  public static ThingComp CreateComp(ThingWithComps parent, CompProperties compProperties)
  {
    ThingComp instance = (ThingComp) Activator.CreateInstance(compProperties.compClass);
    instance.parent = parent;
    return instance;
  }
}
