// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Dialog_AnimationEditor
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

[StaticConstructorOnStartup]
public class Dialog_AnimationEditor : Window, IHighPriorityOnGUI
{
  private List<TabRecord> tabs = new List<TabRecord>();
  private Dialog_AnimationEditor.DialogTab dialogTab = Dialog_AnimationEditor.DialogTab.Controller;
  public IAnimator animator;
  public AnimationLayer animLayer;
  public AnimationController controller;
  private AnimationControllerEditor controllerEditor;
  private AnimationClipEditor clipEditor;

  public Dialog_AnimationEditor(IAnimator animator)
    : base((IWindowDrawing) null)
  {
    this.SetWindowProperties();
    this.InitializeTabs();
    Dialog_MethodSelector.InitStaticEventMethods();
    this.animator = animator;
    this.controllerEditor = new AnimationControllerEditor(this);
    this.clipEditor = new AnimationClipEditor(this);
  }

  private AnimationEditor ActiveTab
  {
    get
    {
      switch (this.dialogTab)
      {
        case Dialog_AnimationEditor.DialogTab.Animator:
          return (AnimationEditor) this.clipEditor;
        case Dialog_AnimationEditor.DialogTab.Controller:
          return (AnimationEditor) this.controllerEditor;
        default:
          throw new NotImplementedException();
      }
    }
  }

  private bool UnsavedChanges { get; set; }

  public float EditorMargin => this.Margin;

  public virtual Vector2 InitialSize
  {
    get => new Vector2((float) UI.screenWidth * 0.75f, (float) UI.screenHeight * 0.75f);
  }

  public virtual void PostOpen()
  {
    base.PostOpen();
    this.LoadAnimator(this.animator);
  }

  private void SetWindowProperties()
  {
    this.resizeable = true;
    this.doCloseX = true;
    this.closeOnAccept = false;
    this.closeOnClickedOutside = false;
    this.closeOnCancel = false;
    this.absorbInputAroundWindow = false;
    this.preventCameraMotion = true;
  }

  public void ChangeMade() => this.UnsavedChanges = true;

  private void LoadAnimator(IAnimator animator)
  {
    if (CameraView.InUse)
      CameraView.Close();
    this.animator = animator;
    this.controller = animator.Manager?.controller;
    if (!(bool) this.controller || this.controller.layers.NullOrEmpty<AnimationLayer>())
      this.controller = AnimationController.EmptyController();
    this.animLayer = this.controller.layers.FirstOrDefault<AnimationLayer>();
    this.controllerEditor.AnimatorLoaded(animator);
    this.clipEditor.AnimatorLoaded(animator);
  }

  public virtual void PostClose()
  {
    base.PostClose();
    CameraView.Close();
    this.controllerEditor.OnClose();
    this.clipEditor.OnClose();
  }

  public virtual void WindowUpdate()
  {
    base.WindowUpdate();
    this.controllerEditor.Update();
    this.clipEditor.Update();
  }

  public void OnGUIHighPriority()
  {
    if (Input.GetKeyDown((KeyCode) 102))
      this.ActiveTab.ResetToCenter();
    if (KeyBindingDefOf.Cancel.KeyDownEvent)
    {
      Event.current.Use();
      if (this.UnsavedChanges)
        Find.WindowStack.Add((Window) new Dialog_Confirm("You have unsaved changes. Close anyways?", (Action) (() => this.Close(true))));
      else
        this.Close(true);
    }
    this.ActiveTab.OnGUIHighPriority();
  }

  private void InitializeTabs()
  {
    this.tabs = new List<TabRecord>();
    this.tabs.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate("ST_ControllerWindow")), (Action) (() =>
    {
      this.dialogTab = Dialog_AnimationEditor.DialogTab.Controller;
      this.ActiveTab.OnTabOpen();
    }), (Func<bool>) (() => this.dialogTab == Dialog_AnimationEditor.DialogTab.Controller)));
    this.tabs.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate("ST_AnimationWindow")), (Action) (() =>
    {
      this.dialogTab = Dialog_AnimationEditor.DialogTab.Animator;
      this.ActiveTab.OnTabOpen();
    }), (Func<bool>) (() => this.dialogTab == Dialog_AnimationEditor.DialogTab.Animator)));
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    this.ResetControlFocus();
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Rect rect;
      // ISSUE: explicit constructor call
      ((Rect) ref rect).\u002Ector(((Rect) ref inRect).x, ((Rect) ref inRect).y + 32f, ((Rect) ref inRect).width, 32f);
      TabDrawer.DrawTabs<TabRecord>(rect, this.tabs, 200f);
      ref Rect local = ref inRect;
      ((Rect) ref local).yMin = ((Rect) ref local).yMin + ((Rect) ref rect).height;
      GUI.enabled = this.animator != null;
      this.ActiveTab.Draw(inRect);
      GUI.enabled = true;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void ResetControlFocus()
  {
    if (Event.current.type != 4 || Event.current.keyCode != 13 && Event.current.keyCode != 271 && Event.current.keyCode != 27)
      return;
    UI.UnfocusCurrentControl();
  }

  private enum DialogTab
  {
    Animator,
    Controller,
  }
}
