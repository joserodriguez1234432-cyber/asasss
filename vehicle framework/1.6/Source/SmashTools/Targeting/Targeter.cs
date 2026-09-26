// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.Targeter`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Targeting;

public abstract class Targeter<T> : ITargeter
{
  private readonly ITargeterUpdate<T> updater;
  protected readonly TargetData<T> targetData;

  protected Targeter([CanBeNull] ITargeterUpdate<T> updater)
  {
    this.targetData = new TargetData<T>();
    this.updater = updater;
  }

  protected abstract TargeterResult PrimaryClick();

  protected virtual TargeterResult SecondaryClick() => TargeterResult.Cancel;

  public virtual void OnStart()
  {
  }

  public virtual void OnStop()
  {
  }

  public virtual void OnGUI()
  {
    this.ProcessInput();
    this.updater?.TargeterOnGUI();
  }

  public virtual void Update() => this.updater?.TargeterUpdate(ref this.targetData);

  protected abstract void Submit(ITargetOption option);

  private void Finalize(List<ITargetOption> options)
  {
    if (options.NullOrEmpty<ITargetOption>())
    {
      Trace.Fail("Finalizing results with no options to choose.");
      this.Stop();
    }
    else if (options.Count == 1)
    {
      ChooseOption(options[0]);
    }
    else
    {
      List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
      foreach (ITargetOption option1 in options)
      {
        ITargetOption option = option1;
        floatMenuOptionList.Add(new FloatMenuOption(TaggedString.op_Implicit(option.Label), (Action) (() => ChooseOption(option)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
      }
      Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
    }

    void ChooseOption(ITargetOption option)
    {
      this.Submit(option);
      this.Stop();
    }
  }

  private void ProcessInput()
  {
    Event current = Event.current;
    if (current != null && current.type == null)
    {
      TargeterResult targeterResult1;
      switch (Event.current.button)
      {
        case 0:
          targeterResult1 = this.PrimaryClick();
          break;
        case 1:
          targeterResult1 = this.SecondaryClick();
          break;
        default:
          targeterResult1 = TargeterResult.None;
          break;
      }
      TargeterResult targeterResult2 = targeterResult1;
      switch (targeterResult2.action)
      {
        case TargeterAction.Cancel:
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
          this.Stop();
          break;
        case TargeterAction.Reject:
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
          break;
        case TargeterAction.Submit:
          this.Finalize(targeterResult2.options);
          break;
      }
      Event.current.Use();
    }
    if (!KeyBindingDefOf.Cancel.KeyDownEvent)
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.CancelMode, (Map) null);
    this.Stop();
    Event.current.Use();
  }
}
