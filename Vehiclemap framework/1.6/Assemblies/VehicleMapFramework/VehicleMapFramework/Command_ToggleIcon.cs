// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Command_ToggleIcon
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Command_ToggleIcon : Command
{
  public Func<bool> isActive;
  public Action toggleAction;
  public SoundDef toggleSound;
  public string labelTwo;
  public Texture iconTwo;

  public virtual SoundDef CurActivateSound => this.toggleSound;

  public virtual string Label => !this.isActive() ? this.labelTwo : this.defaultLabel;

  public virtual void ProcessInput(Event ev)
  {
    base.ProcessInput(ev);
    this.toggleAction();
  }

  public virtual void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
  {
    Texture texture = (Texture) ((this.isActive() ? (object) this.icon : (object) this.iconTwo) ?? (object) BaseContent.BadTex);
    ref Rect local = ref rect;
    ((Rect) ref local).position = Vector2.op_Addition(((Rect) ref local).position, new Vector2(this.iconOffset.x * ((Rect) ref rect).size.x, this.iconOffset.y * ((Rect) ref rect).size.y));
    GUI.color = !((Gizmo) this).disabled || parms.lowLight ? this.IconDrawColor : GenColor.SaturationChanged(this.IconDrawColor, 0.0f);
    if (parms.lowLight)
      GUI.color = ColorExtension.ToTransparent(GUI.color, 0.6f);
    Widgets.DrawTextureFitted(rect, texture, this.iconDrawScale * 0.85f, this.iconProportions, this.iconTexCoords, this.iconAngle, buttonMat, 1f);
    GUI.color = Color.white;
  }
}
