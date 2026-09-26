// Decompiled with JetBrains decompiler
// Type: Vehicles.BaseTargeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public abstract class BaseTargeter
{
  protected VehiclePawn vehicle;
  protected Action actionWhenFinished;
  protected Texture2D mouseAttachment;

  public abstract bool IsTargeting { get; }

  public abstract void StopTargeting();

  public abstract void ProcessInputEvents();

  public abstract void TargeterOnGUI();

  public abstract void TargeterUpdate();

  protected virtual void OnStart() => Targeters.PushTargeter(this);

  protected virtual LocalTargetInfo CurrentTargetUnderMouse()
  {
    return !this.IsTargeting ? LocalTargetInfo.Invalid : LocalTargetInfo.op_Implicit(UI.MouseCell());
  }

  public virtual void PostInit()
  {
  }
}
