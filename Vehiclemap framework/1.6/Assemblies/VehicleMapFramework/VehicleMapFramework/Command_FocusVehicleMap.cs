// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Command_FocusVehicleMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public sealed class Command_FocusVehicleMap : Command
{
  public static VehiclePawnWithMap FocusLockedVehicle { get; set; }

  public static VehiclePawnWithMap FocusedVehicle { get; set; }

  public virtual string Label
  {
    get
    {
      return !(Find.Selector.SingleSelectedObject is VehiclePawnWithMap singleSelectedObject) || singleSelectedObject == Command_FocusVehicleMap.FocusLockedVehicle ? TaggedString.op_Implicit(Translator.Translate("VMF_UnfocusVehicleMap")) : TaggedString.op_Implicit(Translator.Translate("VMF_FocusVehicleMap"));
    }
  }

  public Command_FocusVehicleMap() => ((Gizmo) this).Order = 5000f;

  public virtual void ProcessInput(Event ev)
  {
    if (Find.Selector.SingleSelectedObject is VehiclePawnWithMap singleSelectedObject && Command_FocusVehicleMap.FocusLockedVehicle != singleSelectedObject)
    {
      Command_FocusVehicleMap.FocusLockedVehicle = singleSelectedObject;
      Command_FocusVehicleMap.FocusedVehicle = singleSelectedObject;
    }
    else
    {
      Command_FocusVehicleMap.FocusLockedVehicle = (VehiclePawnWithMap) null;
      Command_FocusVehicleMap.FocusedVehicle = (VehiclePawnWithMap) null;
    }
  }

  public readonly struct FocusVehicle : IDisposable
  {
    private readonly VehiclePawnWithMap tmpFocused;

    public FocusVehicle(VehiclePawnWithMap vehicle)
    {
      this.tmpFocused = Command_FocusVehicleMap.FocusedVehicle;
      Command_FocusVehicleMap.FocusedVehicle = vehicle;
    }

    public void Dispose() => Command_FocusVehicleMap.FocusedVehicle = this.tmpFocused;
  }
}
