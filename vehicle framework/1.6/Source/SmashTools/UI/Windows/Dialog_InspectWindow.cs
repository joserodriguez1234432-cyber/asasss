// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_InspectWindow
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[Obsolete]
public class Dialog_InspectWindow : Window
{
  private IInspectable inspectable;
  private Vector2 size = new Vector2(950f, 760f);

  public Dialog_InspectWindow(IInspectable inspectable)
    : base((IWindowDrawing) null)
  {
    this.inspectable = inspectable;
    this.forcePause = true;
    this.doCloseButton = true;
    this.doCloseX = true;
    this.absorbInputAroundWindow = true;
    this.closeOnClickedOutside = true;
    this.soundAppear = SoundDefOf.InfoCard_Open;
    this.soundClose = SoundDefOf.InfoCard_Close;
  }

  public Dialog_InspectWindow(IInspectable inspectable, Vector2 size)
    : this(inspectable)
  {
    this.size = size;
    this.SetInitialSizeAndPosition();
  }

  public virtual Vector2 InitialSize => this.size;

  public virtual void PreOpen() => base.PreOpen();

  public virtual void PostClose() => base.PostClose();

  public virtual void DoWindowContents(Rect inRect)
  {
  }
}
