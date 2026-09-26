// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompEngineLightOverlay
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompEngineLightOverlay : CompOpacityOverlay
{
  private bool ignitionComplete;
  private bool landingComplete;

  public CompProperties_EngineLightOverlay Props
  {
    get => (CompProperties_EngineLightOverlay) ((ThingComp) this).props;
  }

  public override IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    yield break;
  }

  public virtual void CompTick()
  {
    if (!(this.Overlay?.Graphic is Graphic_VehicleOpacity graphic))
      return;
    CompVehicleLauncher compVehicleLauncher = this.Vehicle.CompVehicleLauncher;
    if (compVehicleLauncher != null && compVehicleLauncher.inFlight)
    {
      LaunchProtocol launchProtocol = this.Vehicle.CompVehicleLauncher.launchProtocol;
      float num1 = launchProtocol is VTOLTakeoff vtolTakeoff ? vtolTakeoff.TimeInAnimationVTOL : launchProtocol.TimeInAnimation;
      float num2 = Mathf.Min(graphic.Opacity + (float) (((double) this.Props.inFlightOpacity - (double) graphic.Opacity) * (double) num1 * 0.10000000149011612), this.Props.inFlightOpacity);
      graphic.Opacity = num2;
    }
    else if (this.Vehicle.ignition.Drafted)
    {
      this.landingComplete = false;
      if (this.ignitionComplete)
        return;
      float num = (this.Props.engineOnOpacity - this.Props.engineOffOpacity) / this.Props.ignitionDuration;
      graphic.Opacity += num;
      if ((double) Mathf.Abs(this.Props.engineOnOpacity - graphic.Opacity) > (double) Mathf.Abs(num))
        return;
      this.ignitionComplete = true;
      graphic.Opacity = this.Props.engineOnOpacity;
    }
    else
    {
      this.ignitionComplete = false;
      if (this.landingComplete)
        return;
      float num = (this.Props.engineOffOpacity - this.Props.engineOnOpacity) / this.Props.ignitionDuration;
      graphic.Opacity += num;
      if ((double) Mathf.Abs(this.Props.engineOffOpacity - graphic.Opacity) > (double) Mathf.Abs(num))
        return;
      this.landingComplete = true;
      graphic.Opacity = this.Props.engineOffOpacity;
    }
  }

  public override void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Values.Look<bool>(ref this.ignitionComplete, "ignitionComplete", false, false);
    Scribe_Values.Look<bool>(ref this.landingComplete, "landingComplete", false, false);
  }
}
