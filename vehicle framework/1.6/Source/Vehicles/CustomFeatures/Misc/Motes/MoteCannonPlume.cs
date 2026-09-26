// Decompiled with JetBrains decompiler
// Type: Vehicles.MoteCannonPlume
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class MoteCannonPlume : Mote
{
  protected int ticksActive;
  protected int frame;
  protected bool reverse;
  public int cyclesLeft;
  public AnimationWrapperType animationType;

  public Graphic_Animate Graphic => ((Thing) this).Graphic as Graphic_Animate;

  public virtual bool AnimationFinished
  {
    get => this.frame < 0 || this.frame > this.Graphic.AnimationCount;
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    this.exactPosition.y = Altitudes.AltitudeFor(((BuildableDef) ((Thing) this).def).altitudeLayer);
    this.Graphic.DrawWorkerAnimated((Thing) this, this.frame, this.exactRotation.ClampAngle());
  }

  protected virtual void Tick()
  {
    base.Tick();
    switch (this.animationType)
    {
      case AnimationWrapperType.Reset:
        this.TickReset();
        break;
      case AnimationWrapperType.Oscillate:
        this.TickOscillate();
        break;
      case AnimationWrapperType.Off:
        ((Thing) this).Destroy((DestroyMode) 0);
        break;
    }
    ++this.ticksActive;
  }

  public virtual void TickOscillate()
  {
    if (this.ticksActive >= this.Graphic.AnimationCount - 1)
    {
      if (this.reverse)
      {
        --this.cyclesLeft;
        if (this.cyclesLeft <= 0)
          ((Thing) this).Destroy((DestroyMode) 0);
      }
      this.ticksActive = 0;
      this.reverse = true;
    }
    if (this.reverse)
      --this.frame;
    else
      ++this.frame;
  }

  public virtual void TickReset()
  {
    if (this.ticksActive >= this.Graphic.AnimationCount - 1)
    {
      --this.cyclesLeft;
      if (this.cyclesLeft <= 0)
        ((Thing) this).Destroy((DestroyMode) 0);
      this.ticksActive = 0;
      this.frame = 0;
    }
    ++this.frame;
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (((Thing) this).Graphic is Graphic_Animate)
      return;
    Log.Error("Cannot spawn Mote_Animated without using Graphic_Animate class.");
  }

  public virtual void ExposeData()
  {
    ((Thing) this).ExposeData();
    Scribe_Values.Look<int>(ref this.ticksActive, "ticksActive", 0, false);
    Scribe_Values.Look<int>(ref this.frame, "frame", 0, false);
    Scribe_Values.Look<bool>(ref this.reverse, "reverse", false, false);
    Scribe_Values.Look<int>(ref this.cyclesLeft, "cyclesLeft", 0, false);
    Scribe_Values.Look<AnimationWrapperType>(ref this.animationType, "animationType", AnimationWrapperType.Reset, false);
  }
}
