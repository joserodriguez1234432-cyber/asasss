// Decompiled with JetBrains decompiler
// Type: Vehicles.SoundUpgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class SoundUpgrade : Upgrade
{
  public List<VehicleSoundEventEntry<VehicleEventDef>> addOneShots;
  public List<VehicleSoundEventEntry<VehicleEventDef>> removeOneShots;
  public List<VehicleSustainerEventEntry<VehicleEventDef>> addSustainers;
  public List<VehicleSustainerEventEntry<VehicleEventDef>> removeSustainers;

  public override bool UnlockOnLoad => true;

  public override void Unlock(VehiclePawn vehicle, bool unlockingPostLoad)
  {
    if (!GenList.NullOrEmpty<VehicleSoundEventEntry<VehicleEventDef>>((IList<VehicleSoundEventEntry<VehicleEventDef>>) this.removeOneShots))
    {
      foreach (VehicleSoundEventEntry<VehicleEventDef> removeOneShot in this.removeOneShots)
        vehicle.RemoveEvent<VehicleEventDef>(removeOneShot.key, removeOneShot.removalKey);
    }
    if (!GenList.NullOrEmpty<VehicleSoundEventEntry<VehicleEventDef>>((IList<VehicleSoundEventEntry<VehicleEventDef>>) this.addOneShots))
    {
      foreach (VehicleSoundEventEntry<VehicleEventDef> addOneShot in this.addOneShots)
      {
        VehicleSoundEventEntry<VehicleEventDef> soundEventEntry = addOneShot;
        vehicle.AddEvent<VehicleEventDef>(soundEventEntry.key, (Action) (() => vehicle.PlayOneShotOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
      }
    }
    if (!GenList.NullOrEmpty<VehicleSustainerEventEntry<VehicleEventDef>>((IList<VehicleSustainerEventEntry<VehicleEventDef>>) this.removeSustainers))
    {
      foreach (VehicleSustainerEventEntry<VehicleEventDef> removeSustainer in this.removeSustainers)
      {
        vehicle.sustainers.EndAll(removeSustainer.value);
        vehicle.RemoveEvent<VehicleEventDef>(removeSustainer.start, removeSustainer.removalKey);
        vehicle.RemoveEvent<VehicleEventDef>(removeSustainer.stop, removeSustainer.removalKey);
      }
    }
    if (GenList.NullOrEmpty<VehicleSustainerEventEntry<VehicleEventDef>>((IList<VehicleSustainerEventEntry<VehicleEventDef>>) this.addSustainers))
      return;
    foreach (VehicleSustainerEventEntry<VehicleEventDef> addSustainer in this.addSustainers)
    {
      VehicleSustainerEventEntry<VehicleEventDef> soundEventEntry = addSustainer;
      vehicle.AddEvent<VehicleEventDef>(soundEventEntry.start, (Action) (() => vehicle.StartSustainerOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
      vehicle.AddEvent<VehicleEventDef>(soundEventEntry.stop, (Action) (() => vehicle.StopSustainerOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
    }
  }

  public override void Refund(VehiclePawn vehicle)
  {
    if (!GenList.NullOrEmpty<VehicleSoundEventEntry<VehicleEventDef>>((IList<VehicleSoundEventEntry<VehicleEventDef>>) this.addOneShots))
    {
      foreach (VehicleSoundEventEntry<VehicleEventDef> addOneShot in this.addOneShots)
        vehicle.RemoveEvent<VehicleEventDef>(addOneShot.key, addOneShot.removalKey);
    }
    if (!GenList.NullOrEmpty<VehicleSoundEventEntry<VehicleEventDef>>((IList<VehicleSoundEventEntry<VehicleEventDef>>) this.removeOneShots))
    {
      foreach (VehicleSoundEventEntry<VehicleEventDef> removeOneShot in this.removeOneShots)
      {
        VehicleSoundEventEntry<VehicleEventDef> soundEventEntry = removeOneShot;
        vehicle.AddEvent<VehicleEventDef>(soundEventEntry.key, (Action) (() => vehicle.PlayOneShotOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
      }
    }
    if (!GenList.NullOrEmpty<VehicleSustainerEventEntry<VehicleEventDef>>((IList<VehicleSustainerEventEntry<VehicleEventDef>>) this.addSustainers))
    {
      foreach (VehicleSustainerEventEntry<VehicleEventDef> addSustainer in this.addSustainers)
      {
        vehicle.sustainers.EndAll(addSustainer.value);
        vehicle.RemoveEvent<VehicleEventDef>(addSustainer.start, addSustainer.removalKey);
        vehicle.RemoveEvent<VehicleEventDef>(addSustainer.stop, addSustainer.removalKey);
      }
    }
    if (GenList.NullOrEmpty<VehicleSustainerEventEntry<VehicleEventDef>>((IList<VehicleSustainerEventEntry<VehicleEventDef>>) this.removeSustainers))
      return;
    foreach (VehicleSustainerEventEntry<VehicleEventDef> removeSustainer in this.removeSustainers)
    {
      VehicleSustainerEventEntry<VehicleEventDef> soundEventEntry = removeSustainer;
      vehicle.AddEvent<VehicleEventDef>(soundEventEntry.start, (Action) (() => vehicle.StartSustainerOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
      vehicle.AddEvent<VehicleEventDef>(soundEventEntry.stop, (Action) (() => vehicle.StopSustainerOnVehicle<VehicleEventDef>(soundEventEntry)), soundEventEntry.removalKey);
    }
  }
}
