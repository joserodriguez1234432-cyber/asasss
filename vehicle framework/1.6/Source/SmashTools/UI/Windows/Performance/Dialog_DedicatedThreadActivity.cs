// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_DedicatedThreadActivity
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Performance;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class Dialog_DedicatedThreadActivity : Window
{
  private const float AsyncActionEntryHeight = 30f;
  private const int QueueLimit = 50;
  private DedicatedThread dedicatedThread;
  private Func<DedicatedThread> dedicatedThreadGetter;
  private List<AsyncAction> actionsSnapshot = new List<AsyncAction>();
  private Vector2 scrollPos;
  private Rect viewRect;

  public Dialog_DedicatedThreadActivity(DedicatedThread dedicatedThread)
    : base((IWindowDrawing) null)
  {
    this.dedicatedThread = dedicatedThread;
    this.SetWindowProperties();
  }

  public Dialog_DedicatedThreadActivity(Func<DedicatedThread> dedicatedThreadGetter)
    : base((IWindowDrawing) null)
  {
    this.dedicatedThreadGetter = dedicatedThreadGetter;
    this.SetWindowProperties();
  }

  public virtual Vector2 InitialSize => new Vector2(600f, 400f);

  public DedicatedThread DedicatedThread
  {
    get
    {
      if (this.dedicatedThread != null)
        return this.dedicatedThread;
      return this.dedicatedThreadGetter != null ? this.dedicatedThreadGetter() : (DedicatedThread) null;
    }
  }

  private void SetWindowProperties()
  {
    this.resizeable = true;
    this.doCloseX = true;
    this.closeOnClickedOutside = false;
    this.draggable = true;
    this.absorbInputAroundWindow = false;
    this.preventCameraMotion = false;
  }

  public virtual void PostOpen() => base.PostOpen();

  private void RecalculateViewRect(Rect rect, int entryCount)
  {
    this.viewRect = rect;
    ((Rect) ref this.viewRect).height = (float) entryCount * 30f;
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    DedicatedThread dedicatedThread = this.DedicatedThread;
    if (dedicatedThread == null)
      return;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 2);
    try
    {
      Rect rect1 = GenUI.ContractedBy(inRect, 5f);
      ((Rect) ref rect1).height = 32f;
      this.actionsSnapshot.Clear();
      dedicatedThread.Snapshot(this.actionsSnapshot);
      int entryCount = Mathf.Min(this.actionsSnapshot.Count, 50);
      Widgets.Label(rect1, $"DedicatedThread #{dedicatedThread.id} (Count={this.actionsSnapshot.Count})");
      Text.Font = (GameFont) 1;
      Rect rect2 = inRect;
      ((Rect) ref rect2).yMin = ((Rect) ref rect1).yMax + 5f;
      ref Rect local = ref rect2;
      ((Rect) ref local).height = ((Rect) ref local).height - 5f;
      Widgets.DrawMenuSection(rect2);
      Rect rect3 = GenUI.ContractedBy(rect2, 2f);
      Widgets.BeginScrollView(rect3, ref this.scrollPos, this.viewRect, true);
      for (int index = 0; index < entryCount; ++index)
      {
        AsyncAction asyncAction = this.actionsSnapshot[index];
        Rect viewRect = this.viewRect;
        ((Rect) ref viewRect).y = (float) index * 30f;
        ((Rect) ref viewRect).height = 30f;
        Widgets.Label(viewRect, $"{index}. {asyncAction.GetType().Name}");
        Widgets.DrawLineHorizontal(((Rect) ref viewRect).x, ((Rect) ref viewRect).yMax, ((Rect) ref viewRect).width);
      }
      Widgets.EndScrollView();
      this.RecalculateViewRect(rect3, entryCount);
    }
    finally
    {
      textBlock.Dispose();
    }
  }
}
