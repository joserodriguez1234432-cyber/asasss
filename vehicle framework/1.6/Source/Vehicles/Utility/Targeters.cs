// Decompiled with JetBrains decompiler
// Type: Vehicles.Targeters
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class Targeters
{
  private static readonly List<BaseTargeter> MapTargeters = new List<BaseTargeter>();
  private static readonly List<BaseWorldTargeter> WorldTargeters = new List<BaseWorldTargeter>();

  private static BaseTargeter CurrentTargeter { get; set; }

  private static BaseWorldTargeter CurrentWorldTargeter { get; set; }

  static Targeters()
  {
    GameEvent.OnGameDisposing += new Action(Targeters.ClearAllTargeters);
    foreach (System.Type type in GenTypes.InstantiableDescendantsAndSelf(typeof (BaseTargeter)))
    {
      BaseTargeter instance = (BaseTargeter) Activator.CreateInstance(type, (object[]) null);
      Targeters.MapTargeters.Add(instance);
      instance.PostInit();
    }
    foreach (System.Type type in GenTypes.InstantiableDescendantsAndSelf(typeof (BaseWorldTargeter)))
    {
      BaseWorldTargeter instance = (BaseWorldTargeter) Activator.CreateInstance(type, (object[]) null);
      Targeters.WorldTargeters.Add(instance);
      instance.PostInit();
    }
  }

  private static void ClearAllTargeters()
  {
    Targeters.CurrentTargeter?.StopTargeting();
    Targeters.CurrentTargeter = (BaseTargeter) null;
    Targeters.CurrentWorldTargeter?.StopTargeting();
    Targeters.CurrentWorldTargeter = (BaseWorldTargeter) null;
  }

  internal static void PushTargeter(BaseTargeter targeter)
  {
    if (Targeters.CurrentTargeter == targeter)
      return;
    Targeters.CurrentTargeter?.StopTargeting();
    Targeters.CurrentTargeter = targeter;
  }

  internal static void PushTargeter(BaseWorldTargeter targeter)
  {
    if (Targeters.CurrentWorldTargeter == targeter)
      return;
    Targeters.CurrentWorldTargeter?.StopTargeting();
    Targeters.CurrentWorldTargeter = targeter;
  }

  private static void StopTargeter(BaseTargeter targeter)
  {
    if (Targeters.CurrentTargeter != targeter)
      return;
    Targeters.CurrentTargeter.StopTargeting();
    Targeters.CurrentTargeter = (BaseTargeter) null;
  }

  private static void StopTargeter(BaseWorldTargeter targeter)
  {
    if (Targeters.CurrentWorldTargeter != targeter)
      return;
    Targeters.CurrentWorldTargeter.StopTargeting();
    Targeters.CurrentWorldTargeter = (BaseWorldTargeter) null;
  }

  internal static void OnGUITargeter()
  {
    if (Targeters.CurrentTargeter == null)
      return;
    if (!Targeters.CurrentTargeter.IsTargeting)
      Targeters.StopTargeter(Targeters.CurrentTargeter);
    else
      Targeters.CurrentTargeter.TargeterOnGUI();
  }

  internal static void UpdateTargeter()
  {
    if (Targeters.CurrentTargeter == null)
      return;
    if (!Targeters.CurrentTargeter.IsTargeting)
      Targeters.StopTargeter(Targeters.CurrentTargeter);
    else
      Targeters.CurrentTargeter.TargeterUpdate();
  }

  internal static void ProcessTargeterInputEvent()
  {
    if (Targeters.CurrentTargeter == null)
      return;
    if (!Targeters.CurrentTargeter.IsTargeting)
      Targeters.StopTargeter(Targeters.CurrentTargeter);
    else
      Targeters.CurrentTargeter.ProcessInputEvents();
  }

  internal static void OnGUIWorldTargeter()
  {
    if (Targeters.CurrentWorldTargeter == null)
      return;
    if (!Targeters.CurrentWorldTargeter.IsTargeting)
      Targeters.StopTargeter(Targeters.CurrentWorldTargeter);
    else
      Targeters.CurrentWorldTargeter.TargeterOnGUI();
  }

  internal static void UpdateWorldTargeter()
  {
    if (Targeters.CurrentWorldTargeter == null)
      return;
    if (!Targeters.CurrentWorldTargeter.IsTargeting)
      Targeters.StopTargeter(Targeters.CurrentWorldTargeter);
    else
      Targeters.CurrentWorldTargeter.TargeterUpdate();
  }

  internal static void ProcessWorldTargeterInputEvent()
  {
    if (Targeters.CurrentWorldTargeter == null)
      return;
    if (!Targeters.CurrentWorldTargeter.IsTargeting)
      Targeters.StopTargeter(Targeters.CurrentWorldTargeter);
    else
      Targeters.CurrentWorldTargeter.ProcessInputEvents();
  }
}
