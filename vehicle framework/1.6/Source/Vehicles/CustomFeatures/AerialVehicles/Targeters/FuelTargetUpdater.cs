// Decompiled with JetBrains decompiler
// Type: Vehicles.World.FuelTargetUpdater
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools.Targeting;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public sealed class FuelTargetUpdater(VehiclePawn vehicle, ILauncher launcher) : 
  FlightPathTargetUpdater(vehicle, launcher)
{
  private float TotalFuelCost { get; set; }

  public override void TargeterOnGUI()
  {
    base.TargeterOnGUI();
    Vector2 mousePosition = Event.current.mousePosition;
    if (this.vehicle.CompFueledTravel == null || (double) this.TotalFuelCost <= 0.0)
      return;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      string str = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleFuelCost", NamedArgument.op_Implicit(this.TotalFuelCost)));
      Vector2 vector2 = Text.CalcSize(str);
      Rect rect = new Rect(mousePosition.x, (float) ((double) mousePosition.y + (double) vector2.y + 20.0), vector2.x, vector2.y);
      GUI.DrawTexture(new Rect(((Rect) ref rect).x - vector2.x * 0.1f, ((Rect) ref rect).y, vector2.x * 1.2f, vector2.y), (Texture) TexUI.GrayTextBG);
      Widgets.Label(rect, str);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public override void TargeterUpdate([RequiresLocation, In] ref TargetData<GlobalTargetInfo> targetData)
  {
    base.TargeterUpdate(ref targetData);
    this.TotalFuelCost = this.vehicle.CompVehicleLauncher.FuelNeededToLaunchAtDist(this.TotalDistance);
  }

  protected override ShuttleLaunchStatus LaunchStatus()
  {
    ShuttleLaunchStatus shuttleLaunchStatus = base.LaunchStatus();
    if (shuttleLaunchStatus == ShuttleLaunchStatus.Invalid || this.vehicle.CompFueledTravel == null)
      return shuttleLaunchStatus;
    GlobalTargetInfo globalTargetInfo = FlightPathTargetUpdater.CurrentTargetUnderMouse();
    if (!((GlobalTargetInfo) ref globalTargetInfo).IsValid || (double) this.TotalFuelCost > (double) this.vehicle.CompFueledTravel.Fuel)
      return ShuttleLaunchStatus.Invalid;
    return (double) this.TotalFuelCost > (double) this.vehicle.CompFueledTravel.Fuel - (double) this.TotalFuelCost ? ShuttleLaunchStatus.NoReturnTrip : shuttleLaunchStatus;
  }
}
