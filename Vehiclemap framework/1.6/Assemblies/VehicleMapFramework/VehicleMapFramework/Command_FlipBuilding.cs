// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Command_FlipBuilding
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Command_FlipBuilding : Command_Action
{
  public Texture2D commandIcon;

  public virtual void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
  {
    ((Command) this).DrawIcon(rect, buttonMat, parms);
    if (!Object.op_Implicit((Object) this.commandIcon))
      return;
    ref Rect local = ref rect;
    ((Rect) ref local).y = ((Rect) ref local).y - 8f;
    Widgets.DrawTextureFitted(rect, (Texture) this.commandIcon, 0.7f, 1f);
  }
}
