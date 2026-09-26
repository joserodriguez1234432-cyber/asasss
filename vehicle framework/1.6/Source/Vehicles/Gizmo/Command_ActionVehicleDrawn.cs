// Decompiled with JetBrains decompiler
// Type: Vehicles.Command_ActionVehicleDrawn
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles;

public class Command_ActionVehicleDrawn : Command_Action
{
  public VehicleBuildDef buildDef;

  public virtual GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
  {
    GizmoResult gizmoResult = VehicleGui.GizmoOnGUIWithMaterial((Command) this, new Rect(topLeft.x, topLeft.y, ((Gizmo) this).GetWidth(maxWidth), 75f), parms, this.buildDef);
    if (((BuildableDef) this.buildDef).MadeFromStuff)
      Designator_Dropdown.DrawExtraOptionsIcon(topLeft, ((Gizmo) this).GetWidth(maxWidth));
    return gizmoResult;
  }
}
