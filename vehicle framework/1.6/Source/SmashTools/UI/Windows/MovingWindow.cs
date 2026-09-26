// Decompiled with JetBrains decompiler
// Type: SmashTools.MovingWindow
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public abstract class MovingWindow : Window
{
  protected bool driftOut;
  private bool driftingOut;
  protected Action clickAction;
  protected int ticksActive;
  private Vector2 drift;

  public virtual Vector2 InitialSize => new Vector2(250f, 75f);

  protected virtual float Margin => 10f;

  protected virtual Vector2 MaxDrift => Vector2.zero;

  protected virtual Vector2 FloatSpeed => Vector2.zero;

  protected virtual int TicksTillRemoval => -1;

  protected virtual Vector2 WindowPosition
  {
    get => new Vector2(((Rect) ref this.windowRect).x, ((Rect) ref this.windowRect).y);
  }

  public virtual void PreOpen()
  {
    base.PreOpen();
    this.drift = Vector2.zero;
    this.ticksActive = 0;
    ((Rect) ref this.windowRect).x = this.WindowPosition.x;
    ((Rect) ref this.windowRect).y = this.WindowPosition.y;
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    this.Drift();
    this.TicksActive();
    if (!Widgets.ButtonInvisible(inRect, true))
      return;
    if (this.clickAction != null)
      this.clickAction();
    else
      this.Close(false);
  }

  protected virtual void Drift()
  {
    if (this.driftingOut)
    {
      ref Rect local1 = ref this.windowRect;
      ((Rect) ref local1).x = ((Rect) ref local1).x - this.FloatSpeed.x;
      this.drift.x -= Mathf.Abs(this.FloatSpeed.x);
      ref Rect local2 = ref this.windowRect;
      ((Rect) ref local2).y = ((Rect) ref local2).y - this.FloatSpeed.y;
      this.drift.y -= Mathf.Abs(this.FloatSpeed.y);
      if (this.ticksActive < this.TicksTillRemoval * 2 && !Vector2.op_Equality(((Rect) ref this.windowRect).position, this.WindowPosition))
        return;
      this.Close(false);
    }
    else
    {
      if ((double) this.drift.x < (double) this.MaxDrift.x)
      {
        ref Rect local = ref this.windowRect;
        ((Rect) ref local).x = ((Rect) ref local).x + this.FloatSpeed.x;
        this.drift.x += Mathf.Abs(this.FloatSpeed.x);
      }
      if ((double) this.drift.y >= (double) this.MaxDrift.y)
        return;
      ref Rect local3 = ref this.windowRect;
      ((Rect) ref local3).y = ((Rect) ref local3).y + this.FloatSpeed.y;
      this.drift.y += Mathf.Abs(this.FloatSpeed.y);
    }
  }

  protected virtual void TicksActive()
  {
    ++this.ticksActive;
    if (this.TicksTillRemoval <= 0 || this.ticksActive <= this.TicksTillRemoval)
      return;
    if (this.driftOut)
      this.driftingOut = true;
    else
      this.Close(false);
  }

  protected MovingWindow()
    : base((IWindowDrawing) null)
  {
  }
}
