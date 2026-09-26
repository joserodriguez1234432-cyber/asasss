// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Command_ToggleWithIcon
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Command_ToggleWithIcon : Command_Toggle
{
  public Texture miniIcon;
  public float miniIconSize = 24f;

  public virtual GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth, GizmoRenderParms parms)
  {
    GizmoResult gizmoResult = ((Command) this).GizmoOnGUIInt(new Rect(loc.x, loc.y, ((Gizmo) this).GetWidth(maxWidth), 75f), parms);
    if (((Gizmo) this).disabled && this.hideIconIfDisabled)
      return gizmoResult;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(loc.x, loc.y, ((Gizmo) this).GetWidth(maxWidth), 75f);
    Widgets.DrawTextureFitted(new Rect(((Rect) ref rect).x + ((Rect) ref rect).width - this.miniIconSize, ((Rect) ref rect).y, this.miniIconSize, this.miniIconSize), this.miniIcon, 1f, this.isActive() ? 1f : 0.3f);
    return gizmoResult;
  }
}
