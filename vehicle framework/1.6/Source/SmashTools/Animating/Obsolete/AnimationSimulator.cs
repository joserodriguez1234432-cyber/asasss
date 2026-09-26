// Decompiled with JetBrains decompiler
// Type: SmashTools.AnimationSimulator
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class AnimationSimulator
{
  internal static readonly float[] playbackSpeeds = new float[7]
  {
    0.25f,
    0.5f,
    0.75f,
    1f,
    2f,
    3f,
    4f
  };
  private static float realTimeToTick;
  private static int ticksThisFrame;
  private static int totalTicksPassed = 0;
  private static IAnimationTarget animationTarget;
  private static AnimationDriver animationDriver;
  private static AnimationSimulator.Ticker ticker;
  private static List<AnimationSimulator.AnimatedButton> animatedButtons = new List<AnimationSimulator.AnimatedButton>();

  public static bool Paused { get; set; }

  public static bool EditingTicks { get; set; }

  public static bool InUse => AnimationSimulator.animationTarget != null;

  public static IAnimationTarget AnimationTarget => AnimationSimulator.animationTarget;

  public static AnimationDriver CurrentDriver => AnimationSimulator.animationDriver;

  public static float PlaybackSpeed { get; set; } = 1f;

  public static bool PausedNoEdit => AnimationSimulator.Paused && !AnimationSimulator.EditingTicks;

  public static int TicksPassed
  {
    get => AnimationSimulator.totalTicksPassed;
    set
    {
      int val = value;
      AnimationDriver animationDriver = AnimationSimulator.animationDriver;
      int max = animationDriver != null ? animationDriver.AnimationLength : int.MaxValue;
      AnimationSimulator.totalTicksPassed = val.Clamp(0, max);
    }
  }

  public static float TimePerTick
  {
    get
    {
      return AnimationSimulator.PausedNoEdit ? 0.0f : (float) (1.0 / (60.0 * (double) AnimationSimulator.PlaybackSpeed));
    }
  }

  public static bool Reserve(IAnimationTarget animationTarget, AnimationSimulator.Ticker ticker)
  {
    if (AnimationSimulator.animationTarget != null && AnimationSimulator.animationTarget != animationTarget)
    {
      Log.Error("Attempting to reserve AnimationManager while it's already in use.  It should only be used by 1 updater at a time to avoid duplicating tick calls.");
      return false;
    }
    AnimationSimulator.animationTarget = animationTarget;
    AnimationSimulator.ticker = ticker;
    AnimationSimulator.Paused = true;
    return true;
  }

  public static void Release()
  {
    AnimationSimulator.Reset();
    AnimationSimulator.animationTarget = (IAnimationTarget) null;
    AnimationSimulator.animatedButtons.Clear();
  }

  public static void Reset()
  {
    AnimationSimulator.TicksPassed = 0;
    AnimationSimulator.realTimeToTick = 0.0f;
    AnimationSimulator.ticksThisFrame = 0;
    AnimationSimulator.Paused = true;
  }

  public static void SetDriver(AnimationDriver animationDriver)
  {
    AnimationSimulator.animationDriver = animationDriver;
    AnimationSimulator.animationDriver?.Select();
  }

  public static void TogglePause(bool validate = false)
  {
    AnimationSimulator.Paused = !AnimationSimulator.Paused;
    if (!validate || AnimationSimulator.Paused)
      return;
    AnimationSimulator.Paused = AnimationSimulator.animationDriver == null;
    if (AnimationSimulator.animationDriver != null)
      return;
    Messages.Message("Must select animation driver in order to play animation.", MessageTypeDefOf.RejectInput, true);
  }

  public static void OnGUI() => AnimationSimulator.UpdateAnimatedButtons_OnGUI();

  public static void Update()
  {
    AnimationSimulator.UpdateAnimatedButtons_Update();
    if (!AnimationSimulator.InUse)
      return;
    AnimationSimulator.ticksThisFrame = 0;
    if (AnimationSimulator.PausedNoEdit)
      return;
    float timePerTick = AnimationSimulator.TimePerTick;
    if ((double) Mathf.Abs(Time.deltaTime - timePerTick) < (double) timePerTick * 0.10000000149011612)
      AnimationSimulator.realTimeToTick += timePerTick;
    else
      AnimationSimulator.realTimeToTick += Time.deltaTime;
    while ((double) AnimationSimulator.realTimeToTick > 0.0 && (double) AnimationSimulator.ticksThisFrame < (double) AnimationSimulator.PlaybackSpeed * 2.0)
    {
      AnimationSimulator.DoSingleTick();
      AnimationSimulator.realTimeToTick -= timePerTick;
      ++AnimationSimulator.ticksThisFrame;
      if (AnimationSimulator.PausedNoEdit)
        break;
    }
    if ((double) AnimationSimulator.realTimeToTick <= 0.0)
      return;
    AnimationSimulator.realTimeToTick = 0.0f;
  }

  private static void DoSingleTick()
  {
    ++AnimationSimulator.TicksPassed;
    AnimationSimulator.ticker();
  }

  public static bool ButtonUpdated(
    Rect rect,
    Action updateHandler,
    Action onGUI,
    Func<bool> exitCondition,
    bool doMouseoverSound = true)
  {
    bool flag = Widgets.ButtonInvisible(rect, doMouseoverSound);
    if (flag)
      AnimationSimulator.animatedButtons.Add(new AnimationSimulator.AnimatedButton(updateHandler, onGUI, exitCondition));
    return flag;
  }

  private static void UpdateAnimatedButtons_Update()
  {
    for (int index = AnimationSimulator.animatedButtons.Count - 1; index >= 0; --index)
    {
      if (!AnimationSimulator.animatedButtons[index].Update())
        AnimationSimulator.animatedButtons.RemoveAt(index);
    }
  }

  private static void UpdateAnimatedButtons_OnGUI()
  {
    foreach (AnimationSimulator.AnimatedButton animatedButton in AnimationSimulator.animatedButtons)
      animatedButton.OnGUI();
  }

  public delegate void Ticker();

  public class AnimatedButton
  {
    private Action updateHandler;
    private Action onGUI;
    private Func<bool> exitCondition;

    public AnimatedButton(Action updateHandler, Action onGUI, Func<bool> exitCondition)
    {
      this.updateHandler = updateHandler;
      this.onGUI = onGUI;
      this.exitCondition = exitCondition;
    }

    public void OnGUI()
    {
      Action onGui = this.onGUI;
      if (onGui == null)
        return;
      onGui();
    }

    public bool Update()
    {
      Action updateHandler = this.updateHandler;
      if (updateHandler != null)
        updateHandler();
      return !this.exitCondition();
    }
  }
}
