// Decompiled with JetBrains decompiler
// Type: Vehicles.Command_ActionHighlighter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles;

public class Command_ActionHighlighter : Command_Action
{
  public Action mouseOver;

  public virtual void GizmoUpdateOnMouseover()
  {
    base.GizmoUpdateOnMouseover();
    if (this.mouseOver == null)
      return;
    this.mouseOver();
  }
}
