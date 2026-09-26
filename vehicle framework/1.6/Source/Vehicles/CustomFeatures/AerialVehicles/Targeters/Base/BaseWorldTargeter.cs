// Decompiled with JetBrains decompiler
// Type: Vehicles.World.BaseWorldTargeter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public abstract class BaseWorldTargeter
{
  protected PlanetTile origin = PlanetTile.op_Implicit(-1);
  protected Vector3 originOnMap;
  protected Action actionWhenFinished;
  protected Action onUpdate;
  protected Texture2D mouseAttachment;
  protected bool canTargetTiles;
  public bool closeWorldTabWhenFinished;

  public abstract bool IsTargeting { get; }

  public abstract void StopTargeting();

  public abstract void ProcessInputEvents();

  public abstract void TargeterOnGUI();

  public abstract void TargeterUpdate();

  public virtual void OnStart() => Targeters.PushTargeter(this);

  protected virtual GlobalTargetInfo CurrentTargetUnderMouse()
  {
    if (!this.IsTargeting)
      return GlobalTargetInfo.Invalid;
    List<WorldObject> worldObjectList = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
    if (GenCollection.Any<WorldObject>(worldObjectList))
      return GlobalTargetInfo.op_Implicit(worldObjectList[0]);
    if (!this.canTargetTiles)
      return GlobalTargetInfo.Invalid;
    int num = PlanetTile.op_Implicit(GenWorld.MouseTile(false));
    return num >= 0 ? new GlobalTargetInfo(PlanetTile.op_Implicit(num)) : GlobalTargetInfo.Invalid;
  }

  public virtual void PostInit()
  {
  }
}
