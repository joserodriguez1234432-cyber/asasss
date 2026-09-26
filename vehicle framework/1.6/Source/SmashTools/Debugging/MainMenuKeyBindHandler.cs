// Decompiled with JetBrains decompiler
// Type: SmashTools.MainMenuKeyBindHandler
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class MainMenuKeyBindHandler
{
  private static readonly List<(KeyBindingDef keyBindingDef, Action action)> KeyBindings = new List<(KeyBindingDef, Action)>();

  public static void RegisterKeyBind(KeyBindingDef keyBindingDef, Action action)
  {
    if (GenCollection.Any<(KeyBindingDef, Action)>(MainMenuKeyBindHandler.KeyBindings, (Predicate<(KeyBindingDef, Action)>) (pair => pair.keyBindingDef == keyBindingDef)))
      return;
    MainMenuKeyBindHandler.KeyBindings.Add((keyBindingDef, action));
  }

  internal static bool HandleKeyInputs()
  {
    if (!Prefs.DevMode)
      return true;
    foreach ((KeyBindingDef keyBindingDef, Action action) in MainMenuKeyBindHandler.KeyBindings)
    {
      if (Event.current != null && keyBindingDef.KeyDownEvent)
      {
        action();
        Event.current.Use();
      }
    }
    return true;
  }
}
