// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompPowerPole
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompPowerPole : CompPowerNetLink
{
  private Vector3 prevDrawPos;

  protected override float Radius => ModCompat.PowerPoles.CableMaxDistance.Invoke();

  protected override float MaxPowerPush => 5000f;

  protected override float PowerLossFactor => 1f;

  protected override CompPowerNetLink.PowerTransferMode Mode
  {
    get => CompPowerNetLink.PowerTransferMode.Transmit;
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    if (!respawningAfterLoad)
      return;
    FrameDelay.DelayOne<Vector3>(new Action<Vector3>(this.RegeneratePoints), this.prevDrawPos);
  }

  public override void CompTick()
  {
    Vector3 drawPos = ((Thing) ((ThingComp) this).parent).DrawPos;
    if (Vector3.op_Inequality(this.prevDrawPos, drawPos))
      this.RegeneratePoints(drawPos);
    if (Gen.IsHashIntervalTick((Thing) ((ThingComp) this).parent, 250))
      ((Entity) ((ThingComp) this).parent).TickRare();
    base.CompTick();
  }

  private void RegeneratePoints(Vector3 drawPos)
  {
    this.prevDrawPos = drawPos;
    if (!GenTypes.SameOrSubclassOf(((Thing) ((ThingComp) this).parent).def.thingClass, ModCompat.PowerPoles.Building_LongDistanceCabled))
      return;
    foreach (Building building in Patch_Building_LongDistancePower_GetAllLinked.GetAllLinked((Building) ((ThingComp) this).parent, false))
    {
      if (GenTypes.SameOrSubclassOf(((Thing) building).def.thingClass, ModCompat.PowerPoles.Building_LongDistanceCabled))
      {
        ModCompat.PowerPoles.GeneratePointsAsync.Invoke((object) ((ThingComp) this).parent, Params<(object, object)>.Get(((object) ((ThingComp) this).parent, (object) building)));
        ModCompat.PowerPoles.GeneratePointsAsync.Invoke((object) building, Params<(object, object)>.Get(((object) building, (object) ((ThingComp) this).parent)));
      }
    }
  }

  protected override bool TryFindConnection(out CompPowerNetLink linkTo)
  {
    linkTo = (CompPowerNetLink) null;
    return false;
  }

  public override void Disconnect()
  {
    ThingWithComps linkedTo = this.LinkedTo;
    if (linkedTo != null && GenTypes.SameOrSubclassOf(((Thing) ((ThingComp) this).parent).def.thingClass, ModCompat.PowerPoles.Building_LongDistancePower) && GenTypes.SameOrSubclassOf(((Thing) linkedTo).def.thingClass, ModCompat.PowerPoles.Building_LongDistancePower) && (bool) ModCompat.PowerPoles.IsLinkedTo.Invoke((object) ((ThingComp) this).parent, SingleParam.Get((object) linkedTo)))
      Delay.AfterNSeconds(0.5f, (Action) (() =>
      {
        ModCompat.PowerPoles.TryRemoveLink.Invoke((object) ((ThingComp) this).parent, SingleParam.Get((object) linkedTo));
        if (!GenTypes.SameOrSubclassOf(((Thing) ((ThingComp) this).parent).def.thingClass, ModCompat.PowerPoles.Building_LongDistanceCabled) || !GenTypes.SameOrSubclassOf(((Thing) linkedTo).def.thingClass, ModCompat.PowerPoles.Building_LongDistanceCabled))
          return;
        ModCompat.PowerPoles.connectionToPoints.Invoke((Thing) ((ThingComp) this).parent).Remove((object) linkedTo);
        ModCompat.PowerPoles.connectionToPoints.Invoke((Thing) linkedTo).Remove((object) ((ThingComp) this).parent);
      }));
    base.Disconnect();
  }
}
