// Decompiled with JetBrains decompiler
// Type: SmashTools.AnimationDriver
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;

#nullable disable
namespace SmashTools;

public class AnimationDriver
{
  private string label;
  private Func<int, int> animator;
  private AnimationDriver.AnimationDrawer drawHandler;
  private int totalAnimationTicks;
  private Action onSelect;

  public AnimationDriver(
    string label,
    Func<int, int> animator,
    AnimationDriver.AnimationDrawer drawHandler,
    int totalAnimationTicks,
    Action onSelect = null)
  {
    this.label = label;
    this.animator = animator;
    this.drawHandler = drawHandler;
    this.totalAnimationTicks = totalAnimationTicks;
    this.onSelect = onSelect;
  }

  public string Name => this.label;

  public int AnimationLength => this.totalAnimationTicks;

  public (Vector3 drawPos, float rotation) Draw(Vector3 drawPos, float rotation)
  {
    return this.drawHandler(drawPos, rotation);
  }

  public void Tick(int ticksPassed)
  {
    int num = this.animator(ticksPassed);
  }

  public void Select()
  {
    Action onSelect = this.onSelect;
    if (onSelect == null)
      return;
    onSelect();
  }

  public delegate (Vector3 drawPos, float rotation) AnimationDrawer(Vector3 drawPos, float rotation);
}
