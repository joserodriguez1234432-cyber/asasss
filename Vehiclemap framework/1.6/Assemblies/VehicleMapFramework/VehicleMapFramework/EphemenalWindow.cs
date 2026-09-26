// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.EphemenalWindow
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework;

public class EphemenalWindow : Window
{
  public Action doWindowFunc;
  public bool vanishIfMouseDistant = true;
  private Color baseColor = Color.white;

  public virtual Vector2 InitialSize => ((Rect) ref this.windowRect).size;

  protected virtual float Margin => 0.0f;

  public EphemenalWindow()
    : base((IWindowDrawing) null)
  {
    this.layer = (WindowLayer) 3;
    this.closeOnClickedOutside = true;
    this.doWindowBackground = false;
    this.drawShadow = false;
    this.doCloseButton = false;
    this.doCloseX = false;
    this.soundAppear = (SoundDef) null;
    this.soundClose = (SoundDef) null;
    this.closeOnAccept = false;
    this.closeOnCancel = false;
    this.focusWhenOpened = false;
    this.preventCameraMotion = false;
  }

  protected virtual void SetInitialSizeAndPosition()
  {
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    this.UpdateBaseColor();
    GUI.color = this.baseColor;
    this.doWindowFunc();
    GUI.color = Color.white;
  }

  private void UpdateBaseColor()
  {
    this.baseColor = Color.white;
    if (!this.vanishIfMouseDistant)
      return;
    Rect rect = GenUI.ContractedBy(GenUI.AtZero(this.windowRect), -5f);
    if (((Rect) ref rect).Contains(Event.current.mousePosition))
      return;
    float num = GenUI.DistFromRect(rect, Event.current.mousePosition);
    this.baseColor = new Color(1f, 1f, 1f, (float) (1.0 - (double) num / 95.0));
    if ((double) num <= 95.0)
      return;
    this.Close(false);
    this.Cancel();
  }

  public void Cancel()
  {
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.FloatMenu_Cancel, (Map) null);
    Find.WindowStack.TryRemove((Window) this, true);
  }
}
